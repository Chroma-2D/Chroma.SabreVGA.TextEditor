using System;
using Chroma.Graphics;
using Chroma.Input;
using Chroma.SabreVGA.TextEditor.DataModel;

namespace Chroma.SabreVGA.TextEditor
{
    public class StatusLine
    {
        private readonly int _messageVisibilitySeconds = 3;
        private DateTime? _messageShownAt;
        private string _message;

        private CodeEditor Owner { get; }
        private Action<string> ReadLineCallback { get; set; }

        public bool TakingInput { get; private set; }
        public Line Input { get; private set; }
        public string Prompt { get; private set; }

        public string Text { get; set; }
        public int CaretIndex => Input.CaretIndex;

        public StatusLine(CodeEditor owner)
        {
            Owner = owner;
        }

        public void ReadLine(string prompt, Action<string> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(
                    nameof(callback),
                    "callback cannot be null"
                );
            }

            if (TakingInput)
                return;

            Prompt = prompt;
            Input = new Line();
            ReadLineCallback = callback;

            Owner.Screen.Cursor.AllowMovementOutOfWindow = true;
            TakingInput = true;
        }

        public void EndInput(bool accept)
        {
            Owner.Screen.Cursor.AllowMovementOutOfWindow = false;
            TakingInput = false;

            if (accept)
                ReadLineCallback(Input.Text);

            Input = null;
            ReadLineCallback = null;
        }

        public void Render()
        {
            if (TakingInput)
            {
                Text = $"{Prompt}{Input.Text}";
            }
            else if (!string.IsNullOrEmpty(_message))
            {
                Text = _message;

                if (_messageShownAt != null
                    && (DateTime.Now - _messageShownAt)?.TotalSeconds > _messageVisibilitySeconds)
                {
                    ClearMessage();
                }
            }
            else if (Owner.CurrentBuffer == null)
            {
                Text = "no buffers opened";
            }
            else
            {
                if (Owner.CurrentBuffer.Dirty
                    || Owner.FileExistsFunc != null 
                    && !Owner.FileExistsFunc(Owner.CurrentBuffer.FilePath))
                {
                    Text = " * | ";
                }
                else
                {
                    Text = " - | ";
                }

                if (string.IsNullOrEmpty(Owner.CurrentBuffer.FilePath))
                    Text += "<untitled>";
                else
                    Text += Owner.CurrentBuffer.FilePath;

                Text +=
                    $" | Ln {Owner.CurrentBuffer.CurrentLineIndex + 1} : Col {Owner.CurrentBuffer.CurrentLine.CaretIndex + 1}";

                if (!Owner.CurrentBuffer.Selection.IsNone)
                {
                    var abs = Owner.CurrentBuffer.Selection.ToAbsolute();
                    Text += $" | SEL {abs.StartLine}:{abs.StartColumn} -> {abs.EndLine}:{abs.EndColumn}";
                }
            }

            Text = Text.PadLeft(1, ' ');
            Text = Text.PadRight(Owner.Screen.TotalColumns, ' ');

            PutString(
                Text,
                Owner.Screen.Margins.Left,
                Owner.Screen.WindowRows - 1,
                Color.Black,
                Owner.Screen.ActiveForegroundColor
            );
        }

        public void KeyPressed(KeyEventArgs e)
        {
            if (!TakingInput)
                return;

            switch (e.KeyCode)
            {
                case KeyCode.Escape:
                    EndInput(false);
                    break;

                case KeyCode.Return:
                    EndInput(true);
                    break;

                case KeyCode.Left:
                    Input.Left();
                    break;

                case KeyCode.Right:
                    Input.Right();
                    break;

                case KeyCode.Up:
                case KeyCode.Home:
                    Input.Home();
                    break;

                case KeyCode.Down:
                case KeyCode.End:
                    Input.End();
                    break;

                case KeyCode.Backspace:
                    Input.Backspace();
                    break;

                case KeyCode.Delete:
                    Input.Delete();
                    break;
            }

            ClearMessage();
        }

        public void SetMessage(string message)
        {
            _message = message;
            _messageShownAt = DateTime.Now;
        }

        public void TextInput(char c)
        {
            if (!TakingInput)
                return;

            Input.InsertAtCaret(c);
        }

        private void ClearMessage()
        {
            _message = string.Empty;
            _messageShownAt = null;
        }

        private void PutString(string s, int x, int y, Color fg, Color bg)
        {
            for (var i = 0; i < s.Length; i++)
            {
                if (x + i > Owner.Screen.WindowColumns)
                    break;

                Owner.Screen.PutCharAt(
                    x + i, 
                    y, 
                    s[i], 
                    fg, 
                    bg, 
                    false
                );
            }
        }
    }
}