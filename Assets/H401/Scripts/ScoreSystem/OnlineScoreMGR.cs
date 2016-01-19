using UnityEngine;
using RankingExtension;
using UniRx;

public class OnlineScoreMGR : MonoBehaviour {

    [SerializeField]public string AWSPrefabName = "";
    private DynamoConnecter AWS = null;
    private bool SendStack = false;
    private int StackScore = 0;
    private string StackName = "";

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
            DynamoConnecter.SetPear(GetComponentInParent<OnlineRankingMGR>());

            if(SendStack)
            {
                Send(StackScore, StackName);
                SendStack = false;
            }

            AWS.Read();
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
        if (AWS == null)
        {
            StackScore = Score;
            StackName = Name;
            SendStack = true;
        }
        else {
            AWS.Add(/*SListInstance[0].*/Score, Name/*NameEntryField.text*/);
        }
    }
    
	// Update is called once per frame
	void Update () {
	
	}
}
