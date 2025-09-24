using System.ComponentModel;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using FolderDiff.AsyncForms;
using System.Threading.Tasks;
using System.Windows.Forms;
using FolderDiff.DiffLibs;
using FolderDiff.Enums;

namespace FolderDiff;

public partial class Form1 : Form
{
    public string containerFolderPath = string.Empty;
    public string parentFolder = string.Empty;
    public Form1Mode createOrPatch = Form1Mode.None;
    public ProcessingMode processingMode = ProcessingMode.None;
    public Form1()
    {
        InitializeComponent();
    }

    private void createPatchesToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using var d = new FolderBrowserDialog();
        if (d.ShowDialog() == DialogResult.OK)
        {
            containerFolderPath = d.SelectedPath;
            lblFolderPath.Text = d.SelectedPath;
            ShowFieldsForPatching();
            PopulateDropDown(containerFolderPath);
            createOrPatch = Form1Mode.Create;
            chkDeleteFoldersOnComplete.Visible = true;
            chkDeleteFoldersOnComplete.Checked = true;
        }
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        ResetFields();
    }

    private void ResetFields()
    {
        btnStart.Visible = false;
        lblFolderPath.Visible = false;
        lblFolderPathLabel.Visible = false;
        lblSelectParentFolder.Visible = false;
        cboParentFolder.Items.Clear();
        cboParentFolder.Visible = false;
        txtProcessingFile.Visible = false;
        lblProcessingFileLabel.Visible = false;
        chkDeleteFoldersOnComplete.Visible = false;
        chkDeleteFoldersOnComplete.Checked = false;
        lblProcessingMode.Visible = false;
        cboProcessingMode.Visible = false;
    }

    private void ShowFieldsForPatching()
    {
        btnStart.Visible = true;
        lblFolderPath.Visible = true;
        lblFolderPathLabel.Visible = true;
        lblSelectParentFolder.Visible = true;
        cboParentFolder.Visible = true;
        lblProcessingMode.Visible = true;
        cboProcessingMode.Visible = true;
        cboProcessingMode.SelectedIndex = 0;
    }

    private void PopulateDropDown(string folderPath)
    {
        cboParentFolder.Items.Clear();
        var folders = Directory.EnumerateDirectories(folderPath);
        foreach (var folder in folders)
        {
            var name = new DirectoryInfo(folder).Name;
            if (name != Constants.PatchesFolder)
            {
                cboParentFolder.Items.Add(name);
            }
        }
        cboParentFolder.SelectedIndex = -1;
    }

    private void btnStart_Click(object sender, EventArgs e)
    {
        if (cboParentFolder.SelectedIndex == -1)
        {
            MessageBox.Show(Constants.PleaseSelectParentFolder);
            return;
        }
        parentFolder = cboParentFolder.Text;
        if (createOrPatch == Form1Mode.Create)
        {
            chkDeleteFoldersOnComplete.Visible = true;
            txtProcessingFile.Visible = true;
            lblProcessingFileLabel.Visible = true;
            Task.Run(async () =>
            {
                await StartPatching();
                MessageBox.Show(Constants.AllDoneMessage);
            });
        }
        else if (createOrPatch == Form1Mode.Restore)
        {
            chkDeleteFoldersOnComplete.Visible = false;
            txtProcessingFile.Visible = true;
            lblProcessingFileLabel.Visible = true;
            Task.Run(async () =>
            {
                await StartRestore();
                MessageBox.Show(Constants.AllDoneMessage);
            });
        }
    }

    private async Task StartRestore()
    {
        Directory.CreateDirectory(Path.Combine(containerFolderPath, Constants.RestoredFolder));
        var folders = Directory.EnumerateDirectories(Path.Combine(containerFolderPath, Constants.PatchesFolder));
        var origFiles = Directory.EnumerateFiles(Path.Combine(containerFolderPath, parentFolder)).ToArray();

        foreach (var folder in folders)
        {
            var folderName = new DirectoryInfo(folder).Name;
            var targetFiles = Directory.EnumerateFiles(Path.Combine(containerFolderPath, Constants.PatchesFolder, folderName)).ToArray();
            var keyValuePairs = new Dictionary<string, string>();

            if (folderName != parentFolder && folderName != Constants.PatchesFolder && folderName != Constants.RestoredFolder)
            {
                Directory.CreateDirectory(Path.Combine(containerFolderPath, Constants.RestoredFolder, folderName));
                for (int i = 0; i < origFiles.Length; i++)
                {
                    if (targetFiles.Length >= i)
                    {
                        await RestoreFile(origFiles[i], targetFiles[i], folderName);
                    }
                }
                if (targetFiles.Length > origFiles.Length)
                {
                    for (int i = origFiles.Length; i < targetFiles.Length; i++)
                    {
                        var filename = Path.GetFileName(targetFiles[i]);
                        File.Copy(targetFiles[i], Path.Combine(containerFolderPath, Constants.RestoredFolder, folderName, filename), true);
                    }
                }
            }
        }
    }

    private Task RestoreFile(string oldFile, string newFile, string folderName)
    {
        var filename = Path.GetFileName(newFile);
        using var oldData = new FileStream(oldFile, FileMode.Open, FileAccess.Read, FileShare.None);
        var newData = File.ReadAllBytes(newFile);
        using var outputStream = new MemoryStream();
        txtProcessingFile.WriteTextSafe(filename);
        var newFileName = filename.Replace(Constants.BsPatchExtension, string.Empty).Replace("\\", string.Empty);
        BsDiff.BinaryPatch.Apply(oldData, () => new MemoryStream(newData), outputStream);
        File.WriteAllBytes(Path.Combine(containerFolderPath, Constants.RestoredFolder, folderName, newFileName), outputStream.ToArray());
        return Task.CompletedTask;
    }

    private async Task StartPatching()
    {
        SwitchUI(false);
        Directory.CreateDirectory(Path.Combine(containerFolderPath, Constants.PatchesFolder));
        var folders = Directory.EnumerateDirectories(containerFolderPath);
        var origFiles = Directory.EnumerateFiles(Path.Combine(containerFolderPath, parentFolder)).ToArray();

        foreach (var folder in folders)
        {
            var folderName = new DirectoryInfo(folder).Name;
            var targetFiles = Directory.EnumerateFiles(Path.Combine(containerFolderPath, folderName)).ToArray();

            if (folderName != parentFolder && folderName != Constants.PatchesFolder && folderName != Constants.RestoredFolder)
            {
                Directory.CreateDirectory(Path.Combine(containerFolderPath, Constants.PatchesFolder, folderName));
                for (int i = 0; i < origFiles.Length; i++)
                {
                    try
                    {
                        if (targetFiles.Length - 1 >= i)
                        {
                            await ProcessFile(origFiles[i], targetFiles[i], folderName);
                            UglyMemoryLeakHack();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.StackTrace);
                    }
                }
                if (targetFiles.Length > origFiles.Length)
                {
                    for (int i = origFiles.Length; i < targetFiles.Length; i++)
                    {
                        var filename = Path.GetFileName(targetFiles[i]);
                        File.Copy(targetFiles[i], Path.Combine(containerFolderPath, Constants.PatchesFolder, folderName, filename));
                    }
                }
            }
        }
        if (chkDeleteFoldersOnComplete.Checked)
        {
            DeleteFolders(folders, parentFolder);
        }
        UglyMemoryLeakHack();
        SwitchUI(true);
    }

    private static void DeleteFolders(IEnumerable<string> folders, string parentFolder)
    {
        foreach (var folder in folders)
        {
            var folderName = new DirectoryInfo(folder).Name;
            if (Directory.Exists(folder) && folderName != parentFolder && !folder.EndsWith(Constants.PatchesFolder) && !folder.EndsWith(Constants.RestoredFolder))
            {
                try
                {
                    Directory.Delete(folder, true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }
    }

    private Task ProcessFile(string oldFile, string newFile, string folderName)
    {
        try
        {
            var filename = Path.GetFileName(newFile);
            using var outputStream = new MemoryStream();
            txtProcessingFile.WriteTextSafe(filename);
            switch (processingMode)
            {
                case ProcessingMode.BsDiff:
                    var oldData = File.ReadAllBytes(oldFile);
                    var newData = File.ReadAllBytes(newFile);
                    BsDiff.BinaryPatch.Create(oldData, newData, outputStream);
                    break;
                case ProcessingMode.ChunkedBsDiffExperimental:
                    using (var diffGen = new ChunkedBsdiffGenerator(Path.Combine(containerFolderPath, Constants.PatchesFolder, folderName, filename)))
                    {
                        diffGen.GenerateDiff(oldFile, newFile, outputStream);
                    }
                    break;
                default:
                    MessageBox.Show(Constants.ThisShouldntHappen);
                    return Task.CompletedTask;
            }
            File.WriteAllBytes(Path.Combine(containerFolderPath, Constants.PatchesFolder, folderName, $"{filename}{Constants.BsPatchExtension}"), outputStream.ToArray());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"{Constants.SomethingWentWrong}{ex.Message}");
        }
        return Task.CompletedTask;
    }

    private static void UglyMemoryLeakHack()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
        Environment.Exit(0);
    }

    private void applyPatchesToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using var d = new FolderBrowserDialog();
        if (d.ShowDialog() == DialogResult.OK)
        {
            containerFolderPath = d.SelectedPath;
            lblFolderPath.Text = d.SelectedPath;
            ShowFieldsForPatching();
            PopulateDropDown(containerFolderPath);
            createOrPatch = Form1Mode.Restore;
            chkDeleteFoldersOnComplete.Visible = false;
            chkDeleteFoldersOnComplete.Checked = false;
        }
    }

    private void SwitchUI(bool isVisible)
    {
        chkDeleteFoldersOnComplete.ShowHideCheckboxAsync(isVisible);
        btnStart.ShowHideButtonAsync(isVisible);
        cboParentFolder.ShowHideComboBoxAsync(isVisible);
        cboProcessingMode.ShowHideComboBoxAsync(isVisible);
    }

    private void progressBar1_Click(object sender, EventArgs e)
    {
    }

    private void cboProcessingMode_SelectedIndexChanged(object sender, EventArgs e)
    {
        processingMode = (ProcessingMode)cboProcessingMode.SelectedIndex;
    }
}
