using UnityEngine;
using System.Collections.Generic;
using Amazon;
using Amazon.Runtime;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DataModel;
using System.IO;
using CustomDebugLog;

public class DynamoConnecter : MonoBehaviour {
    // 固有関数
    //private static readonly CDebugLog Log = new CDebugLog("DynamoConnecter");
    [SerializeField]    public bool UseProxy = false;
    [SerializeField]    public string ProxyHost = null;
    [SerializeField]    public int ProxyPort = 8080;
    [SerializeField]    public string UserName = null;
    [SerializeField]    public string Password = null;
    [SerializeField]    public const string DefaultName = "NoName";

    public const string TABLE_NAME = "H401";                                    // テーブルの名前
    private string publicKey = "AKIAJLMLMLC4KEYK77LQ";                            // 接続用ID
    private string secretKey = "QmbJyIdcQ+db4jho2qaea6ZdWaaU/60La8lxbCdP";        // 接続用PASSWORD
    private AmazonDynamoDBClient client = null;                                   // コネクタ
    private static OnlineRankingMGR Pear = null;
    private ScoreManager ScoreMGR = null;
    private bool Ready = false;
    public bool isReady { get { return Ready; } }

    public static void SetPear(OnlineRankingMGR pear)
    {
        if (Pear == null && pear != null)
        {
            Pear = pear;
        }
    }

    private List<ScoreTable> ScoreCash = null;
    public List<int> OnlineScore
    {
        get {
            if(ScoreCash == null)
            {
                return null;
            }
            var ret = new List<int>();
            foreach(var it in ScoreCash)
            {
                ret.Add(it.Score);
            }
            return ret;
        }
    }

    // テーブル定義(DBと同期させること)
    [DynamoDBTable(TABLE_NAME)]
    private class ScoreTable {
        // 変数群
        public string ID { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
        public long Date { get; set; }

        // DBへ通信するための辞書データを返す。
        public Dictionary<string, AttributeValue> GetDictionary() {
            return new Dictionary<string, AttributeValue> {
                { "ID",new AttributeValue {S = ID } },
                {"Name",new AttributeValue {S = Name } },
                {"Date",new AttributeValue {N = Date.ToString() } },
                {"Score",new AttributeValue {N = Score.ToString() } }
            };
        }

        // デフォルトコンストラクタ 全ての値を不正値に
        public ScoreTable() {
            ID = "null";
            Name = DefaultName;
            Score = -1;
            Date = -1;
        }

        // コンストラクタ。正しく引数を与えて初期化。
        public ScoreTable(string iName, int iScore) {
            ID = SystemInfo.deviceUniqueIdentifier.ToString();
            Date = long.Parse(System.DateTime.Now.ToString("yyMMddHHmmss", System.Globalization.DateTimeFormatInfo.InvariantInfo));
            Name = iName;
            Score = iScore;
        }

    // [Debug] 文字列変換
    override public string ToString() {
            return
             "   ID [" + ID + "]\n" +
             " Name:" + Name + "\n" +
             " Date:" + Date.ToString() + "\n" +
             "Score " + Score.ToString() + "\n";
        }
    };

    
    // Use this for initialization
    void Start() {
        //Log.Debug("Start");
        //キーをロード
        // データを読みだして暗号化
        //Log.Debug("KeyWrite");
        //var fw = new AESWriter("");
        //fw.Save("AWS", "pksk", System.Text.Encoding.UTF8);

        try
        {
            ScoreMGR = GetComponentInParent<RankingMGR>().ScoreObj.GetComponent<ScoreManager>();
        }
        catch
        {
            ScoreMGR = null;
        }

        try
        {
            // データを読みだして復号化
            //Log.Debug("DataLoad");
            var fl = new AESLoader(@"H401_AESEnctyptSystemFirstVector");
            fl.Load("AWS", System.Text.Encoding.UTF8);

            // データセット
            //Log.Debug("DataSet");
            string str = fl.GetString();
            if (str != null)
            {
                publicKey = str.Substring(0, 20);
                secretKey = str.Substring(20);
            }
            else
            {
                publicKey = "";
                secretKey = "";
                return;
            }
        }
        catch (FileNotFoundException) { /*Log.Error("NotFound Exception"); */return; }      // ファイルが見当たらない場合
        catch (System.FormatException) { /*Log.Error("Format Exception"); */return; }       // ファイルデータ不正

        // AWSへの接続
       // Log.Debug("Initialize AWS System");
        try {
            var awsCredentials = new BasicAWSCredentials(publicKey, secretKey);
            var Cconfig = new AmazonDynamoDBConfig();
            Cconfig.RegionEndpoint = RegionEndpoint.USEast1;
            // タイムアウト時間をコントロールしたい(みかん)
            Cconfig.Timeout = new System.TimeSpan(3);

            if (UseProxy) {
                //Log.Info("Proxy Enabled");
                Cconfig.ProxyPort = ProxyPort;
                Cconfig.ProxyHost = ProxyHost;
                Cconfig.ProxyCredentials = new System.Net.NetworkCredential(UserName, Password);
            }
            //      Cconfig.ReadWriteTimeout = new System.TimeSpan(1);
            Cconfig.MaxErrorRetry = 1;
            client = new AmazonDynamoDBClient(awsCredentials, Cconfig);
        }
        catch
        {
            //Log.Debug("AWS Initialize Failed");
            if (Pear != null) { Pear.LinkFailed(); }
        }
        Ready = true;
    }

