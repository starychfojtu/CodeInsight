using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ChartJSCore.Models;
using FuncSharp;
using NodaTime;

namespace CodeInsight.Web.Common.Charts
{
    public enum ChartType
    {
        Line,
        Scatter
    }
    
    public class Chart
    {
        public Chart(string title, ChartType type, ChartJSCore.Models.Data data)
        {
            Id = "Chart" + Guid.NewGuid().ToString().Replace("-", "");
            Title = title;
            JsChart = new ChartJSCore.Models.Chart
            {
                Type = type.Match(
                    ChartType.Line, _ => "line",
                    ChartType.Scatter, _ => "scatter"
                ),
                Data = data
            };
        }
        
        public string Id { get; }
        
        public string Title { get; }
        
        public ChartJSCore.Models.Chart JsChart { get; }
        
        public static Chart FromInterval(string title, DateInterval interval, IReadOnlyList<Dataset> dataSets)
        {
            return new Chart(title, ChartType.Line, new ChartJSCore.Models.Data
            {
                Labels = interval.Select(d => $"{d.Day}.{d.Month}").ToImmutableList(),
                Datasets = dataSets.ToList()
            });
        }
    }
}