using System;
using TripEngine.Core;

namespace TripEngine.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Meriken's Tripcode Engine modern console frontend ===");

            string key = "12345678";
            bool isSha1 = false;

            if (args.Length > 0)
            {
                key = args[0];
            }
            if (args.Length > 1 && args[1].ToLower() == "sha1")
            {
                isSha1 = true;
            }

            Console.WriteLine($"Key: {key}");
            Console.WriteLine($"Mode: {(isSha1 ? "SHA-1 (12-char)" : "DES (10-char)")}");

            try
            {
                var backend = new ScalarTripcodeBackend();
                string trip = backend.GenerateTripcode(key, isSha1);
                Console.WriteLine($"Generated Tripcode: {trip}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
