namespace Chroma.SabreVGA.TextEditor.DataModel
{
    public class Options
    {
        public int IndentationSize { get; set; } = 2;
        public bool HighlightSyntax { get; set; } = false;
        public bool HighlightCurrentLine { get; set; } = true;
        public CursorShape CursorShape { get; set; } = CursorShape.Underscore;
    }
}