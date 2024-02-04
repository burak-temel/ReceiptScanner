using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using DTO;
using Newtonsoft.Json;

namespace ReceiptScanner
{
    namespace ReceiptScanner
    {
        class Program
        {
            static void Main(string[] args)
            {
                string jsonFilePath = "./../../../response.json";

                try
                {
                    string jsonContent = File.ReadAllText(jsonFilePath);
                    List<DescriptionItem> items = JsonConvert.DeserializeObject<List<DescriptionItem>>(jsonContent);

                    var processedItems = items
                        .Where(item => item.BoundingPoly != null && item.BoundingPoly.Vertices != null && item.Locale == null)
                        .SelectMany(item => item.Description.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(desc => new ProcessedItems
                        {
                            Text = desc.Trim(),
                            Y = item.BoundingPoly.Vertices.Average(v => v.Y),
                            X = item.BoundingPoly.Vertices.Average(v => v.X)
                        }))
                        .OrderBy(item => item.Y).ThenBy(item => item.X)
                        .ToList();

                    var groupedTexts = GroupTexts(processedItems);

                    Console.WriteLine($"{"line".PadLeft(4)} Text");


                    int lineNumber = 1;
                    foreach (var line in groupedTexts)
                    {
                        Console.WriteLine($"{lineNumber.ToString().PadLeft(4)} {line}");
                        lineNumber++;
                    }
                }
                catch (IOException ex)
                {
                    Console.WriteLine("Error reading the JSON file:");
                    Console.WriteLine(ex.Message);
                }
            }


            //private static IEnumerable<string> GroupTexts(List<ProcessedItems> items)
            //{
            //    double yTolerance = 10; // Y koordinatları arasındaki toleransı ayarlayın
            //    var lines = new List<string>();
            //    var line = new List<string>();
            //    double lastY = -1;

            //    foreach (var item in items)
            //    {
            //        if (lastY < 0 || Math.Abs(item.Y - lastY) <= yTolerance)
            //        {
            //            line.Add(item.Text);
            //        }
            //        else
            //        {
            //            if (line.Count > 0)
            //            {
            //                lines.Add(string.Join(" ", line));
            //                line.Clear();
            //            }
            //            line.Add(item.Text);
            //        }
            //        lastY = item.Y;
            //    }

            //    if (line.Count > 0)
            //    {
            //        lines.Add(string.Join(" ", line));
            //    }

            //    return lines;
            //}


            private static IEnumerable<string> GroupTexts(List<ProcessedItems> items)
            {
                double yTolerance = 10;
                var lines = new List<string>();
                var currentLineItems = new List<ProcessedItems>();
                double lastY = 0;

                foreach (var item in items)
                {
                    if (lastY < 0 || Math.Abs(item.Y - lastY) <= yTolerance)
                    {
                        currentLineItems.Add(item);
                    }
                    else
                    {
                        if (currentLineItems.Count > 0)
                        {
                            lines.Add(CreateLine(currentLineItems));
                            currentLineItems.Clear();
                        }
                        currentLineItems.Add(item);
                    }
                    lastY = item.Y;
                }

                if (currentLineItems.Count > 0)
                {
                    lines.Add(CreateLine(currentLineItems));
                }

                return lines;
            }

            private static string CreateLine(List<ProcessedItems> items)
            {
                var sortedItems = items.OrderBy(item => item.X).ToList();
                return string.Join(" ", sortedItems.Select(item => item.Text));
            }
        }
    }
}
