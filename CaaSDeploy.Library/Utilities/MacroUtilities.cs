using System.Text.RegularExpressions;

namespace DD.CBU.CaasDeploy.Library.Utilities
{
    /// <summary>
    /// Provides utilitiy methods for macros.
    /// </summary>
    public static class MacroUtilities
    {
        /// <summary>
        /// Checkes whether the replacement of the supplied token requires quotes.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="match">The match.</param>
        /// <returns>True if quotes are required, otherwise false.</returns>
        public static bool IsNested(string input, Match match)
        {
            if ((match.Index == 0) || (match.Index + match.Value.Length == input.Length))
            {
                return false;
            }

            if ((input[match.Index - 1] == '[') || (input[match.Index + match.Value.Length] == ']'))
            {
                return true;
            }

            return false;
        }
    }
}
