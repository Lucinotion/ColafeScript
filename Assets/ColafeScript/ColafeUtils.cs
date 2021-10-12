using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;

namespace ColafeScript.Utils{

    static class REGX{
        const string VAR_IDENTIFIER = @"(\.?([^\d\W]+\w*))(?!.*\1)";
        const string FUNCTION_DRECT = @"\.?[^\d\W]+\w*\s*\[.*?(?<!\\)\]";
        const string FUNCTION_CMPLX = @"(?<!\\)#.*?(?<!\\)\]";
        const string FLOATING_VALUE = @"([\-\+]?\.?\d+\.?\d*)";
        const string STRING_LITERAL = @"""(.*?(?<!\\))\""";
        const string VAR_IN_LITERAL = @"(?<!\\)`(\.?\w+)`";
        const string INLINE_COMMENT = @"(?<=^|(?<!\\);)\s*\/\/.*";
        const string BLOCK__COMMENT = @"(?<=^|(?<!\\);)\s*\/\*[\S\s]*?\*\/";

        internal static readonly Regex FIND_LITERALS_ = new Regex( STRING_LITERAL                                     );
        internal static readonly Regex FIND_VARIABLES = new Regex( VAR_IDENTIFIER                                     );
        internal static readonly Regex FIND_LITER_VAR = new Regex( VAR_IN_LITERAL                                     );
        internal static readonly Regex FIND_FUNCTIONS = new Regex( FUNCTION_CMPLX + @"|"             + FUNCTION_DRECT );
        internal static readonly Regex FIND_ARGUMENTS = new Regex(                  @"(?:^|,)(.*?[^\\])(?=,|$)"       );
        internal static readonly Regex FIND_LCOMMENTS = new Regex( INLINE_COMMENT , RegexOptions.Multiline            );
        internal static readonly Regex FIND_BCOMMENTS = new Regex( BLOCK__COMMENT , RegexOptions.Multiline            );
        internal static readonly Regex INC_DEC_PREFIX = new Regex(                  @"(\+\+|\-\-)(\.?([^\d\W]+\w*))"  );
        internal static readonly Regex UN_ESCAPE_STR_ = new Regex(                  @"\\(?=[^\w\d\s])"                );
        internal static readonly Regex ESCAPE_STRING_ = new Regex(                  @"(?<!\\)[""`;:,\[\](){}<>#&|]"   );
        internal static readonly Regex ESCAPE_IN_CODE = new Regex(                  @"(?<!\\)[;:,\[\](){}<>#&|]"      );
        internal static readonly Regex UNARY_PS_MS___ = new Regex(                  @"[\+\-]{2,}"                     );
        internal static readonly Regex IS_ASSIGNED_OP = new Regex(                  @"\$\s*"         + VAR_IDENTIFIER );
        internal static readonly Regex LOGIC_BW_NOT__ = new Regex(                  @"([\!\~]+)(?!=)"+ FLOATING_VALUE );
        internal static readonly Regex PO_MUL_DIV_REM = new Regex( FLOATING_VALUE + @"([\*\/\%]\*?)" + FLOATING_VALUE );
        internal static readonly Regex ADD_SUBTRACT__ = new Regex( FLOATING_VALUE + @"([\+\-])"      + FLOATING_VALUE );
        internal static readonly Regex SHIFTLEFTRIGHT = new Regex( FLOATING_VALUE + @"(\>\>|\<\<)"   + FLOATING_VALUE );
        internal static readonly Regex LT_GT_LE_GE___ = new Regex( FLOATING_VALUE + @"([\<\>]=?)"    + FLOATING_VALUE );
        internal static readonly Regex EQUAL_NOTEQUAL = new Regex( FLOATING_VALUE + @"([=\!]=)"      + FLOATING_VALUE );
        internal static readonly Regex BITWISE_AND___ = new Regex( FLOATING_VALUE + @"&"             + FLOATING_VALUE );
        internal static readonly Regex BITWISE_XOR___ = new Regex( FLOATING_VALUE + @"\^"            + FLOATING_VALUE );
        internal static readonly Regex BITWISE_OR____ = new Regex( FLOATING_VALUE + @"\|"            + FLOATING_VALUE );
    }

    static class Msg{
        const bool LOG_TO_UNITY_CONSOLE = true;
        public static System.Action<string> OnErrorCallback = null;
        public static System.Action<string> OnWarningCallback = null;
        public static System.Action<string> OnLogCallback = null;

        internal static string Log (string msg) {
            if(OnLogCallback != null)
                OnLogCallback(msg);
            if(LOG_TO_UNITY_CONSOLE)
                Debug.Log(msg);
            return null;
        }
        internal static string Wrn (string msg) {
            if(OnWarningCallback != null)
                OnWarningCallback(msg);
            if(LOG_TO_UNITY_CONSOLE)
                Debug.LogWarning(msg);
            return null;
        }
        internal static string Err (string msg) {
            if(OnErrorCallback != null)
                OnErrorCallback(msg);
            if(LOG_TO_UNITY_CONSOLE)
                Debug.LogError(msg);
            return null;
        }
    }

    static class StringExtensions{

        public static string ProcessNestedStrings(this string exp, char startToken, char endToken, System.Func<string, string> process){
            int startIndex, endIndex;
            string nested = exp.StringInBetween(startToken, endToken, out startIndex, out endIndex);
            while (!string.IsNullOrEmpty(nested)){
                exp = exp.Substring(0, startIndex) + nested.ProcessNestedStrings(startToken, endToken, process) + exp.Substring(endIndex + 1);
                nested = exp.StringInBetween(startToken, endToken, out startIndex, out endIndex);
            }
            return process(exp);
        }

        public static string UnescapeString(this string strToUnescape, bool removeQuotes = true){
            if (string.IsNullOrEmpty(strToUnescape))
                return strToUnescape;
            else if (removeQuotes)
                return REGX.UN_ESCAPE_STR_.Replace(strToUnescape.Substring(1, strToUnescape.Length - 2), string.Empty);
            else
                return REGX.UN_ESCAPE_STR_.Replace(strToUnescape, string.Empty);
        }

        public static string EscapeString(this string strToEscape, bool addQuotes = true){
            if (strToEscape == null)
                return null;
            else if (strToEscape.Length == 0)
                return addQuotes ? "\"\"" : string.Empty;
            else{
                foreach (Match match in REGX.ESCAPE_STRING_.Matches(strToEscape))
                    strToEscape = REGX.ESCAPE_STRING_.Replace(strToEscape, "\\" + match.Value, 1);
                return addQuotes ? "\"" + strToEscape + "\"" : strToEscape;
            }
        }
        
        public static string StringInBetween(this string str, char startToken, char endToken, out int startIndex, out int endIndex){
            int start = -1, end = -1, depth = 0, i = -1;

            while (++i < str.Length){
                if (str[i] == startToken){
                    if(i > 0 && str[i-1] == '\\'){
                        // skip if the character is escaped
                    }else{
                        start = i;
                        break;
                    }
                }
            }
            while (++i < str.Length){
                if (str[i] == endToken){
                    if(i > 0 && str[i-1] == '\\'){
                        // skip if the character is escaped
                    }else{
                        end = i;
                        if (depth == 0) { break; }
                        else { --depth; }
                    }
                }
                else if (str[i] == startToken){
                    if(i > 0 && str[i-1] == '\\'){
                        // skip if the character is escaped
                    }else{
                        ++depth;
                    }
                }
            }
            if (end > start){
                startIndex = start;
                endIndex   = end;
                return str.Substring(start + 1, end - start - 1);
            }

            startIndex = -1;
            endIndex   = -1;
            return null;
        }

        /// <summary>
        /// Finds the first instance of a string in between two tokens.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="startToken"></param>
        /// <param name="endToken"></param>
        /// <returns>The substring if found, empty string if the inside of the tokens is empty, null if not found.</returns>
        public static string StringInBetween(this string str, char startToken, char endToken){
            return str.StringInBetween(startToken, endToken, out int si, out int ei);
        }

        public static string[] StringsInBetween(this string str, char startToken, char endToken){
            Queue<string> substrings = new Queue<string>();
            string slice = str;
            string subtracted = str;
            int startIndex = 0, endIndex = 0;
            if (str.Count(f => f == startToken || f == endToken) % 2 != 0) {
                Debug.LogError("Cannot get substring from '" + str + "': Uneven number of tokens '" + startToken + endToken + "'.");
                return null;
            }
            do{
                slice = subtracted.StringInBetween(startToken, endToken, out startIndex, out endIndex);

                if(slice != null){
                    subtracted = subtracted.Substring(0, startIndex) + subtracted.Substring(endIndex + 1);
                    substrings.Enqueue(slice);
                }

            } while ((subtracted.Count(f => f == startToken) > 0 && subtracted.Count(f => f == endToken) > 0));
            return substrings.ToArray();
        }

        public static bool ContainsUnescaped(this string str, char token){
            int i = 0;
            if(string.IsNullOrEmpty(str))
                return false;
            if(str[0] == token)
                return true;
            foreach (char c in str){
                if (i != 0 && c == token && str[i - 1] != '\\')
                    return true;
                ++i;
            }
            return false;
        }

        public static int IndexOfUnescaped(this string str, char token, int startIndex = 0){
            if(string.IsNullOrEmpty(str) || startIndex > str.Length - 1)
                return -1;
            else if(str[startIndex] == token)
                return startIndex;
            for (int i = startIndex + 1; i < str.Length; ++i){
                if(str[i] == token && str[i - 1] != '\\')
                    return i;
            }
            return -1;
        }

        public static bool IsStringLiteral(this string str) { return str.Contains('"') ? true : false; }

    }

    static class ConversionExtensions{        
        public static bool  ToBool(this float flt) { return flt == 0f ? false : true; }
        public static float ToFloat(this bool bln) { return bln ? 1f : 0f; }
    }
}
