
public static class Globals {
    public static string msg = "Please Provide Your T9 Input: ";

    public static void Unaccepted(string text){
        Console.WriteLine($"The Text {text} Given Was Unaccepted");
    }

    public static void Unaccepted(){
        // If I just want to give a general error then Overload the above method.
        Console.WriteLine($"The Text Given Was Unaccepted");
    }
};

namespace T9Texting
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter T9 Text:");
            var letterMappings = new Dictionary<string,string>(){
                // This was created to simplify my life. It's easily understood what it is, doesn't take up much
                // memory and makes it easy to fetch values from, so long as the keys are properly cleaned.
                {"1", "&"}, {"11", "&"}, {"111", "("},
                {"2", "a"}, {"22", "b"}, {"222", "c"},
                {"3", "d"}, {"33", "e"}, {"333", "f"},
                {"4", "g"}, {"44", "h"}, {"444", "i"},
                {"5", "j"}, {"55", "k"}, {"555", "l"},
                {"6", "m"}, {"66", "n"}, {"666", "o"},
                {"7", "p"}, {"77", "q"}, {"777", "r"},
                {"8", "s"}, {"88", "t"}, {"888", "u"},
                {"9", "w"}, {"99", "x"}, {"999", "y"}, {"9999", "z"},
                {"*", "backspace"}, {"0", " "}, {"#", "send"}
            };

            var t9Input = "";
            string currentWord = "";
            string allWords = "";

            string currentNumBeingUsed = "";

            Console.WriteLine(Globals.msg);

            while (currentWord != "#" && t9Input != "#" )
            /*
                If a '#' symbol is pressed or if enter is pressed it will finish this loop.
            */
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                
                if (keyInfo.Key == ConsoleKey.Enter)
                // The way I understand this is something akin to javascript events so when enter is pressed it 
                // also affects the GetUserInput function and starts the loop over again with the continue statement.
                {
                    currentWord = "";
                    t9Input = "";
                    continue;
                }

                string[] data = GetUserInput(currentNumBeingUsed);

                t9Input = data[0].ToString();

                if (currentNumBeingUsed != data[1]){
                    if (data[1] == "null"){
                        break;
                    }
                    // So GetUserInput Returns Two Things
                    // I did not make the variable static in the function nor would I want to
                    // So I must change the variable here to reflect actual changes
                    // Before sending it back to the function the next time around.
                    currentNumBeingUsed = data[1].ToString();
                }
                
                if (t9Input == "#" || t9Input == " " || t9Input == ""){
                    // This condition just checks to see if the user simply tried sending an empty message
                    continue;
                } else {
                    string[] t9Inputs = t9Input.Split(" ");
                    int t9InputsLength = t9Inputs.Length;
                    currentWord += SplitAndCheckText(t9Inputs, "-", letterMappings);
                }
                Console.WriteLine($"Current Word: {currentWord}");
                allWords += " " + currentWord;
                currentWord = "";

            }
            Console.WriteLine($"All Words: {allWords}");
        }

        static long TimeSinceLastInput(string input)
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - DateTimeOffset.Parse(input.Substring(1)).ToUnixTimeMilliseconds();
        }

        static string[] GetUserInput(string currentNumBeingUsed){
            string words = "";
            System.Timers.Timer timer = new (interval: 1000 );

            void handleTimeElapsed(){
                words += " ";
                timer.Stop();
            };

            timer.Elapsed += (sender, e) => {handleTimeElapsed();};
            
            int removeCounter = 0;
            bool typing = true;
            while(typing){
                if (words.Length > 0 && words.Last() != ' '){
                    // We don't want the timer indefinitely adding spaces
                    // So Instead It is checked that the string is not empty and the last item is not already a space
                    // This prevents the timer from ever 'STARTING' in such a case.
                    timer.Start();
                }
                
                // Getting user input below.
                char currentChar = Console.ReadKey().KeyChar;
                if (currentChar == 'x' || currentChar =='X'){
                    string[] exiting = {"", "null"};
                    timer.Dispose();
                    return exiting;
                }

                if (char.IsLetter(currentChar)){
                    continue;
                }
                
                if (timer.Enabled){
                    // Timer only needs to be stopped if it has started. Not sure if the extra check
                    // is necessary as it is a check at all times
                    // but changing enabled to false is a single action as well
                    // so at worst the efficiency is equal but at least this way the logic is consistent.
                    timer.Stop();
                }
                string currentString = currentChar.ToString();       
                if (char.IsDigit(currentChar)){
                    // First type verification we need numbers and not anything else other than
                    // spaces which is handled in the else block and are much easier to handle.
                    bool numberHasntChanged = currentString == currentNumBeingUsed;
                    bool numberHasChanged = !numberHasntChanged;
                    /* 
                        I don't like using CounterIntuitive boolean logic because it's more 
                        difficult for me to read and understand so I just created the 
                        inverse of the inverse in order to make the control flow below more 
                        easily understandable.
                    */

                    if (numberHasChanged && currentNumBeingUsed != ""){
                        /*
                            The first number being used is unknown in the beginning and so the variable is
                            set to an empty string hence the reason for (currentNumBeingUsed != "")
                        */

                        
                        currentNumBeingUsed = currentString;
                        words += "-";
                        removeCounter = 1;
                        /*
                            I set a '-' delimiter between blocks of text that don't have a space but are
                            made of different numbers in order to separate individual letters in the main
                            function.
                            I could have surely gone through them with a for loop and checked for a change
                            in numbers but I think this way is more elegant and more easily understood.
                            It's also easier to program because I'd have to mix quite a bit of logic together
                            To Achieve it in that way, at least with my current implementation.
                        */
                        words += currentString;
                    } else if (currentString == ""){
                        // In case someone tries to send in empty text, this is how it would be handled.
                        break;
                    } else {
                        words += currentString;
                        removeCounter += 1;
                        // Console.WriteLine($" == REMOVE COUNTER: {removeCounter}");
                    }
                    currentNumBeingUsed = currentString;
                } else if (currentString == "*"){
                    // If the remove counter is not more than 0 then there's nothing to remove.
                    if (removeCounter > 0){
                        // If the removeCounter > 0 then obviously I have some text to delete and so...
                        words = words.Remove(words.Length - removeCounter, removeCounter);
                        removeCounter = 0;
                        for (int i = words.Length-1; i > 0; i--){
                            /* 
                                This is For Removing Both Dashes and Spaces
                                To Clean Up The Text and Make It Less Error Prone
                            */
                            if (char.IsDigit(words[i])){
                                // breaking because it means that a number is the last char in the string
                                // And no more removing actions need be done.
                                break;
                            } else {
                                words.Remove(i);
                            }
                        }
                    }
                    
                } else if (currentString == " "){
                    // This just means the person typed a space
                    // And so in the 'text message' a space is added.
                    // Counter is reset because we're on to a new set of numbers to translate to text.
                    removeCounter = 0;
                    words += " ";
                } else {
                    // safety measure. Means that most likely nothing needs to be added and was a fluke
                    // or a typo of some sort. 
                    break;
                }                
            }
            timer.Dispose();
            string[] data = {words.Trim(), currentNumBeingUsed};
            return data;
        }

        static string SplitAndCheckText(string[] text, string delimiter, Dictionary<string, string> letterMappings){
            string newText = "";
            foreach(string input in text){
                string[] letterNums = input.Split("-");
                if (letterNums.Length > 1){
                    foreach(string numSet in letterNums){
                        try{
                            // Trying because there may be a key not found error
                            newText += letterMappings[numSet];  
                        } catch {
                            // if (numSet != " " && numSet != "*" && numSet != "  "){
                            //     Globals.Unaccepted(numSet);
                            // }
                            continue;
                        }
                    }
                } else {
                    try {
                        newText += letterMappings[input];
                    } catch {
                        // if (input != " " && input != "*" && input != "  "){
                            // Globals.Unaccepted(input);
                        // }
                        continue;
                    }
                }  
            };
            return newText;
        }
    }
}
