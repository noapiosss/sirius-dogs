using System;
using System.IO;

namespace Api
{
    public static class ReadEnv
    {
        public static void Load(string filePath = ".env")
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            string[] file = File.ReadAllLines(filePath);
            foreach (var line in file)
            {
                var keyValue = line.Split("=", 2, System.StringSplitOptions.TrimEntries);
                Environment.SetEnvironmentVariable(keyValue[0], keyValue[1]);
            }
        }
    }
}