////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>                                                                                                        ///
/// Colade statements module with all the statements used when evaluating Colafe code.                               ///
/// </summary>                                                                                                       ///
/// <note>                                                                                                           ///
/// Author: Luciano A. Candal                                                                                        ///
/// License: Mozilla Public License 2.0 <href>https://github.com/Lucinotion/ColafeScript/blob/main/LICENSE</href>    ///
/// Repository: This code is available at <href>https://github.com/Lucinotion/ColafeScript</href>                    ///
/// </note>                                                                                                          ///
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using ColafeScript.Arithmetic;
using ColafeScript.Expressions;
using ColafeScript.Utils;
using ColafeScript.Finder;

namespace ColafeScript.Statements{

    abstract class Statement
    {
        protected readonly string _exp;
        protected readonly Colafe _parent;
        internal abstract string Run();
        internal Statement(string exp, Colafe parent) {
            _exp = exp;
            _parent = parent;
        }
    }

    static class ColafeRunner
    {
        internal static string Run(string exp, Colafe cf){
            Preprocess.RemoveComments(ref exp);
            Preprocess.EscapeAllCodeLiterals(ref exp);
            return new CodeBlockStatement(exp, cf).Run();
        }
    }

    class WhileStatement : Statement
    {
        Statement whileStatement;

        internal WhileStatement(string exp, Statement whileStatement, Colafe parent) : base(exp, parent){
            this.whileStatement = whileStatement;
        }

        internal override string Run(){
            Queue<string> results = new Queue<string>();
            while(float.Parse(new ExpressionStatement(_exp, _parent).Run()).ToBool()){
                results.Enqueue(whileStatement.Run());
            }
            return string.Join("\n", results);
        }
    }

    class ConditionStatement : Statement
    {
        Statement ifStatement;
        Statement elseStatement;

        internal ConditionStatement(string exp, Statement ifStatement, Statement elseStatement, Colafe parent) : base(exp, parent){
            this.ifStatement = ifStatement;
            this.elseStatement = elseStatement;
        }

        internal override string Run(){
            float evalResult;
            if(float.TryParse(new ExpressionStatement(_exp, _parent).Run(), out evalResult)){
                if(evalResult.ToBool())
                    return ifStatement.Run();
                else if(elseStatement != null)
                    return elseStatement.Run();
                else
                    return "0";
            }else
                Msg.Err("ColafeScript: Expression '" + _exp + "' is not a valid boolean expression.");
            return null;
        }

        internal static int FindElseIndex(in string str, int start){
            int colonIndex = str.IndexOfUnescaped(':', start);
            if(colonIndex == -1)
                return -1;
            int semicolonIndex = str.IndexOfUnescaped(';', start);
            if(semicolonIndex == -1)
                semicolonIndex = int.MaxValue;
            int curlyBraceIndex = str.IndexOfUnescaped('}', start);
            if(curlyBraceIndex == -1)
                curlyBraceIndex = int.MaxValue;
            int closest = semicolonIndex < curlyBraceIndex ? semicolonIndex : curlyBraceIndex;
            if(colonIndex < closest)
                return colonIndex;
            else
                return -1;
        }
    }

    class CodeBlockStatement : Statement
    {
        static readonly char[] trimChars = new char[] { ' ', '\n', '\t', '\r' };

        internal CodeBlockStatement(string exp, Colafe parent) : base(exp.Trim(trimChars), parent){}

        internal override string Run(){
            int end, carry = 0;
            Queue<string> results = new Queue<string>();
            Statement foundStatement = null;
            while(StatementFoundAfter(carry, out end, ref foundStatement)){
                carry = end;
                results.Enqueue(foundStatement.Run());
            }
            return string.Join("\n", results);
        }

        internal bool StatementFoundAfter(int start, out int endIndex, ref Statement result){
            result = FindStatementInString(_exp, out endIndex, start);
            return result == null ? false : true;
        }

