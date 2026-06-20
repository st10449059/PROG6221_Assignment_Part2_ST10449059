using System;
using System.Data; // Cache data sets locally
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PROG6221_Assignment_Part2_ST10449059
{
    /// <summary>
    /// Form1 class manages the User Interface, presentation layer grid bindings, and event handling loops.
    /// </summary>
    public partial class Form1 : Form
    {
        // Object-Oriented Initialization of our core processing engine logic class
        Chatbot myBot = new Chatbot();

        public Form1()
        {
            InitializeComponent();
            this.AcceptButton = button1; // Maps the keyboard Enter key directly to execute the chat submission button
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Styling aesthetics matching your design profile setup
            this.BackColor = Color.FromArgb(25, 25, 25);
            richTextBox1.BackColor = Color.Black;
            richTextBox1.ForeColor = Color.Cyan;
            richTextBox1.Font = new Font("Consolas", 10);
            richTextBox1.SelectionIndent = 10;

            // Display identity branding structures
            richTextBox1.AppendText(myBot.GetLogo() + Environment.NewLine);
            richTextBox1.SelectionColor = Color.Cyan;
            richTextBox1.AppendText("\nCyberShield: System Online. What is your name, User?\n");

            myBot.PlayVoiceGreeting();
            textBox2.Focus();

            // Pull structural layout definitions down from MySQL tables on startup
            RefreshTaskList();
        }

        /// <summary>
        /// Pulls down a fresh copy of the data rows from the server to keep your visual grid synchronized.
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
        /// Central submission router handling chat interactions and dynamic database triggers.
        /// </summary>
        private async void button1_Click(object sender, EventArgs e)
        {
            string input = textBox2.Text.Trim();

            if (!string.IsNullOrWhiteSpace(input))
            {
                // Print user query onto screen console logger
                richTextBox1.SelectionColor = Color.White;
                richTextBox1.AppendText($"\n[{DateTime.Now:HH:mm}] {myBot.UserName}: {input}\n");
                textBox2.Clear();

                // Asynchronous flow delay mimicking calculations
                await Task.Delay(400);

                if (myBot.UserName == "User")
                {
                    // Catch user registration statement profile
                    myBot.UserName = input;
                    richTextBox1.SelectionColor = Color.Cyan;
                    richTextBox1.AppendText($"CyberShield: Setup complete. Welcome, {myBot.UserName}. How can I help you?\n");
                }
                else
                {
                    // Pass input parameter down to processing engine layers
                    string response = myBot.ProcessInput(input);

                    // --- DYNAMIC REMINDER COMMAND ROUTER ---
                    if (response.StartsWith("REMINDER_UPDATE:"))
                    {
                        if (int.TryParse(response.Split(':')[1], out int days))
                        {
                            // Ensure the user actually has a valid grid row selected to attach the reminder to
                            if (dgvTasks.CurrentRow != null && dgvTasks.CurrentRow.Cells["id"].Value != null)
                            {
                                int targetId = Convert.ToInt32(dgvTasks.CurrentRow.Cells["id"].Value);
                                bool success = myBot.UpdateTaskReminder(targetId, days);

                                richTextBox1.SelectionColor = Color.Cyan;
                                if (success)
                                {
                                    richTextBox1.AppendText($"CyberShield: Confirmed. Database entity Record #{targetId} will trigger a reminder in {days} days.\n");
                                }
                                else
                                {
                                    richTextBox1.AppendText($"CyberShield: Alert. Failed to alter the reminder schedule for Record #{targetId}.\n");
                                }

                                RefreshTaskList();
                            }
                            else
                            {
                                richTextBox1.SelectionColor = Color.Orange;
                                richTextBox1.AppendText("CyberShield: Alert! You must highlight a specific task row in the grid view first before setting a schedule.\n");
                            }
                        }
                    }
                    else
                    {
                        // Standard chat handling
                        richTextBox1.SelectionColor = Color.Cyan;
                        richTextBox1.AppendText(response + Environment.NewLine);

                        // REFRESH CHECK: If the user just ran a task command, sync our visual UI Grid view automatically!
                        string cleanInp = input.ToLower();
                        if (cleanInp.StartsWith("add task ") ||
                            cleanInp.StartsWith("complete task ") || cleanInp.StartsWith("finish task ") ||
                            cleanInp.StartsWith("delete task ") || cleanInp.StartsWith("remove task "))
                        {
                            RefreshTaskList();
                        }
                    }
                }

                // Append demarcation lines
                richTextBox1.SelectionColor = Color.FromArgb(40, 40, 40);
                richTextBox1.AppendText("________________________________________________\n");
                richTextBox1.ScrollToCaret();
                textBox2.Focus();
            }
        }

        // --- BACKWARD MANUAL UTILITY SUPPORT CHANNELS (Optional Form Buttons) ---
        private void button2_Click(object sender, EventArgs e)
        {
            // Standard backup button click option fallback routing mapping onto string injection commands
            string textValue = textBox2.Text.Trim();
            if (!string.IsNullOrWhiteSpace(textValue)) { button1_Click(sender, e); }
            else { MessageBox.Show("Please type a new task title statement into the text box field area.", "Validation Gate"); }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Tip: Simply type 'complete task [ID]' directly into the chat console box to finish an item using the AI assistant!", "Direct Chat Tip");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Tip: Simply type 'delete task [ID]' directly into the chat console box to purge an item using the AI assistant!", "Direct Chat Tip");
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e) { }
        private void richTextBox1_TextChanged_1(object sender, EventArgs e) { }
    }
}