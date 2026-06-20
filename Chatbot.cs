using System;
using System.Collections.Generic;
using System.Data; // PART 3 MULTI-TASKING: Handles local memory row caching
using System.IO;
using System.Media;
using MySql.Data.MySqlClient; // CONNECTIVITY: Bridges application layer directly to local MySQL engine

namespace PROG6221_Assignment_Part2_ST10449059
{
    /// <summary>
    /// Helper class to structure the Quiz data as requested in Task 2.
    /// </summary>
    public class QuizQuestion
    {
        public string QuestionText { get; set; }
        public string Options { get; set; }
        public string CorrectAnswer { get; set; }
        public string Explanation { get; set; }
    }

    /// <summary>
    /// Chatbot logic class responsible for processing cybersecurity queries and handling task database records.
    /// </summary>
    public class Chatbot
    {
        // --- DATABASE PIPELINE SETUP ---
        private readonly string connectionString = "Server=localhost;Database=CyberShieldDB;Uid=root;Pwd=@Labs2026!;";

        // TASK 5: Memory state management properties
        public string UserName { get; set; } = "User";
        public string FavoriteTopic { get; set; } = "";

        // TASK 4: Context Tracking variables
        public string LastTopic { get; set; } = "";

        // TASK 3: Defensive security advisory repository
        private string[] _phishingTips = {
            "Be cautious of emails asking for personal information. Scammers often disguise themselves as trusted organisations.",
            "Always check the sender's email address for slight misspellings or odd domains.",
            "If a link looks suspicious, hover over it to see the real destination URL before clicking."
        };

        private Random _rng = new Random();

        // ==========================================
        //  TASK 2: QUIZ MINI-GAME STATE & DATA
        // ==========================================
        public bool IsQuizActive { get; private set; } = false;
        private int _quizScore = 0;
        private int _currentQuestionIndex = 0;
        private List<QuizQuestion> _quizQuestions;

        // Constructor to initialize the quiz questions
        public Chatbot()
        {
            // Initialize 11 questions covering required cybersecurity topics (Mixed Multiple Choice & True/False)
            _quizQuestions = new List<QuizQuestion>
            {
                new QuizQuestion {
                    QuestionText = "What should you do if you receive an email asking for your password?",
                    Options = "A) Reply with your password\nB) Delete the email\nC) Report the email as phishing\nD) Ignore it",
                    CorrectAnswer = "c",
                    Explanation = "Reporting phishing emails helps security teams block the sender and prevent scams."
                },
                new QuizQuestion {
                    QuestionText = "True or False: Using the same password for all your accounts is safe if it is very complex.",
                    Options = "A) True\nB) False",
                    CorrectAnswer = "b",
                    Explanation = "False. If one database gets breached, hackers will try that complex password on every other site."
                },
                new QuizQuestion {
                    QuestionText = "What does the 's' in 'https' stand for in a website URL?",
                    Options = "A) Standard\nB) Secure\nC) System\nD) Socket",
                    CorrectAnswer = "b",
                    Explanation = "It stands for Secure. It means the data sent between your browser and the site is encrypted."
                },
                new QuizQuestion {
                    QuestionText = "Someone calls claiming to be from IT and needs your login to 'fix a server issue'. What is this an example of?",
                    Options = "A) Malware\nB) Ransomware\nC) Social Engineering\nD) Brute Force",
                    CorrectAnswer = "c",
                    Explanation = "Social Engineering involves manipulating humans rather than hacking software."
                },
                new QuizQuestion {
                    QuestionText = "True or False: A padlock icon in the browser means a website is 100% safe to buy from.",
                    Options = "A) True\nB) False",
                    CorrectAnswer = "b",
                    Explanation = "False. The padlock only means the connection is encrypted; scammers can easily set up encrypted sites."
                },
                new QuizQuestion {
                    QuestionText = "What is the primary purpose of Two-Factor Authentication (2FA)?",
                    Options = "A) To make logging in faster\nB) To require a second form of verification beyond just a password\nC) To encrypt your hard drive\nD) To block pop-up ads",
                    CorrectAnswer = "b",
                    Explanation = "2FA adds an extra layer of security, usually a code sent to your phone or an authenticator app."
                },
                new QuizQuestion {
                    QuestionText = "True or False: You should always update your software and operating system as soon as updates are available.",
                    Options = "A) True\nB) False",
                    CorrectAnswer = "a",
                    Explanation = "True. Updates often contain critical patches for newly discovered security vulnerabilities."
                },
                new QuizQuestion {
                    QuestionText = "Which of the following makes the strongest password?",
                    Options = "A) Your pet's name and birth year\nB) 'Password123'\nC) A randomly generated 16-character passphrase\nD) Your home address",
                    CorrectAnswer = "c",
                    Explanation = "Length and randomness are the best defenses against brute-force password cracking."
                },
                new QuizQuestion {
                    QuestionText = "True or False: Public Wi-Fi networks at coffee shops are generally safe for online banking.",
                    Options = "A) True\nB) False",
                    CorrectAnswer = "b",
                    Explanation = "False. Public Wi-Fi is often unsecured, allowing attackers to intercept your unencrypted data."
                },
                new QuizQuestion {
                    QuestionText = "What is 'Ransomware'?",
                    Options = "A) A tool to clean viruses\nB) Software that locks your files until you pay a fee\nC) A type of secure browser\nD) A firewall rule",
                    CorrectAnswer = "b",
                    Explanation = "Ransomware encrypts your data and demands payment (usually crypto) for the decryption key."
                },
                new QuizQuestion {
                    QuestionText = "True or False: Phishing attacks only happen via email.",
                    Options = "A) True\nB) False",
                    CorrectAnswer = "b",
                    Explanation = "False. Phishing can happen over SMS (Smishing), voice calls (Vishing), or social media."
                }
            };
        }

