// Copyright (c) 2024 Nubsuki
// All rights reserved.
// This application is free to use, modify, and distribute for personal and educational purposes.
// Commercial use or selling of this code is not permitted without explicit permission.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
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
        private string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");
        private const string gameRootPathFilePath = "gameRootPath.txt"; // Path to save the game root
        private HashSet<string> addedMods = new HashSet<string>(); // To track added mods
        private Label selectedRootFolderLabel; // Label to display the selected game root folder

        public Form1()
        {
            InitializeComponent();
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
        }

        private void LoadGameRootPath()
        {
            if (File.Exists(gameRootPathFilePath))
            {
                gameRootPath = File.ReadAllText(gameRootPathFilePath);
                selectedRootFolderLabel.Text = $"Root Folder: {gameRootPath}"; // Update the label
            }
            else
            {
                selectedRootFolderLabel.Text = "Root folder not yet selected."; // Default message if not set
            }
        }

        private void LoadInstalledMods()
        {
            if (File.Exists(logFilePath))
            {
                var logEntries = File.ReadAllLines(logFilePath);
                foreach (var entry in logEntries)
                {
                    var parts = entry.Split('|'); // Split by the delimiter
                    if (parts.Length >= 2 && parts[1] == "installed")
                    {
                        string modName = parts[0]; // Get the mod name
                        CreateModCard(modName, true); // Create mod card and indicate it's installed
                    }
                }
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
                string modName = Path.GetFileNameWithoutExtension(file); // Get the mod name
                if (!addedMods.Contains(modName)) // Check if the mod is already added
                {
                    CreateModCard(file);
                    addedMods.Add(modName); // Add to the set of added mods
                }
                else
                {
                    MessageBox.Show($"Mod '{modName}' is already added.");
                }
            }
        }

        private void CreateModCard(string archivePath, bool isInstalled = false)
        {
            string modName = Path.GetFileNameWithoutExtension(archivePath); // Get the mod name without the file extension

            // Check if the mod is already in the flowLayoutPanel
            if (flowLayoutPanelModCards.Controls.OfType<Panel>().Any(panel => panel.Tag.ToString() == modName))
            {
                MessageBox.Show($"File '{modName}' is already in the list.");
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

                await Task.Run(() =>
                {
                    if (movedArchivePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        ExtractWithZip(movedArchivePath, modExtractPath);
                    }
                    else if (movedArchivePath.EndsWith(".rar", StringComparison.OrdinalIgnoreCase))
                    {
                        ExtractWithWinRAR(movedArchivePath, modExtractPath);
                    }
                    else if (movedArchivePath.EndsWith(".7z", StringComparison.OrdinalIgnoreCase))
                    {
                        ExtractWith7Zip(movedArchivePath, modExtractPath);
                    }
                    else
                    {
                        throw new Exception("Unsupported file format.");
                    }

                    LogExtractedFiles(modExtractPath);
                });

                statusLabel.Text = "Status: Installed";
                Button uninstallButton = cardPanel.Controls.OfType<Button>().FirstOrDefault(btn => btn.Text == "Uninstall");
                if (uninstallButton != null)
                {
                    uninstallButton.Enabled = true;
                }
                installButton.Text = "Installed";
                installButton.Enabled = false;

                // Log the installed mod info to log.txt with status
                File.AppendAllText(logFilePath, $"{modName}|installed{Environment.NewLine}");

                // Log the mod order to modOrder.txt with the extension
                File.AppendAllText("modOrder.txt", $"{archiveFileName}{Environment.NewLine}"); // Save the mod order with extension
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error installing mod: {ex.Message}");
                statusLabel.Text = "Status: Error installing mod.";
                installButton.Enabled = true;
            }
        }

        private void ExtractWith7Zip(string archivePath, string extractPath)
        {
            string sevenZipPath = @"C:\Program Files\7-Zip\7z.exe"; // Update if needed

            if (!File.Exists(sevenZipPath))
            {
                throw new FileNotFoundException("7-Zip executable not found. Please check the path.");
            }

            if (!File.Exists(archivePath))
            {
                throw new FileNotFoundException($"Archive file not found: {archivePath}");
            }

            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo
                {
                    FileName = sevenZipPath,
                    Arguments = $"x \"{archivePath}\" -o\"{extractPath}\"",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (Process process = Process.Start(processInfo))
                {
                    process.WaitForExit();
                    if (process.ExitCode != 0)
                    {
                        throw new Exception($"7-Zip extraction failed with exit code {process.ExitCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error extracting with 7-Zip: {ex.Message}", ex);
            }
        }

        private void ExtractWithWinRAR(string archivePath, string extractPath)
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

            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo
                {
                    FileName = winRarPath,
                    Arguments = $"x \"{archivePath}\" \"{extractPath}\\\"",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (Process process = Process.Start(processInfo))
                {
                    process.WaitForExit();
                    if (process.ExitCode != 0)
                    {
                        throw new Exception($"WinRAR extraction failed with exit code {process.ExitCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error extracting with WinRAR: {ex.Message}", ex);
            }
        }

        private void ExtractWithZip(string archivePath, string extractPath)
        {
            if (!File.Exists(archivePath))
            {
                throw new FileNotFoundException($"Archive file not found: {archivePath}");
            }

            try
            {
                ZipFile.ExtractToDirectory(archivePath, extractPath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error extracting with Zip: {ex.Message}", ex);
            }
        }

        private void LogExtractedFiles(string extractPath)
        {
            try
            {
                // Use a using statement to ensure the file is properly closed after writing
                using (StreamWriter logFile = new StreamWriter(logFilePath, append: true))
                {
                    foreach (string directory in Directory.GetDirectories(extractPath, "*", SearchOption.AllDirectories))
                    {
                        foreach (string file in Directory.GetFiles(directory))
                        {
                            logFile.WriteLine(file);
                        }
                    }
                }
            }
            catch (IOException ioEx)
            {
                MessageBox.Show($"Error logging extracted files: {ioEx.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error logging extracted files: {ex.Message}");
            }
        }

        private void UninstallMod(Panel cardPanel, Label statusLabel, Button installButton, Button uninstallButton)
        {
            try
            {
                if (!File.Exists(logFilePath))
                {
                    MessageBox.Show("Log file not found. Cannot proceed with uninstallation.");
                    return;
                }

                var logEntries = File.ReadAllLines(logFilePath).ToList();
                var remainingEntries = new List<string>();
                string archiveFileName = cardPanel.Tag as string; // Get the archive file name from the Tag property
                string modName = Path.GetFileNameWithoutExtension(archiveFileName); // Extract mod name without extension

                foreach (var file in logEntries)
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                    else if (Directory.Exists(file))
                    {
                        Directory.Delete(file, true);
                    }
                    else
                    {
                        // Keep entries for files that weren't found
                        remainingEntries.Add(file);
                    }
                }

                // Remove entries that contain the mod name or archive file name
                remainingEntries = remainingEntries.Where(entry =>
                    !entry.Contains(modName) &&
                    !entry.Contains(archiveFileName)).ToList(); // Remove entries containing the mod name or archive file name

                // Update log file with remaining entries
                File.WriteAllLines(logFilePath, remainingEntries); // Write the updated entries back to the log file

                statusLabel.Text = "Status: Uninstalled";

                // Remove the mod card from the flowLayoutPanel
                flowLayoutPanelModCards.Controls.Remove(cardPanel);
                cardPanel.Dispose();

                // Find and remove the corresponding archive file from modMovedFolderPath
                if (!string.IsNullOrEmpty(archiveFileName))
                {
                    string archivePath = Path.Combine(modMovedFolderPath, archiveFileName);
                    if (File.Exists(archivePath))
                    {
                        File.Delete(archivePath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error uninstalling mod: {ex.Message}");
                statusLabel.Text = "Status: Error uninstalling mod.";
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
                MessageBox.Show("Mod moved folder does not exist.");
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
                        MessageBox.Show($"Error installing mod '{mod}': {ex.Message}"); // Debug message
                    }
                }
                else
                {
                    failedMods.Add(mod);
                    MessageBox.Show($"Mod not found: {modPath}"); // Debug message
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
                MessageBox.Show("All mods installed successfully.");
            }
        }
    }
}