        internal Statement FindStatementInString(string str, out int endIndex, int startSearchIndex = 0){
            string bodyStr;
            for (int i = startSearchIndex; i < str.Length; ++i){
                switch (str[i]){
                    case ' ': case '\t': case '\n': case  '\r': case ';': case '}':
                        continue;
                    case '{':{
                        bodyStr = str.Substring(i).StringInBetween('{', '}');
                        endIndex = i + bodyStr.Length + 1;
                        return new CodeBlockStatement(bodyStr, _parent);
                    }
                    case '%':{
                        bodyStr = str.Substring(i).StringInBetween('(', ')', out int bodyStart, out int bodyEnd);
                        Statement whileContent = FindStatementInString(str, out int whileContentEnd, i + bodyEnd + 1);
                        endIndex = whileContentEnd;
                        return new WhileStatement(bodyStr, whileContent, _parent);
                    }
                    case '?':{
                        bodyStr = str.Substring(i).StringInBetween('(', ')', out int bodyStart, out int bodyEnd);
                        Statement ifContent = FindStatementInString(str, out int ifContentEnd, i + bodyEnd + 1);
                        Statement elseContent = null;
                        int elseIndex = ConditionStatement.FindElseIndex(str, ifContentEnd + 1);
                        if(elseIndex != -1){
                            elseContent = FindStatementInString(str, out int elseContentEnd, elseIndex + 1);
                            endIndex = elseContentEnd;
                        }else
                            endIndex = ifContentEnd;
                        return new ConditionStatement(bodyStr, ifContent, elseContent, _parent);
                    }
                    case ':':{
                        Msg.Err("ColafeScript: Else statement ':' must come after If statement '?'.");
                        endIndex = -1;
                        return null;
                    }
                    case '@':{
                        endIndex = str.IndexOfUnescaped(';', i);
                        if(endIndex == -1){ // single statement call
                            endIndex = str.Length;
                            return new InvokeStatement(str.Substring(i + 1), _parent);
                        }
                        return new InvokeStatement(str.Substring(i + 1, endIndex - i + 1), _parent);
                    }
                    default:{
                        endIndex = str.IndexOfUnescaped(';', i);
                        if(endIndex == -1){ // single statement call
                            endIndex = str.Length;
                            return new ExpressionStatement(str, _parent);
                        }
                        else
                            return new ExpressionStatement(str.Substring(i, endIndex - i), _parent);
                    }
                }
            }
            endIndex = -1;
            return null;
        }
    }

    class AssignStatement : Statement{

        internal enum AssignType { 
            NONE, DEFINE, ASSIGN, INCREMENT, DECREMENT, POWER, MULTIPLY, DIVIDE, REMAINDER,
            BITWISEAND, BITWISEOR, BITWISEXOR, SHIFTLEFT, SHIFTRIGHT
        }

        readonly AssignType assignType;
        readonly string varname;
        readonly string content;

        internal AssignStatement(string varname, string exp, AssignType assigntype, Colafe parent):base(exp, parent){
            this.varname = varname.Trim();
            this.content = exp.Trim();
            this.assignType = assigntype;
        }

        internal override string Run(){
            string expValue = new ExpressionStatement(content, _parent).Run();
            GenericValue newValue = null;

            bool varIsDefined = Colafe.Defined(varname, _parent);
            if(varIsDefined){
                GenericValue variableValue = Colafe.Peek(varname, _parent);
                switch (assignType){
                    case AssignType.DEFINE     : return variableValue;
                    case AssignType.ASSIGN     : newValue = new GenericValue(expValue);                       break;
                    case AssignType.INCREMENT  : newValue = variableValue.AsFloat  +   float.Parse(expValue); break;
                    case AssignType.DECREMENT  : newValue = variableValue.AsFloat  -   float.Parse(expValue); break;
                    case AssignType.POWER      : newValue = Math.Pow(variableValue,   float.Parse(expValue)); break;
                    case AssignType.MULTIPLY   : newValue = variableValue.AsFloat  *   float.Parse(expValue); break;
                    case AssignType.DIVIDE     : newValue = variableValue.AsFloat  /   float.Parse(expValue); break;
                    case AssignType.REMAINDER  : newValue = variableValue.AsFloat  %   float.Parse(expValue); break;
                    case AssignType.BITWISEAND : newValue = variableValue.AsUShort &  ushort.Parse(expValue); break;
                    case AssignType.BITWISEOR  : newValue = variableValue.AsUShort |  ushort.Parse(expValue); break;
                    case AssignType.BITWISEXOR : newValue = variableValue.AsUShort ^  ushort.Parse(expValue); break;
                    case AssignType.SHIFTLEFT  : newValue = variableValue.AsUShort << ushort.Parse(expValue); break;
                    case AssignType.SHIFTRIGHT : newValue = variableValue.AsUShort >> ushort.Parse(expValue); break;
                    default : Msg.Err("ColafeScript: This shouldn't even be possible.                     "); break;
                }
            }else if(assignType == AssignType.ASSIGN || assignType == AssignType.DEFINE){
                newValue = new GenericValue(expValue);
            }else{
                Msg.Err("ColafeScript: Tried to use a self assign operator on unnasigned variable '" + varname + "' .");
                return null;
            }

            Colafe.Poke(varname, newValue, _parent);
            return newValue.Raw;
        }