        // TASK 1: System Identifiers
        public string GetLogo()
        {
            return @"
    ::================================::
    || .............................. ||
    || .. C Y B E R   S H I E L D ..  ||
    || .............................. ||
    || ........... v2.0 ............. ||
    ::================================::";
        }

        // TASK 1 & 7: IO operations hardware check
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
            catch { /* Silent fail safety boundary */ }
        }

        // ==========================================
        //         DATABASE CORE LOGIC ENGINE
        // ==========================================

        public DataTable GetAllTasks()
        {
            DataTable dt = new DataTable();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT id, title, description, reminder_days, is_completed FROM security_tasks ORDER BY id DESC;";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Database Read Error: " + ex.Message, "System Integrity Notice");
            }
            return dt;
        }

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
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Database Write Error: " + ex.Message, "System Integrity Notice");
                return false;
            }
        }

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
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Database Update Error: " + ex.Message, "System Integrity Notice");
                return false;
            }
        }

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
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Database Dropping Error: " + ex.Message, "System Integrity Notice");
                return false;
            }
        }

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
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Database Update Error: " + ex.Message, "System Integrity Notice");
                return false;
            }
        }

        // ==========================================
        //         INTELLIGENT PROCESSING ENGINE
        // ==========================================

        public string ProcessInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return "CyberShield: Input streams cannot be null. Please provide an interaction query.";
            }

            string cleanInput = input.Trim().ToLower();
            string BotName = "CyberShield";

            // ==========================================
            //  TASK 2: QUIZ MODE ROUTING
            // ==========================================
            if (cleanInput == "start quiz" && !IsQuizActive)
            {
                IsQuizActive = true;
                _quizScore = 0;
                _currentQuestionIndex = 0;

                var firstQ = _quizQuestions[_currentQuestionIndex];
                return $"{BotName}: 🎮 Cybersecurity Quiz Started! Type A, B, C, or D to answer.\n\n" +
                       $"Question {_currentQuestionIndex + 1}/{_quizQuestions.Count}: {firstQ.QuestionText}\n{firstQ.Options}";
            }

            if (IsQuizActive)
            {
                // Ensure the user actually typed a valid option (A, B, C, or D)
                if (cleanInput != "a" && cleanInput != "b" && cleanInput != "c" && cleanInput != "d")
                {
                    return $"{BotName}: Please answer using a single letter: A, B, C, or D.";
                }

                var currentQ = _quizQuestions[_currentQuestionIndex];
                bool isCorrect = (cleanInput == currentQ.CorrectAnswer);

                if (isCorrect) _quizScore++;

                string feedback = isCorrect ? "✅ Correct!" : $"❌ Incorrect. The right answer was {currentQ.CorrectAnswer.ToUpper()}.";
                string fullFeedback = $"{BotName}: {feedback} {currentQ.Explanation}\n\n";

                _currentQuestionIndex++; // Move to next question

                // Check if the game is over
                if (_currentQuestionIndex >= _quizQuestions.Count)
                {
                    IsQuizActive = false;
                    string finalEval = _quizScore >= 8 ? "Great job! You're a cybersecurity pro!" : "Keep learning to stay safe online!";

                    return fullFeedback +
                           $"🏁 Quiz Complete!\n" +
                           $"Your Final Score: {_quizScore}/{_quizQuestions.Count}\n" +
                           $"Evaluation: {finalEval}";
                }
                else
                {
                    // Send the next question
                    var nextQ = _quizQuestions[_currentQuestionIndex];
                    return fullFeedback +
                           $"Question {_currentQuestionIndex + 1}/{_quizQuestions.Count}: {nextQ.QuestionText}\n{nextQ.Options}";
                }
            }

            // --- SMART CHAT INTERCEPTION: REMINDER COMMAND ---
            if (cleanInput.StartsWith("remind me in ") && cleanInput.Contains(" day"))
            {
                string numberPart = cleanInput.Replace("remind me in ", "").Split(' ')[0];
                if (int.TryParse(numberPart, out int days))
                {
                    return $"REMINDER_UPDATE:{days}";
                }
                return $"{BotName}: Alert. Could not parse numerical index. Syntax validation requirement: 'remind me in [number] days'";
            }

            // --- SMART CHAT INTERCEPTION: ADD TASK COMMAND ---
            if (cleanInput.StartsWith("add task "))
            {
                string rawTaskTitle = input.Substring(9).Trim();
                if (string.IsNullOrWhiteSpace(rawTaskTitle)) return $"{BotName}: The task title string cannot be blank.";

                bool success = AddNewTask(rawTaskTitle);
                if (success)
                {
                    return $"{BotName}: Success! I have recorded your administrative requirement into the active schema:\n👉 \"{rawTaskTitle}\"";
                }
                return $"{BotName}: Alert. I encountered an operational structural anomaly committing that file record to the database engine.";
            }

            // --- SMART CHAT INTERCEPTION: COMPLETE TASK COMMAND ---
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

            // --- SMART CHAT INTERCEPTION: DELETE TASK COMMAND ---
            if (cleanInput.StartsWith("delete task ") || cleanInput.StartsWith("remove task "))
            {
                string numericSegment = cleanInput.Replace("delete task ", "").Replace("remove task ", "").Trim();
                if (int.TryParse(numericSegment, out int parseTargetId))
                {
                    bool success = DeleteTaskById(parseTargetId);
                    if (success) return $"{BotName}: Purge execution successful. Database Record #{parseTargetId} dropped from server memory.";
                    return $"{BotName}: Failure to drop target entity segment index #{parseTargetId}. Ensure targeted rows correspond with live schema keys.";
                }
                return $"{BotName}: Parsing target structure fault. Syntax pattern validation requirement: 'delete task [ID number]'";
            }

            // --- TASK 4: SEAMLESS CONVERSATION FLOW ---
            if (cleanInput.Contains("another") || cleanInput.Contains("more") || cleanInput.Contains("explain"))
            {
                if (LastTopic == "phishing") return $"{BotName}: Here is another phishing tip: {_phishingTips[_rng.Next(_phishingTips.Length)]}";
                if (LastTopic == "password") return $"{BotName}: Another password tip: Use a reputable password manager to store complex, unique passwords.";
                if (FavoriteTopic == "privacy") return $"{BotName}: Since you're interested in privacy, you should also review which apps have location permissions.";

                return $"{BotName}: I'd be happy to explain more! Try asking about 'passwords', 'browsing', or 'scams'.";
            }

            // --- TASK 6: SENTIMENT DETECTION ---
            if (cleanInput.Contains("worried") || cleanInput.Contains("scared") || cleanInput.Contains("frustrated"))
            {
                return $"{BotName}: It's completely understandable to feel that way. Scammers can be very convincing. \nTip: {_phishingTips[_rng.Next(_phishingTips.Length)]}";
            }

            // --- TASK 5: MEMORY (STORE) ---
            if (cleanInput.Contains("interested in privacy") || cleanInput.Contains("privacy"))
            {
                FavoriteTopic = "privacy";
                LastTopic = "privacy";
                return $"{BotName}: Great! I'll remember that you're interested in privacy, {UserName}.";
            }

            // --- TASK 2 (Original Fallback): KEYWORD RECOGNITION (CYBERSECURITY GUIDANCE) ---
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

            // --- TASK 7: DEFAULT FALLBACK ---
            return $"{BotName}: I'm not sure how to respond to that. Try asking about 'passwords', 'browsing', or 'scams'.\n💡 (Or type 'start quiz' to play the mini-game, or manage tasks with 'add task [name]')";
        }
    }
}