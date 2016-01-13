using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

/// <summary>
/// iOSでのビルドに必須なオブジェクトを生成します。
/// ビルド毎に実行されます。
/// </summary>
public static class PostProcessBuild
{
    const string CodeHead = "#import <Foundation/Foundation.h>\n extern \"C\" \n";

    static string RetFunction(string Name, string FuncRet)
    {
        return "char** " + Name + "() { return new char*(\"" + FuncRet + "\"); }\n";
    }

    [PostProcessBuild(100)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        FileStream f = new FileStream("Assets/Plugins/iOS/AWSSDK_iOS_Extension.mm", FileMode.Create, FileAccess.Write);
        BinaryWriter writer = new BinaryWriter(f);

        string str = "// this is AutoCreated Code for iOS. \n";

        str += CodeHead + "{\n";
        str += RetFunction("locale", "");
        str += RetFunction("title", "Linx");
        str += RetFunction("packageName", "");
        str += RetFunction("versionCode", "");
        str += RetFunction("versionName", "");
        str += "}\n";

        Debug.Log(str);

        writer.Write(System.Text.Encoding.UTF8.GetBytes(str.ToCharArray()));

        if (target == BuildTarget.iOS)
        {
            Debug.Log("iOS");
        }
    }
}

