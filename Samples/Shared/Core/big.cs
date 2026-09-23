using System;
using System.Diagnostics;
using System.Globalization;

[Serializable]
public struct big
{
    private const int MantissaFractionDigits = 7;
    private const int MantissaDigits = MantissaFractionDigits + 1;
    private const double MantissaScale = 10000000d;
    private const int InsignificantExponentGap = -MantissaFractionDigits;
    private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

    // Example: 3.2598716 * 10e24
    public double v; // 1 <= |v| < 10, 8 significant digits
    public sbyte e; // 10^-128 ~ 10^127

    public big(double v, sbyte e)
    {
        this = CreateNormalized(v, e);
    }

    public big(int num) : this(num.ToString(InvariantCulture))
    {
    }

    public big(float num) : this((double)num, (sbyte)0)
    {
    }

    public big(double num) : this(num, (sbyte)0)
    {
    }

    public big(long num) : this(num.ToString(InvariantCulture))
    {
    }

    public big(string num)
    {
        this = ParseFromString(num);
    }

    private static big CreateNormalized(double value, int exponent)
    {
        Normalize(value, exponent, out double normalizedV, out sbyte normalizedE);
        return new big
        {
            v = normalizedV,
            e = normalizedE
        };
    }

    private static void Normalize(double value, int exponent, out double normalizedV, out sbyte normalizedE)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new ArgumentException("big cannot represent NaN or Infinity.");
        }

        if (value == 0d)
        {
            normalizedV = 0d;
            normalizedE = 0;
            return;
        }

        double absValue = Math.Abs(value);
        int normalizedExponent = exponent;

        while (absValue < 1d)
        {
            absValue *= 10d;
            normalizedExponent -= 1;
        }

        while (absValue >= 10d)
        {
            absValue /= 10d;
            normalizedExponent += 1;
        }

        absValue = TruncateMantissa(absValue);

        while (absValue > 0d && absValue < 1d)
        {
            absValue *= 10d;
            normalizedExponent -= 1;
            absValue = TruncateMantissa(absValue);
        }

        while (absValue >= 10d)
        {
            absValue /= 10d;
            normalizedExponent += 1;
            absValue = TruncateMantissa(absValue);
        }

        if (absValue == 0d || normalizedExponent < sbyte.MinValue)
        {
            normalizedV = 0d;
            normalizedE = 0;
            return;
        }

        if (normalizedExponent > sbyte.MaxValue)
        {
            throw new OverflowException($"Exponent {normalizedExponent} is outside the supported big range.");
        }

        normalizedV = value < 0d ? -absValue : absValue;
        normalizedE = (sbyte)normalizedExponent;
    }

    private static big ParseFromString(string num)
    {
        if (string.IsNullOrWhiteSpace(num))
        {
            throw new ArgumentException("Input string should not be empty.");
        }

        num = num.Trim();

        int exponentSeparatorIndex = num.IndexOf('E');
        if (exponentSeparatorIndex < 0)
        {
            exponentSeparatorIndex = num.IndexOf('e');
        }

        if (exponentSeparatorIndex >= 0)
        {
            if (num.IndexOf('E', exponentSeparatorIndex + 1) >= 0 || num.IndexOf('e', exponentSeparatorIndex + 1) >= 0)
            {
                throw new FormatException($"Invalid big format: {num}");
            }

            string mantissaPart = num.Substring(0, exponentSeparatorIndex);
            string exponentPart = num.Substring(exponentSeparatorIndex + 1);
            if (exponentPart.Length == 0)
            {
                throw new FormatException($"Invalid big format: {num}");
            }

            ParsePlainString(mantissaPart, out double mantissa, out int mantissaExponent);
            int explicitExponent = int.Parse(exponentPart, NumberStyles.Integer, InvariantCulture);
            return CreateNormalized(mantissa, mantissaExponent + explicitExponent);
        }

        ParsePlainString(num, out double parsedMantissa, out int parsedExponent);
        return CreateNormalized(parsedMantissa, parsedExponent);
    }

    private static void ParsePlainString(string num, out double mantissa, out int exponent)
    {
        if (string.IsNullOrWhiteSpace(num))
        {
            throw new FormatException("Input string should not be empty.");
        }

        num = num.Trim();

        bool isNegative = false;
        if (num[0] == '+' || num[0] == '-')
        {
            isNegative = num[0] == '-';
            num = num.Substring(1);
        }

        if (num.Length == 0)
        {
            throw new FormatException("Input string should contain digits.");
        }

        string[] parts = num.Split('.');
        if (parts.Length > 2)
        {
            throw new FormatException($"Invalid big format: {num}");
        }

        string integerPart = parts[0];
        string decimalPart = parts.Length == 2 ? parts[1] : string.Empty;

        if (integerPart.Length == 0 && decimalPart.Length == 0)
        {
            throw new FormatException($"Invalid big format: {num}");
        }

        if (!IsDigitsOnly(integerPart) || !IsDigitsOnly(decimalPart))
        {
            throw new FormatException($"Invalid big format: {num}");
        }

        integerPart = integerPart.TrimStart('0');

        string significantDigits;
        if (integerPart.Length > 0)
        {
            exponent = integerPart.Length - 1;
            significantDigits = integerPart + decimalPart;
        }
        else
        {
            int firstNonZeroIndex = FindFirstNonZero(decimalPart);
            if (firstNonZeroIndex < 0)
            {
                mantissa = 0d;
                exponent = 0;
                return;
            }

            exponent = -(firstNonZeroIndex + 1);
            significantDigits = decimalPart.Substring(firstNonZeroIndex);
        }

        if (significantDigits.Length > MantissaDigits)
        {
            significantDigits = significantDigits.Substring(0, MantissaDigits);
        }

        string mantissaString = significantDigits[0].ToString();
        if (significantDigits.Length > 1)
        {
            mantissaString += "." + significantDigits.Substring(1);
        }

        mantissa = double.Parse(mantissaString, NumberStyles.Float, InvariantCulture);
        if (isNegative)
        {
            mantissa *= -1d;
        }
    }

    private static bool IsDigitsOnly(string value)
    {
        for (int i = 0; i < value.Length; i++)
        {
            if (!char.IsDigit(value[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static int FindFirstNonZero(string value)
    {
        for (int i = 0; i < value.Length; i++)
        {
            if (value[i] != '0')
            {
                return i;
            }
        }

        return -1;
    }

    private static double TruncateMantissa(double value)
    {
        return Math.Truncate(value * MantissaScale) / MantissaScale;
    }

    private static double CanonicalMantissa(double value)
    {
        if (value == 0d)
        {
            return 0d;
        }

        double absValue = TruncateMantissa(Math.Abs(value));
        return value < 0d ? -absValue : absValue;
    }

    private static int Compare(big a, big b)
    {
        if (a.v == 0d)
        {
            return b.v == 0d ? 0 : (b.v > 0d ? -1 : 1);
        }

        if (b.v == 0d)
        {
            return a.v > 0d ? 1 : -1;
        }

        if (a.v < 0d && b.v >= 0d)
        {
            return -1;
        }

        if (a.v >= 0d && b.v < 0d)
        {
            return 1;
        }

        if (a.e != b.e)
        {
            if (a.v >= 0d)
            {
                return a.e < b.e ? -1 : 1;
            }

            return a.e > b.e ? -1 : 1;
        }

        double left = CanonicalMantissa(a.v);
        double right = CanonicalMantissa(b.v);

        if (left < right)
        {
            return -1;
        }

        if (left > right)
        {
            return 1;
        }

        return 0;
    }

    public static big operator +(big a, big b)
    {
        if (a.v == 0d)
        {
            return b;
        }

        if (b.v == 0d)
        {
            return a;
        }

        big primary = a.e >= b.e ? a : b;
        big secondary = a.e >= b.e ? b : a;
        int exponentDifference = secondary.e - primary.e;

        if (exponentDifference < InsignificantExponentGap)
        {
            return primary;
        }

        double combinedMantissa = primary.v + (secondary.v * Math.Pow(10d, exponentDifference));
        return CreateNormalized(combinedMantissa, primary.e);
    }

    public static big operator -(big value)
    {
        return CreateNormalized(-value.v, value.e);
    }

    public static big operator -(big a, big b)
    {
        return a + (-b);
    }

    public static big operator *(big a, big b)
    {
        if (a.v == 0d || b.v == 0d)
        {
            return CreateNormalized(0d, 0);
        }

        return CreateNormalized(a.v * b.v, a.e + b.e);
    }

    public static big operator /(big a, big b)
    {
        if (b.v == 0d)
        {
            throw new DivideByZeroException();
        }

        if (a.v == 0d)
        {
            return CreateNormalized(0d, 0);
        }

        return CreateNormalized(a.v / b.v, a.e - b.e);
    }

    public static bool operator <(big a, big b)
    {
        return Compare(a, b) < 0;
    }

    public static bool operator >(big a, big b)
    {
        return Compare(a, b) > 0;
    }

    public static bool operator <=(big a, big b)
    {
        return Compare(a, b) <= 0;
    }

    public static bool operator >=(big a, big b)
    {
        return Compare(a, b) >= 0;
    }

    public static bool operator ==(big a, big b)
    {
        return Compare(a, b) == 0;
    }

    public static bool operator !=(big a, big b)
    {
        return Compare(a, b) != 0;
    }

    public override readonly bool Equals(object obj)
    {
        if (!(obj is big))
        {
            return false;
        }

        return this == (big)obj;
    }

    public override int GetHashCode()
    {
        if (v == 0d)
        {
            return 0;
        }

        unchecked
        {
            int hash = 17;
            hash = (hash * 31) + CanonicalMantissa(v).GetHashCode();
            hash = (hash * 31) + e.GetHashCode();
            return hash;
        }
    }

    public override string ToString()
    {
        if (v == 0d)
        {
            return "0E0";
        }

        return v.ToString("0.0000000", InvariantCulture) + "E" + e.ToString(InvariantCulture);
    }

    public string ToPowerString(int? roundDigits = null)
    {
        if (this == 0) return "0";
        big n = new big(v, e);

        if (roundDigits.HasValue)
        {
            double val = n.v * Math.Pow(10, n.e);
            return Math.Round(val, roundDigits.Value).ToString($"F{roundDigits.Value}", InvariantCulture);
        }

        string first = "";
        string second = "";
        if (n.e > 31) { first = "구"; second = "양"; }
        else if (n.e > 27) { first = "양"; second = "자"; }
        else if (n.e > 23) { first = "자"; second = "해"; }
        else if (n.e > 19) { first = "해"; second = "경"; }
        else if (n.e > 15) { first = "경"; second = "조"; }
        else if (n.e > 11) { first = "조"; second = "억"; }
        else if (n.e > 7) { first = "억"; second = "만"; }
        else if (n.e > 3) { first = "만"; second = ""; }
        else if (n.e <= 3)
        {
            double val = Math.Round(n.v * Math.Pow(10, n.e));
            return ((long)val).ToString();
        }

        string vs = n.v.ToString("F7", InvariantCulture);
        vs = vs.Replace(".", "");
        if (vs.Length < 8)
        {
            vs += new string('0', 8 - vs.Length);
        }

        int displayNum = n.e % 4 + 1;
        string fv = vs.Substring(0, displayNum);
        string svTemp = vs.Substring(displayNum, 4);
        string sv = "";
        for (int i = 0; i < 4; i++)
        {
            if (svTemp[i] == '0') continue;
            sv = svTemp.Substring(i, 4 - i);
            break;
        }

        return $"{fv}{first} {sv}{second}";
    }

    private static readonly string[] ShortUnits = { "", "만", "억", "조", "경", "해", "자", "양", "구" };

    public string ToShortString(int? roundDigits = null)
    {
        if (roundDigits.HasValue && (roundDigits.Value < 0 || roundDigits.Value > 15))
            throw new ArgumentOutOfRangeException(nameof(roundDigits));

        big n = new big(v, e);
        if (n.e <= 3)
        {
            double value = n.v * Math.Pow(10, n.e);
            if (roundDigits.HasValue)
                return Math.Round(value, roundDigits.Value).ToString($"F{roundDigits.Value}", InvariantCulture);

            double roundedValue = Math.Round(value);
            if (roundedValue == 0) return "0";
            if (Math.Abs(roundedValue) < 10000)
                return roundedValue.ToString("0", InvariantCulture);
            return (roundedValue / 10000).ToString("0", InvariantCulture) + "만";
        }

        // 큰 수는 유효 숫자 3자리; 지원 단위를 넘어가면 지수로 표시
        int unit = n.e / 4;
        if (unit >= ShortUnits.Length)
            return ToShortScientific(n.v, n.e);

        double scaled = n.v * Math.Pow(10, n.e % 4);
        int digits = 2 - (int)Math.Floor(Math.Log10(Math.Abs(scaled)));
        double scale = Math.Pow(10, digits);
        double rounded = Math.Round(scaled * scale) / scale;
        if (Math.Abs(rounded) >= 10000)
        {
            rounded /= 10000;
            unit++;
        }
        if (unit >= ShortUnits.Length)
            return ToShortScientific(n.v, n.e);
        return rounded.ToString("0.##", InvariantCulture) + ShortUnits[unit];
    }

    private static string ToShortScientific(double mantissa, int exponent)
    {
        double rounded = Math.Round(mantissa, 2);
        if (Math.Abs(rounded) >= 10)
        {
            rounded /= 10;
            exponent++;
        }
        return rounded.ToString("0.##", InvariantCulture) + "E" + exponent.ToString(InvariantCulture);
    }

    public static implicit operator big(int v)
    {
        return new big(v);
    }

    public static implicit operator big(float v)
    {
        return new big(v);
    }

    public static implicit operator big(double v)
    {
        return new big(v);
    }

    public static implicit operator big(long v)
    {
        return new big(v);
    }

    public static implicit operator big(string v)
    {
        return new big(v);
    }

    public static explicit operator int(big p)
    {
        return checked((int)(p.v * Math.Pow(10, p.e)));
    }

    public static explicit operator float(big p)
    {
        return (float)(p.v * Math.Pow(10, p.e));
    }
}
