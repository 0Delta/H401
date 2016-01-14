/// <summary>
/// Inspectorを拡張します。
/// Editor下なので、実環境でビルドされません。
/// </summary>
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Node))]
public class Node_Editor : Editor
{
    private Node tgt = null;
    private bool DebugStringfoldout = false;
    private Vector2 scrollPosition;
    GUIStyle guiStyle = new GUIStyle();
    public override void OnInspectorGUI()
    {
        tgt = target as Node;
        EditorGUILayout.LabelField(tgt.ToString());

        string str = "   " +
        (tgt.bitLink[5] ? "LU" : "  ") + " " +
        (tgt.bitLink[0] ? "RU" : "  ") + "  \n " +
        (tgt.bitLink[4] ? "L" : "  ") + "   ■   " +
        (tgt.bitLink[1] ? "R" : " ") + "\n   " +
        (tgt.bitLink[3] ? "LD" : "  ") + " " +
        (tgt.bitLink[2] ? "RD" : "  ") + "  \n";
        EditorGUILayout.LabelField(str, GUILayout.Height(58f));

        str = "";
        str += tgt.IsAction ? "Action" : "NoAct";
        str += "  ";
        str += tgt.IsSlideStart ? "SliSt" : "     ";
        str += "  ";
        str += tgt.IsSlideStart ? "Slide" : "     ";
        str += "  ";
        str += tgt.IsSlideEnd ? "SliEd" : "     ";
        str += "  ";
        str += tgt.IsTurning ? "Turn" : "    ";
        EditorGUILayout.LabelField(str, GUILayout.Height(20f));

        DebugStringfoldout = EditorGUILayout.Foldout(DebugStringfoldout, "DebugStr");
        if (DebugStringfoldout)
        {
            float Height = guiStyle.CalcSize(new GUIContent(tgt.DebugLog)).y;
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150f));
            EditorGUILayout.SelectableLabel(tgt.DebugLog, GUILayout.MinHeight(Height));
            EditorGUILayout.EndScrollView();
        }

        base.OnInspectorGUI();
    }
}
[CustomEditor(typeof(DynamoConnecter))]
public class AWS_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        var tgt = target as DynamoConnecter;

        tgt.UseProxy = EditorGUILayout.Toggle("UseProxy", tgt.UseProxy);
        if (tgt.UseProxy)
        {
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
public class Score3D_Editor : Editor
{
    public override void OnInspectorGUI()
    {
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

class SpriteLoader
{
    static public string SpritePath = "";
    static public Sprite[] spr = null;
    static public Texture2D tex = null;
}
[CustomEditor(typeof(NodeController))]
public class NodeController_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        try {
            var tgt = target as NodeController;
            SpriteLoader.SpritePath = tgt.gameNodeSpritePath;
            var spr = Resources.LoadAll<Sprite>(SpriteLoader.SpritePath);
            if (spr != null)
            {
                SpriteLoader.spr = spr;
            }
            else
            {
                SpriteLoader.spr = null;
                SpriteLoader.tex = null;
            }
            var tex = spr[0].texture;
            if (tex != null)
            {
                SpriteLoader.tex = tex;
            }
            else
            {
                SpriteLoader.tex = null;
            }
        }
        catch {
            SpriteLoader.spr = null;
            SpriteLoader.tex = null;
        }

        base.OnInspectorGUI();
    }
}

[CustomPropertyDrawer(typeof(NodeTemplate))]
public class NodeTemplate_Editor : PropertyDrawer
{
    string MatBak = "";
    Material Mat = null;
    Texture2D texture = new Texture2D(1, 1);

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        //return Screen.width < 333 ? (16f + 18f) : 16f;
        return 80f;

    }

    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 初期設定
        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.indentLevel = 0;

        // ワーク用の場所を設定
        Rect contentPosition = position;
        var Link = property.FindPropertyRelative("LinkDir");
        float ImagePosXRevision = position.width / -4f;
        var sIdx = property.FindPropertyRelative("SpriteIdx").intValue;
        var mIdx = property.FindPropertyRelative("MaskIdx").intValue;


        // 外枠描画
        contentPosition.height -= 3f;
        EditorGUI.DrawRect(contentPosition, Color.gray);

        EditorGUI.BeginChangeCheck();
        // 入力フィールド
        EditorGUIUtility.labelWidth = position.width * 0.25f;
        contentPosition.height = 16f;
        // スプライトのインデックス
        contentPosition.width = (position.width - 15f)/4f;
        EditorGUI.LabelField(contentPosition, "SpriteIndex");
        contentPosition.x += (position.width - 15f) / 4f;
        property.FindPropertyRelative("SpriteIdx").intValue = EditorGUI.IntField(contentPosition, sIdx);
        // マスクのインデックス
        contentPosition.x += (position.width - 15f) / 4f + 10f;
        EditorGUI.LabelField(contentPosition, "MaskIndex");
        contentPosition.x += (position.width - 15f) / 4f;
        property.FindPropertyRelative("MaskIdx").intValue = EditorGUI.IntField(contentPosition, mIdx);

        // 画像の表示
        try
        {
            if (SpriteLoader.tex != null)
            {
                if (SpriteLoader.spr.Length > sIdx && sIdx > -1)
                {
                    contentPosition.x = position.width / 2f - 20f + ImagePosXRevision;
                    contentPosition.y = position.y + 25f;
                    contentPosition.width = 40f;
                    contentPosition.height = 40f;
                    var rect = SpriteLoader.spr[sIdx].rect;
                    rect.x /= SpriteLoader.tex.width;
                    rect.width /= SpriteLoader.tex.width;
                    rect.y /= SpriteLoader.tex.height;
                    rect.height /= SpriteLoader.tex.height;
                    GUI.DrawTextureWithTexCoords(contentPosition, SpriteLoader.tex, rect);
                }
                else
                {
                    contentPosition.x = position.width / 3f;
                    contentPosition.y = position.y + position.height / 2f;
                    contentPosition.width = position.width / 4f * 3f;
                    EditorGUI.LabelField(contentPosition, "Error : Length Error");
                }
                if (SpriteLoader.spr.Length > mIdx && mIdx > -1)
                {
                    contentPosition.x = position.width / 2f - 20f - ImagePosXRevision;
                    contentPosition.y = position.y + 25f;
                    contentPosition.width = 40f;
                    contentPosition.height = 40f;
                    var rect = SpriteLoader.spr[mIdx].rect;
                    rect.x /= SpriteLoader.tex.width;
                    rect.width /= SpriteLoader.tex.width;
                    rect.y /= SpriteLoader.tex.height;
                    rect.height /= SpriteLoader.tex.height;
                    GUI.DrawTextureWithTexCoords(contentPosition, SpriteLoader.tex, rect);
                }
                else
                {
                    contentPosition.x = position.width / 3f * 2f;
                    contentPosition.y = position.y + position.height / 2f;
                    contentPosition.width = position.width / 4f * 3f;
                    EditorGUI.LabelField(contentPosition, "Error : Length Error");
                }
            }
            else
            {
                contentPosition.x = position.width / 3f;
                contentPosition.y = position.y + position.height / 2f;
                contentPosition.width = position.width / 4f * 3f;
                EditorGUI.LabelField(contentPosition, "Error : Texture Null");
            }
        }
        catch (System.Exception excep)
        {
            Debug.LogWarning(excep.ToString());
        }

        contentPosition.width = 16f;
        // 各チェックボックス
        if (Link != null && Link.arraySize > 5)
        {
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
            contentPosition.x = position.width / 5f * 2f;
            contentPosition.y = position.y + position.height / 4f * 3;
            int Cnt = 0;
            for (int n = 0; n < 6; n++)
            {
                if (Link.GetArrayElementAtIndex(n).boolValue)
                {
                    Cnt++;
                }
            }
            EditorGUI.LabelField(contentPosition, Cnt.ToString());

        }
        EditorGUI.EndChangeCheck();

        // 終了
        EditorGUI.EndProperty();
    }

    public class EditorExWindow : EditorWindow
    {

        int SelectLogIdx;
        private static Vector2 ScrollPos = new Vector2();
        int Exported = 0;

        [MenuItem("Window/CDebugLogConsole")]
        static void Open()
        {
            GetWindow<EditorExWindow>("CustomLogger");
        }

        void OnGUI()
        {
            if (CustomDebugLog.CDebugLog.InstanceList.Count == 0)
            {
                EditorGUILayout.LabelField("Not Run CDebugLog");
            }
            else {
                string[] LogList = new string[CustomDebugLog.CDebugLog.InstanceList.Count];
                CustomDebugLog.CDebugLog.InstanceList.Keys.CopyTo(LogList, 0);

                SelectLogIdx = EditorGUILayout.Popup(SelectLogIdx, LogList, GUILayout.ExpandWidth(true));
                CustomDebugLog.CDebugLog Log = null;
                try
                {
                    CustomDebugLog.CDebugLog.InstanceList.TryGetValue(LogList[SelectLogIdx], out Log);
                }
                catch (System.IndexOutOfRangeException)
                {
                    SelectLogIdx = 0;
                }
                var ExportBtn = GUILayout.Button("Export", GUILayout.ExpandWidth(true));
                if (Log != null)
                {
                    ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos);
                    EditorGUILayout.SelectableLabel(Log.ToStringReverse(100), GUILayout.Height(Log.Count < 100 ? Log.Count : 100 * EditorGUIUtility.singleLineHeight));
                    EditorGUILayout.EndScrollView();
                }
                else {
                    EditorGUILayout.LabelField("faled get Instance");
                }

                // ExportBtn
                if (ExportBtn && Exported <= 0)
                {
                    Exported = 20;
                    System.IO.File.WriteAllText(Application.persistentDataPath + "/" + LogList[SelectLogIdx] + ".log", Log.ToStringReverse());
                }
                else if (Exported > 0)
                {
                    Exported--;
                }
            }
        }

        void Update()
        {
            Repaint();
        }
    }
}