        /// <summary>
        /// Gets a Assignment Statement form a expression
        /// </summary>
        /// <param name="exp">The expression, usually like varname = exp</param>
        /// <param name="cf"></param>
        /// <returns></returns>
        internal static AssignStatement GetAssignStatementFromExp(string exp, Colafe cf){
            int beforeIndex;
            int afterIndex;
            AssignType assigntype = GetAssignTypeAndIndex(exp, out beforeIndex, out afterIndex);
            if(assigntype != AssignType.NONE){
                return new AssignStatement(exp.Substring(0, beforeIndex + 1), exp.Substring(afterIndex), assigntype, cf);
            }
            return null;
        }

        static AssignType GetAssignTypeAndIndex(string exp, out int beforeindex, out int afterindex){
            afterindex  = -1;
            beforeindex = -1;
            if(string.IsNullOrEmpty(exp))
                return AssignType.NONE;
            int i = 0;
            foreach (char c in exp){
                if(!char.IsLetterOrDigit(c) && c!='_' && c!='.' && c!=' '){
                    beforeindex = i - 1;
                    afterindex = i + 2;
                    char charAfter = exp[i + 1];
                    switch (c){
                        case '=': afterindex = i + 1;
                                  if (charAfter != '=') return AssignType.ASSIGN    ; else goto default;
                        case '+': if (charAfter == '=') return AssignType.INCREMENT ; else goto default;
                        case '-': if (charAfter == '=') return AssignType.DECREMENT ; else goto default;
                        case '*':                             
                            if(charAfter == '*' && afterindex < exp.Length - 1 && exp[afterindex] == '='){
                                afterindex = i + 3;
                                return AssignType.POWER;
                            }else if (charAfter == '=') return AssignType.MULTIPLY  ; else goto default;
                        case '/': if (charAfter == '=') return AssignType.DIVIDE    ; else goto default;
                        case '%': if (charAfter == '=') return AssignType.REMAINDER ; else goto default;
                        case '&': if (charAfter == '=') return AssignType.BITWISEAND; else goto default;
                        case '|': if (charAfter == '=') return AssignType.BITWISEOR ; else goto default;
                        case '^': if (charAfter == '=') return AssignType.BITWISEXOR; else goto default;
                        case '$': if (charAfter == '=') return AssignType.DEFINE    ; else goto default;
                        case '<':
                            if(charAfter == '<' && afterindex < exp.Length - 1 && exp[afterindex] == '='){
                                afterindex = i + 3;
                                return AssignType.SHIFTLEFT;
                            }else goto default;
                        case '>':
                            if(charAfter == '>' && afterindex < exp.Length - 1 && exp[afterindex] == '='){
                                afterindex = i + 3;
                                return AssignType.SHIFTRIGHT;
                            }else goto default;
                        default:
                            beforeindex = -1;
                            afterindex  = -1;
                            return AssignType.NONE;
                    }
                }
                ++i;
            }
            return AssignType.NONE;
        }
    }

    class ExpressionStatement : Statement
    {
        internal ExpressionStatement(string exp, Colafe parent) : base(exp, parent){}

        internal static string EvalExpression(string exp, Colafe cf){
            bool isLiteralExp = exp.First() == '"' && exp.Last() == '"';
            bool isComplexExp = exp.ContainsUnescaped('(') || exp.Contains("&&") || exp.Contains("||");
            bool hasFunctions = exp.ContainsUnescaped('[');
            if(isLiteralExp)
                return EvalLiteralExpression(exp, cf);
            else if(isComplexExp)
                return EvalComplexExpression(exp, cf);
            else if(hasFunctions)
                return EvalSimpleExpressionWithFunctionCalls(exp, cf);
            else
                return EvalSimpleExpression(exp, cf);
        }

