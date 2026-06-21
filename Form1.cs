using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PROG6221_Assignment_Part2_ST10449059
{
    /// <summary>
    /// Partial class defining the main Graphical User Interface.
    /// Manages the terminal aesthetics, keystroke capture, and grid data binding.
    /// </summary>
    public partial class Form1 : Form
    {
        // Instantiates the backend logic class to process commands.
        Chatbot myBot = new Chatbot();

        public Form1()
        {
            InitializeComponent();

            // Intercept keystrokes directly in the RichTextBox to build a terminal-style interface
            this.richTextBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.richTextBox1_KeyDown);
        }

        /// <summary>
        /// Executes on application launch. Applies dark mode styling and pulls initial data.
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            // --- NEW LAYOUT FIXES ---
            // 1. Stop the text from hiding behind the grid
            richTextBox1.Dock = DockStyle.Left;
            richTextBox1.Width = 814;
            dgvTasks.Dock = DockStyle.Fill; // Fills the remaining space on the right perfectly
            // ------------------------

            // Apply terminal-style colors and flat layout to the main form controls
            this.BackColor = Color.FromArgb(25, 25, 25);
            richTextBox1.BackColor = Color.Black;
            richTextBox1.ForeColor = Color.Cyan;
            richTextBox1.Font = new Font("Consolas", 10);
            richTextBox1.SelectionIndent = 10;
            richTextBox1.BorderStyle = BorderStyle.None;

            // Display application branding and initial greeting
            richTextBox1.AppendText(myBot.GetLogo() + Environment.NewLine);
            richTextBox1.SelectionColor = Color.Cyan;
            richTextBox1.AppendText("\nCyberShield: System Online. What is your name, User?\n\n");

            // Drop the initial terminal input prompt
            richTextBox1.SelectionColor = Color.LimeGreen;
            richTextBox1.AppendText("User> ");

            myBot.PlayVoiceGreeting();

            // Format the DataGridView to match the dark mode terminal theme
            dgvTasks.BackgroundColor = Color.FromArgb(25, 25, 25);
            dgvTasks.BorderStyle = BorderStyle.None;
            dgvTasks.GridColor = Color.FromArgb(60, 60, 60);
            dgvTasks.RowHeadersVisible = false;
            dgvTasks.AllowUserToAddRows = false;

            // Layout fixes to remove the horizontal scrollbar and stretch columns
            dgvTasks.ScrollBars = ScrollBars.Vertical;
            dgvTasks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvTasks.DefaultCellStyle.BackColor = Color.FromArgb(35, 35, 35);
            dgvTasks.DefaultCellStyle.ForeColor = Color.LightGray;
            dgvTasks.DefaultCellStyle.SelectionBackColor = Color.DarkCyan;
            dgvTasks.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvTasks.DefaultCellStyle.Font = new Font("Segoe UI", 9);

            // Allow custom header coloring by disabling the visual styles override
            dgvTasks.EnableHeadersVisualStyles = false;
            dgvTasks.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 20, 20);
            dgvTasks.ColumnHeadersDefaultCellStyle.ForeColor = Color.Cyan;
            dgvTasks.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvTasks.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

            // Connect to database and populate the grid
            RefreshTaskList();

            // Make the ID column only as wide as it needs to be after data is loaded
            if (dgvTasks.Columns["id"] != null)
            {
                dgvTasks.Columns["id"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }

            // Set input focus to the terminal immediately upon loading
            richTextBox1.Focus();
        }

        /// <summary>
        /// Contacts the Chatbot class to retrieve database rows and binds them to the visual grid.
        /// </summary>
        private void RefreshTaskList()
        {
            try
            {
                DataTable tasksData = myBot.GetAllTasks();
                dgvTasks.DataSource = tasksData;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load tasks into the grid view: " + ex.Message, "UI Sync Notice");
            }
        }

        /// <summary>
        /// Replaces standard button clicks with terminal keyboard logic. 
        /// Detects the 'Enter' key to process commands and prevents backspacing over prompts.
        /// </summary>
        private async void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            // Detect if the user pressed Enter to submit a command
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Prevents jumping to a blank new line prematurely

                string[] allLines = richTextBox1.Lines;
                string lastLine = allLines[allLines.Length - 1]; // Target the active typing line

                // Strip the visual prompt prefix to extract only the user's raw text
                string promptPrefix = $"{myBot.UserName}> ";
                string input = lastLine.Replace(promptPrefix, "").Trim();

                if (!string.IsNullOrWhiteSpace(input))
                {
                    // Exit command implementation to close the application safely
                    if (input.ToLower() == "exit" || input.ToLower() == "quit")
                    {
                        Application.Exit();
                        return;
                    }

                    // Artificial delay to simulate processing latency
                    await Task.Delay(300);

                    if (myBot.UserName == "User")
                    {
                        // Registration Phase Handling
                        myBot.UserName = input;
                        richTextBox1.SelectionColor = Color.Cyan;
                        richTextBox1.AppendText($"\nCyberShield: Setup complete. Welcome, {myBot.UserName}. How can I help you?\n\n");
                    }
                    else
                    {
                        // Standard chat phase: Forward extracted text to the NLP routing engine
                        string response = myBot.ProcessInput(input);

                        richTextBox1.SelectionColor = Color.Cyan;
                        richTextBox1.AppendText($"\n{response}\n\n");

                        // Dynamically refresh the UI grid if a database-altering command was executed
                        string cleanInp = input.ToLower();
                        if (cleanInp.Contains("add") ||
                            cleanInp.Contains("complete") || cleanInp.Contains("finish") ||
                            cleanInp.Contains("delete") || cleanInp.Contains("remove") ||
                            cleanInp.Contains("remind"))
                        {
                            RefreshTaskList();
                        }
                    }

                    // Provide a fresh terminal prompt for the next interaction loop
                    richTextBox1.SelectionColor = Color.LimeGreen;
                    richTextBox1.AppendText($"{myBot.UserName}> ");
                    richTextBox1.ScrollToCaret();
                }
                else
                {
                    // Empty submission handling: Drop down and give a new prompt
                    richTextBox1.AppendText($"\n{myBot.UserName}> ");
                    richTextBox1.ScrollToCaret();
                }
            }
            // Prevent users from accidentally deleting the command line prompt prefix using backspace
            else if (e.KeyCode == Keys.Back)
            {
                string[] lines = richTextBox1.Lines;
                if (lines.Length > 0)
                {
                    string lastLine = lines[lines.Length - 1];
                    if (lastLine == $"{myBot.UserName}> ")
                    {
                        e.SuppressKeyPress = true;
                    }
                }
            }
        }

        // Empty event handlers to satisfy the Windows Forms Designer configuration
        private void richTextBox1_TextChanged(object sender, EventArgs e) { }
        private void richTextBox1_TextChanged_1(object sender, EventArgs e) { }
    }
}