using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

[Conditional("UNITY_EDITOR")]
class NodeSaver : System.Attribute
{
    private static FileStream Stream = null;
    private static StreamWriter Writer = null;


    public static void Write(List<NodeController.FinNode> NodeList)
    {
        //Stream = new FileStream(Application.dataPath + "/NodeCash", FileMode.Append, FileAccess.Write);
        //Writer = new StreamWriter(Stream);
        //foreach (var it in NodeList)
        //{
        //    Writer.WriteLine(it.ToString());
        //}
        //Writer.WriteLine();
        //Writer.Flush();
        //Writer.Close();
    }
}