        internal static string EvalSimpleExpression(string exp, Colafe cf){
            exp = EvalLiteralExpression(exp, cf);
            if(exp.IsStringLiteral()) return exp;
            Preprocess.ReplaceTrueAndFalse(ref exp);
            Operator.ExecuteIsAssignedOperator(ref exp, cf);
            Operator.ExecutePrefixIncrementDecrementOperator(ref exp, cf);
            Variable.ReplaceVariableIdentifiers(ref exp, cf);
            return Maths.EvalArithmeticExp(exp);
        }

        internal static string EvalComplexExpression(string exp, Colafe cf){
            return exp.ProcessNestedStrings('(', ')', (e) => {
                e = Operator.ReplaceAndRunLogicGates(e, cf);
                return EvalSimpleExpressionWithFunctionCalls(e, cf);
            });
        }

        internal static string EvalSimpleExpressionWithFunctionCalls(string exp, Colafe cf){
            Function.CallFunctionsAndReplaceIn(ref exp, cf);
            return EvalSimpleExpression(exp, cf);
        }

        static string EvalLiteralExpression(string exp, Colafe cf){
            Literal.ReplaceLiteralVariablesInExpression(ref exp, cf);
            //Literal.EscapeLiteralsInExpression(ref exp);
            return exp;
        }

        internal override string Run(){
            AssignStatement assignment = AssignStatement.GetAssignStatementFromExp(_exp.Trim(), _parent);
            if(assignment != null)
                return assignment.Run();
            else
                return EvalExpression(_exp, _parent);
        }
    }

    class CallStatement : Statement
    {
        const string defaultComponent = "ColafeScript.Colafe";
        string pathToObjExp;
        string componentExp;
        string functionName;
        string arguments;
        GenericValue[] argumentsArray;

        bool isSimpleCall;
        bool isGlobalCall = false;

        internal CallStatement(string exp, Colafe parent) : base(exp, parent){
            isSimpleCall = !exp.ContainsUnescaped('#');
            if(isSimpleCall){
                //Simple call Funcname[]
                functionName = exp.Substring(0, exp.IndexOf('[')).Trim();
                pathToObjExp = null;
                componentExp = null;
                if(functionName[0] == '.'){
                    functionName = functionName.Trim('.');
                    isGlobalCall = true;
                }
            }
            else{
                exp = exp.Trim('#');
                // Complex call "Object"<component>Funcname[]
                functionName = exp.StringInBetween('>', '[').Trim();
                componentExp = exp.StringInBetween('<', '>').Trim();
                if(string.IsNullOrEmpty(componentExp))
                    componentExp = defaultComponent;
                else if(string.IsNullOrEmpty(componentExp.Trim('*')))
                    componentExp = defaultComponent + '*';
                if(exp.First() == '"'){
                    pathToObjExp = exp.Substring(0, exp.IndexOfUnescaped('<')).Trim().UnescapeString().Trim();
                    if(string.IsNullOrEmpty(pathToObjExp))
                        pathToObjExp = null;
                }
                else
                    pathToObjExp = null;
            }
            arguments = exp.StringInBetween('[', ']').Trim();
        }

        internal override string Run(){
            if(string.IsNullOrEmpty(arguments))
                argumentsArray = null;
            else
                argumentsArray = EvalArgumentsAndEnqueue(arguments);
            if(functionName == null)
                return null;
            if(isSimpleCall){
                // Simple call Funcname[]
                GenericValue simpleCallResult;
                if(isGlobalCall){
                    simpleCallResult = Colafe.CallGlobalFunction(functionName, argumentsArray);
                    return simpleCallResult != null ? simpleCallResult.Raw : null;
                }else{
                    simpleCallResult = _parent.CallFunction(functionName, argumentsArray);
                    if(simpleCallResult != null) return simpleCallResult.Raw;
                    else Msg.Err("ColafeScript: Function '" + functionName + "' not found.");
                }
            }
            else{
                // Complex call "Object"<component>Funcname[]
                GenericValue complexCallResult;
                List<Component> foundComponents = null;
                if(pathToObjExp == null)
                    foundComponents = Find.FindComponents(componentExp);
                else{
                    List<GameObject> foundObjects = Find.FindObjects(pathToObjExp);
                    if(foundObjects != null)
                        foundComponents = Find.FindComponentsInObjects(foundObjects, componentExp);
                }
                if(foundComponents != null){
                    if(foundComponents.Count == 1){
                        complexCallResult = (foundComponents.First() as IColafeCallable).CallFunction(functionName, argumentsArray);
                        if(complexCallResult != null) return complexCallResult.Raw;
                        else Msg.Err("ColafeScript: Function '" + functionName + "' not found.");
                    }else{
                        string[] results = new string[foundComponents.Count];
                        int i = 0;
                        foreach (MonoBehaviour component in foundComponents){
                            complexCallResult = (component as IColafeCallable).CallFunction(functionName, argumentsArray);
                            if (complexCallResult != null) results[i] = complexCallResult.Raw;
                            else Msg.Err("ColafeScript: Function '" + functionName + "' not found.");
                            ++i;
                        }
                        return string.Join("\n", results);
                    }
                }
            }
            return null;
        }

