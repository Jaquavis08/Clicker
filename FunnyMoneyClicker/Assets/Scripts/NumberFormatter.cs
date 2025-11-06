using BreakInfinity;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class NumberFormatter
{
    public enum FormatMode
    {
        Suffix,
        Scientific
    }

    private static FormatMode currentMode = FormatMode.Suffix;

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

    public static string Format(BigDouble number)
    {
        if (BigDouble.IsNaN(number) || BigDouble.IsInfinity(number))
            return "∞";

        switch (currentMode)
        {
            case FormatMode.Scientific:
                return FormatScientific(number.ToDouble());
            case FormatMode.Suffix:
            default:
                return FormatSuffix(number.ToDouble());
        }
    }

    private static string FormatSuffix(double number)
    {
        bool negative = number < 0;
        number = Math.Abs(number);

        if (number < 1000d)
            return (negative ? "-" : "") + number.ToString("0.##");

        double log10 = Math.Log10(number);
        int magnitude = (int)Math.Floor(log10 / 3.0);

        if (magnitude < suffixes.Length)
        {
            double mantissa = SafeMantissaFromLog(log10, magnitude);
            return (negative ? "-" : "") + mantissa.ToString("0.##") + suffixes[magnitude];
        }

        int extendedIndex = magnitude - (suffixes.Length - 1);
        string extSuffix = GenerateExtendedSuffix(extendedIndex);

        double mant = SafeMantissaFromLog(log10, magnitude);

        if (double.IsNaN(mant) || double.IsInfinity(mant))
            return FormatScientific(negative ? -number : number);

        return (negative ? "-" : "") + mant.ToString("0.##") + extSuffix;
    }

    private static double SafeMantissaFromLog(double log10Number, int magnitude)
    {
        double mantissaLog = log10Number - magnitude * 3.0;
        double mantissa = Math.Pow(10.0, mantissaLog);

        if (double.IsNaN(mantissa) || double.IsInfinity(mantissa) || mantissa <= 0.0)
        {
            try
            {
                double scale = Math.Pow(1000.0, Math.Min(magnitude, 308 / 3));
                mantissa = Math.Pow(10.0, log10Number) / scale;

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

    private static string GenerateExtendedSuffix(int extendedIndex)
    {
        if (extendedIndex <= 0) extendedIndex = 1;

        extendedIndex -= 1;
        string result = "";
        int baseSize = 26;

        do
        {
            int remainder = extendedIndex % baseSize;
            result = (char)('a' + remainder) + result;
            extendedIndex = (extendedIndex / baseSize) - 1;
        } while (extendedIndex >= 0);

        if (result.Length < 2)
            result = result.PadLeft(2, 'a');

        return result;
    }

    private static string FormatScientific(double number)
    {
        return number.ToString("0.#e+0");
    }
}
