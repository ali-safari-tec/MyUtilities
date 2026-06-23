
// We shoulde not use any NuGet for this scope.

// ************************************************************* Explain This Class *************************************************************
// **                                                                                                                                          **
// **     This class create for : This class is for check any text in any text or list and return         **
// **                                                                                                                                          **
// **     Public Methodes :                                                                                                                    **
// **                        => BuildJsonFromText : Get two string input 1. string value from user                                             **
// **                                                                    2. string key from user                                               **
// **                                               Get key and text from user to make JSON text. (you can use (.) in key for multi tag)       **
// **                                                                                                                                          **
// **                        => FlattenJsonToText : Get JSON text and convert all data to simple text.                                         **
// **                                               Use private helper method ExtractStringValuesRecursive for extract all data from JSON      **
// **                                               text.                                                                                      **
// **                        => GetJsonValuesByKey : Get two string input 1. JSON text                                                         **
// **                                                                     2. string key from user.                                             **
// **                                                Get JSON text and key.                                                                    **
// **                                                Search all tag and find that tag user write in key and return value to string.            **
// **                                                Use private helper method FindValuesByKeyRecursive for find that exact tag and return     **
// **                                                value to string.                                                                          **
// **     Private Methodes :                                                                                                                   **
// **                        => ExtractStringValuesRecursive : Get three string input 1. That Parse JSON text from user.                       **
// **                                                                                 2. String builder for make string from JSON value.       **
// **                                                                                 3. Indent. (white space for make tree view)              **
// **                                               Check 3 mod => 1. node is object  2. node is arry  3. node is simple type.                 **
// **                                               in every mod extract JSON value to string.                                                 **
// **                                                                                                                                          **
// **                        => FindValuesByKeyRecursive : Get four string input 1. That Parse JSON text from user.                            **
// **                                                                            2. String key from user.                                      **
// **                                                                            3. String builder for make string from JSON value.            **
// **                                                                            4. Boolian check root. (for tree view)                        **
// **                                                      Check 2 mod => 1. node is object  2. node is arry                                   **
// **                                                      in every mod search tags that similar to key and return their value.                **
// **                                                                                                                                          **
// **********************************************************************************************************************************************  

namespace MyUtilities
{
    public static class SearchHelper
    {
        /// <summary>
        /// Checks if a target string contains a preferred text, with optional case sensitivity.
        /// </summary>
        /// <param name="preferredText">The text to search for.</param>
        /// <param name="targetText">The string to search within.</param>
        /// <param name="ignoreCase">Whether to ignore case during comparison. Defaults to true.</param>
        /// <returns>True if the preferredText is found in targetText, false otherwise.</returns>
        public static bool ContainsText(string preferredText, string targetText, bool ignoreCase = true)
        {
            if (string.IsNullOrEmpty(preferredText) || string.IsNullOrEmpty(targetText))
                return false;

            StringComparison comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return targetText.Contains(preferredText, comparison);
        }

        /// <summary>
        /// Checks if any string within a list of target strings contains the preferred text, with optional case sensitivity.
        /// </summary>
        /// <param name="preferredText">The text to search for.</param>
        /// <param name="targetTexts">The list of strings to search within.</param>
        /// <param name="ignoreCase">Whether to ignore case during comparison. Defaults to true.</param>
        /// <returns>True if the preferredText is found in at least one of the target strings, false otherwise.</returns>
        public static bool ContainsText(string preferredText, List<string> targetTexts, bool ignoreCase = true)
        {
            if (string.IsNullOrEmpty(preferredText) || targetTexts == null || targetTexts.Count == 0)
                return false;

            StringComparison comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            foreach (var text in targetTexts)
            {
                if (!string.IsNullOrEmpty(text) && text.Contains(preferredText, comparison))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Finds all occurrences of preferred text within a target string.
        /// This version is a bit misleading as it only returns the preferredText if found, or a message.
        /// Consider using ContainsText for a boolean result or refactoring if specific substring extraction is needed.
        /// </summary>
        /// <param name="preferredText">The text to search for.</param>
        /// <param name="targetText">The string to search within.</param>
        /// <param name="ignoreCase">Whether to ignore case during comparison. Defaults to true.</param>
        /// <returns>The preferredText if found, or "Not found any answer!".</returns>
        public static string FindAllContainsText(string preferredText, string targetText, bool ignoreCase = true)
        {
            if (string.IsNullOrEmpty(preferredText) || string.IsNullOrEmpty(targetText))
                return "Your input text or target text was null.";


            StringComparison comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return targetText.Contains(preferredText, comparison) ? preferredText : "Not found any answer!";
        }

        /// <summary>
        /// Finds all strings within a list that contain the preferred text.
        /// </summary>
        /// <param name="preferredText">The text to search for.</param>
        /// <param name="targetTexts">The list of strings to search within.</param>
        /// <param name="ignoreCase">Whether to ignore case during comparison. Defaults to true.</param>
        /// <returns>A list of strings from targetTexts that contain the preferredText.</returns>
        public static List<string> FindAllContainsText(string preferredText, List<string> targetTexts, bool ignoreCase = true)
        {
            if (string.IsNullOrEmpty(preferredText) || targetTexts.Count == 0 || targetTexts is null)
                return [];

            var matches = new List<string>();
            StringComparison comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            foreach (var text in targetTexts)
            {
                if (!string.IsNullOrEmpty(text) && text.Contains(preferredText, comparison))
                    matches.Add(text);
            }
            return matches;
        }
    }
}
