using CommandDotNet;

namespace mrGitTags
{
    class Program
    {
        static void Main(string[] args)
        {
            new AppRunner<App>(
                    new AppSettings
                    {
                        DefaultArgumentMode = ArgumentMode.Operand,
                        Help =
                        {
                            ExpandArgumentsInUsage = true
                        }
                    })
                .UseDefaultMiddleware()
                .Run(args);
        }
    }
}