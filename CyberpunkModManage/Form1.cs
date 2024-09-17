// Copyright (c) 2024 Nubsuki
// All rights reserved.
// This application is free to use, modify, and distribute for personal and educational purposes.
// Commercial use or selling of this code is not permitted without explicit permission.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Compression;

namespace CyberpunkModManager
{
    public partial class Form1 : Form
    {
        private string gameRootPath = string.Empty;  // To store the root folder of the game
        private string modMovedFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ModManagerMovedFiles");
        private readonly string logFilePath; // Change to readonly
        private const string gameRootPathFilePath = "gameRootPath.txt"; // Path to save the game root
        private HashSet<string> addedMods = new HashSet<string>(); // To track added mods
        private Label selectedRootFolderLabel; // Label to display the selected game root folder
        private bool areUninstallButtonsEnabled = false; // Global variable to track uninstall button state
        private string backupFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backup"); // Backup folder path
        private HashSet<string> existingFiles = new HashSet<string>();

        public Form1()
        {
            InitializeComponent();
            logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.json"); // Initialize in constructor
            selectedRootFolderLabel = new Label
            {
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 400, 
                Height = 100,
                Font = new Font("Arial", 8, FontStyle.Regular)
            };
            buttonPanel.Controls.Add(selectedRootFolderLabel);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(Form1_DragEnter);
            this.DragDrop += new DragEventHandler(Form1_DragDrop);
            LoadInstalledMods(); // Load installed mods on startup
            LoadGameRootPath(); // Load the saved game root path
            LogToConsole("Application started.");
            LogToConsole("Make sure to backup your game before installing mods. Always good to have a backup :)");
        }

        private void LoadGameRootPath()
        {
            if (File.Exists(gameRootPathFilePath))
            {
                gameRootPath = File.ReadAllText(gameRootPathFilePath);
                selectedRootFolderLabel.Text = $"Root Folder: {gameRootPath}"; 
                LogToConsole($"Loaded game root path: {gameRootPath}");
                InitializeExistingFiles();
            }
            else
            {
                selectedRootFolderLabel.Text = "Root folder not yet selected."; 
                LogToConsole("Game root folder not set.");
            }
        }

        private void InitializeExistingFiles()
        {
            if (!string.IsNullOrEmpty(gameRootPath))
            {
                existingFiles = new HashSet<string>(Directory.GetFiles(gameRootPath, "*", SearchOption.AllDirectories));
            }
        }

        private void LoadInstalledMods()
        {
            if (File.Exists(logFilePath))
            {
                var logEntries = JsonConvert.DeserializeObject<List<ModLogEntry>>(File.ReadAllText(logFilePath));
                foreach (var entry in logEntries)
                {
                    if (entry.Status == "installed")
                    {
                        CreateModCard(entry.ModName, true); // Create mod card and indicate it's installed
                        LogToConsole($"Loaded installed mod: {entry.ModName}");
                    }
                }
            }
            else
            {
                LogToConsole("No installed mods found.");
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                string modName = Path.GetFileNameWithoutExtension(file);
                if (!addedMods.Contains(modName))
                {
                    CreateModCard(file);
                    addedMods.Add(modName);
                    LogToConsole($"Added mod: {modName}");
                }
                else
                {
                    LogToConsole($"Mod '{modName}' is already added. Skipping.");
                }
            }
        }

        private void CreateModCard(string archivePath, bool isInstalled = false)
        {
            string modName = Path.GetFileNameWithoutExtension(archivePath); // Get the mod name without the file extension

            // Check if the mod is already in the flowLayoutPanel
            if (flowLayoutPanelModCards.Controls.OfType<Panel>().Any(panel => panel.Tag.ToString() == modName))
            {
                LogToConsole($"Mod '{modName}' is already added. Dont Panic if repeted Mutiple Times ");
                return; // Exit the method if the mod is already present
            }

            Panel cardPanel = new Panel
            {
                Size = new Size(300, 150),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10),
                Tag = modName // Store the mod name in the Tag property
            };

            Label modNameLabel = new Label
            {
                Text = modName, // Set the text to the mod name
                Dock = DockStyle.Top,
                Font = new Font("Arial", 12, FontStyle.Bold), 
                TextAlign = ContentAlignment.MiddleCenter 
            };
            cardPanel.Controls.Add(modNameLabel); 

            Label statusLabel = new Label
            {
                Text = "Status: Moving...",
                Dock = DockStyle.Top
            };
            cardPanel.Controls.Add(statusLabel);

