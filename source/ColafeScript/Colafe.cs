using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ColafeScript.Expressions;
using ColafeScript.Statements;
using ColafeScript.Utils;

namespace ColafeScript
{
    public interface IColafeCallable{
        GenericValue CallFunction(string functionName, GenericValue[] args);
    }
    
    /// <summary>
    /// A generic value, use the properties or the implicit conversions, not the constructor.
    /// </summary>
    public class GenericValue
    {
        readonly string rawValue = null;

        internal string Raw      { get => rawValue;                  }
        public   int    AsInt    { get => (int)AsFloat;              }
        public   bool   AsBool   { get => AsFloat.ToBool();          }
        public   short  AsShort  { get => (short)AsFloat;            }
        public  ushort  AsUShort { get => (ushort)AsFloat;           }
        public   float  AsFloat  { get => float.Parse(rawValue);     }
        public   double AsDouble { get => double.Parse(rawValue);    }
        public   string AsString { get => rawValue.IsStringLiteral() ? rawValue.UnescapeString() : Raw; }

        /// <summary>
        /// Only use the constructor internally to directly assign already escaped strings or values
        /// </summary>
        /// <param name="rawValue"></param>
        internal GenericValue (string rawValue) { this.rawValue = rawValue; }

        public static implicit operator int    (GenericValue val) => val.AsInt;
        public static implicit operator bool   (GenericValue val) => val.AsBool;
        public static implicit operator short  (GenericValue val) => val.AsShort;
        public static implicit operator ushort (GenericValue val) => val.AsUShort;
        public static implicit operator float  (GenericValue val) => val.AsFloat;
        public static implicit operator double (GenericValue val) => val.AsDouble;
        public static implicit operator string (GenericValue val) => val.AsString;

        public static implicit operator GenericValue (int    itg) => new GenericValue(itg.ToString());
        public static implicit operator GenericValue (bool   bln) => new GenericValue(bln ? "1" : "0");
        public static implicit operator GenericValue (short  srt) => new GenericValue(srt.ToString());
        public static implicit operator GenericValue (ushort ush) => new GenericValue(ush.ToString());
        public static implicit operator GenericValue (float  flt) => new GenericValue(flt.ToString("0.#########"));
        public static implicit operator GenericValue (double dbl) => new GenericValue(dbl.ToString("0.################"));
        public static implicit operator GenericValue (string str) => new GenericValue(str.EscapeString());

        public override string ToString(){ throw new System.NotImplementedException(); }
    }

    /// <summary>
    /// Main class used to execute code and call functions.
    /// </summary>
    public class Colafe : MonoBehaviour, IColafeCallable{
        /// <summary>
        /// Return this if your function returns void.
        /// </summary>
        public const char VOID = '\0';
        public delegate GenericValue FunctionCaller(string functionName, GenericValue[] args);
        static List<FunctionCaller>          globalFunctions = new List<FunctionCaller>();
        static Dictionary<string, GenericValue> globalMemory = new Dictionary<string, GenericValue>();
        Dictionary<string, GenericValue>         localMemory = new Dictionary<string, GenericValue>();

        /// <summary>
        /// Execute a piece of code at runtime.
        /// </summary>
        /// <param name="str">The code to run.</param>
        /// <returns>The output log of the program.</returns>
        public string RunCode(string str){
            return ColafeRunner.Run(str, this);
        }

        /// <summary>
        /// Adds a IColafeCallabe to the callback list of global functions.
        /// </summary>
        /// <param name="icc">The instace of an object implementing IColafeCallable.</param>
        public static void AddToGlobalFunctions(IColafeCallable icc){globalFunctions.Add(icc.CallFunction);}

        /// <summary>
        /// Adds a FunctionCaller to the callback list of global functions.
        /// </summary>
        /// <param name="fc">The FunctionCaller to add to the list.</param>
        public static void AddToGlobalFunctions(FunctionCaller fc){globalFunctions.Add(fc);}

        /// <summary>
        /// Returns the value of a variable in local or global memory.
        /// Make sure to add a Colafe instance if you are searching for a local variable.
        /// </summary>
        /// <param name="varname">The variable name, add a . at the start of the variable name if it's global.</param>
        /// <param name="cf">The instance with the local memory to search in.</param>
        /// <returns>The value if the variable is found, null otherwise.</returns>
        public static GenericValue Peek(string varname, Colafe cf = null){
            if(Variable.IsGlobalVariable(varname))
                return Colafe.PeekGlobal(varname.Trim('.'));
            else if(cf != null)
                return cf.PeekLocal(varname);
            else
                Msg.Err("ColafeScript: Tried to Peek '" + varname + "' but no reference to local memory was given as function argument.");
            return null;
        }

