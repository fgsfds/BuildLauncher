namespace Core.All.Helpers;

public enum ComparisonOperatorEnum
{
    Equals,
    GreaterThan,
    GreaterOrEquals,
    LessThan,
    LessOrEquals
}


public static class VersionComparer
{
    public static bool Compare(string? v1, string? v2)
    {
        if (string.IsNullOrWhiteSpace(v1) || string.IsNullOrWhiteSpace(v2))
        {
            return true;
        }

        var s2 = v2.AsSpan();
        ComparisonOperatorEnum comparisonOperator;

        if (s2.StartsWith("=="))
        {
            s2 = s2[2..];
            comparisonOperator = ComparisonOperatorEnum.Equals;
        }
        else if (s2.StartsWith(">="))
        {
            s2 = s2[2..];
            comparisonOperator = ComparisonOperatorEnum.GreaterOrEquals;
        }
        else if (s2.StartsWith("<="))
        {
            s2 = s2[2..];
            comparisonOperator = ComparisonOperatorEnum.LessOrEquals;
        }
        else if (s2[0] == '>')
        {
            s2 = s2[1..];
            comparisonOperator = ComparisonOperatorEnum.GreaterThan;
        }
        else if (s2[0] == '<')
        {
            s2 = s2[1..];
            comparisonOperator = ComparisonOperatorEnum.LessThan;
        }
        else
        {
            comparisonOperator = ComparisonOperatorEnum.Equals;
        }

        return InternalCompare(v1.AsSpan(), s2, comparisonOperator);
    }

    public static bool Compare(string? v1, string? v2, ComparisonOperatorEnum op)
    {
        if (v1 is null || v2 is null)
        {
            return true;
        }

        return InternalCompare(v1.AsSpan(), v2.AsSpan(), op);
    }


    private static bool InternalCompare(ReadOnlySpan<char> v1, ReadOnlySpan<char> v2, ComparisonOperatorEnum op)
    {
        var result = CompareVersions(v1, v2);

        return op switch
        {
            ComparisonOperatorEnum.Equals => result == 0,
            ComparisonOperatorEnum.GreaterOrEquals => result >= 0,
            ComparisonOperatorEnum.LessOrEquals => result <= 0,
            ComparisonOperatorEnum.GreaterThan => result > 0,
            ComparisonOperatorEnum.LessThan => result < 0,
            _ => throw new NotSupportedException()
        };
    }

    public static int CompareVersions(ReadOnlySpan<char> v1, ReadOnlySpan<char> v2)
    {
        var dash1 = v1.IndexOf('-');
        var dash2 = v2.IndexOf('-');

        var num1 = dash1 >= 0 ? v1[..dash1] : v1;
        var num2 = dash2 >= 0 ? v2[..dash2] : v2;

        while (num1.Length > 0 || num2.Length > 0)
        {
            var dot1 = num1.IndexOf('.');
            var dot2 = num2.IndexOf('.');

            var seg1 = dot1 >= 0 ? num1[..dot1] : num1;
            var seg2 = dot2 >= 0 ? num2[..dot2] : num2;

            int? ToInt(ReadOnlySpan<char> s) => s.Length == 0 ? 0 : (int.TryParse(s, out var v) ? v : null);

            var n1 = ToInt(seg1);
            var n2 = ToInt(seg2);

            if (n1.HasValue && n2.HasValue)
            {
                if (n1.Value != n2.Value)
                    return n1.Value.CompareTo(n2.Value);
            }
            else
            {
                var result = seg1.SequenceCompareTo(seg2);

                if (result != 0)
                    return result;
            }

            num1 = dot1 >= 0 ? num1[(dot1 + 1)..] : [];
            num2 = dot2 >= 0 ? num2[(dot2 + 1)..] : [];
        }

        if (dash1 < 0 && dash2 < 0)
        {
            return 0;
        }

        if (dash1 < 0)
        {
            return 1;
        }

        if (dash2 < 0)
        {
            return -1;
        }

        return v1[(dash1 + 1)..].SequenceCompareTo(v2[(dash2 + 1)..]);
    }
}
