using System.IO;
using System.Linq;
using Chroma.Input;
using Chroma.SabreVGA.TextEditor.DataModel;

namespace Chroma.SabreVGA.TextEditor.KeyBindings
{
    public class FileSystem
    {
        [KeyBinding(KeyCode.Q, KeyModifiers.Control, RequiresBuffer = true)]
        public static void Quit(Buffer buffer)
        {
            buffer.Owner.Close();
        }

        [KeyBinding(KeyCode.O, KeyModifiers.Control, false, RequiresBuffer = true)]
        public static void Save(Buffer buffer)
        {
            if (string.IsNullOrEmpty(buffer.FilePath))
            {
                buffer.Owner.StatusLine.ReadLine("Path: ", s =>
                {
                    if (s.Intersect(Path.GetInvalidPathChars()).Any())
                    {
                        buffer.Owner.StatusLine.SetMessage("The path provided was invalid.");
                    }
                    else
                    {
                        buffer.FilePath = s;
                    }
                });
            }

            if (!string.IsNullOrEmpty(buffer.FilePath) && buffer.Owner.FileSaveAction != null)
            {
                var text = string.Join('\n', buffer.Lines.Select(l => l.Text));

                buffer.Owner.FileSaveAction(buffer.FilePath, text);
                buffer.UpdateHash();
            }
        }
    }
}