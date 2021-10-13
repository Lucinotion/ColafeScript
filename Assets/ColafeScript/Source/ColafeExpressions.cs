////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>                                                                                                        ///
/// Colafe expressions module used for code preprocesses + operator, variable and literal operations.                ///
/// </summary>                                                                                                       ///
/// <note>                                                                                                           ///
/// Author: Luciano A. Candal                                                                                        ///
/// License: Mozilla Public License 2.0 <href>https://github.com/Lucinotion/ColafeScript/blob/main/LICENSE</href>    ///
/// Repository: This code is available at <href>https://github.com/Lucinotion/ColafeScript</href>                    ///
/// </note>                                                                                                          ///
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Text.RegularExpressions;
using System.Linq;
using ColafeScript.Statements;
using ColafeScript.Utils;

namespace ColafeScript.Expressions{

    static class Preprocess{

        internal static void RemoveComments(ref string exp){
            if(exp.Contains("//"))
                exp = REGX.FIND_LCOMMENTS.Replace(exp, string.Empty);
            if(exp.Contains("/*"))
                exp = REGX.FIND_BCOMMENTS.Replace(exp, string.Empty);
        }

        internal static void EscapeAllCodeLiterals(ref string exp){
            if(exp.Contains('"'))
                exp = REGX.FIND_LITERALS_.Replace(exp, m => '"' + EscapeCodeLiteral(m.Groups[1].Value) + '"');
        }

        static string EscapeCodeLiteral(string str){
            if (string.IsNullOrEmpty(str))
                return string.Empty;
            return str = REGX.ESCAPE_IN_CODE.Replace(str, m => '\\' + m.Value);
        }

        internal static void ReplaceTrueAndFalse(ref string exp){
            exp = exp.Replace("true", "1");
            exp = exp.Replace("false", "0");
        }
    }

    static class Operator{

        internal enum LogicGateType { AND, OR }

        internal static void ExecuteIsAssignedOperator(ref string exp, Colafe cf){
            if(exp.Contains('$'))
                exp = REGX.IS_ASSIGNED_OP.Replace(exp, m => Colafe.Defined(m.Groups[1].Value, cf) ? "1" : "0");
        }

        internal static void ExecutePrefixIncrementDecrementOperator(ref string exp, Colafe cf){
            if(exp.Contains("++") || exp.Contains("--"))
                exp = REGX.INC_DEC_PREFIX.Replace(exp, (m) => {
                    string varname = m.Groups[2].Value;
                    GenericValue varvalue = Colafe.Peek(varname, cf);
                    if(varvalue == null){
                        Msg.Err("ColafeScript: Cannot self increment or drecrement unasigned variable'" + varname + "' .");
                        return string.Empty;
                    }
                    GenericValue newvalue = m.Groups[1].Value == "++" ? varvalue + 1f : varvalue - 1f;
                    Colafe.Poke(varname, newvalue, cf);
                    return newvalue;
                });
        }

        internal static string ReplaceAndRunLogicGates(string exp, Colafe cf){
            string expA, expB;
            int start = 0, end = 0, expStart = 0, expEnd = 0;
            for (string logicGate = "&&"; exp.Contains("&&") || exp.Contains("||"); logicGate = "||"){
                while (exp.Contains(logicGate)){
                    end = exp.IndexOf(logicGate) - 1;
                    for (int i = end; i >= 0; --i){
                        if (i == 0)
                            expStart = start = 0;
                        else if ((exp[i] == '|' && exp[i - 1] == '|') || (exp[i] == '&' && exp[i - 1] == '&')){
                            expStart = start = i + 1;
                            break;
                        }
                    }
                    expA = exp.Substring(start, end - start + 1);
                    start = end + 3;
                    for (int i = start; i < exp.Length; ++i){
                        if (i == exp.Length - 1)
                            expEnd = end = i;
                        else if ((exp[i] == '|' && exp[i + 1] == '|') || (exp[i] == '&' && exp[i + 1] == '&')){
                            expEnd = end = i - 1;
                            break;
                        }
                    }
                    expB = exp.Substring(start, end - start + 1);
                    switch (logicGate){
                        case "&&":
                            if(float.Parse(ExpressionStatement.EvalSimpleExpressionWithFunctionCalls(expA, cf)).ToBool()
                            && float.Parse(ExpressionStatement.EvalSimpleExpressionWithFunctionCalls(expB, cf)).ToBool())
                                exp = exp.Remove(expStart, expEnd - expStart + 1).Insert(expStart, "1");
                            else
                                return "0";
                            break;
                        case "||":
                            if(float.Parse(ExpressionStatement.EvalSimpleExpressionWithFunctionCalls(expA, cf)).ToBool()
                            || float.Parse(ExpressionStatement.EvalSimpleExpressionWithFunctionCalls(expB, cf)).ToBool())
                                return "1";
                            else
                                exp = exp.Remove(expStart, expEnd - expStart + 1).Insert(expStart, "0");
                            break;
                    }
                }
            }
            return exp;
        }
    }

    static class Literal{

        internal static bool ContainsLiteralVariables(string literal){
            return literal.ContainsUnescaped('`');
        }

        // TURN THIS INTO A PREPROCESS THAT RUNS BEFORE THE CODE IS EXECUTED
        internal static void EscapeLiteralsInExpression(ref string exp){
            if (exp.IsStringLiteral()){
                foreach (Match match in REGX.FIND_LITERALS_.Matches(exp)){
                    exp = REGX.FIND_LITERALS_.Replace(exp, match.Groups[1].Value.EscapeString(), 1);
                }
            }
        }

        internal static void ReplaceLiteralVariablesInExpression(ref string exp, Colafe cf){
            if (ContainsLiteralVariables(exp))
                exp = REGX.FIND_LITER_VAR.Replace(exp, (m) => {
                    var varContent = Colafe.Peek(m.Groups[1].Value, cf);
                    if(varContent != null)
                        if(varContent.Raw.IsStringLiteral())
                            return varContent.Raw.Substring(1, varContent.Raw.Length - 2);
                        else
                            return varContent.Raw;
                    else
                        Msg.Err("ColafeScript: Variable '" + m.Groups[1].Value + "' is not defined.");
                    return string.Empty;
                });
        }
    }

    static class Variable{

        internal static bool IsGlobalVariable(string varname){ return varname[0] == '.'; }

        internal static void ReplaceVariableIdentifiers(ref string exp, Colafe cf){
            foreach (Match match in REGX.FIND_VARIABLES.Matches(exp)){
                string identifier = match.Groups[2].Value;
                var varContent = Colafe.Peek(match.Value, cf);
                if(varContent == null){
                    Msg.Err("ColafeScript: Variable '" + match.Value + "' is not defined.");
                    exp = string.Empty;
                    return;
                }
                else if(varContent.Raw.IsStringLiteral()){
                    exp = varContent.Raw;
                    return;
                }else
                    exp = exp.Replace(match.Value, varContent.Raw);
            }
        }
    }

    static class Function{

        internal static void CallFunctionsAndReplaceIn(ref string exp, Colafe cf){
            if(exp.ContainsUnescaped('['))
                exp = REGX.FIND_FUNCTIONS.Replace(exp, m => new CallStatement(m.Value, cf).Run());
        }
    }
}