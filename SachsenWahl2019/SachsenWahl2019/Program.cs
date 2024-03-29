﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Annytab.Stemmer;

namespace SachsenWahl2019
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var whitespaceRegex = new Regex(@"\s");
            var stopWords = File.ReadAllLines(@"C:\Projects\SachsenWahl2019\stopwords.txt");

            var content = File.ReadAllText(@"C:\Projects\SachsenWahl2019\Manifestos\Sachsen-SPD.MD");
            content = content.Replace("-\n", "").Replace("-\r", "").Replace("\n", " ").Replace("\r", " ");
            content = whitespaceRegex.Replace(content, " ");
            var tokenized = new List<string>(
                content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(x => new string(x.Where(char.IsLetterOrDigit).ToArray())).Where(x => !string.IsNullOrEmpty(x)));
            tokenized = tokenized.Where(x => !stopWords.Contains(x, StringComparer.InvariantCultureIgnoreCase)).ToList();

            var counts = new Dictionary<string, int>();
            foreach (var token in tokenized)
            {
                if (counts.TryGetValue(token, out int count))
                {
                    count++;
                    counts[token] = count;
                }
                else
                    counts.Add(token, 1);
            }

            var orderedCounts = counts.Where(c => char.IsUpper(c.Key.First())).Select(c => c.Value).OrderByDescending(c => c).ToArray();
            var most20Number = orderedCounts.Take(20).Last();
            var mostCounts = counts.Where(c => c.Value >= most20Number && char.IsUpper(c.Key.First())).OrderByDescending(c => c.Value).ToList();
            var germanStemmer = new GermanStemmer();
            for (var index = mostCounts.Count - 1; index > 0; index--)
            {
                for (var innerIndex = index - 1; innerIndex >= 0; innerIndex--)
                {
                    if (germanStemmer.GetSteamWord(mostCounts[index].Key) == germanStemmer.GetSteamWord(mostCounts[innerIndex].Key))
                    {
                        mostCounts[innerIndex] = new KeyValuePair<string, int>(mostCounts[innerIndex].Key, mostCounts[innerIndex].Value + mostCounts[index].Value);
                        mostCounts.RemoveAt(index);
                        break;
                    }
                }
            }

            mostCounts = mostCounts.OrderByDescending(c => c.Value).ToList();

            var printed = string.Join("\n", mostCounts.Select(m => m.Value + " " + m.Key));

            Console.WriteLine(printed);

            Console.ReadKey();
        }
    }
}
