using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Media;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

namespace PROG6221_Assignment_Part2_ST10449059
{
    /// <summary>
    /// Represents a single question for the cybersecurity mini-game.
    /// Stores the question text, multiple choice options, correct answer, and an educational explanation.
    /// </summary>
    public class QuizQuestion
    {
        public string QuestionText { get; set; }
        public string Options { get; set; }
        public string CorrectAnswer { get; set; }
        public string Explanation { get; set; }
    }

    /// <summary>
    /// The core logic engine for the application. 
    /// Handles database connectivity, natural language processing (NLP), quiz state management, and activity logging.
    /// </summary>
    public class Chatbot
    {
        // Database connection string mapping to the local MySQL server instance.
        private readonly string connectionString = "Server=localhost;Database=CyberShieldDB;Uid=root;Pwd=@Labs2026!;";

        // State properties to track user context during the conversation session.
        public string UserName { get; set; } = "User";
        public string FavoriteTopic { get; set; } = "";
        public string LastTopic { get; set; } = "";

        // TASK 4: Activity Log list used to store recent actions for the session summary.
        private List<string> _activityLog = new List<string>();

        // Repository of randomized security tips for varied conversation flow.
        private string[] _phishingTips = {
            "Be cautious of emails asking for personal information. Scammers often disguise themselves as trusted organisations.",
            "Always check the sender's email address for slight misspellings or odd domains.",
            "If a link looks suspicious, hover over it to see the real destination URL before clicking."
        };

        private Random _rng = new Random();

        // TASK 2: State variables managing the active cybersecurity quiz loop.
        public bool IsQuizActive { get; private set; } = false;
        private int _quizScore = 0;
        private int _currentQuestionIndex = 0;
        private List<QuizQuestion> _quizQuestions;

        /// <summary>
        /// Constructor initializes the list of cybersecurity quiz questions into memory.
        /// </summary>
        public Chatbot()
        {
            _quizQuestions = new List<QuizQuestion>
            {
                new QuizQuestion { QuestionText = "What should you do if you receive an email asking for your password?", Options = "A) Reply with your password\nB) Delete the email\nC) Report the email as phishing\nD) Ignore it", CorrectAnswer = "c", Explanation = "Reporting phishing emails helps security teams block the sender and prevent scams." },
                new QuizQuestion { QuestionText = "True or False: Using the same password for all your accounts is safe if it is very complex.", Options = "A) True\nB) False", CorrectAnswer = "b", Explanation = "False. If one database gets breached, hackers will try that complex password on every other site." },
                new QuizQuestion { QuestionText = "What does the 's' in 'https' stand for in a website URL?", Options = "A) Standard\nB) Secure\nC) System\nD) Socket", CorrectAnswer = "b", Explanation = "It stands for Secure. It means the data sent between your browser and the site is encrypted." },
                new QuizQuestion { QuestionText = "Someone calls claiming to be from IT and needs your login to 'fix a server issue'. What is this an example of?", Options = "A) Malware\nB) Ransomware\nC) Social Engineering\nD) Brute Force", CorrectAnswer = "c", Explanation = "Social Engineering involves manipulating humans rather than hacking software." },
                new QuizQuestion { QuestionText = "True or False: A padlock icon in the browser means a website is 100% safe to buy from.", Options = "A) True\nB) False", CorrectAnswer = "b", Explanation = "False. The padlock only means the connection is encrypted; scammers can easily set up encrypted sites." },
                new QuizQuestion { QuestionText = "What is the primary purpose of Two-Factor Authentication (2FA)?", Options = "A) To make logging in faster\nB) To require a second form of verification beyond just a password\nC) To encrypt your hard drive\nD) To block pop-up ads", CorrectAnswer = "b", Explanation = "2FA adds an extra layer of security, usually a code sent to your phone or an authenticator app." },
                new QuizQuestion { QuestionText = "True or False: You should always update your software and operating system as soon as updates are available.", Options = "A) True\nB) False", CorrectAnswer = "a", Explanation = "True. Updates often contain critical patches for newly discovered security vulnerabilities." },
                new QuizQuestion { QuestionText = "Which of the following makes the strongest password?", Options = "A) Your pet's name and birth year\nB) 'Password123'\nC) A randomly generated 16-character passphrase\nD) Your home address", CorrectAnswer = "c", Explanation = "Length and randomness are the best defenses against brute-force password cracking." },
                new QuizQuestion { QuestionText = "True or False: Public Wi-Fi networks at coffee shops are generally safe for online banking.", Options = "A) True\nB) False", CorrectAnswer = "b", Explanation = "False. Public Wi-Fi is often unsecured, allowing attackers to intercept your unencrypted data." },
                new QuizQuestion { QuestionText = "What is 'Ransomware'?", Options = "A) A tool to clean viruses\nB) Software that locks your files until you pay a fee\nC) A type of secure browser\nD) A firewall rule", CorrectAnswer = "b", Explanation = "Ransomware encrypts your data and demands payment (usually crypto) for the decryption key." },
                new QuizQuestion { QuestionText = "True or False: Phishing attacks only happen via email.", Options = "A) True\nB) False", CorrectAnswer = "b", Explanation = "False. Phishing can happen over SMS (Smishing), voice calls (Vishing), or social media." }
            };
        }

