using System.Text.RegularExpressions;
using System.Linq;
using ColafeScript.Utils;

namespace ColafeScript.Arithmetic{

    static class Maths{
        const int MAX_ITERATIONS = 256;

        internal static string EvalArithmeticExp(string exp){
            string result = exp.Trim();
            if (string.IsNullOrEmpty(result))
                return result;
            if (result.IsStringLiteral())
                return result;
            if (result.Contains(' '))
                result = result.Replace(" ", string.Empty);
            if (result.Contains('+') || result.Contains('-'))
                CalculateUnaryPlusAndMinus(ref result);
            if (result.Contains('!') || result.Contains('~'))
                CalculateLogicNotAndBitwiseNot(ref result);
            if (result.Contains('*') || result.Contains('/') || result.Contains('%'))
                CalculatePowersMultiplicationsDivisionsAndRemainders(ref result);
            if (result.Contains('+') || result.Contains('-'))
                CalculateAdditionsAndSustractions(ref result);
            if (result.Contains("<<") || result.Contains(">>"))
                CalculateShiftLeftAndRight(ref result);
            if (result.Contains('<') || result.Contains('>'))
                CalculateLessGreaterLessEqualGreaterEqual(ref result);
            if (result.Contains('='))
                CalculateEqualsNotEquals(ref result);
            if (result.Contains('&'))
                CalculateBitwiseAND(ref result);
            if (result.Contains('^'))
                CalculateBitwiseXOR(ref result);
            if (result.Contains('|'))
                CalculateBitwiseOR(ref result);
            if (!string.IsNullOrEmpty(result))
                return result.Trim('+');
            else
                Msg.Err("ColafeScript: Is expression '" + exp + "' malformed?");
            return null;
        }

        internal static void CheckIterations(ref string exp, in int iterations){
            if (iterations > MAX_ITERATIONS){
                Msg.Err("ColafeScript: Too many iterations. Cannot eval '" + exp + "' .");
                exp = string.Empty;
            }
        }

        internal static void CalculateUnaryPlusAndMinus(ref string exp){
            foreach (Match match in REGX.UNARY_PS_MS___.Matches(exp)){
                exp = exp.Replace(match.Value, match.Value.Count(f => f == '-') % 2 == 0 ? "+" : "-");
            }
        }

        internal static void CalculateLogicNotAndBitwiseNot(ref string exp){
            bool   blnValue;
            ushort intValue;
            foreach (Match match in REGX.LOGIC_BW_NOT__.Matches(exp)){
                if (match.Value.Contains('!')){
                    blnValue = (float.Parse(match.Groups[2].Value).ToBool());
                    exp = REGX.LOGIC_BW_NOT__.Replace(exp, match.Value.Count(c => c == '!') % 2 == 0 ? blnValue.ToFloat().ToString() : (!blnValue).ToFloat().ToString(), 1);
                }
                else{
                    intValue = ushort.Parse(match.Groups[2].Value);
                    exp = REGX.LOGIC_BW_NOT__.Replace(exp, match.Value.Count(c => c == '~') % 2 == 0 ? (intValue).ToString() : (~intValue).ToString(), 1);
                }
            }
        }

        internal static void CalculatePowersMultiplicationsDivisionsAndRemainders(ref string exp){
            int iterations = 0;
            float fltValue = 0;
            Match match = REGX.PO_MUL_DIV_REM.Match(exp);
            do{
                switch (match.Groups[2].Value){
                    case "**":
                        fltValue = (float)System.Math.Pow(float.Parse(match.Groups[1].Value), float.Parse(match.Groups[3].Value));
                        fltValue = match.Groups[1].Value[0] == '-' ? -fltValue : fltValue;
                        break;
                    case "*":
                        fltValue = float.Parse(match.Groups[1].Value) * float.Parse(match.Groups[3].Value);
                        break;
                    case "/":
                        fltValue = float.Parse(match.Groups[1].Value) / float.Parse(match.Groups[3].Value);
                        break;
                    case "%":
                        fltValue = float.Parse(match.Groups[1].Value) % float.Parse(match.Groups[3].Value);
                        break;
                    default:
                        Msg.Err("ColafeScript: Operator: '" + match.Groups[2].Value + "' is malformed.");
                        break;
                }
                exp = exp.Replace(match.Value, fltValue >= 0f ? "+" + fltValue.ToString("0.#########") : fltValue.ToString("0.#########"));
                match = REGX.PO_MUL_DIV_REM.Match(exp);
            } while (match.Value != string.Empty && iterations++ < MAX_ITERATIONS);
            CheckIterations(ref exp, iterations);
        }

        internal static void CalculateAdditionsAndSustractions(ref string exp){
            int iterations = 0;
            float fltValue;
            MatchCollection matches = REGX.ADD_SUBTRACT__.Matches(exp);
            while (matches.Count > 0 && iterations++ < MAX_ITERATIONS){
                foreach (Match match in matches){
                    switch (match.Groups[2].Value){
                        case "+":
                            fltValue = float.Parse(match.Groups[1].Value) + float.Parse(match.Groups[3].Value);
                            exp = exp.Replace(match.Value, fltValue >= 0f ? "+" + fltValue.ToString("0.#########") : fltValue.ToString("0.#########"));
                            break;
                        case "-":
                            fltValue = float.Parse(match.Groups[1].Value) - float.Parse(match.Groups[3].Value);
                            exp = exp.Replace(match.Value, fltValue.ToString("0.#########"));
                            break;
                    }
                }
                matches = REGX.ADD_SUBTRACT__.Matches(exp);
            }
            CheckIterations(ref exp, iterations);
        }

