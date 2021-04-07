using System.Text.RegularExpressions;
using Chroma.Graphics;

namespace Chroma.SabreVGA.TextEditor.SyntaxHighlighting
{
    public abstract class SyntaxRule
    {
        public abstract Regex Regex { get; }

        public virtual Color Foreground { get; } = Color.Gray;
        public virtual Color Background { get; } = Color.Transparent;
    }
}