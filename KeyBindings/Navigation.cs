using Chroma.Input;
using Chroma.SabreVGA.TextEditor.DataModel;

namespace Chroma.SabreVGA.TextEditor.KeyBindings
{
    public class Navigation
    {
        [KeyBinding(KeyCode.Left)]
        public static void Left(Buffer buffer)
            => buffer.Left();

        [KeyBinding(KeyCode.Right)]
        public static void Right(Buffer buffer)
            => buffer.Right();

        [KeyBinding(KeyCode.Up)]
        public static void Up(Buffer buffer)
            => buffer.Up();

        [KeyBinding(KeyCode.Down)]
        public static void Down(Buffer buffer)
            => buffer.Down();

        [KeyBinding(KeyCode.Home)]
        public static void Home(Buffer buffer)
            => buffer.Home();

        [KeyBinding(KeyCode.End)]
        public static void End(Buffer buffer)
            => buffer.End();

        [KeyBinding(KeyCode.Home, KeyModifiers.Control)]
        public static void FirstLine(Buffer buffer)
            => buffer.FirstLine();

        [KeyBinding(KeyCode.Home, KeyModifiers.Control)]
        public static void LastLine(Buffer buffer)
            => buffer.LastLine();
    }
}