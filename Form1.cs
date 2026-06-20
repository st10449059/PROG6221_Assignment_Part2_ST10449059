using System;
using System.Data; // Added to allow C# to handle database data tables
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PROG6221_Assignment_Part2_ST10449059
{
    /// <summary>
    /// Form1 class manages the User Interface and event handling.
    /// Code Attribution: GUI asynchronous threading and UI styling 
    /// developed with assistance from Microsoft Copilot AI (2024).
    /// </summary>
    public partial class Form1 : Form
    {
        // TASK 8: Object-Oriented Programming - Initializing logic class
        Chatbot myBot = new Chatbot();

        public Form1()
        {
            InitializeComponent();
            this.AcceptButton = button1;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TASK 1: UI Branding and Professional Aesthetic
            this.BackColor = Color.FromArgb(25, 25, 25);
            richTextBox1.BackColor = Color.Black;
            richTextBox1.ForeColor = Color.Cyan;
            richTextBox1.Font = new Font("Consolas", 10);
            richTextBox1.SelectionIndent = 10;

            richTextBox1.AppendText(myBot.GetLogo() + Environment.NewLine);
            richTextBox1.SelectionColor = Color.Cyan;
            richTextBox1.AppendText("\nCyberShield: System Online. What is your name, User?\n");

            myBot.PlayVoiceGreeting();
            textBox2.Focus();

            // --- PART 3 ADDITION: Load database items automatically on startup ---
            RefreshTaskList();
        }

        /// <summary>
        /// PART 3 ADDITION: Helper method to sync the DataGridView with the MySQL database.
        /// </summary>
        private void RefreshTaskList()
        {
            try
            {
                // 1. Fetch the data table from our Chatbot logic layer
                DataTable tasksData = myBot.GetAllTasks();

                // 2. Bind the data table to your visual dgvTasks grid on the form
                dgvTasks.DataSource = tasksData;
            }
            catch (Exception ex)
            {
                // Simple error catch to notify you if the database fails to link up
                MessageBox.Show("Could not load tasks into the grid view: " + ex.Message, "UI Sync Notice");
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // String trimming logic (Ref: Portfolio Courses, 2021)
            string input = textBox2.Text.Trim();

            if (!string.IsNullOrWhiteSpace(input))
            {
                richTextBox1.SelectionColor = Color.White;
                richTextBox1.AppendText($"\n[{DateTime.Now:HH:mm}] {myBot.UserName}: {input}\n");
                textBox2.Clear();

                // TASK 8: Simulated async delay for natural flow
                await Task.Delay(500);

                if (myBot.UserName == "User")
                {
                    // TASK 5: Memory - Store user name
                    myBot.UserName = input;
                    richTextBox1.SelectionColor = Color.Cyan;
                    richTextBox1.AppendText($"CyberShield: Setup complete. Welcome, {myBot.UserName}. How can I help you?\n");
                }
                else
                {
                    // TASK 2, 4, & 6: Process logic
                    string response = myBot.ProcessInput(input);
                    richTextBox1.SelectionColor = Color.Cyan;
                    richTextBox1.AppendText(response + Environment.NewLine);
                }

                richTextBox1.SelectionColor = Color.FromArgb(40, 40, 40);
                richTextBox1.AppendText("________________________________________________\n");
                richTextBox1.ScrollToCaret();
                textBox2.Focus();
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e) { }

        private void richTextBox1_TextChanged_1(object sender, EventArgs e) { }
    }
}