    // Update is called once per frame
    void Update() { }

    // データ追加
    public void Add(int Score, string Name = DefaultName) {
        // 変数宣言
        if(Name == "") { Name = DefaultName; }
        ScoreTable Dat = new ScoreTable(Name, Score);                               // スコアデータ初期化
        PutItemRequest pReq = new PutItemRequest(TABLE_NAME, Dat.GetDictionary());  // リクエスト作成

        // Putリクエスト
        if (Pear != null) { Pear.StartLink(); }         // 親に通信開始を通知
        //Log.Debug("AWS Put");
        client.PutItemAsync(pReq, (PutCallBack) => {
            if (Pear != null) { Pear.SetResponse(); }   // 反応があったら親に通知
            if(PutCallBack.Exception == null) {
                // 正常完了
                //Log.Debug("AWS Put Successed");
            } else {
                // エラー
                if (Pear != null) { Pear.LinkFailed(); }
                //Log.Error("AWS Put Failed\n" + PutCallBack.Exception.ToString());
            }
        });
    }

    // データ読込
    public void Read() {
        // 絞り込み用変数
        Condition ScoreLine = new Condition {
            AttributeValueList = {
                new AttributeValue { N = "1" }
            },
            ComparisonOperator = "GE"
        };

        // リクエスト作成
        ScanRequest ScanRequest = new ScanRequest {
            TableName = TABLE_NAME,
            ScanFilter =
            new Dictionary<string, Condition> { { "Score", ScoreLine } },
            Select = "ALL_ATTRIBUTES",
        };

        // Scanリクエスト
        //Log.Debug("AWS Scan");
        if (Pear != null) { Pear.StartLink(); }         // 親に通信開始を通知
        client.ScanAsync(ScanRequest, sr => {
            if (Pear != null) { Pear.SetResponse(); }   // 反応があったら親に通知
            if (sr.Exception == null) {
                // スキャン成功
                ScoreCash = new List<ScoreTable>();
                //Log.Debug("GetData");
                foreach(var attribs in sr.Response.Items) {
                    // 取得したデータをパッキング
                    ScoreTable ScTemp = new ScoreTable();
                    foreach(var attrib in attribs) {
                        switch(attrib.Key) {
                            case "ID":   ScTemp.ID    = attrib.Value.S;             break;
                            case "Score":ScTemp.Score = int.Parse(attrib.Value.N);  break;
                            case "Name": ScTemp.Name  = attrib.Value.S;             break;
                            case "Date": ScTemp.Date  = long.Parse(attrib.Value.N); break;
                        }
                    }
                    ScoreCash.Add(ScTemp);
                }
                //Log.Debug("AWS Scan Successed");
                // 取得したリストを使ってなんやかんや。
                foreach (var it in ScoreCash)
                {
                    //Log.Info(it.Date.ToString() + " , " + it.Score);
                }
            } else {
                // エラー
                if (Pear != null) { Pear.LinkFailed(); }
                //Log.Error("AWS Scan Failed\n" + sr.Exception.ToString());
            }
        });
    }
}

