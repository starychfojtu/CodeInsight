using System;
using System.Drawing;
using CodeInsight.PullRequests;

namespace CodeInsight.Web.Common.Charts
{
    public class LineDataSetConfiguration
    {
        public LineDataSetConfiguration(string label, Func<RepositoryStatistics, double?> valueGetter, Color color)
        {
            Label = label;
            ValueGetter = valueGetter;
            Color = color;
        }

        public string Label { get; }
        
        public Func<RepositoryStatistics, double?> ValueGetter { get; }
        
        public Color Color { get; }
    }
}