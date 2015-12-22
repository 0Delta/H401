using UnityEngine;
using System.Collections.Generic;
using Amazon;
using Amazon.Runtime;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DataModel;
using AES;
using System.IO;

public class DynamoConnecter : MonoBehaviour {
    // 固有関数
    [SerializeField]
    public bool UseProxy;
    [SerializeField]
    public string ProxyHost;
    [SerializeField]
    public int ProxyPort;
    [SerializeField]
    public string UserName;
    [SerializeField]
    public string Password;
    [SerializeField]
    public const string DefaultName = "NoName";

    public const string TABLE_NAME = "H401_6";                                          // テーブルの名前
    private string publicKey = "AKIAJLMLMLC4KEYK77LQ";                            // 接続用ID
    private string secretKey = "QmbJyIdcQ+db4jho2qaea6ZdWaaU/60La8lxbCdP";        // 接続用PASSWORD
    private AmazonDynamoDBClient client;                                                // コネクタ
    private AesCryptography AesInstance = new AesCryptography(@"H401_AESEnctyptSystemFirstVector");

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
        // キーをロード
        //// データを読みだして暗号化
        //byte[] Dat = System.Text.Encoding.UTF8.GetBytes("pksk");
        //byte[] Sav = AesInstance.Encrypt(Dat);

        //// 書き込み
        //var sw = File.Create(Application.persistentDataPath + "/AWS");
        //sw.Dispose();
        //File.WriteAllBytes(Application.persistentDataPath + "/AWS", Sav);


        try {
            // データを読みだして復号化
            byte[] Sav = File.ReadAllBytes(Application.persistentDataPath + "/AWS");
            byte[] Dat = AesInstance.Decrypt(Sav);
            
            // データセット
            string str = System.Text.Encoding.UTF8.GetString(Dat);
            publicKey = str.Substring(0, 20);
            secretKey = str.Substring(20);
     }
        catch(FileNotFoundException) { return; }        // ファイルが見当たらない場合
        catch(System.FormatException) { return; }       // ファイルデータ不正

        // AWSへの接続
        var awsCredentials = new BasicAWSCredentials(publicKey, secretKey);
        var Cconfig = new AmazonDynamoDBConfig();
        Cconfig.RegionEndpoint = RegionEndpoint.USEast1;
        // タイムアウト時間をコントロールしたい(みかん)
        Cconfig.Timeout = new System.TimeSpan(10000);

        if(UseProxy) {
            Cconfig.ProxyPort = ProxyPort;
            Cconfig.ProxyHost = ProxyHost;
            Cconfig.ProxyCredentials = new System.Net.NetworkCredential(UserName, Password);
        }
        //      Cconfig.ReadWriteTimeout = new System.TimeSpan(1);
        Cconfig.MaxErrorRetry = 1;

        client = new AmazonDynamoDBClient(awsCredentials, Cconfig);
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
        client.PutItemAsync(pReq, (PutCallBack) => {
            //Pear.SetResponse();                 // 反応があったら親に通知
            if(PutCallBack.Exception == null) {
                // 正常完了
                Debug.Log("正常に送信されました");
            } else {
                // エラー
                Debug.LogError(PutCallBack.Exception.ToString());
            }
            //Destroy(this);  // 自身を消去
        });
    }

    // データ読込
    public void Read() {
        // 絞り込み用変数
        Condition ScoreLine = new Condition {
            AttributeValueList = {
                new AttributeValue { N = "200" }
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
        client.ScanAsync(ScanRequest, sr => {
            //Pear.SetResponse();                 // 反応があったら親に通知
            if(sr.Exception == null) {
                // スキャン成功
                List<ScoreTable> GetList = new List<ScoreTable>();
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
                        GetList.Add(ScTemp);
                    }
                    // 取得したリストを使ってなんやかんや。
                }
            } else {
                // エラー
                Debug.LogError(sr.Exception.ToString());
            }
        });
        //Destroy(this);  // 自身を消去
    }
}

