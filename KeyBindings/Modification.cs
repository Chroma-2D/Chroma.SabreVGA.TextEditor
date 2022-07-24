using Chroma.Input;
using Chroma.SabreVGA.TextEditor.DataModel;

namespace Chroma.SabreVGA.TextEditor.KeyBindings
{
    public class Modification
    {
        [KeyBinding(KeyCode.Backspace, RequiresBuffer = true)]
        public static void Backspace(Buffer buffer)
        {
            buffer.Backspace();
        }

        [KeyBinding(KeyCode.Delete, RequiresBuffer = true)]
        public static void Delete(Buffer buffer)
        {
            buffer.Delete();
        }

        [KeyBinding(KeyCode.Return, KeyModifiers.Control, RequiresBuffer = true)]
        public static void NewLineBeforeCurrent(Buffer buffer)
        {
            buffer.NewLineBeforeCurrent();
        }

        [KeyBinding(KeyCode.Return, RequiresBuffer = true)]
        [KeyBinding(KeyCode.NumEnter, RequiresBuffer = true)]
        public static void NewLineAfterCurrent(Buffer buffer)
        {
            buffer.NewLineAfterCurrent(true);
        }

        [KeyBinding(KeyCode.Up, KeyModifiers.Alt, RequiresBuffer = true)]
        public static void MoveLineUp(Buffer buffer)
        {
            buffer.MoveLineUp();
        }

        [KeyBinding(KeyCode.Down, KeyModifiers.Alt, RequiresBuffer = true)]
        public static void MoveLineDown(Buffer buffer)
        {
            buffer.MoveLineDown();
        }

        [KeyBinding(KeyCode.Tab, RequiresBuffer = true)]
        public static void Indent(Buffer buffer)
        {
            for (var i = 0; i < buffer.Owner.Options.IndentationSize; i++)
            {
                buffer.TextInput(' ');
            }
        }

        [KeyBinding(KeyCode.X, KeyModifiers.Control, RequiresBuffer = true)]
        public static void Cut(Buffer buffer)
        {
            Clipboard.Text = string.Join('\n', buffer.Cut());
        }

        [KeyBinding(KeyCode.C, KeyModifiers.Control, false, RequiresBuffer = true)]
        public static void Copy(Buffer buffer)
        {
            Clipboard.Text = string.Join('\n', buffer.GetSelectionText());
        }

        [KeyBinding(KeyCode.V, KeyModifiers.Control, RequiresBuffer = true)]
        public static void Paste(Buffer buffer)
        {
            buffer.Paste();
        }

        [KeyBinding(KeyCode.A, KeyModifiers.Control, false, RequiresBuffer = true)]
        public static void SelectAll(Buffer buffer)
        {
            buffer.SelectAll();
        }
    }
}