        /// <summary>
        /// Retrieves the value of a variable from local memory.
        /// </summary>
        /// <param name="varname">The name of the variable.</param>
        /// <returns>The variable's value or null if the variable is not found/defined</returns>
        public GenericValue PeekLocal(string varname){
            return localMemory.ContainsKey(varname) ? localMemory[varname] : null;
        }

        /// <summary>
        /// Retrieves the value of a variable from global memory.
        /// </summary>
        /// <param name="varname">The name of the variable to peek.</param>
        /// <returns>A generic value or null if the variable is unassigned</returns>
        public static GenericValue PeekGlobal(string varname){
            return Colafe.globalMemory.ContainsKey(varname) ? Colafe.globalMemory[varname] : null;
        }

        /// <summary>
        /// Saves a value to local or global memory.
        /// Make sure to add a Colafe instance if you are poking a local variable.
        /// </summary>
        /// <param name="varname">The variable name, add a . at the start of the variable name if it's global.</param>
        /// <param name="value">The value to set that variable to.</param>
        /// <param name="cf">The instance with the local memory to search in.</param>
        /// <returns>True if the variable is already assigned, flase orthewise.</returns>
        public static bool Poke(string varname, GenericValue value, Colafe cf = null){ 
            if(Variable.IsGlobalVariable(varname))
                return Colafe.PokeGlobal(varname.Trim('.'), value);
            else if(cf != null)
                return cf.PokeLocal(varname, value);
            else
                Msg.Err("ColafeScript: Tried to Poke '" + varname + "' but no reference to local memory was given as function argument.");
            return false;
        }

        /// <summary>
        /// Saves a value to local memory.
        /// </summary>
        /// <param name="varname">The name of the local variable to store the value in.</param>
        /// <param name="value">The geneirc value to store in the local variable.</param>
        /// <returns>true if the variable is already assigned, false otherwise.</returns>
        public bool PokeLocal(string varname, GenericValue value){
            if (localMemory.ContainsKey(varname)){
                localMemory[varname] = value;
                return true;
            }else{
                localMemory.Add(varname, value);
                return false;
            }
        }

        /// <summary>
        /// Saves a value to global memory.
        /// </summary>
        /// <param name="varname">The variable to save to.</param>
        /// <param name="value">The value to store.</param>
        /// <returns>true if the variable is already assigned, false otherwise.</returns>
        public static bool PokeGlobal(string varname, GenericValue value){
            if (Colafe.globalMemory.ContainsKey(varname)){
                Colafe.globalMemory[varname] = value;
                return true;
            }else{
                Colafe.globalMemory.Add(varname, value);
                return false;
            }
        }

        /// <summary>
        /// Checks if a global or local variable is Defined.
        /// Make sure to add a Colafe instance if you are checking a local variable.
        /// </summary>
        /// <param name="varname">The variable name, add a . at the start of the variable name if it's global.</param>
        /// <param name="cf">The instance with the local memory to search in.</param>
        /// <returns>True if defined, false otherwise.</returns>
        public static bool Defined(string varname, Colafe cf = null){
            if(Variable.IsGlobalVariable(varname))
                return Colafe.DefinedGlobal(varname.Trim('.'));
            else if(cf != null)
                return cf.DefinedLocal(varname);
            else
                Msg.Err("ColafeScript: Tried to check if Defined local variable '" + varname + "' but no reference to local memory was given as function argument.");
            return false;
        }

        /// <summary>
        /// Checks if a variable is defined in local memory.
        /// </summary>
        /// <param name="varname">The variable to check.</param>
        /// <returns>true if the variable is defined in local memory, false otherwise.</returns>
        public bool DefinedLocal(string varname){
            return localMemory.ContainsKey(varname) ? true : false;
        }

        /// <summary>
        /// Checks if a variable is defined in global memory.
        /// </summary>
        /// <param name="varname">The variable to check.</param>
        /// <returns>true if the variable is defined in global memory, false otherwise.</returns>
        public static bool DefinedGlobal(string varname){
            return Colafe.globalMemory.ContainsKey(varname) ? true : false;
        }

