using System.IO;
using System.Linq;
using System.Xml;
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
                    buffer.FilePath = s;
                    Write(buffer);
                });
            }
            else
            {
                Write(buffer);
            }
        }

        private static void Write(Buffer buffer)
        {
            if (!string.IsNullOrEmpty(buffer.FilePath) && buffer.Owner.FileSaveAction != null)
            {
                var text = string.Join('\n', buffer.Lines.Select(l => l.Text));
                var status = buffer.Owner.FileSaveAction(buffer.FilePath, text);
                
                if (status != 0)
                {
                    buffer.Owner.StatusLine.SetMessage($"Failed to save the file. Error code: {status}");
                    return;
                }
                
                buffer.UpdateHash();
            }
        }
    }
}