using System;
using CommandDotNet;
using CommandDotNet.TypeDescriptors;
using Semver;

namespace mrGitTags
{
    class Program
    {
        static void Main(string[] args)
        {
            var appSettings = new AppSettings
            {
                DefaultArgumentMode = ArgumentMode.Operand,
                Help =
                {
                    ExpandArgumentsInUsage = true
                }
            };
            var semVerDescriptor = new DelegatedTypeDescriptor<SemVersion>(
                nameof(SemVersion), 
                v => SemVersion.Parse(v));
            appSettings.ArgumentTypeDescriptors.Add(semVerDescriptor);
            
            var appRunner = new AppRunner<RepoApp>(appSettings)
                .UseDefaultMiddleware();
                //.UseCommandLogger(includeAppConfig: true);

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