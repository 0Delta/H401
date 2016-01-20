using UnityEngine;
using RankingExtension;
using UniRx;
using System.Collections.Generic;

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
        }
        catch
        {
            AWS = null;
            return;
        }

        Observable
            .EveryUpdate()
            .Where(_ => AWS.isReady)
            .Take(1)
            .Subscribe(_ =>
            {
                if (SendStack)
                {
                    Send(StackScore, StackName);
                    SendStack = false;
                }
                AWS.Read();
            }).AddTo(this);

    }

    /// <summary>
    /// AWSに送信する
    /// </summary>
    public void Send(int Score, string Name)
    {
        if (AWS == null || !AWS.isReady)
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

    public List<int> GetOnlineRanking()
    {
        return AWS.OnlineScore;
    }
}
