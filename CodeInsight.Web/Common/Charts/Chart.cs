using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ChartJSCore.Models;
using CodeInsight.Library.Types;
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
        public Chart(string title, ChartType type, ChartJSCore.Models.Data data, NonEmptyString xAxis, NonEmptyString yAxis)
        {
            Id = "Chart" + Guid.NewGuid().ToString().Replace("-", "");
            Title = title;
            JsChart = new ChartJSCore.Models.Chart
            {
                Type = type.Match(
                    ChartType.Line, _ => "line",
                    ChartType.Scatter, _ => "scatter"
                ),
                Data = data,
                Options = new Options
                {
                    Scales = new Scales
                    {
                        XAxes = new List<Scale>
                        {
                            new Scale
                            {
                                Display = true,
                                PluginDynamic = new Dictionary<string, object>
                                {
                                    { 
                                        "scaleLabel",
                                        new ScaleLabel
                                        {
                                            Display = true,
                                            LabelString = xAxis.Value,
                                            FontSize = 16
                                        } 
                                    }
                                }
                            }
                        },
                        YAxes = new List<Scale>
                        {
                            new Scale
                            {
                                Display = true,
                                PluginDynamic = new Dictionary<string, object>
                                {
                                    { 
                                        "scaleLabel",
                                        new ScaleLabel
                                        {
                                            Display = true,
                                            LabelString = yAxis.Value,
                                            FontSize = 16
                                        } 
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
        
        public string Id { get; }
        
        public string Title { get; }
        
        public ChartJSCore.Models.Chart JsChart { get; }
        
        public static Chart FromInterval(string title, DateInterval interval, IEnumerable<Dataset> dataSets, NonEmptyString xAxis, NonEmptyString yAxis)
        {
            return new Chart(title, ChartType.Line, new ChartJSCore.Models.Data
            {
                Labels = interval.Select(d => $"{d.Day}.{d.Month}").ToImmutableList(),
                Datasets = dataSets.ToList()
            }, xAxis, yAxis);
        }

        public static ChartJSCore.Models.Data CreateScatterData(string label, IEnumerable<LineScatterData> data)
        {
            return new ChartJSCore.Models.Data
            {
                Datasets = new List<Dataset>
                {
                    new LineScatterDataset
                    {
                        Fill = "false",
                        ShowLine = false,
                        Label = label,
                        Data = data.ToList()
                    }
                }
            };
        }
    }
}