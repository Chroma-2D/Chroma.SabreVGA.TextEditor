using System;
using Chroma.Input;

namespace Chroma.SabreVGA.TextEditor.KeyBindings
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class KeyBindingAttribute : Attribute
    {
        public KeyModifiers Modifiers { get; }
        public KeyCode Key { get; }

        public bool RequiresBuffer { get; set; }
        public bool ClearsSelection { get; }

        public KeyBindingAttribute(KeyCode key, KeyModifiers modifiers = KeyModifiers.None, bool clearsSelection = true)
        {
            Modifiers = modifiers;
            Key = key;

            ClearsSelection = clearsSelection;
        }
    }
}