        /// <summary>
        /// Frees a variable from local or global memory.
        /// Make sure to add a Colafe instance if you are poking a local variable.
        /// </summary>
        /// <param name="varname">The variable name, add a . at the start of the variable name if it's global.</param>
        /// <param name="cf">The instance with the local memory to search in.</param>
        /// <returns>true if the variable was freed, false if the variable was not found/defined.</returns>
        public static bool Free(string varname, Colafe cf = null){
            if(Variable.IsGlobalVariable(varname))
                return FreeGlobal(varname.Trim('.'));
            else if(cf != null)
                return cf.FreeLocal(varname);
            else
                Msg.Err("ColafeScript: Tried to Free local variable '" + varname + "' but no reference to local memory was given as function argument.");
            return false;
        }

        /// <summary>
        /// Clears a variable defined in local memory.
        /// </summary>
        /// <param name="varname">The local variable to clear.</param>
        /// <returns>true if the variable was cleared, false if the variable was not found/defined.</returns>
        public bool FreeLocal(string varname){
            if (localMemory.ContainsKey(varname)){
                localMemory.Remove(varname);
                return true;
            }else
                return false;
        }

        /// <summary>
        /// Clears a particular variable defined in global memory.
        /// </summary>
        /// <param name="varname">The name of the global variable to clear.</param>
        /// <returns>true if the variable was freed, false if the variable was not found/defined.</returns>
        public static bool FreeGlobal(string varname){
            if(Colafe.globalMemory.ContainsKey(varname)){
                Colafe.globalMemory.Remove(varname);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Clears all variables from local or global memory.
        /// Make sure to add a Colafe instance if you are poking a local variable.
        /// </summary>
        /// <param name="cf">The instance with the local memory to search in.</param>
        /// <returns>true if the memory was sucessfully cleared, false if memory was already empty.</returns>
        public static bool Wipe(Colafe cf = null){
            if(cf == null)
                return Colafe.WipeGlobal();
            else
                return cf.WipeLocal();
        }

        /// <summary>
        /// Clears all variables defined in local memory.
        /// </summary>
        /// <returns>true if the memory was sucessfully cleared, false if memory was already empty.</returns>
        public bool WipeLocal(){
            if(localMemory.Any()){
                localMemory.Clear();
                return true;
            }else
                return false;
        }

        /// <summary>
        /// Clears all the variables defined in global memory.
        /// </summary>
        /// <returns>true if the memory was sucessfully cleared, false if memory was already empty.</returns>
        public static bool WipeGlobal(){
            if(Colafe.globalMemory.Any()){
                Colafe.globalMemory.Clear();
                return true;
            }else
                return false;
        }

        public static GenericValue CallGlobalFunction(string functionName, GenericValue[] args){
            GenericValue retval = null;
            foreach(FunctionCaller fc in globalFunctions){
                retval = fc(functionName, args);
                if(retval != null)
                    return retval;
            }
            Msg.Err("ColafeScript: Failed to call global function '" + functionName + "' : not found.");
            return null;
        }

        /// <summary>
        /// Prints a string to the console.
        /// </summary>
        /// <param name="str">The string to print to the console.</param>
        /// <returns>The string printed to the console.</returns>
        public static string Echo(string str){
            Msg.Log(str);
            return str;
        }

        /// <summary>
        /// Compares two strings.
        /// </summary>
        /// <param name="valueA">First string.</param>
        /// <param name="valueB">Second string.</param>
        /// <returns>true if they are the same, false otherwise.</returns>
        static bool CompareStr(GenericValue valueA, GenericValue valueB){
            return valueA.Raw == valueB.Raw ? true : false;
        }

        public virtual GenericValue CallFunction(string functionName, GenericValue[] args){
            switch (functionName){
                case  "Peek" :
                return Peek(args[0], this);
                case  "Poke" :
                return Poke(args[0], args[1].AsString, this);
                case  "Free" :
                return Free(args[0], this);
                case  "Defined":
                return Defined(args[0], this);
                case  "WipeG":
                return WipeGlobal();
                case  "Wipe":
                return WipeLocal();
                case  "Echo":
                return Echo(args[0]);
                case  "CompareStr":
                return CompareStr(args[0], args[1]);
                default:
                    return null; // return null if nothing was found
            }
        }
    }
}