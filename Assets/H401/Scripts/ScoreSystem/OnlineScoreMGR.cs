using UnityEngine;
using RankingExtension;

public class OnlineScoreMGR : MonoBehaviour {

    [SerializeField]public string AWSPrefabName = "";
    private DynamoConnecter AWS = null;

    /// <summary>
    /// AWSシステムを起動させる
    /// </summary>
    void Start()
    {
        try
        {
            GameObject AWSObj = this.InstantiateChild(AWSPrefabName);
            AWSObj.name = "AWS";
            AWS = AWSObj.GetComponent<DynamoConnecter>();
        }
        catch
        {
            AWS = null;
        }
    }

    /// <summary>
    /// AWSに送信する
    /// </summary>
    public void Send(int Score, string Name)
    {
        AWS.Add(/*SListInstance[0].*/Score, Name/*NameEntryField.text*/);
    }
    
	// Update is called once per frame
	void Update () {
	
	}
}
