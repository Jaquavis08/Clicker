//using System;
//using System.Collections.Generic;
//using UnityEngine;

//public static class NumberFormatter
//{
//    private static readonly Dictionary<int, string> units = new Dictionary<int, string>
//    {
//        {0, ""}, {1, "K"}, {2, "M"}, {3, "B"}, {4, "T"}, {5, "Qa"}, {6, "Qi"},
//        {7, "Sx"}, {8, "Sp"}, {9, "Oc"}, {10, "No"}, {11, "Dc"}, {12, "Ud"},
//        {13, "Dd"}, {14, "Td"}, {15, "QaD"}, {16, "QiD"}, {17, "SxD"}, {18, "SpD"},
//        {19, "OcD"}, {20, "NoD"}, {21, "Vg"}, {22, "Uv"}, {23, "Dv"}, {24, "Tv"},
//        {25, "QaV"}, {26, "QiV"}, {27, "SxV"}, {28, "SpV"}, {29, "Ov"}, {30, "Nv"}, {31, "Tg"}
//    };

//    private static readonly int charA = Convert.ToInt32('a');

//    public static string Format(double value)
//    {
//        if (double.IsNaN(value) || value <= 0) return "0";
//        if (double.IsInfinity(value)) return "∞";

//        // prevent math errors at extreme ranges
//        value = Math.Min(value, 1e308);

//        // compute tier safely
//        int n = 0;
//        try
//        {
//            n = (int)Math.Floor(Math.Log(value, 1000));
//        }
//        catch
//        {
//            n = units.Count - 1;
//        }

//        n = Math.Max(0, n);
//        double m = value / Math.Pow(1000, n);

//        string unit;
//        if (units.TryGetValue(n, out unit))
//        {
//            return m.ToString("0.##") + unit;
//        }

//        // fallback suffix generator (aa, ab, ac...)
//        int unitInt = n - units.Count;
//        int firstUnit = unitInt / 26;
//        int secondUnit = unitInt % 26;
//        unit = $"{(char)(charA + firstUnit)}{(char)(charA + secondUnit)}";

//        return m.ToString("0.##") + unit;
//    }
//}




using System;
using System.Collections.Generic;
using UnityEngine;

public static class NumberFormatter
{
    public enum FormatMode
    {
        Suffix,      // K, M, B, T, aa, bb...
        Scientific   // e+ notation
    }

    private static FormatMode currentMode = FormatMode.Suffix;

    // Optional: Save player preference (so it remembers on restart)
    private const string PlayerPrefsKey = "NumberFormatMode";

    private static readonly string[] suffixes = {
        "", "K", "M", "B", "T", "Qa", "Qi",
        "Sx", "Sp", "Oc", "No", "Dc", "Ud",
        "Dd", "Td", "QaD", "QiD", "SxD", "SpD",
        "OcD", "NoD", "Vg", "Uv", "Dv", "Tv",
        "QaV", "QiV", "SxV", "SpV", "Ov", "Nv", "Tg"
    };

    static NumberFormatter()
    {
        if (PlayerPrefs.HasKey(PlayerPrefsKey))
            currentMode = (FormatMode)PlayerPrefs.GetInt(PlayerPrefsKey);
    }

    public static void SetFormatMode(FormatMode mode)
    {
        currentMode = mode;
        PlayerPrefs.SetInt(PlayerPrefsKey, (int)mode);
        PlayerPrefs.Save();
    }

    public static FormatMode GetFormatMode() => currentMode;

    public static string Format(double number)
    {
        if (double.IsNaN(number) || double.IsInfinity(number))
            return "∞";

        switch (currentMode)
        {
            case FormatMode.Scientific:
                return FormatScientific(number);
            case FormatMode.Suffix:
            default:
                return FormatSuffix(number);
        }
    }

    private static string FormatSuffix(double number)
    {
        bool negative = number < 0;
        number = Math.Abs(number);

        // Small numbers: show normally
        if (number < 1000d)
            return (negative ? "-" : "") + number.ToString("0.##");

        // compute magnitude (thousands grouping)
        // magnitude = floor(log10(number) / 3)
        double log10 = Math.Log10(number);
        int magnitude = (int)Math.Floor(log10 / 3.0);

        // If magnitude fits inside our fixed suffix array, use it directly.
        if (magnitude < suffixes.Length)
        {
            double mantissa = SafeMantissaFromLog(log10, magnitude);
            return (negative ? "-" : "") + mantissa.ToString("0.##") + suffixes[magnitude];
        }

        // For magnitudes beyond fixed list, generate an extended suffix.
        // Example: after "Tg" -> "aa", "ab", ... then "ba", etc.
        int extendedIndex = magnitude - (suffixes.Length - 1); // 1-based beyond last fixed suffix
        string extSuffix = GenerateExtendedSuffix(extendedIndex);

        double mant = SafeMantissaFromLog(log10, magnitude);
        // If mantissa calculation failed (NaN/Inf) fallback to scientific
        if (double.IsNaN(mant) || double.IsInfinity(mant))
            return FormatScientific(negative ? -number : number);

        return (negative ? "-" : "") + mant.ToString("0.##") + extSuffix;
    }

    // Compute mantissa safely using logs:
    // mantissa = 10^(log10(number) - magnitude*3)
    private static double SafeMantissaFromLog(double log10Number, int magnitude)
    {
        double mantissaLog = log10Number - magnitude * 3.0;
        double mantissa = Math.Pow(10.0, mantissaLog);

        // Keep mantissa in a usable range (1 <= mantissa < 1000)
        // If it's out-of-range for some reason, attempt fallback normalization.
        if (double.IsNaN(mantissa) || double.IsInfinity(mantissa) || mantissa <= 0.0)
        {
            try
            {
                // Try computing mantissa using safe division (avoiding overflow)
                double scale = Math.Pow(1000.0, Math.Min(magnitude, 308 / 3)); // limit to avoid Inf
                mantissa = Math.Pow(10.0, log10Number) / scale;

                // If still invalid, just default to 1
                if (double.IsNaN(mantissa) || double.IsInfinity(mantissa))
                    mantissa = 1.0;
            }
            catch
            {
                mantissa = 1.0;
            }
        }

        return mantissa;
    }

    // Generate extended suffixes in base-26 like aa, ab, ac, ... ba, bb ... etc.
    // extendedIndex is 1-based (1 => "aa", 2 => "ab")
    private static string GenerateExtendedSuffix(int extendedIndex)
    {
        if (extendedIndex <= 0) extendedIndex = 1;

        // We'll produce lowercase two-or-more-letter suffixes:
        // Convert extendedIndex-1 to base-26 and map 0->'a', 1->'b', ..., 25->'z'
        extendedIndex -= 1; // make it zero-based
        string result = "";
        int baseSize = 26;

        // create at least two letters so "aa" is first
        do
        {
            int remainder = extendedIndex % baseSize;
            result = (char)('a' + remainder) + result;
            extendedIndex = (extendedIndex / baseSize) - 1;
        } while (extendedIndex >= 0);

        // ensure minimum length 2 (makes ranges clearer)
        if (result.Length < 2)
            result = result.PadLeft(2, 'a');

        return result;
    }

    private static string FormatScientific(double number)
    {
        return number.ToString("0.###e+0");
    }
}
