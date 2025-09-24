using DeltaQ.SuffixSorting;
using DeltaQ.SuffixSorting.SAIS;

using ICSharpCode.SharpZipLib.BZip2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderDiff.DiffLibs
{
    internal class BSDiffOptimized
    {
        public static void GenerateDiff(byte[] oldData, byte[] newData, Stream output)
        {
            // Build suffix array using DeltaQ
            ISuffixSort suffixSort = new SAIS(); // or new LibDivSufSort()
            using var suffixArrayHandle = suffixSort.Sort(oldData);
            ReadOnlySpan<int> suffixArray = suffixArrayHandle.Memory.Span;

            using var ctrlBlock = new MemoryStream();
            using var diffBlock = new MemoryStream();
            using var extraBlock = new MemoryStream();

            long oldPos = 0;
            long newPos = 0;

            while (newPos < newData.Length)
            {
                int scan = (int)newPos;
                int matchLen, matchPos;
                Search(oldData, newData, suffixArray, scan, out matchPos, out matchLen);

                // Clamp matchLen to avoid buffer overrun
                matchLen = Math.Min(matchLen, Math.Min(oldData.Length - (int)oldPos, newData.Length - (int)newPos));

                // Write diff bytes (difference between matched bytes)
                int diffLen = 0;
                while (diffLen < matchLen &&
                       oldPos + diffLen < oldData.Length &&
                       newPos + diffLen < newData.Length)
                {
                    byte oldByte = oldData[oldPos + diffLen];
                    byte newByte = newData[newPos + diffLen];
                    diffBlock.WriteByte((byte)((newByte - oldByte) & 0xFF));
                    diffLen++;
                }

                // Write extra bytes (bytes in newData not matching oldData)
                int extraLen = matchLen - diffLen;
                if (extraLen > 0 &&
                    (newPos + diffLen + extraLen) <= newData.Length)
                {
                    extraBlock.Write(newData, (int)(newPos + diffLen), extraLen);
                }

                // Write control block (diffLen, extraLen, offset)
                WriteInt64(ctrlBlock, diffLen);
                WriteInt64(ctrlBlock, extraLen);
                WriteInt64(ctrlBlock, matchPos - (int)(oldPos + diffLen));

                // Advance pointers
                oldPos += diffLen + (matchPos - (oldPos + diffLen));
                newPos += matchLen;
            }

            // Parallel compression tasks
            var ctrlTask = Task.Run(() => CompressToMemory(ctrlBlock));
            var diffTask = Task.Run(() => CompressToMemory(diffBlock));
            var extraTask = Task.Run(() => CompressToMemory(extraBlock));

            Task.WaitAll(ctrlTask, diffTask, extraTask);

            var compressedCtrl = ctrlTask.Result;
            var compressedDiff = diffTask.Result;
            var compressedExtra = extraTask.Result;

            // BSDIFF43 header
            var header = new byte[32];
            Encoding.ASCII.GetBytes("BSDIFF43").CopyTo(header, 0);
            WriteInt64(header, 8, compressedCtrl.Length);
            WriteInt64(header, 16, compressedDiff.Length);
            WriteInt64(header, 24, newData.Length);
            output.Write(header, 0, header.Length);

            // Write compressed blocks
            compressedCtrl.Position = 0;
            compressedCtrl.CopyTo(output);

            compressedDiff.Position = 0;
            compressedDiff.CopyTo(output);

            compressedExtra.Position = 0;
            compressedExtra.CopyTo(output);
        }

        private static MemoryStream CompressToMemory(Stream source)
        {
            source.Position = 0;
            var dest = new MemoryStream();
            BZip2.Compress(source, dest, true, 9);
            dest.Position = 0;
            return dest;
        }

        private static void Search(ReadOnlySpan<byte> oldData, ReadOnlySpan<byte> newData, ReadOnlySpan<int> suffixArray, int newPos, out int matchPos, out int matchLen)
        {
            int left = 0, right = suffixArray.Length;
            matchPos = 0;
            matchLen = 0;

            while (left < right)
            {
                int mid = (left + right) / 2;
                int pos = suffixArray[mid];

                int len = CompareSuffix(oldData, newData.Slice(newPos), pos);
                if (len > matchLen)
                {
                    matchLen = len;
                    matchPos = pos;
                }

                if (CompareByte(oldData, pos, newData[newPos]) < 0)
                    left = mid + 1;
                else
                    right = mid;
            }
        }

        private static int CompareSuffix(ReadOnlySpan<byte> oldData, ReadOnlySpan<byte> newSuffix, int oldPos)
        {
            int i = 0;
            while (oldPos + i < oldData.Length && i < newSuffix.Length)
            {
                if (oldData[oldPos + i] != newSuffix[i])
                    break;
                i++;
            }
            return i;
        }

        private static int CompareByte(ReadOnlySpan<byte> data, int pos, byte b)
        {
            if (pos >= data.Length) return -1;
            return data[pos].CompareTo(b);
        }

        private static void WriteInt64(Stream stream, long value)
        {
            for (int i = 0; i < 8; i++)
                stream.WriteByte((byte)(value >> (i * 8)));
        }

        private static void WriteInt64(byte[] buffer, int offset, long value)
        {
            for (int i = 0; i < 8; i++)
                buffer[offset + i] = (byte)(value >> (i * 8));
        }
    }
}
