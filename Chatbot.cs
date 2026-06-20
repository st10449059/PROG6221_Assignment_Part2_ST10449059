using System;
using System.Collections.Generic;
using System.Data; // PART 3 ADDITION: Allows us to use DataTables to hold database rows
using System.IO;
using System.Media;
using MySql.Data.MySqlClient; // PART 3 ADDITION: Connects C# to your local MySQL server

namespace PROG6221_Assignment_Part2_ST10449059
{
    /// <summary>
    /// Chatbot logic class responsible for processing cybersecurity queries.
    /// Code Attribution: Logic flow, context tracking, and sentiment analysis 
    /// developed with assistance from Microsoft Copilot AI (2024).
    /// </summary>
    public class Chatbot
    {
        // --- PART 3 ADDITION: Database Connection Configuration ---
        // NOTE: If you set a custom password when you installed MySQL, replace 'YourPassword' with it.
        private readonly string connectionString = "Server=localhost;Database=CyberShieldDB;Uid=root;Pwd=@Labs2026!;";

        // TASK 5: Memory - Stores user-specific data to personalize interaction
        public string UserName { get; set; } = "User";
        public string FavoriteTopic { get; set; } = "";

        // TASK 4: Conversation Flow - Tracks the last discussed subject for seamless follow-ups
        public string LastTopic { get; set; } = "";

        // TASK 3: Randomized Responses - Array to manage variations in security tips (Ref: APWG, 2024)
        private string[] _phishingTips = {
            "Be cautious of emails asking for personal information. Scammers often disguise themselves as trusted organisations.",
            "Always check the sender's email address for slight misspellings or odd domains.",
            "If a link looks suspicious, hover over it to see the real destination URL before clicking."
        };

        private Random _rng = new Random();

        // TASK 1: UI Branding - Returns professional ASCII identity (Ref: ASCII Art Archive, 2024)
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

        // TASK 1 & 7: Multimedia & Error Handling - Plays audio with file safety checks (Ref: Microsoft, 2024)
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
            catch { /* TASK 7: Silent fail ensures the app doesn't crash if audio fails */ }
        }

        // --- PART 3 ADDITION: Fetch all tasks from MySQL ---
        /// <summary>
        /// Reads all task records directly out of your MySQL database table 
        /// and hands them back as a DataTable to populate Form1's DataGridView.
        /// </summary>
        public DataTable GetAllTasks()
        {
            DataTable dt = new DataTable();
            try
            {
                // 1. Establish a secure link to the local MySQL Server using our connection string
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    // Open the database tunnel
                    conn.Open();

                    // 2. Define the standard SQL command to fetch your security tasks
                    string query = "SELECT id, title, description, reminder_days, is_completed FROM security_tasks ORDER BY id DESC;";

                    // 3. Use the DataAdapter to extract the raw rows and fill up our C# DataTable container
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                // Displays a safe error warning box if your connection credentials or server is offline
                System.Windows.Forms.MessageBox.Show("Database Read Error: " + ex.Message, "Database Status");
            }
            return dt;
        }

        /// <summary>
        /// Main logic method for processing user input and returning bot responses.
        /// String trimming logic informed by Portfolio Courses (2021).
        /// </summary>
        public string ProcessInput(string input)
        {
            // TASK 7: Input validation
            if (string.IsNullOrWhiteSpace(input))
            {
                return "CyberShield: I didn't catch that. Could you please type a question?";
            }

            string cleanInput = input.Trim().ToLower();
            string BotName = "CyberShield";

            // --- TASK 4: SEAMLESS CONVERSATION FLOW (Follow-ups) ---
            if (cleanInput.Contains("another") || cleanInput.Contains("more") || cleanInput.Contains("explain"))
            {
                if (LastTopic == "phishing")
                {
                    return $"{BotName}: Here is another phishing tip: {_phishingTips[_rng.Next(_phishingTips.Length)]}";
                }
                if (LastTopic == "password")
                {
                    return $"{BotName}: Another password tip: Use a reputable password manager to store complex, unique passwords.";
                }
                if (FavoriteTopic == "privacy")
                {
                    return $"{BotName}: Since you're interested in privacy, you should also review which apps have location permissions.";
                }

                return $"{BotName}: I'd be happy to explain more! Try asking about 'passwords', 'browsing', or 'scams'.";
            }

            // --- TASK 6: SENTIMENT DETECTION ---
            if (cleanInput.Contains("worried") || cleanInput.Contains("scared") || cleanInput.Contains("frustrated"))
            {
                return $"{BotName}: It's completely understandable to feel that way. Scammers can be very convincing. " +
                       $"\nTip: {_phishingTips[_rng.Next(_phishingTips.Length)]}";
            }

            // --- TASK 5: MEMORY (STORE) ---
            if (cleanInput.Contains("interested in privacy") || cleanInput.Contains("privacy"))
            {
                FavoriteTopic = "privacy";
                LastTopic = "privacy";
                return $"{BotName}: Great! I'll remember that you're interested in privacy, {UserName}.";
            }

            // --- TASK 2: KEYWORD RECOGNITION (CYBERSECURITY GUIDANCE) ---

            // Password logic (Ref: NIST SP 800-63B, 2024)
            if (cleanInput.Contains("password"))
            {
                LastTopic = "password";
                return $"{BotName}: (NIST Standard) Use at least 12 characters. A phrase like 'BlueElephantJump!' is hard to crack.";
            }

            // Phishing logic (Ref: OWASP, 2024)
            if (cleanInput.Contains("phishing") || cleanInput.Contains("scam"))
            {
                LastTopic = "phishing";
                string definition = "Phishing is a social engineering attack to steal info via fake links.";
                return $"{BotName}: {definition} \nRandom Tip: {_phishingTips[_rng.Next(_phishingTips.Length)]}";
            }

            // Browsing logic (Ref: IEEE Xplore, 2024)
            if (cleanInput.Contains("browsing"))
            {
                return $"{BotName}: Safe browsing means using TLS/HTTPS protocols and checking for the padlock icon.";
            }

            // General Interaction
            if (cleanInput.Contains("how are you"))
            {
                return $"{BotName}: I'm doing great, {UserName}! My firewall is strong and I'm ready to assist.";
            }

            if (cleanInput.Contains("purpose"))
            {
                return $"{BotName}: My purpose is to serve as your personal cybersecurity assistant.";
            }

            // --- TASK 7: DEFAULT FALLBACK ---
            return $"{BotName}: I'm not sure how to respond to that. Try asking about 'passwords', 'browsing', or 'scams'.";
        }
    }
}