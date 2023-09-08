//Author: Gali Polyak
//File Name: PolyakGPASS1
//Project Name: PASS2
//Creation Date: Feb. 18, 2021
//Modified Date: Feb.23, 2021
//Description: User-interactive wordle game!

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PolyakGPASS1
{
    class MainClass
    {
        //Creates random number
        static Random rng = new Random();

        //Set up variables to use file.io
        static StreamWriter outFile = null;
        static StreamReader inFile = null;

        //Stores dictonaries for answer words and extra word
        static List<string> answers = new List<string>();
        static List<string> extras = new List<string>();

        //Stores list of alphabet
        static List<char> alphabet = new List<char>() { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'};

        //Stores list of exact (green) letters to display in the alphabet
        static List<char> exactLetters = new List<char>();
        //Stores list of approximate (yellow) letters to display in the alphabet
        static List<char> existLetters = new List<char>();
        //Stores list of used (dark gray) letters to display in the alphabet
        static List<char> usedLetters = new List<char>();

        //Stores user inputted word to display as board
        static char[,] wordleWord = new char[6, 5];
        //Stores user inputted word to display as board edited to display colours
        static char[,] wordleResult = new char[6, 5];

        //Stores randomly generated answerword
        static string answerWord;

        //Stores amount of user attempts
        static int attempt;

        //Stores whether the user exits the program
        static bool exit = false;

        //Stores whether the user wins the program
        static bool gameWon;

        //Stores user inputted word
        static string word;

        //Stores statistics
        static int gmsPlayed;
        static int gmsWon;
        static int winPercent;
        static int currStreak;
        static int maxStreak;
        //Stores win distribution
        static int[] distr = new int[6];

        //Store game states
        private const string INSTRUCTIONS = "1";
        private const string GAMEPLAY = "2";
        private const string EXIT = "3";

        //Store the current gamestate
        static string gameState;

        //Store whether game is done
        static bool gameDone;

        static void Main(string[] args)
        {
            //Read in statistics into file
            ReadStatistics();

            //game is ongoing (not exited)
            while (!exit)
            {
                //Clear screen
                Console.Clear();

                //Display menu 
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Welcome to Wordle!!");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n1. Instructions\n2. Play game\n3. Exit\n\nChoice:");
                Console.ResetColor();
                gameState = Console.ReadLine();

                switch (gameState)
                {
                    case INSTRUCTIONS:
                        //Display game rules and instructions
                        DisplayGameRules();
                        break;
                    case GAMEPLAY:
                        //Implement standard game logic
                        StartGame();
                        break;
                    case EXIT:
                        //Exit game
                        exit = true;
                        break;
                }
            }
        }

        //Pre: none
        //Post: none
        //Description: Implement standard game logic
        private static void StartGame()
        {
            //Load dictionaries of wordle answer words and extra words
            LoadDictionaries("WordleAnswers.txt", answers);
            LoadDictionaries("WordleExtras.txt", extras);

            //Reset game variables updated through the program
            ResetVariables();

            //While wordle game is not done
            while (!gameDone)
            {
                //Draw game board
                DrawGameBoard(answerWord);


                //Enter word and check validity
                EnterAndCheckWord(answerWord);

                //If word is a perfect match 
                if (word.Equals(answerWord))
                {
                    //Game is finished and won
                    gameDone = true;
                    gameWon = true;
                }

                //If all attempts are used up 
                if (attempt == 5)
                {
                    //Game is finished
                    gameDone = true;
                }

                //Add one to each attempt
                attempt++;              
            }

            //Update statistics
            UpdateStats();

            //Read updated statistics into file
            SaveStatistics();

            //Display statistics 
            DisplayStatistics(answerWord);

        }

        //Pre: None
        //Post: None
        //Description: Resets statistics to zero
        private static void resetStats()
        {
            gmsPlayed=0;
            gmsWon=0;
            currStreak=0;
            maxStreak = 0;
            winPercent = 0;
            for (int i=0; i < distr.Length; i++)
            {
                distr[i] = 0;
            }
        }

        //Pre: None
        //Post: None
        //Description: Update statistics after each completed game
        private static void UpdateStats()
        {
            //Add one
            gmsPlayed++;


            if (gameWon)
            {
                //Add one to each 
                gmsWon++;
                currStreak++;

                //If current streak exceeds max streak
                if (currStreak > maxStreak)
                {
                    //Set max streak to current streak
                    maxStreak = currStreak;
                }

                //Add one to the win distribution based on attempt
                distr[attempt - 1]++;
            }
            else
            {
                //If game isn't won set current streak to zero
                currStreak = 0;
            }

            //Calculate win percentage
            winPercent = 100 * gmsWon / gmsPlayed;

        }
        
        
        //Pre: fileName is a string that is the valid name of the file, listName is a string list to store wordle extra and wordle answer words
        //Post: none
        //Description: Load wordle answer words and extra words into list
        private static void LoadDictionaries(string fileName, List <string> listName)
        {
            //Try-catch statement to error handle info read from file
            try
            {
                //Use line to read through file
                string line = "";

                //Open file
                inFile = File.OpenText(fileName);

                //Read in
                inFile.ReadLine(); 

                //Continue reading until line equals the end bracket
                while (!line.Equals("}"))
                {
                    //Read in words from file and convert them to uppercase
                    line = inFile.ReadLine().ToUpper();

                    if (!line.Equals("}"))
                    {
                        //add each line to list
                        listName.Add(line);
                    }
                }
            }
            catch (FileNotFoundException fnf)
            {
                Console.WriteLine("ERROR: " + fnf.Message);
            }
            catch (FormatException fe)
            {
                Console.WriteLine("ERROR: " + "File was not properly saved");
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
            }
            finally
            {
                if (inFile != null)
                {
                    //Close file
                    inFile.Close();
                }
            }
        }

        //Pre: answerWord is a string - not needed but easier in terms of marking :)
        //Post: none
        //Description: Draws the game board
        private static void DrawGameBoard(string answerWord)
        {
            //Clears screen
            Console.Clear();

            //Displays answer word
            //Console.WriteLine(answerWord);

            //Loop through alphabet list
            for (int i = 0; i < alphabet.Count(); i++)
            {
                //If there is an exact letter in the word
                if (exactLetters.Contains(alphabet[i]))
                {
                    //Colour letter green in alphabet
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                //If there is an approx letter in the word
                else if (existLetters.Contains(alphabet[i]))
                {
                    //Colour letter yellow in alphabet
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                //If there are no yellow or green letters in the word
                else if (usedLetters.Contains(alphabet[i]))
                {
                    //Colour letter dark gray in alphabet
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                }

                //Write coloured alphabet letter than reset colour
                Console.Write(alphabet[i]);
                Console.ResetColor();
            }
              
            //Draw the board
            Console.WriteLine("\n\n┌───┬───┬───┬───┬───┐");

            //Determine whether word has any matches and draw
            for (int row = 0; row < wordleWord.GetLength(0); row++)
            {
                for (int col = 0; col < wordleWord.GetLength(1); col++)
                {
                    Console.Write("│ ");

                    //Determine whether wordle result is E - for Exact
                    if (wordleResult[row, col] == 'E')
                    {
                        //Colour green
                        Console.ForegroundColor = ConsoleColor.Green;
                    }

                    //Determine whether wordle result is A - for Approx
                    else if (wordleResult[row, col] == 'A')
                    {
                        //Colour yellow
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    }

                    else
                    {
                        //Colour dark gray
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }
                    //Write coloured wordle word and reset colour
                    Console.Write(wordleWord[row, col]);
                    Console.ResetColor();

                    Console.Write(" ");

                }
                //display border line
                Console.Write("|\n");

                //If row is not the last, draw row transition
                if (row != 5)
                {
                    Console.WriteLine("├───┼───┼───┼───┼───┤");
                }

                //Display bottom of board
                else
                {
                    Console.WriteLine("└───┴───┴───┴───┴───┘");
                }

            }

        }

        //Pre: None
        //Post: None
        //Description: Resets all variables updated in game to original
        private static void ResetVariables()
        {
            //Reset console colour
            Console.ResetColor();

            //Set wordle word to spaces for each index
            for (int row = 0; row < wordleWord.GetLength(0); row++)
            {

                for (int col = 0; col < wordleWord.GetLength(1); col++)

                {
                    wordleWord[row, col] = ' ';
                }
            }

            //Set wordle result word to spaces for each index
            for (int row = 0; row < wordleResult.GetLength(0); row++)
            {

                for (int col = 0; col < wordleResult.GetLength(1); col++)

                {
                    wordleResult[row, col] = ' ';
                }
            }

            //Set updated game variables to false (game is not complete)
            gameDone = false;
            gameWon = false;

            //Generates random index
            int index = rng.Next(0, answers.Count);

            //Store random answer word randomly generated from list of answer words
            answerWord = answers[index];

            //Set number of attempts to zero
            attempt = 0;

            //Clear match checking lists for wordle words
            exactLetters.Clear();
            existLetters.Clear();
            usedLetters.Clear();
            Array.Clear(wordleResult, 6, 5);

        }

        //Pre: answerWord is a string
        //Post: none
        //Description: Get user inputted word, check if word is valid, check if word has any matches with answerWord
        private static void EnterAndCheckWord(string answerWord)
        {
            //word is not valid
            bool wordValid = false;

            //While word is not valid
            while (!wordValid)
            {
                //Ask user for guess and read their input
                Console.Write("\nEnter Guess " + (attempt + 1) + ": ");
                word = (Console.ReadLine()).ToUpper();

                //Check if iputted word is valid
                wordValid = CheckIfValid();

                //If word isn't valid write default statement
                if (!wordValid)
                {
                    Console.Write("\n\nSorry, the word you inputted is invalid.\n\nPress <ENTER> to try again!\n");
                    Console.ReadLine();

                }

                else
                {
                    //Add user inputted word into an array to be displayed
                    for (int i = 0; i < word.Length; i++)
                    {
                        wordleWord[attempt, i] = word[i];

                    }

                    //Create an array to fill with finding match logic
                    char[] res = CheckWord(answerWord);
                    //Loop array through wordleResult array to be compared to wordleWord array to determine the colour
                    for (int i = 0; i < res.Length; i++)
                    {
                        wordleResult[attempt, i] = res[i];
                    }
                }
            }

        }

        //Pre: answerWord is a string
        //Post: Return the resulting matches as indexes in array
        //Description: Find matches from user inputted word to the answer word
        private static char[] CheckWord(string answerWord)
        {
            //Store word into 'temporary' local char array so it won't update through the program
            char[] tmpWord = word.ToCharArray();
            //Store answerWord into 'temporary' local char array
            char[] tmpAnswerWord = answerWord.ToCharArray();

            //Create new array to store matches in each index
            char[] result = new char[] { ' ', ' ', ' ', ' ', ' ' };

            //Loop through temporary answer word and word letters to find perfect matches
            for (int i = 0; i < tmpAnswerWord.Length; i++)
            {
                //If exact matches
                if (tmpAnswerWord[i] == tmpWord[i])
                {
                    //Add letter to exact letters array to later colour green in alphabet
                    exactLetters.Add(tmpAnswerWord[i]);

                    //Add, 'E' - for exact, into results array
                    result[i] = 'E';

                    //? - means not being checked further
                    tmpAnswerWord[i] = '?';
                    tmpWord[i] = '?';
                }
            }

            //Loop through temporary answer word and word letters to find approx matches
            for (int i = 0; i < tmpWord.Length; i++)
            {
                //Add letters to used letters array to later colour dark gray in alphabet
                usedLetters.Add(tmpWord[i]);

                //If word indexes have not been checked
                if (tmpWord[i] != '?')
                {
                    //Loop through both answer word and word letters to find approximate matches
                    for (int j = 0; j < tmpAnswerWord.Length; j++)
                    {
                        if (tmpAnswerWord[j] == tmpWord[i])
                        {
                            //Add letter to existing letters array to later colour yellow in alphabet
                            existLetters.Add(tmpAnswerWord[j]);

                            //Add, 'A' - for approximate, into results array
                            result[i] = 'A';
                            tmpAnswerWord[j] = '?';
                            break;
                        }
                    }
                }
            }
            //return resulting array of five indexes containing, 'E' for exact, 'A' for approx, and '?' for dark gray
            return result;

        }

        //Pre: none
        //Post: none
        //Description: Check if user inputted word is valid
        private static bool CheckIfValid()
        {
            //Check if word is in extra words list or answer words list
            bool checkAnswers = answers.Contains(word);
            bool checkExtras = extras.Contains(word);

            if (checkAnswers == true || checkExtras == true)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        //Pre: none
        //Post: none
        //Description: Save new statistic values into statistic file
        private static void SaveStatistics()
        {
            //Try-catch statement to error handle info written to file
            try
            {
                //Create text to statistics file
                outFile = File.CreateText("Statistics.txt");

                //Write statistics into file
                outFile.WriteLine(gmsPlayed + "," + gmsWon + "," + winPercent + "," + currStreak + "," + maxStreak + "," +
                distr[0] + "," +distr[1] + "," + distr[2] + "," + distr[3] + "," + distr[4] + "," + distr[5]);
                outFile.Close();  

            }

            catch (IndexOutOfRangeException re)
            {
                Console.WriteLine("ERROR: " + re.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
            }
            finally
            {
                outFile.Close();
            }
        }

        //Pre: none
        //Post: none
        //Description: Read statistics from file
        private static void ReadStatistics()
        {
            //Create data variable to store statistics from file
            string[] data;

            //Check if file exists!!
            if (File.Exists("Statistics.txt"))
            {
                //Open file
                inFile = File.OpenText("Statistics.txt");

                //Use to read lines in file
                string line = inFile.ReadLine();
                Console.WriteLine(line);

                //Read data between every comma
                data = line.Split(',');

                //Check whether all data is in file
                if (data.Length == 11)
                {
                    //Store data read from file back into original variables 
                    gmsPlayed = Convert.ToInt32(data[0]);
                    gmsWon = Convert.ToInt32(data[1]);
                    winPercent = Convert.ToInt32(data[2]);
                    currStreak = Convert.ToInt32(data[3]);
                    maxStreak = Convert.ToInt32(data[4]);

                    distr[0] = Convert.ToInt32(data[5]);
                    distr[1] = Convert.ToInt32(data[6]);
                    distr[2] = Convert.ToInt32(data[7]);
                    distr[3] = Convert.ToInt32(data[8]);
                    distr[4] = Convert.ToInt32(data[9]);
                    distr[5] = Convert.ToInt32(data[10]);
                }
                //Close file
                inFile.Close();
            }

        }

        //Pre: answerWord must be a string and be the same throughout each game round
        //Post: none
        //Description: Display statistics and game completion result 
        private static void DisplayStatistics(string answerWord)
        {
            //Clear screen
            Console.Clear();

            //Draw stats message
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("                 STATISTICS\n");

            //Check if user won/lost to determine which message to display
            Console.ForegroundColor = ConsoleColor.Black;
            if (gameWon == true)
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.WriteLine("CONGRADULATIONS!! You won. Correct Word - " + answerWord);
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("      YOU LOST ...      :( Correct Word - " + answerWord);
            }
            Console.WriteLine("\n");
            Console.ResetColor();


            //Display statistic titles
            Console.WriteLine("      Played   Win %  Current  Max");
            Console.WriteLine("                      Streak  Streak\n");

            //Display statistics
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("      " + gmsPlayed + "       " + winPercent + "%      " + currStreak + "       " + maxStreak + "\n");

            //Draw guess distribution title
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("===============================================");
            Console.WriteLine("               Guess Distribution");

            //Display guess distribution statistics
            Console.ResetColor();
            for (int i=0; i < distr.Length; i++)
            {
                Console.WriteLine("-----------------------------------------------");
                Console.WriteLine(i+1 + ": " + distr[i]);
            }
            Console.WriteLine("-----------------------------------------------");

            //Display menu
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("\n\n1. Play Again\n2. Reset Stats\n3. Quit\n\nChoice: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    //Restart game
                    StartGame();
                    break;
                case "2":
                    //Reset statistics
                    resetStats();
                    DisplayStatistics(answerWord);
                    break;
                case "3":
                    //Exit game
                    exit = true;
                    break;
                default:
                    Console.Write("");
                    break;
            }
            Console.ResetColor();
        }

        //Pre: none
        //Post: none
        //Description: Display game rules
        private static void DisplayGameRules()
        {
            //Clear screen
            Console.Clear();

            //Display game rules and colours (no logic)
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Welcome to Wordle!!\n");
            Console.ResetColor();
            Console.WriteLine("Guess the five letter word.\n");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Game rules:");
            Console.ResetColor();
            Console.WriteLine("You get six attempts total to guess the word. \nOnce you've entered your word, press <ENTER> key to submit.");
            Console.Write("\nIf the letter is ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("yellow");
            Console.ResetColor();
            Console.Write(" that means the letter is in the word but in the wrong location.\n");
            Console.Write("If the letter is ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("green");
            Console.ResetColor();
            Console.Write(" that means the letter is in the word and in the right location.\n");
            Console.Write("If the letter is ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("dark gray");
            Console.ResetColor();
            Console.Write(" that means the letter is not in the word at all.");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n\nPress <ENTER> to return to menu");
            Console.ResetColor();
            Console.ReadLine();

        }
    }
}

