////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>                                                                                                        ///
/// The unity editor window for the Colafe Interpreter loacated at Window>Colafe>Interpreter                         ///
/// </summary>                                                                                                       ///
/// <note>                                                                                                           ///
/// Author: Luciano A. Candal                                                                                        ///
/// License: Mozilla Public License 2.0 <href>https://github.com/Lucinotion/ColafeScript/blob/main/LICENSE</href>    ///
/// Repository: This code is available at <href>https://github.com/Lucinotion/ColafeScript</href>                    ///
/// </note>                                                                                                          ///
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using ColafeScript;

/// <summary>
/// The Colafe interpreter window for the unity editor. (Window>Colafe>Interpreter)
/// </summary>
public class ColafeInterpreterWindow : EditorWindow {
    string input;
    Vector2 scroll = new Vector2(0, Mathf.Infinity);
    Colafe colafe;
    GameObject colafeObject;

    [MenuItem("Window/Colafe/Interpreter")]
    private static void ShowWindow() {
        var window = GetWindow<ColafeInterpreterWindow>();
        window.titleContent = new GUIContent("Colafe Interpreter");
        window.Show();
    }

    void Awake(){
        colafeObject = new GameObject();
        colafeObject.name = "_ColafeInterpreterInstance";
        colafe = colafeObject.AddComponent<Colafe>();
    }

    void OnDestroy(){
        DestroyImmediate(colafeObject);
    }

    private void OnGUI() {
        input = EditorGUI.TextArea(new Rect(3, 3, position.width - 6, position.height - 35), input);
        if (GUI.Button(new Rect(0, position.height - 30, position.width, 25), "Run")){
            Debug.Log(colafe.RunCode(input));
        }
    }
}