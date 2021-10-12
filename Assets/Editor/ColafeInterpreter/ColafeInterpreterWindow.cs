using UnityEngine;
using UnityEditor;
using ColafeScript;

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