            Button installButton = new Button
            {
                Text = "Install",
                Dock = DockStyle.Bottom,
                Enabled = true  // Enable the button by default
            };
            installButton.Click += (sender, e) => InstallMod(archivePath, modName, cardPanel, statusLabel, installButton);
            cardPanel.Controls.Add(installButton);

            Button uninstallButton = new Button
            {
                Text = "Uninstall",
                Dock = DockStyle.Bottom,
                Enabled = false  // Disable uninitiall, will be enabled after installation
            };
            uninstallButton.Click += (sender, e) => UninstallMod(cardPanel, statusLabel, installButton, uninstallButton);
            cardPanel.Controls.Add(uninstallButton);

            if (isInstalled)
            {
                uninstallButton.Enabled = true; // Enable uninstall button if mod is installed
                installButton.Enabled = false; // Disable install button if mod is already installed
            }
            else
            {
                uninstallButton.Enabled = false; // Disable uninstall button if mod is not installed
                installButton.Enabled = true; // Enable install button
            }

            flowLayoutPanelModCards.Controls.Add(cardPanel);
            MoveModFile(archivePath, cardPanel, statusLabel, installButton);
        }

        private void MoveModFile(string archivePath, Panel cardPanel, Label statusLabel, Button installButton)
        {
            Directory.CreateDirectory(modMovedFolderPath);

            string archiveFileName = Path.GetFileName(archivePath);
            string newArchivePath = Path.Combine(modMovedFolderPath, archiveFileName);

            try
            {
                File.Move(archivePath, newArchivePath);
                statusLabel.Text = "Status: Moved. Ready to install.";
                installButton.Enabled = !string.IsNullOrEmpty(gameRootPath);  // Enable if folder selected
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Status: Error moving file.But Works";
            }
        }

