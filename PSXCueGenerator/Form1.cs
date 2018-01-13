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
        string cueFileAudioTemplate = @"FILE ""%FILENAME%.bin"" BINARY" + Environment.NewLine + "  TRACK 0%TRACKNUM% AUDIO" + Environment.NewLine + "    INDEX 00 00:00:00" + Environment.NewLine + "    INDEX 01 00:02:00" + Environment.NewLine;
        string cueFileAudioTemplate2 = @"FILE ""%FILENAME%.bin"" BINARY" + Environment.NewLine + "  TRACK %TRACKNUM% AUDIO" + Environment.NewLine + "    INDEX 00 00:00:00" + Environment.NewLine + "    INDEX 01 00:02:00" + Environment.NewLine;
        List<string> audioTrackNamesList = new List<string>();

        public FormMain()
        {
            InitializeComponent();
            populateAudioTrackNamesList();
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
                    // Revert: Let the user decide if they want to create one or not for a single bin file that is track 2 or greater.
                    //bool isAudioTrack = false;
                    foreach (string genericTrack in audioTrackNamesList)
                    {
                        if (filenameNoExtension.ToLower().Contains(genericTrack))
                        {
                            //isAudioTrack = true;
                            textBoxLog.AppendText("Warning: bin files of track 2 or greater are typically included in the main game cue.\r\n");
                            break;
                        }
                    }

                    // Revert: Let the user decide if they want to create one or not for a single bin file that is track 2 or greater.
                    // Don't create a separate cue for an audio bin that is track 2 or more
                    //if (!isAudioTrack)
                    //{
                        createCueFile(cueFile, filenameNoExtension);
                    //}
                    //else
                    //{
                    //    textBoxLog.AppendText("Skipping: files of track 2 or greater are included in main game cue.\r\n");
                    //}
                }
                else
                {
                    textBoxLog.AppendText("Skipping: cue file already exists: " + cueFile + "\r\n");
                }
                textBoxLog.AppendText("Done.\r\n");
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
                    // TODO: Refactor: This is ugly n^2.
                    SearchOption searchOption = checkBoxIsRecursive.Checked ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                    String[] files = Directory.GetFiles(binDirectoryPath, "*.bin", searchOption);

                    foreach (String file in files)
                    {
                        string filenameNoExtension = Path.GetFileNameWithoutExtension(file);
                        string cueFile = Path.Combine(Path.GetDirectoryName(file), filenameNoExtension + ".cue");
                        if (!File.Exists(cueFile))
                        {
                            bool isAudioTrack = false;
                            foreach (string genericTrack in audioTrackNamesList)
                            {
                                if (filenameNoExtension.ToLower().Contains(genericTrack))
                                {
                                    isAudioTrack = true;
                                    break;
                                }
                            }

                            // Don't create a separate cue for an audio bin that is track 2 or more
                            if (!isAudioTrack)
                            {
                                binFilename = file;
                                createCueFile(cueFile, filenameNoExtension);
                            }
                            else
                            {
                                textBoxLog.AppendText("Skipping: " + file + " :bin files of tracks 2 or greater are included in main game cue.\r\n");
                            }

                            //binFilename = file;
                            //createCueFile(cueFile, filenameNoExtension);
                        }
                        else
                        {
                            textBoxLog.AppendText("Skipping: cue file already exists: " + cueFile + "\r\n");
                        }
                    }
                    textBoxLog.AppendText("Done.\r\n");
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
                textBoxLog.AppendText("Found data track: " + binFilename + "\r\n");
                string currentCueFileTemplate = cueFileTemplate.Replace("%FILENAME%", filenameNoExtension);
                //cueFileTemplate = cueFileTemplate.Replace("%FILENAME%", filenameNoExtension);
                string cueFileFinal = currentCueFileTemplate;

                //bool hasAudioTracks = false;
                binDirectoryPath = Path.GetDirectoryName(binFilename);
                if (!String.IsNullOrEmpty(binDirectoryPath))
                {
                    // Get rid of track 1 or 01
                    string filenameNoTrackNum = filenameNoExtension.ToLower().Replace("(track 1)", "").Replace("(track 01)", "").Replace("(track01)", "").Replace("(track1)", "");
                    filenameNoTrackNum = filenameNoTrackNum.ToLower().Replace("track 1", "").Replace("track 01", "").Replace("track01", "").Replace("track1", "");

                    // Check for audio tracks located in the same folder
                    SearchOption searchOption = SearchOption.TopDirectoryOnly;
                    String[] files = Directory.GetFiles(binDirectoryPath, filenameNoTrackNum + "*.bin", searchOption);
                    int i = 2;
                    foreach (String file in files)
                    {
                        string fileNoExtension = Path.GetFileNameWithoutExtension(file);
                        if (filenameNoExtension != fileNoExtension)
                        {
                            textBoxLog.AppendText("Found audio track: " + file + "\r\n");

                            string currentCueFileAudioTemplate = "";
                            if (i <= 9)
                            {
                                currentCueFileAudioTemplate = cueFileAudioTemplate;
                                
                            }
                            else
                            {
                                currentCueFileAudioTemplate = cueFileAudioTemplate2;
                            }
                            currentCueFileAudioTemplate = currentCueFileAudioTemplate.Replace("%FILENAME%", fileNoExtension).Replace("%TRACKNUM%", i.ToString());
                            cueFileFinal += currentCueFileAudioTemplate;
                            i++;
                        }
                    }
                }

                File.WriteAllText(cueFile, cueFileFinal);
                textBoxLog.AppendText("Created cue file: " + cueFile + "\r\n");
            }
            catch(Exception ex)
            {
                textBoxLog.AppendText("Failed to create cue file: " + cueFile + "\r\n");
                textBoxLog.AppendText("Error: " + ex + "\r\n");
            }
        }

        private void populateAudioTrackNamesList()
        {
            // Max tracks for CDDA spec is 99
            // This creats a string list of track names to check for, if track 2 or track 02 or track2 or track 99 etc etc
            for(int i = 2; i <= 99; i++)
            {
                audioTrackNamesList.Add("track " + i);
                audioTrackNamesList.Add("track" + i);
                if (i < 10)
                {
                    audioTrackNamesList.Add("track 0" + i);
                }
            }
        }
        #endregion
    }
}
