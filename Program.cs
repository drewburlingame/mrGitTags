using System;
using CommandDotNet;

namespace mrGitTags
{
    class Program
    {
        static void Main(string[] args)
        {
            var appRunner = new AppRunner<RepoApp>(
                    new AppSettings
                    {
                        DefaultArgumentMode = ArgumentMode.Operand,
                        Help =
                        {
                            ExpandArgumentsInUsage = true
                        }
                    })
                .UseDefaultMiddleware();

            try
            {
                appRunner.Run(args);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
                Environment.ExitCode = -1;
            }
        }
    }
}