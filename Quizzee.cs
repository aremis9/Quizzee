using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Quizzee
{
    class Program
    {
        static readonly Regex notEmpty = new Regex(@"^[a-zA-Z0-9]*$");  // for checking alphanumerical inputs

        static string username = string.Empty;          // name of the user
        static string[] quizNames = new string[] { };   // for printing the created quizzes
        static int attempts = 3;                        // number of attempts to login

        static void Main(string[] args)
        {
            //Console.SetWindowSize(80, 40);            // Console size (width, height) for startup 
            Console.Title = "Quizzee!";                 // title of the console application (obvious ba)

            string QuizzeeAccounts = Directory.GetCurrentDirectory() + @"\QuizzeeAccounts.txt";
            if (!File.Exists(QuizzeeAccounts))
                File.CreateText(QuizzeeAccounts).Close();


            string Quizzes = Directory.GetCurrentDirectory() + @"\Quizzes";
            if (!Directory.Exists(Quizzes))
                Directory.CreateDirectory(Quizzes);

            UserMode();


            //Console.ReadKey(true);
        }



        // Prints the title
        static void PrintTitle()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            // @ -> verbatim
            Console.WriteLine(@"
         ██████╗ ██╗   ██╗██╗███████╗███████╗███████╗███████╗██╗
        ██╔═══██╗██║   ██║██║╚══███╔╝╚══███╔╝██╔════╝██╔════╝██║
        ██║   ██║██║   ██║██║  ███╔╝   ███╔╝ █████╗  █████╗  ██║
        ██║▄▄ ██║██║   ██║██║ ███╔╝   ███╔╝  ██╔══╝  ██╔══╝  ╚═╝
        ╚██████╔╝╚██████╔╝██║███████╗███████╗███████╗███████╗██╗
         ╚══▀▀═╝  ╚═════╝ ╚═╝╚══════╝╚══════╝╚══════╝╚══════╝╚═╝");
            Console.ResetColor();
            Console.WriteLine();


        }


        // lets the user to choose the mode of continuation (login, create accound, as guest, exit)
        public static void UserMode()
        {
            // record of accounts
            // serves as the database (database for kids)
            Dictionary<string, string> accounts = new Dictionary<string, string>();
            using (StreamReader readAccount = new StreamReader(Directory.GetCurrentDirectory() + @"\QuizzeeAccounts.txt"))
            {
                // reads the text file "QuizzeeAccounts.txt"
                while (!readAccount.EndOfStream)
                {
                    string line = readAccount.ReadLine();
                    if (line == "")
                        continue;   // continue if line is empty
                    string[] user = line.Split("|");    // split the strings by "|"

                    // the strings will be split into two
                    // 0 - username and 1 - password
                    // username as key, password as value in the created dictionary
                    accounts.Add(user[0], user[1]);
                }
            }

            // sets the possible choices
            string[] userModeOptions = { "Login", "Create an account", "Continue as Guest", "Exit" };
            // generates the choices
            int userMode = ChoiceIndicator.MultipleChoice(true, "", false, userModeOptions);
            string password = string.Empty;

            // checks the user's selected choice
            switch (userMode)
            {
                case 0: // Login
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("\tLogin\n");
                    Console.ResetColor();
                    Console.Write("\tEnter username: ");
                    username = Console.ReadLine();
                    Console.Write("\tEnter password: ");
                    password = PassProtect();
                    Console.WriteLine();

                    // if the inputted username and password matches, proceed.
                    if (accounts.ContainsKey(username) && accounts[username] == EncryptionHelper.Encrypt(password))
                    {
                        attempts = 3;
                        WhatToDo(username);
                        return;
                    }

                    // if not, reduce the attempts
                    else
                    {
                        attempts--;
                        if (attempts != 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\n\tIncorrect username or password.");
                            Console.Write("\t{0} {1} left.\n\n\tPlease press ENTER to try again.", attempts, attempts == 1 ? "attempt" : "attempts");
                            Console.Beep(frequency: 800, duration: 800);
                            Console.WriteLine();
                            Console.ResetColor();
                        }

                        // if attempts reached 0, the application will close
                        if (attempts == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\n\n\tFailed to open the application.\n");
                            int exitCountdown = 3;
                            while (exitCountdown != 0)
                            {
                                Console.WriteLine("\tClosing the application in {0}...", exitCountdown);
                                Console.Beep(frequency: 800, duration: 800);
                                Thread.Sleep(200);
                                exitCountdown--;
                            }
                            Console.ResetColor();
                            Environment.Exit(0);

                        }

                        // wait the user to click enter to try again
                        ConsoleKey key;
                        var keyInfo = Console.ReadKey(intercept: true);
                        key = keyInfo.Key;
                        do
                        {

                        } while (key != ConsoleKey.Enter);
                        UserMode();
                        break;
                    }
                case 1: // Create an account
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("\tCreate an account\n");
                    Console.ResetColor();
                    Console.Write("\tCreate username: ");
                    username = Console.ReadLine();
                    Console.Write("\tCreate password: ");
                    password = PassProtect();
                    Console.WriteLine();

                    // if the username exists, notify the user
                    if (accounts.ContainsKey(username))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"\n\tThe username \"{username}\" is already taken!");
                        Console.WriteLine();
                        Console.ResetColor();
                        ConsoleKey key;
                        var keyInfo = Console.ReadKey(intercept: true);
                        key = keyInfo.Key;
                        do
                        {

                        } while (key != ConsoleKey.Enter);
                        UserMode();
                        break;
                    }

                    // if the username or password contains "|" or the username starts with space, invalid
                    if (username.Contains("|") || password.Contains("|") || username == "")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"\n\tInvalid.");
                        Console.WriteLine();
                        Console.ResetColor();
                        ConsoleKey key;
                        var keyInfo = Console.ReadKey(intercept: true);
                        key = keyInfo.Key;
                        do
                        {

                        } while (key != ConsoleKey.Enter);
                        UserMode();
                        break;
                    }

                    // gets into here if the username and password is valid
                    else
                    {
                        string encryptedPassword = EncryptionHelper.Encrypt(password);
                        using (StreamWriter addAccount = File.AppendText(Directory.GetCurrentDirectory() + @"\QuizzeeAccounts.txt"))
                        {
                            // writes the username and password to the textfile "QuizzeeAccounts.txt"
                            // seperated with "|" so that the program will be able to split them 
                            addAccount.WriteLine($"{username}|{encryptedPassword}");
                        }

                        accounts.Add(username, encryptedPassword);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\n\tAccount created.\n\n\tPlease press ENTER to login");
                        Console.WriteLine();
                        Console.ResetColor();

                        // wait the user to press enter before proceeding
                        ConsoleKey key;
                        var keyInfo = Console.ReadKey(intercept: true);
                        key = keyInfo.Key;
                        do
                        {

                        } while (key != ConsoleKey.Enter);
                        UserMode();
                        break;
                    }
                case 2: // Continue as guest
                    username = "Guest";
                    WhatToDo(username);
                    break;
                case 3: // Exit
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"\tThank you!\n");
                    Console.ResetColor();
                    Console.WriteLine(@"

        Performance Task in Computer and TLE (3rd Quarter)
        Grade 10 - St. Peter
        2020 - 2021

        By:
        SIMERA's Group

                    ");

                    // wait 2 seconds before closing the application
                    Thread.Sleep(2000);
                    Environment.Exit(0);
                    break;
            }
        }


        // ask what the user wants to do (take/create a quiz, play a game, exit)
        public static void WhatToDo(string username)
        {
            DirectoryInfo filenames = new DirectoryInfo(Directory.GetCurrentDirectory() + @"\Quizzes");
            // gets the quiz titles (filenames) from the directory "Quizzes"
            FileInfo[] Files = filenames.GetFiles("*.txt");
            List<string> quizList = new List<string>();
            foreach (FileInfo file in Files)
            {
                quizNames = file.Name.Split(".txt");
                foreach (string s in quizNames)
                {
                    if (s == "")
                        continue;
                    quizList.Add(s);
                }

            }
            // adds "Back" as the last item in the list
            quizList.Add("Back");
            // Convert the list to array
            quizNames = quizList.ToArray();

            // welcomes the user and generates the choices
            string welcomeUser = String.Format($"Welcome to Quizzee, {username}! What do you want to do?");
            string[] whatToDoOptions = { "Take a quiz", "Create a quiz", "Play a game", "Logout" };
            int whatToDo = ChoiceIndicator.MultipleChoice(true, welcomeUser, true, whatToDoOptions);

            // checks the user's selected choice
            switch (whatToDo)
            {
                case 0: // take a quiz
                    // generates the quizzes list
                    int selectedQuiz = ChoiceIndicator.MultipleChoice(true, "Select a quiz", true, quizNames);
                    if (quizNames[selectedQuiz] == "Back")
                    {
                        // it the user's selected choice is "Back", then back lol
                        WhatToDo(username);
                        return;
                    }
                    else
                    {
                        // generates the selected quiz
                        TakeQuiz(selectedQuiz);
                        return;
                    }
                case 1: // create a quiz
                    PrintTitle();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("\tCreate a quiz\n");
                    Console.ResetColor();

                    Console.Write("\tEnter Quiz Title: ");
                    string newQuizTitle = Console.ReadLine();
                    // restrictions for naming the quiz title
                    if (newQuizTitle.StartsWith(" ") || newQuizTitle.Length < 1)
                    {
                        WhatToDo(username);
                        return;
                    }
                    // creates the new quiz to the directory "Quizzes"
                    string quizPath = Directory.GetCurrentDirectory() + @$"\Quizzes\{newQuizTitle}.txt";

                    // Checks if the title already exists
                    if (File.Exists(quizPath))
                    {
                        Console.WriteLine($"\t{newQuizTitle} already exists!");
                        Thread.Sleep(1500);

                    }
                    else
                    {
                        File.Create(quizPath).Dispose();
                        using (StreamWriter addQuiz = File.AppendText(quizPath))
                        {
                            bool doneCreating = false;
                            int questionCounter = 1;
                            while (doneCreating == false)
                            {
                                string newQuizQuestion = string.Empty;
                                do
                                {
                                    Console.Write($"\n\tEnter Question {questionCounter}: ");
                                    newQuizQuestion = Console.ReadLine();
                                } while (newQuizQuestion.StartsWith(" ") || newQuizQuestion.Length == 0);


                                addQuiz.Write($"{newQuizQuestion}|");

                                string[] abcd = { "Choice a: ", "Choice b: ", "Choice c: ", "Choice d: ", "Correct answer (a,b,c,d): " };
                                char[] abcd2 = { 'a', 'b', 'c', 'd' };
                                string choices = string.Empty;
                                foreach (string s in abcd)
                                {
                                    if (s == abcd[4])
                                    {
                                        do
                                        {
                                            Console.Write("\t" + s);
                                            choices = Console.ReadLine();
                                        } while (!abcd2.Any(choices.Contains) || choices.Length != 1);
                                    }
                                    else
                                    {
                                        do
                                        {
                                            Console.Write("\t" + s);
                                            choices = Console.ReadLine();
                                        } while (choices.StartsWith(" ") || choices.Length == 0);

                                        addQuiz.Write($"{choices}\t");

                                    }
                                }
                                addQuiz.WriteLine($"{choices}");

                                string newItem = string.Empty;
                                string[] yesNo = { "yes", "no" };
                                while (!yesNo.Any(newItem.Contains))
                                {
                                    Console.Write("\n\tAdd another question? (yes/no): ");
                                    newItem = Console.ReadLine();
                                }
                                if (newItem == "yes")
                                {
                                    questionCounter++;
                                    continue;
                                }
                                else
                                {
                                    doneCreating = true;
                                }

                            }
                        }
                    }
                    WhatToDo(username);
                    break;
                case 2:
                    while (true)
                    {
                        Random random = new Random();
                        int randno = random.Next(1, 20);
                        int count = 1;
                        while (true)
                        {
                            int input = 0;
                            do
                            {
                                Console.Write("\tEnter a number between 1 and 20 (0 to quit):");
                            } while (!int.TryParse(Console.ReadLine(), out input));
                            if (input == 0)
                            {
                                WhatToDo(username);
                                break;
                            }
                            else if (input < randno)
                            {
                                Console.WriteLine("\tHigher, try again buddy!\n");
                                count++;
                                continue;
                            }
                            else if (input > randno)
                            {
                                Console.WriteLine("\tLower, try again buddy!\n");
                                count++;
                                continue;
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine($"\tYou guessed it! The number was {randno}!");
                                Console.WriteLine("\tIt took you {0} {1}.\n", count, count == 1 ? "try" : "tries");
                                Console.ResetColor();
                                break;
                            }
                        }
                    }
                case 3:
                    UserMode();
                    return;

            }
        }


        // generates the selected quiz by the user
        public static void TakeQuiz(int selectedQuiz)
        {
            Dictionary<string, string[]> quizzes = new Dictionary<string, string[]>();
            Dictionary<string, char> answers = new Dictionary<string, char>();
            using (StreamReader readQuizzes = new StreamReader(Directory.GetCurrentDirectory() + @$"\Quizzes\{quizNames[selectedQuiz]}.txt"))
            {
                int questionNumber = 1;
                while (!readQuizzes.EndOfStream)
                {
                    string line = readQuizzes.ReadLine();
                    if (line == "")
                        continue;
                    string[] question = line.Split("|");
                    string[] notQuestion = string.Join("", question[1]).Split("\t");
                    string[] choices = { notQuestion[0], notQuestion[1], notQuestion[2], notQuestion[3] };
                    char answer = char.Parse(notQuestion[4]);

                    string[] abcd = { "a. ", "b. ", "c. ", "d. " };
                    for (int i = 0; i < choices.Length; i++)
                    {
                        choices[i] = abcd[i] + choices[i];
                    }
                    string questionOutput = $"{questionNumber}. {question[0]}";
                    quizzes.Add(questionOutput, choices);
                    answers.Add(questionOutput, answer);
                    questionNumber++;

                }
            }

            float score = 0;
            foreach (string question in quizzes.Keys)
            {
                int userAnswer = ChoiceIndicator.MultipleChoice(true, question, false, quizzes[question]);
                if (answers[question] == quizzes[question][userAnswer][0])
                {
                    score++;
                }
            }

            //Console.WriteLine($"\tQuiz:\t{quizNames[selectedQuiz]}");
            //Console.WriteLine($"\tUser:\t{username}");
            //Console.WriteLine($"\tDate:\t{DateTime.Now}");
            float percentage = (score / quizzes.Count) * 100;
            string status;
            if (quizzes.Count == 10 && percentage >= 70)
                status = "PASSED";
            else if (percentage >= 75)
                status = "PASSED";
            else
                status = "FAILED";

            //Console.WriteLine($"\n\tScore:\t{score} out of {quizzes.Count} ({percentage}%)");
            //Console.WriteLine($"\tStatus:\t{status}");

            string quizResult = $"Quiz:\t{quizNames[selectedQuiz]}\n" +
                                $"\tUser:\t{username}\n" +
                                $"\tDate:\t{DateTime.Now}\n" +
                                $"\n\tScore:\t{score} out of {quizzes.Count} ({Math.Round(percentage)}%)\n" +
                                $"\tStatus:\t{status}\n";

            int quizDone = ChoiceIndicator.MultipleChoice(true, quizResult, true, "Done");
            if (quizDone == 0)
            {
                WhatToDo(username);
            }
        }








        // when inputting passwords,
        // print asterisks(*) instead of letters
        static string PassProtect()
        {
            string pass = string.Empty;
            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    Console.Write("\b \b");
                    pass = pass[0..^1];
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    pass += keyInfo.KeyChar;
                }
            } while (key != ConsoleKey.Enter);
            return pass;
        }

        // Highlights the current selection on the choices
        // Indicates the user's decision in a choice making stuff
        public class ChoiceIndicator
        {
            public static int MultipleChoice(bool canCancel, string printable, bool withColor, params string[] options)
            {
                int selectedOption = 0;
                ConsoleKey key;

                Console.CursorVisible = false;

                do
                {
                    PrintTitle();
                    if (withColor)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"\t{printable}\n");
                        Console.ResetColor();
                    }
                    else
                        Console.WriteLine($"\t{printable}\n");

                    for (int i = 0; i < options.Length; i++)
                    {
                        if (i == selectedOption)
                        {
                            Console.Write("\t");
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.BackgroundColor = ConsoleColor.White;
                            Console.WriteLine($">> {options[i]}    ");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.WriteLine($"\t    {options[i]}    ");
                        }

                        Console.ResetColor();
                    }

                    key = Console.ReadKey(true).Key;

                    switch (key)
                    {
                        case ConsoleKey.LeftArrow:
                            {
                                if (selectedOption == 0)
                                    selectedOption = options.Length - 1;
                                else
                                    selectedOption--;
                                break;
                            }
                        case ConsoleKey.RightArrow:
                            {
                                if (selectedOption == options.Length - 1)
                                    selectedOption = 0;
                                else
                                    selectedOption++;
                                break;
                            }
                        case ConsoleKey.UpArrow:
                            {
                                if (selectedOption == 0)
                                    selectedOption = options.Length - 1;
                                else
                                    selectedOption--;
                                break;
                            }
                        case ConsoleKey.DownArrow:
                            {
                                if (selectedOption == options.Length - 1)
                                    selectedOption = 0;
                                else
                                    selectedOption++;
                                break;
                            }
                        case ConsoleKey.Escape:
                            {
                                if (canCancel)
                                    return -1;
                                break;
                            }
                    }
                } while (key != ConsoleKey.Enter);

                Console.CursorVisible = true;
                Console.Clear();
                PrintTitle();
                return selectedOption;
            }
        }

        // Instead of recording the user's password as plaintext,
        // encrypt it and record it as cyphertext.
        public static class EncryptionHelper
        {
            public static string Encrypt(string clearText)
            {
                string EncryptionKey = "abc123";
                byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(clearBytes, 0, clearBytes.Length);
                            cs.Close();
                        }
                        clearText = Convert.ToBase64String(ms.ToArray());
                    }
                }
                return clearText;
            }
            public static string Decrypt(string cipherText)
            {
                string EncryptionKey = "abc123";
                cipherText = cipherText.Replace(" ", "+");
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.Close();
                        }
                        cipherText = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
                return cipherText;
            }
        }


        /* Sources
         * PassProtect -> https://stackoverflow.com/a/3404522
         * ChoiceIndicator -> https://stackoverflow.com/a/46909420
         * EncryptionHelper -> https://stackoverflow.com/a/27484425
         */


    }
}


/*
 * Performance Task in Computer and TLE (3rd Quarter)
 * Grade 10 - St. Peter
 * 2020 - 2021
 * 
 * By:
 * SIMERA's Group
 */




/*

		MAIN / REQUIRED FEATURES
			- The code must run
			- 3 attempts only to open the application
			- The quiz must be answerable and must show the score after taking the quiz


		ADDITIONAL FEATURES
			- Quiz Experience
			- Login, Create an account, Continue as guest
			- Create a quiz
			- Play a game

*/



