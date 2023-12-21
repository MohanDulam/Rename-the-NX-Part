using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Rename_the_NX_Part
{
    public partial class NX_File_Rename : Form
    {
        public NX_File_Rename()
        {
            InitializeComponent();
        }

        private string pathToNxFileToRename;
        private string newPartName;
        private bool filePath = false;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new NX_File_Rename());
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnBrowseFile_Click(object sender, EventArgs e)
        {
            txtNewFileName.Clear();
            lblStatus.Text = "Status...";
            lblStatus.ForeColor = Color.White;

            using (OpenFileDialog fileDialogWindow = new OpenFileDialog())
            {
                DialogResult result = fileDialogWindow.ShowDialog();

                if (result == DialogResult.OK)
                {
                    string fileExtensionCheck = fileDialogWindow.FileName.Trim();
                    string fileExtension = Path.GetExtension(fileExtensionCheck);

                    if (fileExtension.Contains(".prt"))
                    {
                        txtRenameFilePath.Text = fileExtensionCheck;
                        pathToNxFileToRename = txtRenameFilePath.Text;
                        filePath = true;
                    }
                    else
                    {                        
                        MessageBox.Show("Please Select Proper Part File", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        lblStatus.Text = "Please Select Proper Part File :(";
                        lblStatus.ForeColor = Color.White;
                    }
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtRenameFilePath.Clear();
            txtNewFileName.Clear();
            lblStatus.Text = "Status...";
            lblStatus.ForeColor = Color.White;
        }

        private void btnInfo_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This Code was Created By \"Mohan Dulam\" \n" +
                "     ******Instructions to Use this Application******  \n"+
                "1. Clik on Browse bottom to select the part file to Rename.\n"+
                "2. Provide the New Name for Part File in New File Name.\n" +
                "3. Click on \"Rename\" Bottom.\n" +
                "4. Message of Successfull Rename of Part File.", "Information", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnRename_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtNewFileName.Text) && filePath)
            {
                newPartName = txtNewFileName.Text;

                FileReName(pathToNxFileToRename, newPartName);

                lblStatus.Text = "Renaming of File is successfull :) ";
                lblStatus.ForeColor = Color.Green;

                MessageBox.Show("Renaming of File is successfull :) ", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            else
            {
                if (filePath)
                {
                    MessageBox.Show("Please Provide New Name for Part File", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    lblStatus.Text = "Please Provide New Name for Part File :( ";
                    lblStatus.ForeColor = Color.GhostWhite;
                }
                else
                {
                    MessageBox.Show("Please Provide Rename File and Path, New Name for Part File", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    lblStatus.Text = "Rename File and Path, New Name for Part File Missing :( ";
                    lblStatus.ForeColor = Color.GhostWhite;
                }
            }
        }

        /// <summary>
        /// To Rename the Part File of NX
        /// </summary>
        /// <param name="pathToNxFileToRename">Rename File with Path</param>
        /// <param name="name">New Name for Part</param>
        private static void FileReName(string pathToNxFileToRename, string name)
        {
            // Adding Part Extension to the Given New Part Name
            name = name + ".prt";

            // Split selected files directory and name file with extension
            // into separate string variables
            string directory = Path.GetDirectoryName(pathToNxFileToRename);
            string nxFileToRename = Path.GetFileName(pathToNxFileToRename);

            // Convert the paths to Windows style
            directory = directory.Replace("/", "\\");

            // Collect all other NX files in the same directory
            List<string> nxFiles = new List<string>();

            // Specify the file extension to filter only Part file in the directory
            string fileExtension = "*.prt";

            // Get all the files with the specified extension in the source directory and its subdirectories
            string[] files = Directory.GetFiles(directory, fileExtension, SearchOption.AllDirectories);

            // Get all the files with the specified extension in the source directory and its subdirectories
            // string[] files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);

            // Iterate over the files
            foreach (string file in files)
            {
                // Check if the file extension is .prt (adjust as needed)
                if (Path.GetExtension(file).Equals(".prt", StringComparison.OrdinalIgnoreCase))
                {
                    // Add the file path to the list
                    nxFiles.Add(file);
                }
            }

            // Remove the file need to be renamed from the list
            nxFiles.Remove(nxFileToRename);

            // get the file and path of the ug_edit_part_names.exe
            string argFirstSection = Environment.GetEnvironmentVariable("UGII_BASE_DIR") + @"\NXBIN\ug_edit_part_names.exe ";

            // Iterate over the nxfiles
            foreach (string nxFile in nxFiles)
            {
                // Construct the second argument section
                string argSecondSection = $"\"{Path.Combine(directory, nxFile)}\"";

                // Build the command with options of the ug_edit_part_names.exe          
                string getArgument = argSecondSection + " -list";

                try
                {
                    string output, error; // output and error messages from ug_edit_part_names.exe

                    // Calling the NewProcessor for runing the ug_edit_part_names.exe
                    NewProcessor(argFirstSection, getArgument, out output, out error);

                    // Check for the output from the application is not null
                    if (!string.IsNullOrEmpty(output))
                    {
                        // List to store all the induvidual part in directory
                        List<string> childs = new List<string>();
                        string partName; // variable to collect only part name with extension
                        // Split the output of the application based on multiple lines 
                        string[] lines = output.Split('\n');
                        // Iterate over the each line of the output of application
                        foreach (string line in lines)
                        {
                            // Check for line is not null
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                // output of application contains the part name with extension and the path of file
                                // split the part name with extension and path of file
                                partName = line.Substring(0, line.LastIndexOf(".prt")) + ".prt";
                                // Add part name to childs list
                                childs.Add(partName);
                            }
                        }

                        // check for the length of the childs list  
                        if (childs.Count > 1)
                        {
                            // // Iterate over the child list
                            foreach (string childPart in childs)
                            {
                                // check for childpart name and required rename part are same
                                if (nxFileToRename == childPart)
                                {
                                    // Argument to application for renaming the part
                                    string renameArgument = argSecondSection + " -o " + argSecondSection + " -change_name " + $"\"{nxFileToRename}\" " + $"\"{name}\"";
                                    //Console.WriteLine("Remane part Argument is "+argFirstSection + renameArgument);

                                    // Calling the NewProcessor for runing the ug_edit_part_names.exe
                                    NewProcessor(argFirstSection, renameArgument, out output, out error);

                                    // After renaming the required part break the loop
                                    break;
                                }
                            }
                        }
                        childs.Clear();// Clear the list                     
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Error in Renaming Part File", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    //throw;
                }
            }

            // Rename the part file on the directory
            string fromName = Path.Combine(directory, nxFileToRename);
            string toName = Path.Combine(directory, name);
            File.Move(fromName, toName);
        }
        /// <summary>
        /// To Start New Processor 
        /// </summary>
        /// <param name="argFirstSectionOfAppllication">Appliction with Path</param>
        /// <param name="argSecondSection">Commond for the Application</param>
        /// <param name="outputOfApplication">Output of the Application</param>
        /// <param name="errorOfApplication">Errror of application</param>
        private static void NewProcessor(string argFirstSectionOfAppllication, string argSecondSection, out string outputOfApplication, out string errorOfApplication)
        {
            outputOfApplication = null;
            errorOfApplication = null;
            try
            {
                // Arguments for the Processor 
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = argFirstSectionOfAppllication;
                startInfo.Arguments = argSecondSection;
                startInfo.RedirectStandardInput = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.UseShellExecute = false;

                // Intillazation of the New Processor
                Process process = new Process();
                process.StartInfo = startInfo;
                process.Start(); // Start Processor

                // Output and error from the Application
                outputOfApplication = process.StandardOutput.ReadToEnd();
                errorOfApplication = process.StandardError.ReadToEnd();

                process.WaitForExit();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in handling the Processor", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                //throw;
            }
        }

    }
}
