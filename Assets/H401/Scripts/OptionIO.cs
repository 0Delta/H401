using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class OptionIO : MonoBehaviour
{
    [SerializeField]private UnityEngine.Audio.AudioMixer audioMixer;
    private AppliController _appController;
    public AppliController appController { set { _appController = value; } }
    public OptionIO()
    {
    }

    public static bool Save(string prefKey, OptionIOData serializableObject)
    {
        MemoryStream memoryStream = new MemoryStream();
#if UNITY_IPHONE || UNITY_IOS
		System.Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
#endif
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(memoryStream, serializableObject);

        string tmp = System.Convert.ToBase64String(memoryStream.ToArray());
        try
        {
            PlayerPrefs.SetString(prefKey, tmp);
        }
        catch (PlayerPrefsException)
        {
            return false;
        }
        return true;
    }

    public static OptionIOData Load(string prefKey)
    {
        if (!PlayerPrefs.HasKey(prefKey)) return default(OptionIOData);
#if UNITY_IPHONE || UNITY_IOS
		System.Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
#endif
        BinaryFormatter bf = new BinaryFormatter();
        string serializedData = PlayerPrefs.GetString(prefKey);

        MemoryStream dataStream = new MemoryStream(System.Convert.FromBase64String(serializedData));
        OptionIOData deserializedObject = (OptionIOData)bf.Deserialize(dataStream);

        return deserializedObject;
    }
    public void save()
    {
        OptionIOData opData = new OptionIOData();
        audioMixer.GetFloat("MasterVolume", out opData.fMasterVol); opData.fMasterVol = Mathf.InverseLerp(-80, 0, opData.fMasterVol);
        audioMixer.GetFloat("MusicVolume", out opData.fBGMVol); opData.fBGMVol = Mathf.InverseLerp(-80, 0, opData.fBGMVol);
        audioMixer.GetFloat("SEVolume", out opData.fSEVol); opData.fSEVol = Mathf.InverseLerp(-80, 0, opData.fSEVol);
        opData.bGyroEnable = _appController.gyroEnable;

        save(opData);

    }
    public void save(OptionIOData data)
    {
        // 保存用クラスにデータを格納.
        OptionIO.Save("OptionData", data);
        PlayerPrefs.Save();
    }

    public OptionIOData load()
    {
        OptionIOData data_tmp = OptionIO.Load("OptionData");
        if (data_tmp != null)
        {
            {
                audioMixer.SetFloat("MasterVolume", Mathf.Lerp(-80, 0, data_tmp.fMasterVol));
                audioMixer.SetFloat("MusicVolume", Mathf.Lerp(-80, 0, data_tmp.fBGMVol));
                audioMixer.SetFloat("SEVolume", Mathf.Lerp(-80, 0, data_tmp.fSEVol));
                _appController.gyroEnable = data_tmp.bGyroEnable;

            }
            return data_tmp;
        }
        else
        {
            return null;
        }
    }
}