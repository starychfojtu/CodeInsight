using System.Drawing;

namespace CodeInsight.Web.Common
{
    public static class ColorExtensions
    {
        public static string ToArgbString(this Color color) =>
            $"'rgba({color.R}, {color.G}, {color.B}, {color.A})'";
    }
}