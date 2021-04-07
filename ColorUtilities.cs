using Chroma.Graphics;

namespace Chroma.SabreVGA.TextEditor
{
    internal class ColorUtilities
    {
        internal static Color CalculateContrastingColor(Color x)
        {
            return Color.FromHSV(
                x.Hue + 0.5f,
                x.Saturation > 0.5f ? 0f : 1f,
                x.Value > 0.5f ? 0f : 1f
            );
        }
    }
}