using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace item_implementer
{
    public partial class formPack : Form
    {
        public formPack()
        {
            InitializeComponent();
        }

        private void btnPack_Click(object sender, EventArgs e)
        {
            // Step 1: Prompt user to select the source folder (the folder where the extracted files are)
            string sourceFolder = SelectFolderUsingFolderPicker(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "!Extracted"), "Select the source folder to pack");

            if (string.IsNullOrEmpty(sourceFolder) || !Directory.Exists(sourceFolder))
            {
                MessageBox.Show("Please select a valid source folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Step 2: Prompt user to select the destination folder for the .m2d/.m2h files
            string destinationFolder = SelectFolderUsingFolderPicker(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "!Extracted"), "Select the destination folder for .m2d/.m2h files");

            if (string.IsNullOrEmpty(destinationFolder) || !Directory.Exists(destinationFolder))
            {
                MessageBox.Show("Please select a valid destination folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Step 3: Extract the folder name to use as the archive name
            string archiveName = Path.GetFileName(sourceFolder); // Get the last part of the source path (folder name)

            if (string.IsNullOrEmpty(archiveName))
            {
                MessageBox.Show("Invalid source folder. The folder name is empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Step 4: Build the command using MS2Create.exe
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;  // Get the directory where the program's .exe is located
            string toolsFolderPath = Path.Combine(exeDirectory, "Tools");
            string ms2CreatePath = Path.Combine(toolsFolderPath, "MS2Create.exe");

            if (!File.Exists(ms2CreatePath))
            {
                MessageBox.Show("MS2Create.exe not found in the Tools folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Step 5: Check the mode (Async/Sync)
            string mode = radioAsync.Checked ? "0" : "1"; // 0 for Async, 1 for Sync

            // Step 6: Set logMode (2 for debugging)
            string logMode = "2";

            // Construct the command
            string command = $"{ms2CreatePath} \"{sourceFolder}\" \"{destinationFolder}\" {archiveName} MS2F {mode} {logMode}";

            // Step 7: Run the MS2Create.exe with the constructed command in a cmd window
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/k {command}", // Execute the command and keep the window open
                UseShellExecute = false,      // Use shell to open the cmd window
                CreateNoWindow = false        // Show the command prompt window
            };

            try
            {
                Process.Start(processStartInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while starting the packing process: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Helper method to use FolderPicker for folder selection
        private string SelectFolderUsingFolderPicker(string initialPath, string title)
        {
            var folderPicker = new FolderPicker();
            folderPicker.InputPath = initialPath; // Set the initial path to the !Extracted folder
            folderPicker.Title = title; // Set the custom window title

            if (folderPicker.ShowDialog(IntPtr.Zero) == true)
            {
                return folderPicker.ResultPath; // Return the selected folder path
            }

            // If no valid folder is selected, return an empty string
            return string.Empty;
        }

        private void exitBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void minimiseBtn_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
    }
}
