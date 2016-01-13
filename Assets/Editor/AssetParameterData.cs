using UnityEngine;
using UnityEditor;
using System.Collections;
 
public class AssetParameterData {
    public UnityEngine.Object obj { get; set; }            //!< アセットのObject自体
    public string path { get; set; }                    //!< アセットのパス
    public SerializedProperty property { get; set; }    //!< プロパティ
}