﻿using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Node))]
public class Node_Editor : Editor {
    private Node tgt = null;
    private bool DebugStringfoldout = false;
    private Vector2 scrollPosition;
    GUIStyle guiStyle = new GUIStyle();
    public override void OnInspectorGUI() {
        tgt = target as Node;
        EditorGUILayout.LabelField(tgt.ToString());

        string str = "   "+
        (tgt.bitLink[5] ? "LU" : "  ") + " " +
        (tgt.bitLink[0] ? "RU" : "  ") + "  \n " +
        (tgt.bitLink[4] ? "L" : "  ") + "   ■   " +
        (tgt.bitLink[1] ? "R" : " ") + "\n   " +
        (tgt.bitLink[3] ? "LD" : "  ") + " " +
        (tgt.bitLink[2] ? "RD" : "  ") + "  \n";
        EditorGUILayout.LabelField(str, GUILayout.Height(58f));

        DebugStringfoldout = EditorGUILayout.Foldout(DebugStringfoldout,"DebugStr");
        if(DebugStringfoldout) {
            float Height = guiStyle.CalcSize(new GUIContent(tgt.DebugLog)).y;
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition,GUILayout.Height(150f));
            EditorGUILayout.SelectableLabel(tgt.DebugLog, GUILayout.MinHeight(Height));
            EditorGUILayout.EndScrollView();
        }

        base.OnInspectorGUI();
    }
}
[CustomEditor(typeof(DynamoConnecter))]
public class AWS_Editor : Editor {
    public override void OnInspectorGUI() {
        var tgt = target as DynamoConnecter;

        tgt.UseProxy = EditorGUILayout.Toggle("UseProxy", tgt.UseProxy);
        if(tgt.UseProxy) {
            EditorGUILayout.HelpBox("But this function is \"Lazy\"", MessageType.Info, true);
            tgt.ProxyHost = EditorGUILayout.TextField("ProxyHost", tgt.ProxyHost);
            tgt.ProxyPort = EditorGUILayout.IntField("Port", tgt.ProxyPort);
            tgt.UserName = EditorGUILayout.TextField("UserName", tgt.UserName);
            tgt.Password = EditorGUILayout.PasswordField("Password", tgt.Password);
            EditorGUILayout.LabelField("DefaultName", DynamoConnecter.DefaultName);
        }
    }
}

[CustomEditor(typeof(Score3D))]
public class Score3D_Editor : Editor {
    public override void OnInspectorGUI() {
        EditorGUILayout.HelpBox("This is \"DEBUG ONLY\". \n Reset Value at the time of play", MessageType.Info, true);
        GUILayoutOption[] options = { GUILayout.MaxWidth(25.0f), GUILayout.MinWidth(25.0f), GUILayout.ExpandWidth(false) };
        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField("α1", options);
        Score3D.AlphaOneZ = EditorGUILayout.FloatField(Score3D.AlphaOneZ);
        EditorGUILayout.LabelField("α0", options);
        Score3D.AlphaZeroZ = EditorGUILayout.FloatField(Score3D.AlphaZeroZ);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.MinMaxSlider(ref Score3D.AlphaOneZ, ref Score3D.AlphaZeroZ, 0, 150);
    }
}

