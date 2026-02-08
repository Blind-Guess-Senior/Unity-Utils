using System;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Extensions
{
    /// <summary>
    /// Provides extension methods for Strings.
    /// </summary>
    public static class StringExtensions
    {
        #region Fields

        /// <summary>
        /// Hash seed used for Fowler–Noll–Vo 1a hash function.
        /// </summary>
        /// <remarks>
        /// unchecked used for ignore overflow.
        /// </remarks>
        private const int HashSeed = unchecked((int)2166136261);

        /// <summary>
        /// Hash prime used for Fowler–Noll–Vo 1a hash function.
        /// </summary>
        private const int HashPrime = 16777619;

        #endregion

        #region Stable Hash Extensions

        // Algorithm Fowler–Noll–Vo 1a 

        /// <summary>
        /// Get stable hash code for a string.
        /// It's an extension method of string. It's an overload function for readonly span.
        /// </summary>
        /// <param name="span">The string's readonly span that will be hashed.</param>
        /// <returns>The hashcode of input string.</returns>
        public static int GetStableHash(this ReadOnlySpan<char> span)
        {
            var hash = HashSeed;
            for (var i = 0; i < span.Length; i++)
            {
                hash = unchecked((hash ^ span[i]) * HashPrime);
            }

            return hash;
        }

        /// <summary>
        /// Get stable hash code for a string.
        /// It's an extension method to string.
        /// </summary>
        /// <param name="str">The string that will be hashed.</param>
        /// <returns>The hashcode of input string.</returns>
        /// <example>
        /// <code>
        /// string a = "123";
        /// int hashcode = a.GetStableHash();
        /// </code>
        /// </example>
        public static int GetStableHash(this string str)
        {
            if (string.IsNullOrEmpty(str)) return 0;

            return str.AsSpan().GetStableHash();
        }

        /// <summary>
        /// Get stable hash code for 2 strings. Order-dependent.
        /// </summary>
        /// <param name="s1">The string 1 that will be hashed.</param>
        /// <param name="s2">The string 2 that will be hashed.</param>
        /// <returns>The hashcode of input strings.</returns>
        public static int GetStableHash(string s1, string s2)
        {
            var hash = HashSeed;
            hash = AddStringToHash(hash, s1);
            hash = AddStringToHash(hash, s2);
            return hash;
        }

        /// <summary>
        /// Get stable hash code for 3 strings. Order-dependent.
        /// </summary>
        /// <param name="s1">The string 1 that will be hashed.</param>
        /// <param name="s2">The string 2 that will be hashed.</param>
        /// <param name="s3">The string 3 that will be hashed.</param>
        /// <returns>The hashcode of input strings.</returns>
        public static int GetStableHash(string s1, string s2, string s3)
        {
            var hash = HashSeed;
            hash = AddStringToHash(hash, s1);
            hash = AddStringToHash(hash, s2);
            hash = AddStringToHash(hash, s3);
            return hash;
        }

        /// <summary>
        /// Get stable hash code for 4 strings. Order-dependent.
        /// </summary>
        /// <param name="s1">The string 1 that will be hashed.</param>
        /// <param name="s2">The string 2 that will be hashed.</param>
        /// <param name="s3">The string 3 that will be hashed.</param>
        /// <param name="s4">The string 4 that will be hashed.</param>
        /// <returns>The hashcode of input strings.</returns>
        public static int GetStableHash(string s1, string s2, string s3, string s4)
        {
            var hash = HashSeed;
            hash = AddStringToHash(hash, s1);
            hash = AddStringToHash(hash, s2);
            hash = AddStringToHash(hash, s3);
            hash = AddStringToHash(hash, s4);
            return hash;
        }

        /// <summary>
        /// Get an order-independent stable hash code for 2 strings by sorting them lexicographically first.
        /// </summary>
        /// <param name="s1">The string 1 that will be hashed.</param>
        /// <param name="s2">The string 2 that will be hashed.</param>
        /// <returns>The hashcode of input strings.</returns>
        public static int GetStableHashUnOrder(string s1, string s2)
        {
            if (string.CompareOrdinal(s1, s2) > 0) (s1, s2) = (s2, s1);
            return GetStableHash(s1, s2);
        }

        /// <summary>
        /// Get an order-independent stable hash code for 3 strings by sorting them lexicographically first.
        /// </summary>
        /// <param name="s1">The string 1 that will be hashed.</param>
        /// <param name="s2">The string 2 that will be hashed.</param>
        /// <param name="s3">The string 3 that will be hashed.</param>
        /// <returns>The hashcode of input strings.</returns>
        public static int GetStableHashUnOrder(string s1, string s2, string s3)
        {
            if (string.CompareOrdinal(s1, s2) > 0) (s1, s2) = (s2, s1);
            if (string.CompareOrdinal(s2, s3) > 0) (s2, s3) = (s3, s2);
            if (string.CompareOrdinal(s1, s2) > 0) (s1, s2) = (s2, s1);
            return GetStableHash(s1, s2, s3);
        }

        /// <summary>
        /// Get an order-independent stable hash code for 4 strings by sorting them lexicographically first.
        /// </summary>
        /// <param name="s1">The string 1 that will be hashed.</param>
        /// <param name="s2">The string 2 that will be hashed.</param>
        /// <param name="s3">The string 3 that will be hashed.</param>
        /// <param name="s4">The string 4 that will be hashed.</param>
        /// <returns>The hashcode of input strings.</returns>
        public static int GetStableHashUnOrder(string s1, string s2, string s3, string s4)
        {
            if (string.CompareOrdinal(s1, s2) > 0) (s1, s2) = (s2, s1);
            if (string.CompareOrdinal(s3, s4) > 0) (s3, s4) = (s4, s3);
            if (string.CompareOrdinal(s1, s3) > 0) (s1, s3) = (s3, s1);
            if (string.CompareOrdinal(s2, s4) > 0) (s2, s4) = (s4, s2);
            if (string.CompareOrdinal(s2, s3) > 0) (s2, s3) = (s3, s2);
            return GetStableHash(s1, s2, s3, s4);
        }

        /// <summary>
        /// Utility method for combine a string into existed hash code in sequence way.
        /// </summary>
        /// <param name="hash">The hash code that had been calculated before.</param>
        /// <param name="s">The string wanna be added into hash code.</param>
        /// <returns>The new hashcode.</returns>
        private static int AddStringToHash(int hash, string s)
        {
            if (string.IsNullOrEmpty(s)) return hash;

            ReadOnlySpan<char> span = s.AsSpan();
            for (int i = 0; i < span.Length; i++)
            {
                hash = unchecked((hash ^ span[i]) * HashPrime);
            }

            return hash;
        }

        #endregion

        #region Color Extensions

        // Copyright: https://github.com/UnityCommunity/UnityLibrary/blob/master/Assets/Scripts/Extensions/StringExtensions.cs

        /// <summary>
        /// Convert one hex string into Unity Color32.
        /// It's an extension method of string.
        /// </summary>
        /// <param name="hex">The string in regular color format.</param>
        /// <returns>The color represented by input string.</returns>
        /// <example>
        /// <code>
        /// string _black = "#000000";
        /// Color black = _black.ToColor();
        /// string _white_half = "#ffffff80";
        /// Color white_half = _white_half.ToColor();
        /// </code>
        /// </example>
        public static Color ToColor(this string hex)
        {
            hex = hex.Replace("0x", "");
            hex = hex.Replace("#", "");

            byte a = 255;
            byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);

            if (hex.Length == 8)
                a = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);

            return new Color32(r, g, b, a);
        }

        #endregion

        #region Manipulation Extensions

        // Copyright: https://github.com/m-gebhard/uni-utils/blob/master/Runtime/Extensions/StringExtension.cs

        /// <summary>
        /// Swaps occurrences of two specified substrings within the given text.
        /// </summary>
        /// <param name="text">The original text where the swap will occur.</param>
        /// <param name="swapA">The first substring to swap.</param>
        /// <param name="swapB">The second substring to swap.</param>
        /// <returns>A new string with the specified substrings swapped.</returns>
        /// <example>
        /// <code>
        /// string result = "hello world".Swap("hello", "world");
        /// // result == "world hello"
        /// </code>
        /// </example>
        public static string Swap(this string text, string swapA, string swapB)
        {
            return Regex.Replace(text, Regex.Escape(swapA) + "|" + Regex.Escape(swapB),
                m => m.Value == swapA ? swapB : swapA);
        }

        /// <summary>
        /// Sanitizes the input string by replacing spaces with underscores and removing special characters.
        /// </summary>
        /// <param name="text">The input string to be sanitized.</param>
        /// <param name="allowedPattern">
        /// A regex pattern defining the set of allowed characters.
        /// Defaults to "A-Za-z0-9_", which allows alphanumeric characters and underscores.
        /// </param>
        /// <returns>A sanitized string with spaces replaced by underscores and disallowed characters removed.</returns>
        /// <example>
        /// <code>
        /// string sanitized = "Hello, World!".Sanitize();
        /// // sanitized == "Hello_World"
        /// </code>
        /// </example>
        public static string Sanitize(this string text, string allowedPattern = "A-Za-z0-9_")
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            text = text.Replace(" ", "_");
            text = Regex.Replace(text, @$"[^{allowedPattern}]", "");

            return text;
        }

        /// <summary>
        /// Truncates the input string to a specified maximum length without cutting off words.
        /// Appends a truncation indicator if necessary.
        /// </summary>
        /// <param name="text">The input string to be truncated.</param>
        /// <param name="maxLength">The maximum length of the truncated string (not counting the indicator).</param>
        /// <param name="indicator">The string to append if truncation occurs. Defaults to "...".</param>
        /// <returns>
        /// A truncated string that ends on a word boundary (space/tab/newline) with the truncation indicator appended.
        /// Returns the original string if it's shorter than or equal to maxLength.
        /// Returns an empty string if the input is null or maxLength is negative.
        /// </returns>
        /// <example>
        /// <code>
        /// string t1 = "This is a long string".Truncate(10);
        /// // t1 == "This is a…"
        ///
        /// string t2 = "Short".Truncate(10);
        /// // t2 == "Short"
        ///
        /// string t3 = null.Truncate(5);
        /// // t3 == ""
        ///
        /// string t4 = "Supercalifragilistic".Truncate(5);
        /// // t4 == "Super…"
        /// </code>
        /// </example>
        public static string Truncate(this string text, int maxLength, string indicator = "...")
        {
            if (string.IsNullOrEmpty(text) || maxLength < 0)
                return string.Empty;

            if (text.Length <= maxLength)
                return text;

            string slice = text[..maxLength];

            // Find the last whitespace character in the slice
            int lastSpace = slice.LastIndexOfAny(new[] { ' ', '\t', '\r', '\n' });

            if (lastSpace > 0)
            {
                // Cut at the last word boundary
                slice = slice[..lastSpace];
            }

            return slice + indicator;
        }

        // Copyright: https://github.com/adammyhre/Unity-Utils/blob/master/UnityUtils/Scripts/Extensions/StringExtensions.cs

        /// <summary>
        /// Slices a string from the start index to the end index.
        /// </summary>
        /// <result>The sliced string.</result>
        public static string Slice(this string val, int startIndex, int endIndex)
        {
            if (val.IsBlank())
            {
                throw new ArgumentNullException(nameof(val), "Value cannot be null or empty.");
            }

            if (startIndex < 0 || startIndex > val.Length - 1)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }

            // If the end index is negative, it will be counted from the end of the string.
            endIndex = endIndex < 0 ? val.Length + endIndex : endIndex;

            if (endIndex < 0 || endIndex < startIndex || endIndex > val.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(endIndex));
            }

            return val.Substring(startIndex, endIndex - startIndex);
        }

        #endregion

        #region Property Extensions

        // Copyright: https://github.com/adammyhre/Unity-Utils/blob/master/UnityUtils/Scripts/Extensions/StringExtensions.cs

        /// <summary>
        /// Checks if a string is Null or white space
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string val) => string.IsNullOrWhiteSpace(val);

        /// <summary>
        /// Checks if a string is Null or empty
        /// </summary>
        public static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);

        /// <summary>
        /// Checks if a string contains null, empty or white space.
        /// </summary>
        public static bool IsBlank(this string val) => val.IsNullOrWhiteSpace() || val.IsNullOrEmpty();

        /// <summary>
        /// Checks if a string is null and returns an empty string if it is.
        /// </summary>
        public static string OrEmpty(this string val) => val ?? string.Empty;

        #endregion

        #region Rich Text Extensions

        // Copyright: https://github.com/adammyhre/Unity-Utils/blob/master/UnityUtils/Scripts/Extensions/StringExtensions.cs

        // Rich text formatting, for Unity UI elements that support rich text.
        public static string RichColor(this string text, string color) => $"<color={color}>{text}</color>";
        public static string RichSize(this string text, int size) => $"<size={size}>{text}</size>";
        public static string RichBold(this string text) => $"<b>{text}</b>";
        public static string RichItalic(this string text) => $"<i>{text}</i>";
        public static string RichUnderline(this string text) => $"<u>{text}</u>";
        public static string RichStrikethrough(this string text) => $"<s>{text}</s>";
        public static string RichFont(this string text, string font) => $"<font={font}>{text}</font>";
        public static string RichAlign(this string text, string align) => $"<align={align}>{text}</align>";

        public static string RichGradient(this string text, string color1, string color2) =>
            $"<gradient={color1},{color2}>{text}</gradient>";

        public static string RichRotation(this string text, float angle) => $"<rotate={angle}>{text}</rotate>";
        public static string RichSpace(this string text, float space) => $"<space={space}>{text}</space>";

        #endregion
    }
}