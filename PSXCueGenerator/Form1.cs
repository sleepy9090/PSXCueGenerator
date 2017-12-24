using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

// Shawn M. Crawford - [sleepy9090] - 2017+
namespace PSXCueGenerator
{
    public partial class FormMain : Form
    {
        string binFilename = "";
        string binDirectoryPath = "";
        string cueFileTemplate = @"FILE ""%FILENAME%.bin"" BINARY" + Environment.NewLine + "  TRACK 01 MODE2/2352" + Environment.NewLine + "    INDEX 01 00:00:00" + Environment.NewLine;

        public FormMain()
        {
            InitializeComponent();
        }

        #region ToolStripMenuItems
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 aboutBox = new AboutBox1();
            aboutBox.ShowDialog();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void selectFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            buttonSelectFile_Click(sender, e);
        }

        private void selectDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            buttonSelectDirectory_Click(sender, e);
        }

        private void clearLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            buttonClearLog_Click(sender, e);
        }
        #endregion

        #region Buttons
        private void buttonSelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Open file...";
            openFileDialog.Filter = "bin files (*.bin)|*.bin|All files (*.*)|*.*";
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxFile.Text = openFileDialog.FileName;
                binFilename = openFileDialog.FileName;
            }
        }

        private void buttonSelectDirectory_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxDirectory.Text = folderBrowserDialog.SelectedPath;
                binDirectoryPath = folderBrowserDialog.SelectedPath;
            }
        }

        private void buttonProcessFile_Click(object sender, EventArgs e)
        {
            // Example:
            // FILE "Timeless Jade Trade (USA).bin" BINARY
            // TRACK 01 MODE2/2352
            // INDEX 01 00:00:00

            if (!String.IsNullOrEmpty(binFilename))
            {
                string filenameNoExtension = Path.GetFileNameWithoutExtension(binFilename);
                string cueFile = Path.Combine(Path.GetDirectoryName(binFilename), filenameNoExtension + ".cue");

                if (!File.Exists(cueFile))
                {
                    createCueFile(cueFile, filenameNoExtension);
                }
            }
            else
            {
                textBoxLog.AppendText("Error: File not selected.\r\n");
            }
        }

        private void buttonProcessDirectory_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(binDirectoryPath))
            {
                try
                {
                    SearchOption searchOption = checkBoxIsRecursive.Checked ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                    String[] files = Directory.GetFiles(binDirectoryPath, "*.bin", searchOption);
                    foreach (String file in files)
                    {
                        string filenameNoExtension = Path.GetFileNameWithoutExtension(file);
                        string cueFile = Path.Combine(Path.GetDirectoryName(file), filenameNoExtension + ".cue");
                        if (!File.Exists(cueFile))
                        {
                            createCueFile(cueFile, filenameNoExtension);
                        }
                    }
                }
                catch (Exception ex)
                {
                    textBoxLog.AppendText("Catastrophic Failure: " + ex + "\r\n");
                }
            }
            else
            {
                textBoxLog.AppendText("Error: Directory not selected.\r\n");
            }
        }

        private void buttonClearLog_Click(object sender, EventArgs e)
        {
            textBoxLog.Clear();
        }
        #endregion

        #region private methods
        private void createCueFile(string cueFile, string filenameNoExtension)
        {
            try
            {
                File.WriteAllText(cueFile, cueFileTemplate.Replace("%FILENAME%", filenameNoExtension));
                textBoxLog.AppendText("Created cue file: " + cueFile + "\r\n");
            }
            catch(Exception ex)
            {
                textBoxLog.AppendText("Failed to create cue file: " + cueFile + "\r\n");
                textBoxLog.AppendText("Error: " + ex + "\r\n");
            }
        }
        #endregion
    }
}
