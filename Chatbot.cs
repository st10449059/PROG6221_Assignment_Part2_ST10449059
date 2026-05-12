using System;
using System.Collections.Generic;
using System.Media;
using System.IO;

namespace PROG6221_Assignment_Part2_ST10449059
{
    public class Chatbot
    {
        // Task 5: Memory - Store user details
        public string UserName { get; set; } = "User";
        public string FavoriteTopic { get; set; } = "";

        // Task 4: Conversation Flow - Tracks the last discussed subject for seamless follow-ups
        public string LastTopic { get; set; } = "";

        // Task 3: Randomized phishing tips
        private string[] _phishingTips = {
            "Be cautious of emails asking for personal information. Scammers often disguise themselves as trusted organisations.",
            "Always check the sender's email address for slight misspellings or odd domains.",
            "If a link looks suspicious, hover over it to see the real destination URL before clicking."
        };

        private Random _rng = new Random();

        // Task 1: UI Branding and Professional Identity
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

        public void PlayVoiceGreeting()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "greeting.wav");
                if (File.Exists(path))
                {
                    using (SoundPlayer player = new SoundPlayer(path)) { player.Play(); }
                }
            }
            catch (Exception) { /* Task 7: Function smoothly on errors */ }
        }

        public string ProcessInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return "CyberShield: I didn't catch that. Could you please type a question?";
            }

            string cleanInput = input.Trim().ToLower();
            string BotName = "CyberShield";

            // --- TASK 4: SEAMLESS CONVERSATION FLOW (Follow-ups) ---
            // This allows the user to say "another one" or "tell me more" without re-typing the topic
            if (cleanInput.Contains("another") || cleanInput.Contains("more") || cleanInput.Contains("explain"))
            {
                if (LastTopic == "phishing")
                {
                    return $"{BotName}: Here is another phishing tip: {_phishingTips[_rng.Next(_phishingTips.Length)]}";
                }
                if (LastTopic == "password")
                {
                    return $"{BotName}: Here is another password tip: Use a reputable password manager to store complex, unique passwords for every site.";
                }
                if (FavoriteTopic == "privacy")
                {
                    return $"{BotName}: Since you're interested in privacy, you should also review which apps have permission to access your location and microphone.";
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
                return $"{BotName}: Great! I'll remember that you're interested in privacy, {UserName}. It's a crucial part of staying safe online.";
            }

            // --- TASK 2: KEYWORD RECOGNITION (Updated with Context Tracking) ---

            if (cleanInput.Contains("password"))
            {
                LastTopic = "password"; // Set context for Task 4
                return $"{BotName}: (NIST Standard) Use at least 12 characters. A phrase like 'BlueElephantJump!' is harder to crack than 'P@ssword123'.";
            }

            if (cleanInput.Contains("phishing") || cleanInput.Contains("scam"))
            {
                LastTopic = "phishing"; // Set context for Task 4
                string definition = "(APWG Standard) Phishing is a trick to steal info via fake links.";
                return $"{BotName}: {definition} \nRandom Tip: {_phishingTips[_rng.Next(_phishingTips.Length)]}";
            }

            if (cleanInput.Contains("browsing"))
            {
                return $"{BotName}: (Ref: Cloudflare, 2024) Safe browsing means using HTTPS and checking the padlock icon in your URL bar.";
            }

            if (cleanInput.Contains("how are you"))
            {
                return $"{BotName}: I'm doing great, {UserName}! My firewall is strong and I'm ready to assist.";
            }

            if (cleanInput.Contains("purpose"))
            {
                return $"{BotName}: My purpose is to serve as your personal cybersecurity assistant and help you avoid digital threats.";
            }

            // --- TASK 7: DEFAULT FALLBACK ---
            return $"{BotName}: I'm not sure how to respond to that. Try asking about 'passwords', 'browsing', or 'scams'.";
        }
    }
}