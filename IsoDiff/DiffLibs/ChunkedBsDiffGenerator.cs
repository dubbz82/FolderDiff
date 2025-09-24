using DeltaQ.SuffixSorting;
using DeltaQ.SuffixSorting.SAIS;
using ICSharpCode.SharpZipLib.BZip2;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public class ChunkedBsdiffGenerator : IDisposable
{
    private readonly string tempDir;
    private readonly int chunkSize;
    private readonly int preloadSize;

    private readonly ConcurrentBag<string> threadCtrlFiles = new();
    private readonly ConcurrentBag<string> threadDiffFiles = new();
    private readonly ConcurrentBag<string> threadExtraFiles = new();

    public ChunkedBsdiffGenerator(string? tempPath = null, int chunkSize = 1024 * 1024, int preloadSize = 8 * 1024 * 1024)
    {
        this.chunkSize = chunkSize;
        this.preloadSize = preloadSize;

        tempDir = Path.Combine(tempPath ?? Path.GetTempPath(), "ChunkedBsdiff_" + Guid.NewGuid());
        Directory.CreateDirectory(tempDir);
    }

    public void GenerateDiff(string oldFilePath, string newFilePath, Stream output)
    {
        byte[] oldData = File.ReadAllBytes(oldFilePath);
        int[] suffixArray = BuildSuffixArray(oldData);
        long newFileLength = new FileInfo(newFilePath).Length;

        var tasks = new List<Task>();

        foreach (var (newOffset, newChunk) in ReadChunks(newFilePath))
        {
            var chunkCopy = newChunk.ToArray(); // Safe for thread
            var offsetCopy = newOffset;

            tasks.Add(Task.Run(() =>
            {
                ProcessChunkThreadSafe(oldData, chunkCopy, suffixArray, offsetCopy);
            }));
        }

        Task.WaitAll(tasks.ToArray());

        WriteBsdiffPatch(output, newFileLength);
        Cleanup();
    }

    private void ProcessChunkThreadSafe(
        ReadOnlySpan<byte> oldData,
        byte[] newChunk,
        int[] suffixArray,
        long newBasePos)
    {
        int newPos = 0;
        long lastOldPos = 0;

        string id = Guid.NewGuid().ToString("N");
        string ctrlFile = Path.Combine(tempDir, $"ctrl_{id}.tmp");
        string diffFile = Path.Combine(tempDir, $"diff_{id}.tmp");
        string extraFile = Path.Combine(tempDir, $"extra_{id}.tmp");

        threadCtrlFiles.Add(ctrlFile);
        threadDiffFiles.Add(diffFile);
        threadExtraFiles.Add(extraFile);

        using var ctrlStream = File.Create(ctrlFile);
        using var diffStream = File.Create(diffFile);
        using var extraStream = File.Create(extraFile);

        while (newPos < newChunk.Length)
        {
            Search(oldData, newChunk, suffixArray, newPos, out int matchPos, out int matchLen);

            matchLen = Math.Min(matchLen, newChunk.Length - newPos);
            matchLen = Math.Min(matchLen, oldData.Length - matchPos);

            if (matchLen == 0)
            {
                int extraLen = 1;
                while (newPos + extraLen < newChunk.Length)
                {
                    Search(oldData, newChunk, suffixArray, newPos + extraLen, out _, out int nextMatch);
                    if (nextMatch > 0) break;
                    extraLen++;
                }

                extraStream.Write(newChunk, newPos, extraLen);
                WriteControlBlock(ctrlStream, 0, extraLen, 0);
                newPos += extraLen;
                continue;
            }

            byte[] diffBuffer = ArrayPool<byte>.Shared.Rent(matchLen);
            try
            {
                for (int i = 0; i < matchLen; i++)
                    diffBuffer[i] = (byte)(newChunk[newPos + i] - oldData[matchPos + i]);

                diffStream.Write(diffBuffer, 0, matchLen);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(diffBuffer);
            }

            WriteControlBlock(ctrlStream, matchLen, 0, matchPos - lastOldPos);
            lastOldPos = matchPos + matchLen;
            newPos += matchLen;
        }
    }

    private IEnumerable<(long offset, ReadOnlyMemory<byte> chunk)> ReadChunks(string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, preloadSize, FileOptions.SequentialScan);
        byte[] buffer = ArrayPool<byte>.Shared.Rent(preloadSize);

        try
        {
            long offset = 0;
            int read;
            while ((read = fs.Read(buffer, 0, preloadSize)) > 0)
            {
                int preloadOffset = 0;
                while (preloadOffset < read)
                {
                    int size = Math.Min(chunkSize, read - preloadOffset);
                    yield return (offset + preloadOffset, buffer.AsMemory(preloadOffset, size));
                    preloadOffset += size;
                }
                offset += read;
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private int[] BuildSuffixArray(ReadOnlySpan<byte> data)
    {
        ISuffixSort sorter = new SAIS();
        using var result = sorter.Sort(data);
        return result.Memory.ToArray();
    }

    private void Search(ReadOnlySpan<byte> oldData, ReadOnlySpan<byte> newData, int[] suffixArray,
                        int newPos, out int matchPos, out int matchLen)
    {
        int left = 0, right = suffixArray.Length;
        matchPos = 0;
        matchLen = 0;

        while (left < right)
        {
            int mid = (left + right) / 2;
            int pos = suffixArray[mid];
            int len = CommonPrefixLength(oldData, pos, newData, newPos);

            if (len > matchLen)
            {
                matchLen = len;
                matchPos = pos;
            }

            int cmp = CompareByteAt(oldData, pos, newData, newPos);
            if (cmp < 0)
                left = mid + 1;
            else
                right = mid;
        }
    }

    private int CommonPrefixLength(ReadOnlySpan<byte> a, int aPos, ReadOnlySpan<byte> b, int bPos)
    {
        int len = 0;
        while (aPos + len < a.Length && bPos + len < b.Length && a[aPos + len] == b[bPos + len])
            len++;
        return len;
    }

    private int CompareByteAt(ReadOnlySpan<byte> a, int aPos, ReadOnlySpan<byte> b, int bPos)
    {
        if (aPos >= a.Length) return -1;
        if (bPos >= b.Length) return 1;
        return a[aPos].CompareTo(b[bPos]);
    }

    private void WriteControlBlock(Stream stream, long diffLen, long extraLen, long offset)
    {
        Span<byte> buf = stackalloc byte[24];
        WriteInt64LE(buf.Slice(0, 8), diffLen);
        WriteInt64LE(buf.Slice(8, 8), extraLen);
        WriteInt64LE(buf.Slice(16, 8), offset);
        stream.Write(buf);
    }

    private void WriteInt64LE(Span<byte> buf, long value)
    {
        for (int i = 0; i < 8; i++)
            buf[i] = (byte)(value >> (8 * i));
    }

    private void WriteBsdiffPatch(Stream output, long newFileLength)
    {
        string ctrlMergedPath = Path.Combine(tempDir, "ctrl_merged.tmp");
        string diffMergedPath = Path.Combine(tempDir, "diff_merged.tmp");
        string extraMergedPath = Path.Combine(tempDir, "extra_merged.tmp");

        using (var ctrlMerged = File.Create(ctrlMergedPath))
        {
            foreach (var file in threadCtrlFiles)
            {
                using var fs = File.OpenRead(file);
                fs.CopyTo(ctrlMerged);
            }
        }

        using (var diffMerged = File.Create(diffMergedPath))
        {
            foreach (var file in threadDiffFiles)
            {
                using var fs = File.OpenRead(file);
                fs.CopyTo(diffMerged);
            }
        }

        using (var extraMerged = File.Create(extraMergedPath))
        {
            foreach (var file in threadExtraFiles)
            {
                using var fs = File.OpenRead(file);
                fs.CopyTo(extraMerged);
            }
        }

        using var ctrlCompressed = new MemoryStream();
        using var diffCompressed = new MemoryStream();
        using var extraCompressed = new MemoryStream();

        using (var fs = File.OpenRead(ctrlMergedPath))
            BZip2.Compress(fs, ctrlCompressed, true, 9);

        using (var fs = File.OpenRead(diffMergedPath))
            BZip2.Compress(fs, diffCompressed, true, 9);

        using (var fs = File.OpenRead(extraMergedPath))
            BZip2.Compress(fs, extraCompressed, true, 9);

        ctrlCompressed.Position = 0;
        diffCompressed.Position = 0;
        extraCompressed.Position = 0;

        Span<byte> header = stackalloc byte[32];
        Encoding.ASCII.GetBytes("BSDIFF43").CopyTo(header);
        WriteInt64LE(header.Slice(8), ctrlCompressed.Length);
        WriteInt64LE(header.Slice(16), diffCompressed.Length);
        WriteInt64LE(header.Slice(24), newFileLength);

        output.Write(header);
        ctrlCompressed.CopyTo(output);
        diffCompressed.CopyTo(output);
        extraCompressed.CopyTo(output);
    }

    private void Cleanup()
    {
        try
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
        catch
        {
            // Silent cleanup failure
        }
    }

    public void Dispose() => Cleanup();
}
