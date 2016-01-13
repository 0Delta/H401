using UnityEngine;
using System.Collections;
using System.Text;
using System.IO;

public class FileLoader{

    private byte[] dat = null;

    #if UNITY_ANDROID
    public IEnumerator Load(string path,Encoding encode)
#else
    public void Load(string path,Encoding encode)
#endif
    {
        string fullpath = "";

#if UNITY_ANDROID
        fullpath = "jar:file://" + Application.dataPath + "!/assets" + "/" + path;
        WWW www = new WWW(fullpath);
        yield return www;
        AssetBundle bundle = www.assetBundle;        // Load and retrieve the AssetBundle
        dat = encode.GetBytes(www.text);
#elif UNITY_IOS
        fullpath = Application.dataPath + "/Raw" + path;
        dat = File.ReadAllBytes(fullpath);
#else
        fullpath = Application.persistentDataPath + path;        
        dat = File.ReadAllBytes(fullpath);
#endif
    }

    public byte[] GetByte()
    {
        return dat;
    }
}

public class FileWriter
{
    public void Save(string path, byte[] dat)
    {
        string fullpath = "";

#if UNITY_ANDROID
        var sw = new System.IO.StreamWriter(Application.dataPath + "!/assets" + "/" + path);
        sw.Write(dat);
        //fullpath = "jar:file://" + Application.dataPath + "!/assets" + "/" + path;
        //WWW www = new WWW(fullpath);
        //yield return www;
        //AssetBundle bundle = www.assetBundle;        // Load and retrieve the AssetBundle        
#elif UNITY_IOS
        fullpath = Application.dataPath + "/Raw" + path;
        var sw = File.Create(fullpath);
        sw.Dispose();
        File.WriteAllBytes(fullpath,dat);
#else
        fullpath = Application.persistentDataPath + path;
        var sw = File.Create(fullpath);
        sw.Dispose();
        File.WriteAllBytes(fullpath, dat);
#endif
    }
}
