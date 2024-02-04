using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using DTO;
using Newtonsoft.Json;

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

                int RoundToRange(double number, int range)
                {
                    return (int)(Math.Round(number / range) * range);
                }

                const int yRange = 30;

                var processedItems = items
                                        .Where(item => item.BoundingPoly != null && item.BoundingPoly.Vertices != null)
                                        .SelectMany(item => item.Description.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(desc => new
                                        {
                                            Text = desc.Trim(),
                                            Y = RoundToRange(item.BoundingPoly.Vertices.Average(v => v.Y), yRange),
                                            X = item.BoundingPoly.Vertices.Min(v => v.X)
                                        }))
                                        .ToList();

                var groupedTexts = processedItems
                    .GroupBy(item => item.Y)
                    .OrderBy(group => group.Key)
                    .ToList();


                //For döngüsü ile de yapılabilir.
                int lineNumber = 1;
                foreach (var group in groupedTexts)
                {
                    var orderedGroup = group.OrderBy(item => item.X).ToList();
                    string line = string.Join(" ", orderedGroup.Select(item => item.Text));
                    Console.WriteLine($"{lineNumber.ToString().PadLeft(4)} {line}");
                    lineNumber++;
                }

            }
            catch (IOException ex)
            {
                Console.WriteLine("Error reading the JSON file:");
                Console.WriteLine(ex.Message);
                return;
            }
        }
    }
}
