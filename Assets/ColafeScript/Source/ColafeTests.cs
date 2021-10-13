////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>                                                                                                        ///
/// Sute of tests for the Colafe module.                                                                             ///
/// </summary>                                                                                                       ///
/// <note>                                                                                                           ///
/// Author: Luciano A. Candal                                                                                        ///
/// License: Mozilla Public License 2.0 <href>https://github.com/Lucinotion/ColafeScript/blob/main/LICENSE</href>    ///
/// Repository: This code is available at <href>https://github.com/Lucinotion/ColafeScript</href>                    ///
/// </note>                                                                                                          ///
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using ColafeScript.Arithmetic;
using ColafeScript.Utils;
using UnityEngine;

namespace ColafeScript.Tests
{
    static class ColafeTester
    {
        static int errorcount = 0;

        internal static float FuncF (float  num) {return  num;}
        internal static bool  FuncB (bool   bln) {return !bln;}
        internal static int   FuncI (int    num) {return  num;}
        internal static void  FuncV (          ) {Debug.Log("called the FuncV void function");}
        internal static void  Printf(string txt) {Debug.Log(txt);}

        internal static float AddTwo(float A, float B) {return A + B;}

        internal static int Factorial(int number){
            int fact = 1;
            for (int i = 1; i <= number; i++){
                fact *= i;
            }
            return fact;
        }

        internal static void ArithTest()
        {
            string[] sumtest = new string[]{
            "1",
            "1.5",
            "-5",
            "-1.5",
            "+5",
            "+5.5",
            "--5",
            "+-5",
            "++5",
            "-+5",
            "5-5",
            "5-2.5",
            "5+5",
            "5+2.5",
            "-5-6",
            "+5-6",
            "-5.2-6.5",
            "+5.5-6.5",
            "2-5+5",
            "5+10737418-2",
            "+5-5+5",
            "-5++5--5",
            "5.2*4",
            "-5*-5",
            "+5*-0.2",
            "5.5*-5",
            "-5.1*+5.2*-2",
            "5/2",
            "-5/2",
            "2/-0.5",
            "-5.1/-5.2/2",
            "+5.1*-5/2",
            "2%8",
            "2.5%8",
            "-2.3%+8.4",
            "+2.3%-8.4",
            "2.5%8-2.3%+8.4",
            "2.5%8-2.3%+8.4-5.1*+5.2/-2",
            "2.5+8-2.3-8.4-5.1+5.2-2",
            "1==1",
            "1==0",
            "1==0==1",
            "2==2",
            "2.5==4.5",
            "5!=5",
            "5>6",
            "6>=5",
            "7.5>10",
            "80<80",
            "90<=90",
            "!1",
            "!0",
            "!!1",
            "~256-5+8/9",
            "256>>2",
            "256<<2",
            "14&27",
            "4|27",
            "6^12",
            "4*5/60*40/3*64+5-1-1"
            };

            float[] cssumtest = new float[]{
            1f,
            1.5f,
            -5f,
            -1.5f,
            +5f,
            +5.5f,
            +5f,
            +-5f,
            +5f,
            -+5f,
            5f-5f,
            5f-2.5f,
            5f+5f,
            5f+2.5f,
            -5f-6f,
            +5f-6f,
            -5.2f-6.5f,
            +5.5f-6.5f,
            2f-5f+5f,
            5f+10737418f-2f,
            +5f-5f+5f,
            -5f+5f+5f,
            5.2f*4f,
            -5f*-5f,
            +5f*-0.2f,
            5.5f*-5f,
            -5.1f*+5.2f*-2f,
            5f/2f,
            -5f/2f,
            2f/-0.5f,
            -5.1f/-5.2f/2f,
            +5.1f*-5f/2f,
            2f%8f,
            2.5f%8f,
            -2.3f%+8.4f,
            +2.3f%-8.4f,
            2.5f%8f-2.3f%+8.4f,
            2.5f%8f-2.3f%+8.4f-5.1f*+5.2f/-2f,
            2.5f+8f-2.3f-8.4f-5.1f+5.2f-2f,
            (true==true).ToFloat(),
            (true==false).ToFloat(),
            (true==false==true).ToFloat(),
            (2==2).ToFloat(),
            (2.5f==4.5f).ToFloat(),
            (5!=5).ToFloat(),
            (5>6).ToFloat(),
            (6>=5).ToFloat(),
            (7.5>10).ToFloat(),
            (80<80).ToFloat(),
            (90<=90).ToFloat(),
            (!true).ToFloat(),
            (!false).ToFloat(),
            (!!true).ToFloat(),
            (~(ushort)256)-5f+8f/9f,
            256>>2,
            256<<2,
            0b01110&0b11011,
            0b00100|0b11011,
            0b00110^0b01100,
            4f*5f/60f*40f/3f*64f+5f-1f-1f
            };

            for (int i = 0; i < sumtest.Length; i++){
                if (float.Parse(Maths.EvalArithmeticExp(sumtest[i])).ToString("F") != cssumtest[i].ToString("F")){
                    Debug.LogError("Expression: " + sumtest[i] + " returned " + Maths.EvalArithmeticExp(sumtest[i]) + " should return " + cssumtest[i] + " - FAIL");
                    errorcount++;
                }
                else{
                    Debug.Log("Expression: " + sumtest[i] + " returned " + Maths.EvalArithmeticExp(sumtest[i]) + " - SUCCESS");
                }
            }

            CheckErrors();
        }

        private static void CheckErrors(){
            if (errorcount == 0)
                Debug.Log("-- All tests finished sucessfully, you can go to sleep now space cowboy. --");
            else
                Debug.LogError("--> " + errorcount +" errors found!");
        }
    }
}