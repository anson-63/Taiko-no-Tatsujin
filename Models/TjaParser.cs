using System;
using System.Collections.Generic;
using System.IO;

namespace Taiko
{
    public class TjaNote
    {
        public double Time { get; set; }
        public int Type { get; set; } // 1: Don, 2: Kat, 3: Big Don, 4: Big Kat
    }

    public class TjaParser
    {
        public double Bpm { get; private set; } = 120;
        public double Offset { get; private set; } = 0;
        public List<TjaNote> Notes { get; private set; } = new List<TjaNote>();

        public void Parse(string filePath, string targetCourse = "oni")
        {
            Notes.Clear();
            if (!File.Exists(filePath)) return;

            var lines = File.ReadAllLines(filePath);
            bool inChart = false;
            string currentCourse = "";
            double currentTime = 0;
            double measureUpper = 4, measureLower = 4;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("//")) continue;

                if (trimmed.StartsWith("BPM:"))
                {
                    if (double.TryParse(trimmed.Substring(4).Trim(), out var b)) Bpm = b;
                }
                else if (trimmed.StartsWith("OFFSET:"))
                {
                    if (double.TryParse(trimmed.Substring(7).Trim(), out var o)) Offset = o;
                }
                else if (trimmed.StartsWith("COURSE:"))
                {
                    currentCourse = MapCourseValue(trimmed.Substring(7).Trim().ToLowerInvariant());
                }
                else if (trimmed.StartsWith("#START"))
                {
                    string effectiveCourse = string.IsNullOrEmpty(currentCourse) ? "oni" : currentCourse;
                    if (effectiveCourse.Equals(targetCourse, StringComparison.OrdinalIgnoreCase))
                    {
                        inChart = true;
                        currentTime = 0;
                    }
                }
                else if (trimmed.StartsWith("#END")) inChart = false;
                else if (inChart)
                {
                    if (trimmed.StartsWith("#MEASURE"))
                    {
                        var parts = trimmed.Substring(9).Trim().Split('/');
                        if (parts.Length == 2)
                        {
                            double.TryParse(parts[0], out measureUpper);
                            double.TryParse(parts[1], out measureLower);
                        }
                    }
                    else if (!trimmed.StartsWith("#"))
                    {
                        // Remove the comma at the end of measures if it exists
                        var measureData = trimmed.Replace(",", "");
                        double measureDuration = (measureUpper * 4.0 / measureLower) * (60.0 / Bpm);

                        if (measureData.Length > 0)
                        {
                            double interval = measureDuration / measureData.Length;
                            for (int i = 0; i < measureData.Length; i++)
                            {
                                int type = measureData[i] - '0';
                                // Handle types 1, 2, 3 (Big Don), and 4 (Big Kat)
                                if (type >= 1 && type <= 4)
                                {
                                    Notes.Add(new TjaNote { Time = currentTime + (i * interval), Type = type });
                                }
                            }
                        }
                        currentTime += measureDuration;
                    }
                }
            }
        }

        private string MapCourseValue(string value) => value switch
        {
            "0" or "easy" or "kantan" => "easy",
            "1" or "normal" or "futsuu" => "normal",
            "2" or "hard" or "muzukashii" => "hard",
            "3" or "oni" or "extreme" => "oni",
            "4" or "edit" or "ura" => "edit",
            _ => ""
        };
    }
}