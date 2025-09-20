using System.Text;

namespace Vigilance.Codegen;

public static class StringBuilderExtensions
{
    public static void BeginRegion(this StringBuilder sb, string region)
    {
        sb.AppendLine(
            $"""
                #region {region}

            """
        );
    }

    public static void EndRegion(this StringBuilder sb)
    {
        sb.AppendLine(
            """
                #endregion

            """
        );
    }
}
