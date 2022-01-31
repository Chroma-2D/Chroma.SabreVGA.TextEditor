namespace Chroma.SabreVGA.TextEditor.DataModel
{
    public class Line
    {
        private int _caretIndex;

        public string Text { get; set; }

        public string TextBeforeCaret => Text.Substring(0, _caretIndex);
        public string TextAfterCaret => Text.Substring(_caretIndex);

        public bool IsCaretAtEnd => CaretIndex >= Text.Length;
        public bool IsCaretAtStart => CaretIndex == 0;

        public int CaretIndex
        {
            get => _caretIndex;
            set => _caretIndex = value > Text.Length ? Text.Length : value;
        }

        public Line(string text = "")
        {
            Text = text;
        }

        public void InsertAtCaret(string text, bool advanceCaret = true)
        {
            Text = TextBeforeCaret + text + TextAfterCaret;

            if (advanceCaret)
                CaretIndex += text.Length;
        }

        public void InsertAtCaret(char c, bool advanceCaret = true)
        {
            Text = TextBeforeCaret + c + TextAfterCaret;

            if (advanceCaret)
                CaretIndex++;
        }

        public string TextAfter(int index)
            => Text.Substring(index);

        public string TextBefore(int index)
            => Text.Substring(0, index);

        public string TextBetween(int startIndex, int endIndex)
            => Text[startIndex..endIndex];

        public string TextOutside(int startIndex, int endIndex)
            => TextBefore(startIndex) + TextAfter(endIndex);

        public void Left()
        {
            if (_caretIndex == 0)
                return;

            _caretIndex--;
        }

        public void Right()
        {
            if (_caretIndex >= Text.Length)
                return;

            _caretIndex++;
        }

        public void End()
        {
            _caretIndex = Text.Length;
        }

        public void Home()
        {
            _caretIndex = 0;
        }

        public void Delete()
        {
            if (_caretIndex >= Text.Length)
                return;

            var pre = Text.Substring(0, _caretIndex);
            var post = Text.Substring(_caretIndex + 1);

            Text = pre + post;
        }

        public void Backspace()
        {
            if (_caretIndex == 0)
                return;

            var pre = Text.Substring(0, _caretIndex - 1);
            var post = Text.Substring(_caretIndex);

            _caretIndex--;
            Text = pre + post;
        }
    }
}