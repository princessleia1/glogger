using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TeleprompterConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            RunTeleprompter().Wait();
        }

        private static async Task RunTeleprompter()
        {
            var config = new TelePrompterConfig();
            var displayTask = ShowTeleprompter(config);

            var speedTask = GetInput(config);
            await Task.WhenAny(displayTask, speedTask);
        }

        // Output asynchronously in one task while running other task of input for speed
        private static async Task ShowTeleprompter(TelePrompterConfig config)
        {
            var words = ReadFrom("sampleQuotes.txt");   // Quotes to read
            foreach (var word in words)
            {
                Console.Write(word);
                if (!string.IsNullOrWhiteSpace(word))
                {
                    await Task.Delay(config.DelayInMilliseconds);
                }
            }
            config.SetDone();
        }

        // Second asynchronous method to read from the Console with < and > 
        // Creates a lambda expression to represent an Action delegate that reads a key from the Console 
        private static async Task GetInput(TelePrompterConfig config)
        {
            Action work = () =>
            {
                do
                {
                    var key = Console.ReadKey(true);
                    if (key.KeyChar == '>')
                        config.UpdateDelay(-10);  
                    else if (key.KeyChar == '<')
                        config.UpdateDelay(10);
                    else if (key.KeyChar == 'X' || key.KeyChar == 'x')
                        config.SetDone();
                } while (!config.Done);
            };
            await Task.Run(work);   // Use await instead of wait keyword 
        }

        static IEnumerable<string> ReadFrom(string file)
        {
            string line;
            using (var reader = File.OpenText(file))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    var words = line.Split(' ');
                    var lineLength = 0;     // Keep track of each line with new line
                    foreach (var word in words)
                    {
                        yield return word + " ";
                        lineLength += word.Length + 1;
                        if (lineLength > 70)
                        {
                            yield return Environment.NewLine;
                            lineLength = 0;
                        }
                    }
                    yield return Environment.NewLine;
                }
            }
        }
    }
}