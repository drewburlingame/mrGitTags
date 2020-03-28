using System.Linq;

namespace mrGitTags
{
    /// <summary>
    /// Encapsulates logic for creating indents, providing consistency across composed reporting methods.
    /// </summary>
    public class Indent
    {
        private readonly Indent _previousDepth;
        private Indent _nextDepth;

        /// <summary>The value of a single indent</summary>
        public string SingleIndent { get; }

        /// <summary>The number of <see cref="SingleIndent"/>s assigned to this indent</summary>
        public int Depth { get; }

        public string InitialPadding { get; }

        /// <summary>The value of <see cref="SingleIndent"/> repeated <see cref="Depth"/> times</summary>
        public string Value { get; }

        /// <summary>Returns an Indent with <see cref="Depth"/>+1<br/></summary>
        public Indent Increment => _nextDepth ??= new Indent(this);

        /// <summary>Returns an Indent with <see cref="Depth"/>-1<br/></summary>
        public Indent Decrement => _previousDepth ?? this;

        private Indent(Indent previous) 
            : this(previous.SingleIndent, previous.Depth + 1, previous.InitialPadding)
        {
            _previousDepth = previous;
        }

        public Indent(string singleIndent = "  ", int depth = 0, string initialPadding = "")
        {
            SingleIndent = singleIndent;
            Depth = depth;
            InitialPadding = initialPadding;
            Value = initialPadding + string.Join("", Enumerable.Repeat(singleIndent, depth));
        }

        /// <summary>Returns a new Indent with <see cref="Depth"/>+<see cref="by"/></summary>
        public Indent Deeper(int by)
        {
            var indent = this;
            for (int i = 0; i < by; i++)
            {
                indent = indent.Increment;
            }
            return indent;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}