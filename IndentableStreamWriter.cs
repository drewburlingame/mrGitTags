using System;
using CommandDotNet;
using CommandDotNet.Rendering;

namespace mrGitTags
{
    public class IndentableStreamWriter 
    {
        private readonly IStandardStreamWriter _writer;
        private Indent _indent;

        public IndentableStreamWriter(IStandardStreamWriter writer, Indent indent = null)
        {
            _writer = writer;
            _indent = indent ??= new Indent();
        }

        public IDisposable Indent()
        {
            _indent = _indent.Increment;
            return new DisposableAction(() => _indent = _indent.Decrement);
        }

        public void WriteLine(string text) => _writer.WriteLine($"{_indent}{text}");
    }
}