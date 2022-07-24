using Chroma.Input;
using Chroma.SabreVGA.TextEditor.DataModel;

namespace Chroma.SabreVGA.TextEditor.KeyBindings
{
    public class Navigation
    {
        [KeyBinding(KeyCode.Left, RequiresBuffer = true)]
        public static void Left(Buffer buffer)
            => buffer.Left();

        [KeyBinding(KeyCode.Right, RequiresBuffer = true)]
        public static void Right(Buffer buffer)
            => buffer.Right();

        [KeyBinding(KeyCode.Up, RequiresBuffer = true)]
        public static void Up(Buffer buffer)
            => buffer.Up();

        [KeyBinding(KeyCode.Down, RequiresBuffer = true)]
        public static void Down(Buffer buffer)
            => buffer.Down();

        [KeyBinding(KeyCode.Home, RequiresBuffer = true)]
        public static void Home(Buffer buffer)
            => buffer.Home();

        [KeyBinding(KeyCode.End, RequiresBuffer = true)]
        public static void End(Buffer buffer)
            => buffer.End();

        [KeyBinding(KeyCode.Home, KeyModifiers.Control, RequiresBuffer = true)]
        public static void FirstLine(Buffer buffer)
            => buffer.FirstLine();

        [KeyBinding(KeyCode.Home, KeyModifiers.Control, RequiresBuffer = true)]
        public static void LastLine(Buffer buffer)
            => buffer.LastLine();
    }
}