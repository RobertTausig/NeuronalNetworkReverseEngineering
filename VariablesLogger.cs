using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace NeuronalNetworkReverseEngineering
{
    public class VariablesLogger
    {
        private readonly string filePath;

        public VariablesLogger(string fileName)
        {
            this.filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
        }

        public void LogVariables(Dictionary<string, string> variables)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var (Name, Value) in variables)
            {
                sb.Append(Name).Append("\t").Append(Value).AppendLine();
            }

            File.AppendAllText(filePath, sb.ToString());
        }
    }
}


