using System;
using System.Windows.Forms;

namespace PROG6221_Assignment_Part2_ST10449059
{
    /// <summary>
    /// Entry point for the application.
    /// Code Attribution: Exception handling and app lifecycle structure 
    /// assisted by Microsoft Copilot AI (2024).
    /// </summary>
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                ApplicationConfiguration.Initialize();
                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                MessageBox.Show("CyberShield failed to initialize: " + ex.Message,
                                "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

/*
---------------------------------------------------------------------------------
REFERENCE LIST 
---------------------------------------------------------------------------------
APWG (Anti-Phishing Working Group) (2024) Phishing Activity Trends Report. [Online] 
Available at: https://www.antiphishing.org/resources/apwg-reports/ 

ASCII Art Archive (2024) Computer and Monitor ASCII Art Collections. [Online] 
Available at: https://www.asciiart.eu/computers/ 

Bakare, J. (2024) How to Build a Chatbot with Azure OpenAI & C# | Step-by-Step Tutorial. 
[Online Video] Available at: https://youtu.be/K_YIEgj_1is 

Cazzola, M. (2025) Your first AI chatbot in C#. [Online Video] 
Available at: https://youtu.be/gN__G3ztax0 

Cisco (2024) What Is Ransomware?. [Online] 
Available at: https://www.cisco.com/ 

IEEE (2024) 'Internet Security Protocols and Browsing Safety', IEEE Xplore Digital Library. 
Available at: https://ieeexplore.ieee.org/ 

Microsoft (2024) 'Microsoft Copilot AI: C# Code Assistance and Logic Generation'. 
Redmond: Microsoft Corporation.

Microsoft (2024) C# Documentation (System.Media & Threading). [Online] 
Available at: https://learn.microsoft.com/ 

Microsoft (2024) System.Media Namespace (Reference Documentation). [Online] 
Available at: https://learn.microsoft.com/en-us/dotnet/api/system.media 

NIST (National Institute of Standards and Technology) (2024) Digital Identity Guidelines. 
[Online] Available at: https://pages.nist.gov/800-63-3/ 

NIST (National Institute of Standards and Technology) (2024) Special Publication 800-63B: 
Digital Identity Guidelines. [Online] Available at: https://pages.nist.gov/800-63-3/sp800-63b.html 


OWASP (2024) 'Top 10 Social Engineering Risks and Phishing Prevention'. 
Available at: https://owasp.org/ 

Portfolio Courses (2021) Trim Leading Whitespace String Characters | C Programming Example. 
[Online Video] Available at: https://youtu.be/f0BqwfVACKY 
---------------------------------------------------------------------------------
*/