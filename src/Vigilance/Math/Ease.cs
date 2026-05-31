// ReSharper disable CompareOfFloatsByEqualityOperator

namespace Vigilance.Math;

/**
 * https://easings.net/
 */
public static class Ease
{
    private const float C1 = 1.70158f;
    private const float C2 = C1 * 1.525f;
    private const float C3 = C1 + 1f;
    private const float C4 = 2f * MathF.PI / 3f;
    private const float C5 = 2f * MathF.PI / 4.5f;

    public static float Linear(float value)
    {
        return value;
    }

    public static float InQuad(float value)
    {
        return value * value;
    }

    public static float OutQuad(float value)
    {
        return 1f - (1f - value) * (1f - value);
    }

    public static float InOutQuad(float value)
    {
        return value < 0.5f ? 2f * value * value : 1f - MathF.Pow(-2f * value + 2f, 2f) / 2f;
    }

    public static float InCubic(float value)
    {
        return value * value * value;
    }

    public static float OutCubic(float value)
    {
        return 1f - MathF.Pow(1f - value, 3f);
    }

    public static float InOutCubic(float value)
    {
        return value < 0.5f ? 4f * value * value * value : 1f - MathF.Pow(-2f * value + 2f, 3f) / 2f;
    }

    public static float InQuart(float value)
    {
        return value * value * value * value;
    }

    public static float OutQuart(float value)
    {
        return 1f - MathF.Pow(1f - value, 4f);
    }

    public static float InOutQuart(float value)
    {
        return value < 0.5f ? 8f * value * value * value * value : 1f - MathF.Pow(-2f * value + 2f, 4f) / 2f;
    }

    public static float InQuint(float value)
    {
        return value * value * value * value * value;
    }

    public static float OutQuint(float value)
    {
        return 1f - MathF.Pow(1f - value, 5f);
    }

    public static float InOutQuint(float value)
    {
        return value < 0.5f ? 16f * value * value * value * value * value : 1f - MathF.Pow(-2f * value + 2f, 5f) / 2f;
    }

    public static float InSine(float value)
    {
        return 1f - MathF.Cos(value * MathF.PI / 2f);
    }

    public static float OutSine(float value)
    {
        return MathF.Sin(value * MathF.PI / 2f);
    }

    public static float InOutSine(float value)
    {
        return -(MathF.Cos(MathF.PI * value) - 1f) / 2f;
    }

    public static float InExpo(float value)
    {
        return value == 0f ? 0f : MathF.Pow(2f, 10f * value - 10f);
    }

    public static float OutExpo(float value)
    {
        return value == 1f ? 1f : 1f - MathF.Pow(2f, -10f * value);
    }

    public static float InOutExpo(float value)
    {
        return value switch
        {
            0f => 0f,
            1f => 1f,
            _ => value < 0.5f ? MathF.Pow(2f, 20f * value - 10f) / 2f : (2f - MathF.Pow(2f, -20f * value + 10f)) / 2f,
        };
    }

    public static float InCirc(float value)
    {
        return 1f - MathF.Sqrt(1f - MathF.Pow(value, 2f));
    }

    public static float OutCirc(float value)
    {
        return MathF.Sqrt(1f - MathF.Pow(value - 1f, 2f));
    }

    public static float InOutCirc(float value)
    {
        return value < 0.5f
            ? (1f - MathF.Sqrt(1f - MathF.Pow(2f * value, 2f))) / 2f
            : (MathF.Sqrt(1f - MathF.Pow(-2f * value + 2f, 2f)) + 1f) / 2f;
    }

    public static float InBack(float value)
    {
        return C3 * value * value * value - C1 * value * value;
    }

    public static float OutBack(float value)
    {
        return 1f + C3 * MathF.Pow(value - 1f, 3f) + C1 * MathF.Pow(value - 1f, 2f);
    }

    public static float InOutBack(float value)
    {
        return value < 0.5f
            ? MathF.Pow(2f * value, 2f) * ((C2 + 1f) * 2f * value - C2) / 2f
            : (MathF.Pow(2f * value - 2f, 2f) * ((C2 + 1f) * (value * 2f - 2f) + C2) + 2f) / 2f;
    }

    public static float InElastic(float value)
    {
        return value switch
        {
            0f => 0f,
            1f => 1f,
            _ => -MathF.Pow(2f, 10f * value - 10f) * MathF.Sin((value * 10f - 10.75f) * C4),
        };
    }

    public static float OutElastic(float value)
    {
        return value switch
        {
            0f => 0f,
            1f => 1f,
            _ => MathF.Pow(2f, -10f * value) * MathF.Sin((value * 10f - 0.75f) * C4) + 1f,
        };
    }

    public static float InOutElastic(float value)
    {
        return value switch
        {
            0f => 0f,
            1f => 1f,
            _ => value < 0.5f
                ? -(MathF.Pow(2f, 20f * value - 10f) * MathF.Sin((20f * value - 11.125f) * C5)) / 2f
                : MathF.Pow(2f, -20f * value + 10f) * MathF.Sin((20f * value - 11.125f) * C5) / 2f + 1f,
        };
    }

    public static float InBounce(float value)
    {
        return 1f - OutBounce(1f - value);
    }

    public static float OutBounce(float value)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;
        switch (value)
        {
            case < 1f / d1:
                return n1 * value * value;
            case < 2f / d1:
                value -= 1.5f / d1;
                return n1 * value * value + 0.75f;
            case < 2.5f / d1:
                value -= 2.25f / d1;
                return n1 * value * value + 0.9375f;
            default:
                value -= 2.625f / d1;
                return n1 * value * value + 0.984375f;
        }
    }

    public static float InOutBounce(float value)
    {
        return value < 0.5f ? (1f - OutBounce(1f - 2f * value)) / 2f : (1f + OutBounce(2f * value - 1f)) / 2f;
    }
}
