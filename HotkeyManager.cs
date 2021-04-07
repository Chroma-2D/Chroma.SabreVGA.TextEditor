using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Chroma.Input;
using Chroma.SabreVGA.TextEditor.DataModel;
using Chroma.SabreVGA.TextEditor.KeyBindings;

namespace Chroma.SabreVGA.TextEditor
{
    public class HotkeyManager
    {
        private CodeEditor Owner { get; }
        public List<Hotkey> Hotkeys { get; private set; }

        internal HotkeyManager(CodeEditor owner)
        {
            Owner = owner;
            Hotkeys = new List<Hotkey>();

            var asm = Assembly.GetExecutingAssembly();
            var types = asm.GetTypes();

            foreach (var t in types)
            {
                var methods = t.GetMethods(BindingFlags.Public | BindingFlags.Static);

                foreach (var m in methods)
                {
                    var attribs = m.GetCustomAttributes<KeyBindingAttribute>().ToArray();

                    if (attribs.Length > 0)
                    {
                        foreach (var a in attribs)
                        {
                            Bind(
                                a.Modifiers,
                                a.Key,
                                _ => m.Invoke(null, new object[] {owner.CurrentBuffer}),
                                a.ClearsSelection
                            );
                        }
                    }
                }
            }
        }

        public void Bind(KeyModifiers modifiers, KeyCode keyCode, System.Action<Buffer> action,
            bool clearsSelection = false, bool requiresBuffer = false)
        {
            Hotkeys.Add(new Hotkey(modifiers, keyCode, action, clearsSelection, requiresBuffer));
            Hotkeys = Hotkeys.OrderByDescending(h => h.Priority).ToList();
        }
        
        public void UnbindAll()
        {
            Hotkeys.Clear();
        }

        public void Unbind(KeyModifiers modifiers, KeyCode keyCode)
        {
            var hotkey = Hotkeys.FirstOrDefault(
                x => x.Modifiers == modifiers
                     && x.KeyCode == keyCode
            );

            if (hotkey != null)
                Hotkeys.Remove(hotkey);
        }

        public void KeyPressed(KeyEventArgs e, Buffer buffer)
        {
            Hotkey hk = null;
            foreach (var hotkey in Hotkeys)
            {
                if (e.KeyCode != hotkey.KeyCode)
                    continue;

                var pass = true;

                if (hotkey.Modifiers.HasFlag(KeyModifiers.Control))
                {
                    if (!e.Modifiers.HasFlag(KeyModifiers.LeftControl) &&
                        !e.Modifiers.HasFlag(KeyModifiers.RightControl))
                        pass = false;
                }

                if (hotkey.Modifiers.HasFlag(KeyModifiers.Shift))
                {
                    if (!e.Modifiers.HasFlag(KeyModifiers.LeftShift) &&
                        !e.Modifiers.HasFlag(KeyModifiers.RightShift))
                        pass = false;
                }

                if (hotkey.Modifiers.HasFlag(KeyModifiers.Alt))
                {
                    if (!e.Modifiers.HasFlag(KeyModifiers.LeftAlt) &&
                        !e.Modifiers.HasFlag(KeyModifiers.RightAlt))
                        pass = false;
                }

                if (pass)
                {
                    hk = hotkey;
                    break;
                }
            }

            if (hk != null)
            {
                if (hk.RequiresBuffer && Owner.CurrentBuffer == null)
                    return;

                if (Owner.ShiftDown)
                    buffer.UpdateSelection();

                hk.Action(buffer);

                if (hk.ClearsSelection && !Owner.ShiftDown)
                    buffer.ClearSelection();
            }
        }
    }
}