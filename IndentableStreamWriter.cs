using System;
using CommandDotNet;
using CommandDotNet.Rendering;

namespace mrGitTags
{
    public class IndentableStreamWriter 
    {
        private readonly Action<string> _writeLine;
        private Indent _indent;

        public IndentableStreamWriter(IStandardStreamWriter writer, Indent indent = null)
        : this(writer.WriteLine, indent)
        {
        }


        public IndentableStreamWriter(Action<string> writeLine, Indent indent = null)
        {
            _writeLine = writeLine;
            _indent = indent ??= new Indent();
        }

        public IDisposable Indent()
        {
            _indent = _indent.Increment;
            return new DisposableAction(() => _indent = _indent.Decrement);
        }

        public void WriteLine(string text) => _writeLine($"{_indent}{text}");
    }
}