using System;
using ChartJSCore.Models;
using FuncSharp;

namespace CodeInsight.Web.Common.Charts
{
    public enum ChartType
    {
        Line
    }
    
    public class Chart
    {
        public Chart(string title, ChartType type, Data data)
        {
            Id = Guid.NewGuid().ToString().Replace("-", "");
            Title = title;
            JsChart = new ChartJSCore.Models.Chart
            {
                Type = type.Match(
                    ChartType.Line, _ => "line"
                ),
                Data = data
            };
        }
        
        public string Id { get; }
        
        public string Title { get; }
        
        public ChartJSCore.Models.Chart JsChart { get; }
    }
}