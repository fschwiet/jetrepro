using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace RavenConsole
{
    class Program
    {
        public static int Main(string[] args)
        {
            var commands = new AbstractCommand[]
            {
                new ReplicateCommand()
            };

            AbstractCommand selectedCommand = null;

            try
            {
                if (args.Count() < 1)
                    throw new ArgumentException("");

                foreach (var possibleCommand in commands)
                {
                    if (args[0].ToLower() == possibleCommand.CommandText)
                    {
                        selectedCommand = possibleCommand;

                        selectedCommand.HandleArgs(args.Skip(1).ToArray());
                        break;
                    }
                }

                if (selectedCommand == null)
                {
                    throw new Exception("First parameter should be one of " + String.Join(", ", commands.Select(c => c.CommandText)));
                }
            }
            catch (Exception e)
            {
                if (!String.IsNullOrEmpty(e.Message))
                {
                    Console.WriteLine();
                    Console.WriteLine("Error message: " + e.Message);
                }

                foreach (var command in commands)
                {
                    command.WriteCommandHelp(Console.Out, "Transfer.exe");
                }

                Console.WriteLine();

                return -1;
            }

            selectedCommand.Run();
            return 0;
        }

        public static void WriteCommandHelp(AbstractCommand command, string executeableName)
        {
            Console.WriteLine(executeableName + " " + command.CommandText + " ...");
            command.GetOptionSet().WriteOptionDescriptions(Console.Out);
        }
    }
}