[CustomPropertyDrawer(typeof(NodeTemplate))]
public class NodeController_Editor : PropertyDrawer {
    string MatBak = "";
    Material Mat = null;
    Texture2D texture = new Texture2D(1, 1);

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        //return Screen.width < 333 ? (16f + 18f) : 16f;
        return 80f;

    }

    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

        // 初期設定
        EditorGUI.BeginProperty(position, label, property);
        //Rect contentPosition = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), GUIContent.none);
        EditorGUI.indentLevel = 0;

        // ワーク用の場所を設定
        Rect contentPosition = position;
        var Link = property.FindPropertyRelative("LinkDir");
        float ImagePosXRevision = position.width / -4f;
        

        // 外枠描画
        contentPosition.height -= 3f;
        EditorGUI.DrawRect(contentPosition, Color.gray);

        EditorGUI.BeginChangeCheck();
        // マテリアル名の入力フィールド
        EditorGUIUtility.labelWidth = position.width * 0.25f;
        contentPosition.height = 16f;
        contentPosition.width = position.width - 5f;
        EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("MaterialName"), new GUIContent("Mat"));

        // 画像
        var path = property.FindPropertyRelative("MaterialName").stringValue;
        if(path.Length != 0) {
            bool TrueTex = false;
            if(path != MatBak) {
                Mat = Resources.Load<Material>(path);
                if(Mat != null) {
                    texture = Mat.GetTexture("_MainTex") as Texture2D;
                    texture.filterMode = FilterMode.Point;
                    MatBak = path;
                    TrueTex = true;
                }
            } else {
                TrueTex = true;
            }
            if(texture != null && TrueTex) {
                contentPosition.x = position.width / 2f - 20f + ImagePosXRevision;
                contentPosition.y = position.y + 25f;
                contentPosition.width = 40f;
                contentPosition.height = 40f;
                EditorGUI.DrawPreviewTexture(contentPosition, texture);
            }

            contentPosition.width = 16f;
            // 各チェックボックス
            if(Link != null && Link.arraySize > 5) {
                contentPosition.x = position.width / 2f + 12f + ImagePosXRevision;
                contentPosition.y = position.y + 17f;
                Link.GetArrayElementAtIndex(5).boolValue = EditorGUI.Toggle(contentPosition, Link.GetArrayElementAtIndex(5).boolValue);

                contentPosition.x = position.width / 2f + 22f + ImagePosXRevision;
                contentPosition.y = position.y + 17f + 20f;
                Link.GetArrayElementAtIndex(4).boolValue = EditorGUI.Toggle(contentPosition, Link.GetArrayElementAtIndex(4).boolValue);

                contentPosition.x = position.width / 2f + 12f + ImagePosXRevision;
                contentPosition.y = position.y + 17f + 40f;
                Link.GetArrayElementAtIndex(3).boolValue = EditorGUI.Toggle(contentPosition, Link.GetArrayElementAtIndex(3).boolValue);

                contentPosition.x = position.width / 2f - 25f + ImagePosXRevision;
                contentPosition.y = position.y + 17f + 40f;
                Link.GetArrayElementAtIndex(2).boolValue = EditorGUI.Toggle(contentPosition, Link.GetArrayElementAtIndex(2).boolValue);

                contentPosition.x = position.width / 2f - 35f + ImagePosXRevision;
                contentPosition.y = position.y + 17f + 20f;
                Link.GetArrayElementAtIndex(1).boolValue = EditorGUI.Toggle(contentPosition, Link.GetArrayElementAtIndex(1).boolValue);

                contentPosition.x = position.width / 2f - 25f + ImagePosXRevision;
                contentPosition.y = position.y + 17f;
                Link.GetArrayElementAtIndex(0).boolValue = EditorGUI.Toggle(contentPosition, Link.GetArrayElementAtIndex(0).boolValue);

                // ステータス
                contentPosition.x = position.width / 3f * 2;
                contentPosition.y = position.y + position.height / 2f;
                int Cnt = 0;
                for(int n = 0; n < 6; n++) {
                    if(Link.GetArrayElementAtIndex(n).boolValue) {
                        Cnt++;
                    }
                }
                EditorGUI.LabelField(contentPosition, Cnt.ToString());
            } else {
                contentPosition.x = position.width / 3f;
                contentPosition.y = position.y + position.height / 2f;
                contentPosition.width = position.width / 4f * 3f;
                if(Link == null) {
                    EditorGUI.LabelField(contentPosition, "Error : Array is NULL");
                } else {
                    EditorGUI.LabelField(contentPosition, "Error : Array Size - " + Link.arraySize.ToString());
                }
            }
        }
        EditorGUI.EndChangeCheck();

        // 終了
        EditorGUI.EndProperty();
    }


}

public class EditorExWindow : EditorWindow {

    int SelectLogIdx;

    [MenuItem("Window/CDebugLogConsole")]
    static void Open() {
        GetWindow<EditorExWindow>("CustomLogger");
    }

    void OnGUI() {
        if(CustomDebugLog.CDebugLog.InstanceList.Count == 0) {
            EditorGUILayout.LabelField("Not Run CDebugLog");
        } else {
            string[] LogList = new string[CustomDebugLog.CDebugLog.InstanceList.Count];
            CustomDebugLog.CDebugLog.InstanceList.Keys.CopyTo(LogList, 0);

            SelectLogIdx = EditorGUILayout.Popup(SelectLogIdx, LogList, GUILayout.ExpandWidth(true));
            CustomDebugLog.CDebugLog Log;
            CustomDebugLog.CDebugLog.InstanceList.TryGetValue(LogList[SelectLogIdx], out Log);
            if(Log != null) {
                Rect rc = new Rect(0, 0, position.width, position.height);
                EditorGUI.LabelField(rc,Log.ToStringReverse());
            } else {
                EditorGUILayout.LabelField("faled get Instance");
            }
        }
    }

    void Update() {
        Repaint();
    }
}