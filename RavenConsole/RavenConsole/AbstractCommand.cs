﻿using System.IO;
using NDesk.Options;

namespace RavenConsole
{
    public abstract class AbstractCommand
    {
        public abstract string CommandText { get; }
        public abstract string RemainingArgsHelpText { get; }
        public abstract OptionSet GetOptionSet();
        public abstract void HandleArgs(string[] remainingRemainingArgs);
        public abstract void Run();

        public void WriteCommandHelp(TextWriter tw, string executeableName)
        {
            tw.WriteLine();
            tw.WriteLine(executeableName + " " + CommandText + " " + RemainingArgsHelpText);
            GetOptionSet().WriteOptionDescriptions(tw);
        }
    }
}