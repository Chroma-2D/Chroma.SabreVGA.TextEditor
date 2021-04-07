using System;
using Chroma.Input;

namespace Chroma.SabreVGA.TextEditor.DataModel
{
    public class Hotkey
    {
        public KeyModifiers Modifiers { get; }
        public KeyCode KeyCode { get; }
        public bool ClearsSelection { get; }
        public bool RequiresBuffer { get; }

        public int Priority
        {
            get
            {
                var result = 0;

                if (Modifiers == KeyModifiers.None)
                    return result;

                if (Modifiers.HasFlag(KeyModifiers.Control))
                    result++;

                if (Modifiers.HasFlag(KeyModifiers.Shift))
                    result++;

                if (Modifiers.HasFlag(KeyModifiers.Alt))
                    result++;

                return result;
            }
        }

        public Action<Buffer> Action { get; }

        public Hotkey(KeyModifiers modifiers, KeyCode keyCode, Action<Buffer> action, bool clearsSelection = false,
            bool requiresBuffer = false)
        {
            Modifiers = modifiers;
            KeyCode = keyCode;

            Action = action;
            ClearsSelection = clearsSelection;
            RequiresBuffer = requiresBuffer;
        }
    }
}