using System.Text;

namespace Vigilance.Codegen.Helpers;

public static class RegionHelper
{
    extension(StringBuilder sb)
    {
        public void BeginRegion(string region)
        {
            sb.AppendLine(
                $"""
                    #region {region}

                """
            );
        }

        public void EndRegion()
        {
            sb.AppendLine(
                """
                    #endregion

                """
            );
        }
    }
}
