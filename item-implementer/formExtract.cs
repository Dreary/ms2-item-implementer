using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace item_implementer
{
    public partial class formExtract : Form
    {
        public formExtract()
        {
            InitializeComponent();
        }

        private void btnExtractXml_Click(object sender, EventArgs e)
        {
            // Use a FileDialog to allow the user to select the Xml.m2d file
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "M2D files (*.m2d)|*.m2d|All files (*.*)|*.*";
            openFileDialog.Title = "Select the .m2d file";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string xmlM2dPath = openFileDialog.FileName; // Get the selected .m2d file path
                string xmlM2hPath = xmlM2dPath.Replace(".m2d", ".m2h"); // Assume the corresponding .m2h file is in the same folder

                // Check if the Xml.m2h file exists in the same directory
                if (!File.Exists(xmlM2hPath))
                {
                    MessageBox.Show($"Xml.m2h is missing in the same directory as {xmlM2dPath}");
                    return;
                }

                // Check if backup is requested
                if (chBoxBackup.Checked)
                {
                    // Backup the selected files
                    BackupFiles(xmlM2dPath, xmlM2hPath);
                }

                // Proceed with extraction
                ExtractXml(xmlM2dPath, xmlM2hPath);
            }
        }

        private void BackupFiles(string xmlM2dPath, string xmlM2hPath)
        {
            try
            {
                // Get the directory where the application's .exe is located
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;

                // Define the backup folder path
                string backupFolderPath = Path.Combine(exeDirectory, "!Backup");

                // Check if the backup folder exists, if not, create it
                if (!Directory.Exists(backupFolderPath))
                {
                    Directory.CreateDirectory(backupFolderPath);
                }

                // Copy the .m2d file to the backup folder
                string backupM2dPath = Path.Combine(backupFolderPath, Path.GetFileName(xmlM2dPath));
                File.Copy(xmlM2dPath, backupM2dPath, true); // Overwrite if it exists

                // Copy the .m2h file to the backup folder
                string backupM2hPath = Path.Combine(backupFolderPath, Path.GetFileName(xmlM2hPath));
                File.Copy(xmlM2hPath, backupM2hPath, true); // Overwrite if it exists

                MessageBox.Show("Backup completed successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during backup: {ex.Message}");
            }
        }

        private void ExtractXml(string xmlM2dPath, string xmlM2hPath)
        {
            try
            {
                // Path to the MS2Extract.exe tool in the Tools folder located where the main .exe is located
                string toolsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools");
                string ms2ExtractPath = Path.Combine(toolsFolderPath, "MS2Extract.exe");

                if (!File.Exists(ms2ExtractPath))
                {
                    MessageBox.Show("MS2Extract.exe not found in the Tools folder.");
                    return;
                }

                // Get the folder where the program's .exe is located
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;

                // Create the "!Extracted" folder if it doesn't exist
                string extractedFolderPath = Path.Combine(exeDirectory, "!Extracted");
                if (!Directory.Exists(extractedFolderPath))
                {
                    Directory.CreateDirectory(extractedFolderPath);
                }

                // Check which radio button is selected (Sync or Async)
                string mode = radioAsync.Checked ? "0" : "1"; // 0 for Async, 1 for Sync

                // Add loglevel (2) at the end of the command
                string logLevel = "2";

                // Prepare the command to run with Xml.m2d as the source, and extract it to the "!Extracted" folder
                string command = $"{ms2ExtractPath} {xmlM2dPath} {extractedFolderPath} {mode} {logLevel}";

                // Prepare the ProcessStartInfo to run the command in cmd.exe
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/k {command}", // Execute the command and keep the window open
                    UseShellExecute = false,      // Use shell to open the cmd window
                    CreateNoWindow = false        // Show the command prompt window
                };

                // Start the extraction process
                Process.Start(processStartInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
                this.Close(); // Close the form in case of exception
            }
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
