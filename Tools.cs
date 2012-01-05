using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ThouShallNotMessWithMySettings
{
    /// <summary>
    /// static tool methods
    /// </summary>
    class Tools
    {
        /// <summary>
        /// Display a question to the user and read a boolean response from him
        /// </summary>
        /// <param name="question">the question that must be asked. (don't add Y/N at the end, method will do it)</param>
        /// <returns>boolean result</returns>
        public static bool ReadConsoleBool(string question)
        {
            while (true)
            {
                Console.WriteLine(question + (" (Y/N)"));
                String r = (Console.ReadLine() ?? "").ToLower();
                if (r == "y")
                    return true;
                if (r == "n")
                    return false;
            }
        }
        /// <summary>
        /// this method quit the console application gracefully.
        /// </summary>
        public static void Harakiri()
        {
            Console.WriteLine("I will now terminate. Press a key to continue.");
            Console.Read();
            Environment.Exit(1);
        }

        /// <summary>
        /// Copy the contents of input stream to outout stream.
        /// </summary>
        /// <param name="input">The stream to read from</param>
        /// <param name="output">The stream to write to</param>
        public static void CopyStream(Stream input, Stream output)
        {
            // Insert null checking here for production
            byte[] buffer = new byte[8192];

            int bytesRead;
            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }
    }
}