        internal static void CalculateShiftLeftAndRight(ref string exp){
            int iterations = 0;
            int intValue = 0;
            Match match = REGX.SHIFTLEFTRIGHT.Match(exp);
            do{
                switch (match.Groups[2].Value){
                    case "<<":
                        intValue = (ushort.Parse(match.Groups[1].Value) << ushort.Parse(match.Groups[3].Value));
                        break;
                    case ">>":
                        intValue = (ushort.Parse(match.Groups[1].Value) >> ushort.Parse(match.Groups[3].Value));
                        break;
                    default:
                        Msg.Err("ColafeScript: Operator: '" + match.Groups[2].Value + "' is malformed.");
                        break;
                }
                exp = exp.Replace(match.Value, intValue.ToString());
                match = REGX.SHIFTLEFTRIGHT.Match(exp);
            } while (match.Value != string.Empty && iterations++ < MAX_ITERATIONS);
            CheckIterations(ref exp, iterations);
        }

        internal static void CalculateLessGreaterLessEqualGreaterEqual(ref string exp){
            int iterations = 0;
            float fltValue = 0;
            Match match = REGX.LT_GT_LE_GE___.Match(exp);
            do{
                switch (match.Groups[2].Value){
                    case "<":
                        fltValue = (float.Parse(match.Groups[1].Value) < float.Parse(match.Groups[3].Value)).ToFloat();
                        break;
                    case "<=":
                        fltValue = (float.Parse(match.Groups[1].Value) <= float.Parse(match.Groups[3].Value)).ToFloat();
                        break;
                    case ">":
                        fltValue = (float.Parse(match.Groups[1].Value) > float.Parse(match.Groups[3].Value)).ToFloat();
                        break;
                    case ">=":
                        fltValue = (float.Parse(match.Groups[1].Value) >= float.Parse(match.Groups[3].Value)).ToFloat();
                        break;
                    default:
                        Msg.Err("ColafeScript: Operator: '" + match.Groups[2].Value + "' is malformed.");
                        break;
                }
                exp = exp.Replace(match.Value, fltValue.ToString());
                match = REGX.LT_GT_LE_GE___.Match(exp);
            } while (match.Value != string.Empty && iterations++ < MAX_ITERATIONS);
            CheckIterations(ref exp, iterations);
        }

        internal static void CalculateEqualsNotEquals(ref string exp){
            int iterations = 0;
            float fltValue = 0;
            Match match = REGX.EQUAL_NOTEQUAL.Match(exp);
            do{
                switch (match.Groups[2].Value){
                    case "==":
                        fltValue = (float.Parse(match.Groups[1].Value) == float.Parse(match.Groups[3].Value)).ToFloat();
                        break;
                    case "!=":
                        fltValue = (float.Parse(match.Groups[1].Value) != float.Parse(match.Groups[3].Value)).ToFloat();
                        break;
                    default:
                        Msg.Err("ColafeScript: Operator: '" + match.Groups[2].Value + "' is malformed.");
                        break;
                }
                exp = exp.Replace(match.Value, fltValue.ToString());
                match = REGX.EQUAL_NOTEQUAL.Match(exp);
            } while (match.Value != string.Empty && iterations++ < MAX_ITERATIONS);
            CheckIterations(ref exp, iterations);
        }

        internal static void CalculateBitwiseAND(ref string exp){
            int iterations = 0;
            MatchCollection matches = REGX.BITWISE_AND___.Matches(exp);
            while (matches.Count > 0 && iterations++ < MAX_ITERATIONS){
                foreach (Match match in matches){
                    exp = exp.Replace(match.Value, (ushort.Parse(match.Groups[1].Value) & ushort.Parse(match.Groups[2].Value)).ToString());
                }
                matches = REGX.BITWISE_AND___.Matches(exp);
            }
            CheckIterations(ref exp, iterations);
        }

        internal static void CalculateBitwiseXOR(ref string exp){
            int iterations = 0;
            MatchCollection matches = REGX.BITWISE_XOR___.Matches(exp);
            while (matches.Count > 0 && iterations++ < MAX_ITERATIONS){
                foreach (Match match in matches){
                    exp = exp.Replace(match.Value, (ushort.Parse(match.Groups[1].Value) ^ ushort.Parse(match.Groups[2].Value)).ToString());
                }
                matches = REGX.BITWISE_XOR___.Matches(exp);
            }
            CheckIterations(ref exp, iterations);
        }

        internal static void CalculateBitwiseOR(ref string exp){
            int iterations = 0;
            MatchCollection matches = REGX.BITWISE_OR____.Matches(exp);
            while (matches.Count > 0 && iterations++ < MAX_ITERATIONS){
                foreach (Match match in matches){
                    exp = exp.Replace(match.Value, (ushort.Parse(match.Groups[1].Value) | ushort.Parse(match.Groups[2].Value)).ToString());
                }
                matches = REGX.BITWISE_OR____.Matches(exp);
            }
            CheckIterations(ref exp, iterations);
        }
    }
}