        /// <summary>
        /// Returns the ASCII art logo for the application.
        /// </summary>
        public string GetLogo()
        {
            return @"
    ::================================::
    || .............................. ||
    || .. C Y B E R   S H I E L D ..  ||
    || .............................. ||
    || ........... v3.0 ............. ||
    ::================================::";
        }

        /// <summary>
        /// Plays a welcoming audio file on system startup. Fails silently if the file is not found.
        /// </summary>
        public void PlayVoiceGreeting()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "greeting.wav");
                if (File.Exists(path))
                {
                    using (var player = new SoundPlayer(path)) { player.Play(); }
                }
            }
            catch { }
        }

        /// <summary>
        /// TASK 4: Records significant system events and timestamps them into the local activity array.
        /// </summary>
        /// <param name="actionDescription">A short description of the action completed.</param>
        private void LogActivity(string actionDescription)
        {
            string timeStampedAction = $"[{DateTime.Now:HH:mm}] {actionDescription}";
            _activityLog.Add(timeStampedAction);
        }

        // ==========================================
        // TASK 1: DATABASE INTEGRATION LOGIC (CRUD)
        // ==========================================

        /// <summary>
        /// Retrieves all task records from the MySQL database to populate the DataGridView.
        /// </summary>
        public DataTable GetAllTasks()
        {
            DataTable dt = new DataTable();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT id, title, description, reminder_days, is_completed FROM security_tasks ORDER BY id DESC;";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn)) { adapter.Fill(dt); }
                }
            }
            catch { }
            return dt;
        }

        /// <summary>
        /// Inserts a new task record into the database and triggers a log entry.
        /// </summary>
        public bool AddNewTask(string taskTitle)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO security_tasks (title, description, reminder_days, is_completed) VALUES (@title, 'Added via chat assistant conversation processing.', 7, 0);";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@title", taskTitle);
                        cmd.ExecuteNonQuery();

                        LogActivity($"Task added: '{taskTitle}'");
                        return true;
                    }
                }
            }
            catch { return false; }
        }

        /// <summary>
        /// Updates a specific task's boolean status to completed (1).
        /// </summary>
        public bool CompleteTaskById(int taskId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE security_tasks SET is_completed = 1 WHERE id = @id;";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", taskId);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0) LogActivity($"Task ID #{taskId} marked as completed.");
                        return rowsAffected > 0;
                    }
                }
            }
            catch { return false; }
        }

        /// <summary>
        /// Deletes a specific task record from the database based on its unique ID.
        /// </summary>
        public bool DeleteTaskById(int taskId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM security_tasks WHERE id = @id;";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", taskId);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0) LogActivity($"Task ID #{taskId} deleted.");
                        return rowsAffected > 0;
                    }
                }
            }
            catch { return false; }
        }

        /// <summary>
        /// Updates the reminder interval schedule for a specific database row.
        /// </summary>
        public bool UpdateTaskReminder(int taskId, int days)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE security_tasks SET reminder_days = @days WHERE id = @id;";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@days", days);
                        cmd.Parameters.AddWithValue("@id", taskId);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0) LogActivity($"Reminder updated for Task #{taskId} to {days} days.");
                        return rowsAffected > 0;
                    }
                }
            }
            catch { return false; }
        }

        // ==========================================
        // NLP AND CONVERSATIONAL ROUTING ENGINE
        // ==========================================

        /// <summary>
        /// Processes string input from the user. Uses Natural Language Processing (Regex) 
        /// to identify intent, trigger database updates, or respond conversationally.
        /// </summary>
        public string ProcessInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "CyberShield: Please provide an interaction query.";

            string cleanInput = input.Trim().ToLower();
            string BotName = "CyberShield";

            // TASK 4: Show Activity Log 
            if (cleanInput.Contains("show all activity") || cleanInput.Contains("full log") || cleanInput.Contains("show more"))
            {
                if (_activityLog.Count == 0) return $"{BotName}: I haven't performed any specific actions for you yet this session.";

                string fullSummary = $"{BotName}: Here is the complete log of all actions taken:\n";
                for (int i = 0; i < _activityLog.Count; i++)
                {
                    fullSummary += $"  {i + 1}. {_activityLog[i]}\n";
                }
                return fullSummary;
            }

            // TASK 4: Show Activity Log (Limits to recent 8 actions for clean console output)
            if (cleanInput.Contains("activity log") || cleanInput.Contains("what have you done") || cleanInput.Contains("recent actions") || cleanInput.Contains("show log"))
            {
                if (_activityLog.Count == 0) return $"{BotName}: I haven't performed any specific actions for you yet this session.";

                string summary = $"{BotName}: Here's a summary of recent actions. Type 'show full log' to see more.\n";
                int displayCount = Math.Min(8, _activityLog.Count);
                var recentActions = _activityLog.GetRange(_activityLog.Count - displayCount, displayCount);

                for (int i = 0; i < recentActions.Count; i++)
                {
                    summary += $"  {i + 1}. {recentActions[i]}\n";
                }
                return summary;
            }

            // TASK 3: NLP INTENT - Reminders (Pattern Matching)
            Match reminderMatch = Regex.Match(cleanInput, @"(?:remind me to|add a reminder to|set a reminder for)\s+(.+?)(?:\s+(tomorrow|today|in \d+ days))?$");
            if (reminderMatch.Success)
            {
                string taskContent = reminderMatch.Groups[1].Value.Trim().TrimEnd('.');
                string timeFrame = string.IsNullOrWhiteSpace(reminderMatch.Groups[2].Value) ? "soon" : reminderMatch.Groups[2].Value;

                AddNewTask(taskContent);
                LogActivity($"Reminder set: '{taskContent}' on {timeFrame}.");

                return $"{BotName}: Reminder set for '{taskContent}' on {timeFrame}'s date.";
            }

            // TASK 3: NLP INTENT - Adding Tasks (Pattern Matching)
            Match taskMatch = Regex.Match(cleanInput, @"(?:add a task to|create a task to|add task|create task)\s+(.+)");
            if (taskMatch.Success)
            {
                string taskContent = taskMatch.Groups[1].Value.Trim().TrimEnd('.');
                AddNewTask(taskContent);
                return $"{BotName}: Task added: '{taskContent}'. Would you like to set a reminder for this task?";
            }

            // TASK 2: QUIZ MODE ROUTING
            if (cleanInput.Contains("start") && cleanInput.Contains("quiz") && !IsQuizActive)
            {
                IsQuizActive = true;
                _quizScore = 0;
                _currentQuestionIndex = 0;

                LogActivity("Quiz started.");

                var firstQ = _quizQuestions[_currentQuestionIndex];
                return $"{BotName}: Cybersecurity Quiz Started! Type A, B, C, or D to answer.\n\n" +
                       $"Question {_currentQuestionIndex + 1}/{_quizQuestions.Count}: {firstQ.QuestionText}\n{firstQ.Options}";
            }

            // Active Quiz State Handling
            if (IsQuizActive)
            {
                if (cleanInput != "a" && cleanInput != "b" && cleanInput != "c" && cleanInput != "d")
                {
                    return $"{BotName}: Please answer using a single letter: A, B, C, or D.";
                }

                var currentQ = _quizQuestions[_currentQuestionIndex];
                bool isCorrect = (cleanInput == currentQ.CorrectAnswer);

                if (isCorrect) _quizScore++;

                string feedback = isCorrect ? " Correct!" : $"Incorrect. The right answer was {currentQ.CorrectAnswer.ToUpper()}.";
                string fullFeedback = $"{BotName}: {feedback} {currentQ.Explanation}\n\n";

                _currentQuestionIndex++;

                if (_currentQuestionIndex >= _quizQuestions.Count)
                {
                    IsQuizActive = false;
                    LogActivity($"Quiz completed. Final Score: {_quizScore}/{_quizQuestions.Count}.");

                    string finalEval = _quizScore >= 8 ? "Great job! You're a cybersecurity pro!" : "Keep learning to stay safe online!";
                    return fullFeedback +
                           $"Quiz Complete!\n" +
                           $"Your Final Score: {_quizScore}/{_quizQuestions.Count}\n" +
                           $"Evaluation: {finalEval}";
                }
                else
                {
                    var nextQ = _quizQuestions[_currentQuestionIndex];
                    return fullFeedback +
                           $"Question {_currentQuestionIndex + 1}/{_quizQuestions.Count}: {nextQ.QuestionText}\n{nextQ.Options}";
                }
            }

            // DIRECT COMMAND: Complete Task
            if (cleanInput.StartsWith("complete task ") || cleanInput.StartsWith("finish task "))
            {
                string numericSegment = cleanInput.Replace("complete task ", "").Replace("finish task ", "").Trim();
                if (int.TryParse(numericSegment, out int parseTargetId))
                {
                    bool success = CompleteTaskById(parseTargetId);
                    if (success) return $"{BotName}: Confirmed! Operational database entity Record #{parseTargetId} has been successfully flagged as COMPLETED.";
                    return $"{BotName}: Task target sequence record ID #{parseTargetId} could not be altered. Verify that item index exists inside the grid table view container grid layers.";
                }
                return $"{BotName}: I failed to resolve an execution index identifier parameter. Syntax protocol usage: 'complete task [ID number]'";
            }

            // DIRECT COMMAND: Delete Task
            if (cleanInput.StartsWith("delete task ") || cleanInput.StartsWith("remove task ") || cleanInput.StartsWith("delete "))
            {
                string numericString = string.Empty;
                foreach (char c in cleanInput) if (char.IsDigit(c)) numericString += c;

                if (int.TryParse(numericString, out int parseTargetId))
                {
                    bool success = DeleteTaskById(parseTargetId);
                    if (success) return $"{BotName}: Purge execution successful. Database Record #{parseTargetId} dropped from server memory.";
                    return $"{BotName}: Failure to drop target entity segment index #{parseTargetId}. Ensure targeted rows correspond with live schema keys.";
                }
                return $"{BotName}: Parsing target structure fault. Syntax pattern validation requirement: 'delete task [ID number]'";
            }

            // SEAMLESS CONVERSATION FLOW (Context Recall)
            if (cleanInput.Contains("another") || cleanInput.Contains("more") || cleanInput.Contains("explain"))
            {
                if (LastTopic == "phishing") return $"{BotName}: Here is another phishing tip: {_phishingTips[_rng.Next(_phishingTips.Length)]}";
                if (LastTopic == "password") return $"{BotName}: Another password tip: Use a reputable password manager to store complex, unique passwords.";
                if (FavoriteTopic == "privacy") return $"{BotName}: Since you're interested in privacy, you should also review which apps have location permissions.";

                return $"{BotName}: I'd be happy to explain more! Try asking about 'passwords', 'browsing', or 'scams'.";
            }

            // SENTIMENT DETECTION
            if (cleanInput.Contains("worried") || cleanInput.Contains("scared") || cleanInput.Contains("frustrated"))
            {
                return $"{BotName}: It's completely understandable to feel that way. Scammers can be very convincing. \nTip: {_phishingTips[_rng.Next(_phishingTips.Length)]}";
            }

            // CONVERSATION MEMORY: Store user preferences
            if (cleanInput.Contains("interested in privacy") || cleanInput.Contains("privacy"))
            {
                FavoriteTopic = "privacy";
                LastTopic = "privacy";
                LogActivity("Noted user interest in privacy.");
                return $"{BotName}: Great! I'll remember that you're interested in privacy, {UserName}.";
            }

            // KEYWORD RESPONSES: Cybersecurity Education
            if (cleanInput.Contains("firewall"))
            {
                LastTopic = "firewalls";
                return $"{BotName}: When configuring network security, I highly recommend using stateful firewalls, as they monitor the active connections and provide superior dynamic packet filtering compared to standard stateless options.";
            }

            if (cleanInput.Contains("password"))
            {
                LastTopic = "password";
                return $"{BotName}: (NIST Standard) Use at least 12 characters. A phrase like 'BlueElephantJump!' is hard to crack.";
            }

            if (cleanInput.Contains("phishing") || cleanInput.Contains("scam"))
            {
                LastTopic = "phishing";
                string definition = "Phishing is a social engineering attack to steal info via fake links.";
                return $"{BotName}: {definition} \nRandom Tip: {_phishingTips[_rng.Next(_phishingTips.Length)]}";
            }

            if (cleanInput.Contains("browsing"))
            {
                return $"{BotName}: Safe browsing means using TLS/HTTPS protocols and checking for the padlock icon.";
            }

            if (cleanInput.Contains("how are you"))
            {
                return $"{BotName}: I'm doing great, {UserName}! My firewall is strong and I'm ready to assist.";
            }

            if (cleanInput.Contains("purpose"))
            {
                return $"{BotName}: My purpose is to serve as your personal cybersecurity assistant.";
            }

            // DEFAULT FALLBACK 
            return $"{BotName}: I didn't quite understand that. Are you trying to add a task, set a reminder, view your activity log, or take a quiz?";
        }
    }
}