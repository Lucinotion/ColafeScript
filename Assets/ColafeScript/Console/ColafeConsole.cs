////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>                                                                                                        ///
/// Source file for the Built-In Colafe Game Console.                                                                ///
/// </summary>                                                                                                       ///
/// <note>                                                                                                           ///
/// Author: Luciano A. Candal                                                                                        ///
/// License: Mozilla Public License 2.0 <href>https://github.com/Lucinotion/ColafeScript/blob/main/LICENSE</href>    ///
/// Repository: This code is available at <href>https://github.com/Lucinotion/ColafeScript</href>                    ///
/// </note>                                                                                                          ///
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ColafeScript;
using ColafeScript.Utils;

/// <summary>
/// Toggle the console with Ctrl+` (` is the key to the left of 1 or under Esc)
/// </summary>
[HelpURL("https://github.com/Lucinotion/ColafeScript")]
public class ColafeConsole : Colafe
{
    [Space(10)]
    [Header("Open/Close with Ctrl + ` (the key before 1)")]
    [SerializeField]
    Text output;
    [SerializeField]
    InputField inputField;
    [SerializeField]
    ScrollRect scrollrect;
    [SerializeField]
    EventSystem eventSystem;
    [SerializeField]
    [Tooltip("Print the result of each expression.")]
    bool echo = false;

    bool visible = false;

    void HideConsole(){
        eventSystem.SetSelectedGameObject(null);
        inputField.DeactivateInputField();
        transform.GetChild(0).gameObject.SetActive(false);
        visible = false;
    }

    void ShowConsole(){
        transform.GetChild(0).gameObject.SetActive(true);
        eventSystem.SetSelectedGameObject(inputField.gameObject);
        inputField.ActivateInputField();
        inputField.Select();
        visible = true;
    }

    void ToggleConsole(){
        if(visible)
            HideConsole();
        else
            ShowConsole();
    }

    void PrintError(string err){
        output.text = output.text + "<color=red>==ERROR=================================\n" + err + "\n========================================</color>\n";
        Invoke("ScrollDown", 0.05f);
    }

    void PrintWarning(string wrn){
        output.text = output.text + "<color=yellow>~~WARNING~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n" + wrn + "\n~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~</color>\n";
        Invoke("ScrollDown", 0.05f);
    }

    void PrintLog(string msg){
        output.text = output.text + "<color=silver>--LOG-----------------------------------\n" + msg + "\n----------------------------------------</color>\n";
        Invoke("ScrollDown", 0.05f);
    }

    void PrintOutput(string str){
        if(echo)
            output.text = output.text + str + "\n";
        Invoke("ScrollDown", 0.05f);
    }

    void PrintWelcome(){
        output.text = "********** COLAFE SCRIPT v0.1 **********\n\n";
        Invoke("ScrollDown", 0.05f);
    }

    void PrintInput(string msg){
        output.text = output.text + ">>> " + msg + "\n";
    }

    void ScrollDown(){
        scrollrect.verticalNormalizedPosition = 0f;
    }

    void Submit(){
        PrintInput(inputField.text);
        PrintOutput(RunCode(inputField.text));
        inputField.text = string.Empty;
        eventSystem.SetSelectedGameObject(inputField.gameObject);
        inputField.ActivateInputField();
        inputField.Select();
    }

    void Awake(){
        Msg.OnErrorCallback = PrintError;
        Msg.OnWarningCallback = PrintWarning;
        Msg.OnLogCallback = PrintLog;
        PrintWelcome();
    }

    void LateUpdate() {
        if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.BackQuote)){
            ToggleConsole();
        }
        if(visible && Input.GetKeyDown(KeyCode.Return)){
            Submit();
        }
    }
}
