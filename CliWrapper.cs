using System;
using System.Threading;
using CliWrap;
using CommandDotNet;
using CommandDotNet.Rendering;

namespace mrGitTags
{
    public class CliWrapper
    {
        private readonly IConsole _console;
        private CancellationToken _cancellationToken;

        public CliWrapper(CommandContext commandContext)
        {
            _console = commandContext.Console;
            _cancellationToken = commandContext.CancellationToken;
        }

        public void Execute(string program, string arguments)
        {
            Cli.Wrap(program)
                .WithArguments(arguments)
                .WithWorkingDirectory(Environment.CurrentDirectory)
                .WithStandardOutputPipe(PipeTarget.ToDelegate((Action<string>)_console.WriteLine))
                .WithStandardErrorPipe(PipeTarget.ToDelegate(_console.Error.WriteLine))
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync(_cancellationToken).Task.Wait((CancellationToken)_cancellationToken);
        }
    }
}