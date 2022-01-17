using System;
using CommandDotNet;
using CommandDotNet.NameCasing;
using CommandDotNet.Spectre;
using CommandDotNet.TypeDescriptors;
using Semver;

namespace mrGitTags
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var appRunner = BuildAppRunner();
                appRunner.Run(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.ExitCode = 1;
            }
        }

        private static AppRunner BuildAppRunner()
        {
            var appSettings = new AppSettings
            {
                ArgumentTypeDescriptors =
                {
                    new DelegatedTypeDescriptor<SemVersion>(
                        nameof(SemVersion),
                        v => SemVersion.Parse(v, strict: true))
                }
            };

            return new AppRunner<App>(appSettings)
                .UseDefaultMiddleware()
                .UseSpectreAnsiConsole()
                .UsePrompter()
                .UseArgumentPrompter()
                .UseCommandLogger(includeAppConfig: true)
                .UseNameCasing(Case.KebabCase)
                .UseErrorHandler((ctx, ex) =>
                {
                    ctx?.Console.Error.WriteLine(ex);
                    return 1;
                });
        }
    }
}