        internal GenericValue[] EvalArgumentsAndEnqueue(string arguments){
            MatchCollection matches = REGX.FIND_ARGUMENTS.Matches(arguments);
            GenericValue[] returnArr = new GenericValue[matches.Count];
            string currentArg;
            int count = 0;
            foreach (Match arg in REGX.FIND_ARGUMENTS.Matches(arguments)){
                currentArg = arg.Groups[1].Value.Trim();
                string expvalue = ExpressionStatement.EvalSimpleExpression(currentArg, _parent);
                returnArr[count++] = new GenericValue(expvalue);
            }
            return returnArr;
        }
    }

    class InvokeStatement : Statement
    {
        const string defaultComponent = "ColafeScript.Colafe";
        const string ERROR_NOT_FOUND = "[ColafeScript_ERROR]=> Objects/Components not found.";
        const string ERROR_FUNCNAME_MISSING = "[ColafeScript_ERROR]=> Function name missing in invoke statement.";
        string pathToObjExp;
        string componentExp;
        string functionName;
        float  startDelay =  0f;
        float  repeatRate = -1f;

        internal InvokeStatement(string exp, Colafe parent) : base(exp, parent){
            functionName = exp.StringInBetween('>', '(').Trim();
            if(string.IsNullOrEmpty(functionName))
                Msg.Err("ColafeScript: A function name is required for invoke '" + exp + "' .");
            componentExp = exp.StringInBetween('<', '>').Trim();
            if(string.IsNullOrEmpty(componentExp))
                componentExp = defaultComponent;
            else if(componentExp.Trim('*') == string.Empty)
                componentExp = defaultComponent + '*';
            if(exp.First() == '"'){
                pathToObjExp = exp.Substring(0, exp.IndexOfUnescaped('<')).Trim().UnescapeString().Trim();
                if(string.IsNullOrEmpty(pathToObjExp))
                    pathToObjExp = null;
            }
            else
                pathToObjExp = null;
            string arguments = exp.StringInBetween('(', ')').Trim();
            if(!string.IsNullOrEmpty(arguments)){
                string[] splitArgs = arguments.Split(',');
                switch(splitArgs.Length){
                    case 1: 
                        startDelay = float.Parse(splitArgs[0]);
                        break;
                    case 2:
                        startDelay = float.Parse(splitArgs[0]);
                        repeatRate = float.Parse(splitArgs[1]);
                        break;
                    default:
                        Msg.Err("ColafeScript: Invoke can only have 2 arguments: a delay and the repeat rate.");
                        break;
                }
            }
        }

        internal override string Run(){
            if(functionName == null)
                return ERROR_FUNCNAME_MISSING;

            List<Component> foundComponents = null;
            if(pathToObjExp == null)
                foundComponents = Find.FindComponents(componentExp);
            else{
                List<GameObject> foundObjects = Find.FindObjects(pathToObjExp);
                if(foundObjects != null)
                    foundComponents = Find.FindComponentsInObjects(foundObjects, componentExp);
            }

            if(foundComponents != null){
                foreach (MonoBehaviour component in foundComponents){
                    if(repeatRate < 0f)
                        component.Invoke(functionName, startDelay);
                    else
                        component.InvokeRepeating(functionName, startDelay, (float)repeatRate);
                }
                return string.Empty;
            }
            
            return ERROR_NOT_FOUND;
        }
    }

}