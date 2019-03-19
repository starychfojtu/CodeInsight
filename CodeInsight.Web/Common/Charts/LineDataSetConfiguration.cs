using System.Drawing;

namespace CodeInsight.Web.Common.Charts
{
    public class LineDataSetConfiguration
    {
        public LineDataSetConfiguration(string label, Color color)
        {
            Label = label;
            Color = color;
        }

        public string Label { get; }
        
        public Color Color { get; }
    }
}