using System;
using System.Collections.Generic;

public static class NumberFormatter
{
    private static readonly Dictionary<int, string> units = new Dictionary<int, string>
    {
        {0, ""}, {1, "K"}, {2, "M"}, {3, "B"}, {4, "T"}, {5, "Qa"}, {6, "Qi"},
        {7, "Sx"}, {8, "Sp"}, {9, "Oc"}, {10, "No"}, {11, "Dc"}, {12, "Ud"},
        {13, "Dd"}, {14, "Td"}, {15, "QaD"}, {16, "QiD"}, {17, "SxD"}, {18, "SpD"},
        {19, "OcD"}, {20, "NoD"}, {21, "Vg"}, {22, "Uv"}, {23, "Dv"}, {24, "Tv"},
        {25, "QaV"}, {26, "QiV"}, {27, "SxV"}, {28, "SpV"}, {29, "Ov"}, {30, "Nv"}, {31, "Tg"}
    };

    private static readonly int charA = Convert.ToInt32('a');

    public static string Format(double value)
    {
        //Handle all invalid or tiny values safely
        if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
            return "0";

        // Calculate magnitude safely
        int n = (int)Math.Floor(Math.Log(value, 1000));
        if (n < 0) n = 0; // Prevent negative indexes

        double m = value / Math.Pow(1000, n);

        string unit;
        if (units.ContainsKey(n))
        {
            unit = units[n];
        }
        else
        {
            // Handle overflow beyond predefined units
            int unitInt = n - units.Count;
            int firstUnit = unitInt / 26;
            int secondUnit = unitInt % 26;
            unit = Convert.ToChar(firstUnit + charA).ToString() + Convert.ToChar(secondUnit + charA).ToString();
        }

        return (Math.Floor(m * 100) / 100).ToString("0.##") + unit;
    }
}