        private async Task InstallMod(string archivePath, string modName, Panel cardPanel, Label statusLabel, Button installButton)
        {
            if (string.IsNullOrEmpty(gameRootPath))
            {
                MessageBox.Show("Please select the game root folder before installing.");
                return;
            }

            statusLabel.Text = "Status: Installing...";
            installButton.Enabled = false;

            string modExtractPath = gameRootPath;
            string archiveFileName = Path.GetFileName(archivePath);
            string movedArchivePath = Path.Combine(modMovedFolderPath, archiveFileName);

            try
            {
                if (!File.Exists(movedArchivePath))
                {
                    MessageBox.Show($"Archive file not found: {movedArchivePath}");
                    statusLabel.Text = "Status: Error - File not found.";
                    installButton.Enabled = true;
                    return;
                }

                LogToConsole($"Installing mod: {modName}");
                List<string> installedFiles = new List<string>();
                await Task.Run(() =>
                {
                    if (movedArchivePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        installedFiles = ExtractWithZip(movedArchivePath, modExtractPath);
                    }
                    else if (movedArchivePath.EndsWith(".rar", StringComparison.OrdinalIgnoreCase))
                    {
                        installedFiles = ExtractWithWinRAR(movedArchivePath, modExtractPath);
                    }
                    else if (movedArchivePath.EndsWith(".7z", StringComparison.OrdinalIgnoreCase))
                    {
                        installedFiles = ExtractWith7Zip(movedArchivePath, modExtractPath);
                    }
                    else
                    {
                        throw new Exception("Unsupported file format.");
                    }

                    LogExtractedFiles(installedFiles, modName);
                });

                LogToConsole($"Mod installed: {modName}");
                statusLabel.Text = "Status: Installed and Replaced";
                Button uninstallButton = cardPanel.Controls.OfType<Button>().FirstOrDefault(btn => btn.Text == "Uninstall");
                if (uninstallButton != null)
                {
                    uninstallButton.Enabled = true;
                }
                installButton.Text = "Installed";
                installButton.Enabled = false;

                // Log the installed mod info to log.json with status
                var logEntry = new ModLogEntry { ModName = modName, Status = "installed" };
                var logEntries = File.Exists(logFilePath) ? JsonConvert.DeserializeObject<List<ModLogEntry>>(File.ReadAllText(logFilePath)) : new List<ModLogEntry>();
                logEntries.Add(logEntry);
                File.WriteAllText(logFilePath, JsonConvert.SerializeObject(logEntries, Formatting.Indented));

                // Log the mod order to modOrder.txt with the extension
                File.AppendAllText("modOrder.txt", $"{archiveFileName}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                LogToConsole($"Error installing mod {modName}: {ex.Message}");
                statusLabel.Text = "Status: Error installing mod.";
                installButton.Enabled = true;
            }
        }

        private List<string> ExtractWithZip(string archivePath, string extractPath)
        {
            List<string> installedFiles = new List<string>();
            using (ZipArchive archive = ZipFile.OpenRead(archivePath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));
                    string destinationDirectory = Path.GetDirectoryName(destinationPath);

                    // Ensure the directory exists
                    if (!Directory.Exists(destinationDirectory))
                    {
                        Directory.CreateDirectory(destinationDirectory);
                    }

                    // Skip directories (folders)
                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        continue;
                    }

                    entry.ExtractToFile(destinationPath, true);
                    installedFiles.Add(destinationPath);
                }
            }
            return installedFiles;
        }

        private List<string> ExtractWithWinRAR(string archivePath, string extractPath)
        {
            string winRarPath = @"C:\Program Files\WinRAR\rar.exe"; // Update if needed

            if (!File.Exists(winRarPath))
            {
                throw new FileNotFoundException("WinRAR executable not found. Please check the path.");
            }

            if (!File.Exists(archivePath))
            {
                throw new FileNotFoundException($"Archive file not found: {archivePath}");
            }

            List<string> installedFiles = new List<string>();
            string listFile = Path.GetTempFileName();
            try
            {
                // First, list the contents of the archive
                Process.Start(new ProcessStartInfo
                {
                    FileName = winRarPath,
                    Arguments = $"lb \"{archivePath}\" > \"{listFile}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }).WaitForExit();

                // Read the list of files
                string[] files = File.ReadAllLines(listFile);

                foreach (string file in files)
                {
                    string destinationPath = Path.GetFullPath(Path.Combine(extractPath, file));
                    string destinationDirectory = Path.GetDirectoryName(destinationPath);

                    // Ensure the directory exists
                    if (!Directory.Exists(destinationDirectory))
                    {
                        Directory.CreateDirectory(destinationDirectory);
                    }

                    // Extract individual file
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = winRarPath,
                        Arguments = $"x -o+ \"{archivePath}\" \"{file}\" \"{extractPath}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }).WaitForExit();

                    installedFiles.Add(destinationPath);
                }
            }
            finally
            {
                File.Delete(listFile);
            }

            return installedFiles;
        }

        private List<string> ExtractWith7Zip(string archivePath, string extractPath)
        {
            string sevenZipPath = @"C:\Program Files\7-Zip\7z.exe"; 

            if (!File.Exists(sevenZipPath))
            {
                throw new FileNotFoundException("7-Zip executable not found. Please check the path.");
            }

            if (!File.Exists(archivePath))
            {
                throw new FileNotFoundException($"Archive file not found: {archivePath}");
            }

            List<string> installedFiles = new List<string>();
            string listFile = Path.GetTempFileName();
            try
            {
                // First, list the contents of the archive
                Process.Start(new ProcessStartInfo
                {
                    FileName = sevenZipPath,
                    Arguments = $"l -slt \"{archivePath}\" > \"{listFile}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }).WaitForExit();

                // Read the list of files
                string[] lines = File.ReadAllLines(listFile);
                List<string> files = new List<string>();
                foreach (string line in lines)
                {
                    if (line.StartsWith("Path = "))
                    {
                        files.Add(line.Substring(7));
                    }
                }

                foreach (string file in files)
                {
                    string destinationPath = Path.GetFullPath(Path.Combine(extractPath, file));
                    string destinationDirectory = Path.GetDirectoryName(destinationPath);

                    // Ensure the directory exists
                    if (!Directory.Exists(destinationDirectory))
                    {
                        Directory.CreateDirectory(destinationDirectory);
                    }

                    // Extract individual file
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = sevenZipPath,
                        Arguments = $"x \"{archivePath}\" -o\"{extractPath}\" \"{file}\" -y",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }).WaitForExit();

                    installedFiles.Add(destinationPath);
                }
            }
            finally
            {
                File.Delete(listFile);
            }

            return installedFiles;
        }

        private void LogExtractedFiles(List<string> installedFiles, string modName)
        {
            try
            {
                // Create a mod entry with only the installed files
                var modEntry = new
                {
                    modName = modName,
                    filePaths = installedFiles
                };

                // Read existing entries
                var existingEntries = File.Exists("extractedFiles.json") 
                    ? JsonConvert.DeserializeObject<List<dynamic>>(File.ReadAllText("extractedFiles.json")) 
                    : new List<dynamic>();

                // Add the new mod entry
                existingEntries.Add(modEntry);

                // Write back to the JSON file
                File.WriteAllText("extractedFiles.json", JsonConvert.SerializeObject(existingEntries, Formatting.Indented));

                // Update existingFiles set
                foreach (var file in installedFiles)
                {
                    existingFiles.Add(file);
                }

                // Debug output
                LogToConsole($"Logged {installedFiles.Count} files for mod: {modName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error logging extracted files: {ex.Message}");
            }
        }

        private void UninstallMod(Panel cardPanel, Label statusLabel, Button installButton, Button uninstallButton)
        {
            if (!areUninstallButtonsEnabled) // Check if uninstall buttons are enabled
            {
                MessageBox.Show("Uninstall buttons are currently disabled. Please enable them to proceed.");
                return; // Exit if not enabled
            }

            try
            {
                if (!File.Exists(logFilePath))
                {
                    MessageBox.Show("Log file not found. Cannot proceed with uninstallation.");
                    return;
                }

                var logEntries = JsonConvert.DeserializeObject<List<ModLogEntry>>(File.ReadAllText(logFilePath));
                var remainingEntries = new List<ModLogEntry>();
                string modName = Path.GetFileNameWithoutExtension(cardPanel.Tag as string); // Extract mod name without extension
                LogToConsole($"Uninstalling mod: {modName}");

                // Read installed file paths from extractedFiles.json
                var installedMods = JsonConvert.DeserializeObject<List<dynamic>>(File.ReadAllText("extractedFiles.json"));

                foreach (var entry in logEntries)
                {
                    if (entry.ModName == modName)
                    {
                        // Find the mod entry in the installed mods
                        var modEntry = installedMods.FirstOrDefault(m => m.modName == modName);
                        if (modEntry != null)
                        {
                            // Move installed files to backup folder
                            MoveInstalledFiles(modEntry.filePaths.ToObject<List<string>>(), modName);
                            LogToConsole($"Mod uninstalled and moved to Backup Folder : {modName}");
                        }
                        continue; // Remove the mod entry from the log
                    }
                    remainingEntries.Add(entry);
                }

                // Update log file with remaining entries
                File.WriteAllText(logFilePath, JsonConvert.SerializeObject(remainingEntries, Formatting.Indented));

                statusLabel.Text = "Status: Uninstalled";

                // Remove the mod from addedMods set
                addedMods.Remove(modName);

                // Remove the mod card from the flowLayoutPanel
                flowLayoutPanelModCards.Controls.Remove(cardPanel);
                cardPanel.Dispose();

                LogToConsole($"Mod uninstalled and removed from tracking: {modName}");
            }
            catch (Exception ex)
            {
                LogToConsole($"Error uninstalling mod: {ex.Message}");
                statusLabel.Text = "Status: Error uninstalling mod.";
            }
        }

        private void MoveInstalledFiles(List<string> installedFiles, string modName)
        {
            string modBackupFolder = Path.Combine(backupFolderPath, modName); // Create a folder for the mod
            Directory.CreateDirectory(modBackupFolder); // Ensure the mod folder exists

            foreach (var filePath in installedFiles)
            {
                string backupPath = Path.Combine(modBackupFolder, Path.GetFileName(filePath)); // Update backup path to include mod folder

                // Move the file to the backup location
                if (File.Exists(filePath))
                {
                    if (File.Exists(backupPath))
                    {
                        File.Delete(backupPath); // Delete existing backup if it exists
                    }
                    File.Move(filePath, backupPath); // Move the file to backup
                }
            }

            // Create the ModInstalledData.txt file with the mod's file paths
            string modInfoFilePath = Path.Combine(modBackupFolder, "ModInstalledData.txt");
            var modEntry = new
            {
                modName = modName,
                filePaths = installedFiles // Use the installed files list
            };

            // Write the mod entry to the text file
            File.WriteAllText(modInfoFilePath, JsonConvert.SerializeObject(modEntry, Formatting.Indented));

            // Clear the data for the specific mod from extractedFiles.json
            ClearExtractedFiles(modName);
        }

        private void ClearExtractedFiles(string modName)
        {
            string extractedFilesPath = "extractedFiles.json"; // Path to the extracted files JSON
            if (File.Exists(extractedFilesPath))
            {
                var existingEntries = JsonConvert.DeserializeObject<List<dynamic>>(File.ReadAllText(extractedFilesPath));
                var remainingEntries = existingEntries.Where(entry => entry.modName != modName).ToList(); // Filter out the mod entry

                // Write back the remaining entries to the JSON file
                File.WriteAllText(extractedFilesPath, JsonConvert.SerializeObject(remainingEntries, Formatting.Indented));
            }
        }

        private void SelectFolderButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowser = new FolderBrowserDialog())
            {
                DialogResult result = folderBrowser.ShowDialog();
                if (result == DialogResult.OK)
                {
                    gameRootPath = folderBrowser.SelectedPath;
                    selectedRootFolderLabel.Text = $"Root Folder: {gameRootPath}"; // Update the label

                    // Save the selected game root path
                    File.WriteAllText(gameRootPathFilePath, gameRootPath);

                    // Update install buttons to reflect that folder has been selected
                    foreach (Panel panel in flowLayoutPanelModCards.Controls.OfType<Panel>())
                    {
                        Button installButton = panel.Controls.OfType<Button>().FirstOrDefault(btn => btn.Text == "Install");
                        if (installButton != null)
                        {
                            installButton.Enabled = true;
                        }
                    }
                }
            }
        }

        private void OpenGameRootButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(gameRootPath))
            {
                MessageBox.Show("Please select the game root folder first.");
            }
            else if (Directory.Exists(gameRootPath))
            {
                Process.Start(new ProcessStartInfo(gameRootPath) { UseShellExecute = true });
            }
        }

        private void OpenModMovedButton_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(modMovedFolderPath))
            {
                Process.Start(new ProcessStartInfo(modMovedFolderPath) { UseShellExecute = true });
            }
            else
            {
                LogToConsole("Mod moved folder does not exist yet.");
            }
        }

        private void textBoxLogs_TextChanged(object sender, EventArgs e)
        {

        }

        private void flowLayoutPanelModCards_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void ImportModOrderButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Text files (*.txt)|*.txt";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string[] modOrder = File.ReadAllLines(openFileDialog.FileName);
                    await InstallModsInOrder(modOrder);
                }
            }
        }

        private async Task InstallModsInOrder(string[] modOrder)
        {
            installationProgressBar.Maximum = modOrder.Length;
            installationProgressBar.Value = 0;

            List<string> failedMods = new List<string>();

            foreach (string mod in modOrder)
            {
                string modPath = Path.Combine(modMovedFolderPath, mod.Trim());
                if (File.Exists(modPath))
                {
                    try
                    {
                        // Create or get the necessary UI elements for installation
                        Panel cardPanel = new Panel(); 
                        Label statusLabel = new Label(); 
                        Button installButton = new Button(); 

                        // Install the mod and wait for it to finish
                        await InstallMod(modPath, mod, cardPanel, statusLabel, installButton);
                        installationProgressBar.Value++;
                    }
                    catch (Exception ex)
                    {
                        failedMods.Add(mod);
                        LogToConsole($"Error installing mod '{mod}': {ex.Message}"); // Debug message
                    }
                }
                else
                {
                    failedMods.Add(mod);
                    LogToConsole($"Mod not found: {modPath}"); // Debug message
                }
            }

            // Save failed mods to a file
            if (failedMods.Count > 0)
            {
                File.WriteAllLines("failedmodorder.txt", failedMods);
                MessageBox.Show("Some mods failed to install. Check failedmodorder.txt for details.");
            }
            else
            {
                LogToConsole("All mods installed successfully.");
            }
        }

        private void EnableUninstallCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            areUninstallButtonsEnabled = enableUninstallCheckBox.Checked; // Update the global variable based on checkbox state

            foreach (Panel panel in flowLayoutPanelModCards.Controls.OfType<Panel>())
            {
                Button uninstallButton = panel.Controls.OfType<Button>().FirstOrDefault(btn => btn.Text == "Uninstall");
                if (uninstallButton != null)
                {
                    uninstallButton.Enabled = areUninstallButtonsEnabled; // Enable or disable based on the checkbox state
                }
            }

            // Provide feedback to the user
            if (areUninstallButtonsEnabled)
            {
                LogToConsole("Uninstall buttons have been enabled.");
            }
            else
            {
                LogToConsole("Uninstall buttons have been disabled.");
            }
        }

        private void installationProgressBar_Click(object sender, EventArgs e)
        {

        }

        private void LogToConsole(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(LogToConsole), message);
                return;
            }

            consolePanel.AppendText(message + Environment.NewLine);
            consolePanel.ScrollToCaret();
        }
    }

    public class ModLogEntry
    {
        public string ModName { get; set; }
        public string Status { get; set; }
    }
}
