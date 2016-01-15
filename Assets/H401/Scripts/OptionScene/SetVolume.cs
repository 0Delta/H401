using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class SetVolume : MonoBehaviour {
    [SerializeField]private UnityEngine.Audio.AudioMixer audioMixer;
    [SerializeField]private string volumeParam = null;
    // Use this for initialization
    private Slider volSlider;
    void Start()
    {
        volSlider = GetComponent<Slider>();

        volSlider.value = setParam;
    }
    public float setParam{
        set { audioMixer.SetFloat(volumeParam, Mathf.Lerp(-80, 0, value)); }
        get { float val; audioMixer.GetFloat(volumeParam, out val);  return Mathf.InverseLerp(-80,0,val); }
    }
}
