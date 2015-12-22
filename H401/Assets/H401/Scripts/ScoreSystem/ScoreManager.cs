using System.IO;
using UnityEngine;
using RankingExtension;
using AES;

public class ScoreManager : MonoBehaviour {
    [SerializeField]
    public string ScorePlatePrefabName;
    [SerializeField]
    public string AWSPrefabName;
    [SerializeField]
    public string Name;
    private DynamoConnecter AWS;
    //private UnityEngine.UI.InputField NameEntryField;

    //----------------------------------
    // テーブル定義
    private class ScoreClass {
        // 変数群
        public int Score { get; set; }
        public long Date { get; set; }

        // デフォルトコンストラクタ 全ての値を不正値に
        public ScoreClass() {
            Score = -1;
            Date = -1;
        }

        // コンストラクタ。正しく引数を与えて初期化。
        public ScoreClass(string iName, int iScore) {
            Date = GetDate();
            Score = iScore;
        }

        // [Debug] 文字列変換
        override public string ToString() {
            if(Date == -1) {
                return "NoData\n";
            }
            string DateTemp = Date.ToString();
            DateTemp = DateTemp.Insert(2, "/");
            DateTemp = DateTemp.Insert(5, "/");
            DateTemp = DateTemp.Insert(8, "-");
            DateTemp = DateTemp.Insert(11, ":");
            DateTemp = DateTemp.Insert(14, ":");

            return
            "Date " + DateTemp + "\nScore " + Score.ToString() + "\n";
        }

        // 比較演算子
        static public bool operator <(ScoreClass Sc, int tgt) {
            if(Sc.Score < tgt)
                return true;
            return false;
        }
        static public bool operator >(ScoreClass Sc, int tgt) {
            if(Sc.Score >= tgt)
                return true;
            return false;
        }

        // 日付データを取得する
        static public long GetDate() {
            return long.Parse(System.DateTime.Now.ToString("yyMMddHHmmss", System.Globalization.DateTimeFormatInfo.InvariantInfo));
        }
    };

    //----------------------------------
    // スコアリスト
    private class ScoreList {
        // 実体データ
        private ScoreClass[] Val = new ScoreClass[10];

        // 初期化処理
        public ScoreList() {
            for(int i = 0; i < Val.Length; i++) {
                Val[i] = new ScoreClass();
            }
        }

        // 挿入
        public int Insert(int Score) {
            for(int i = 0; i < Val.Length; i++) {
                if(Val[i] < Score) {
                    BackShift(i);
                    Val[i].Score = Score;
                    Val[i].Date = ScoreClass.GetDate();
                    return i;
                }
            }
            return -1;
        }

        // 後方シフト
        private void BackShift(int i) {
            for(int n = Val.Length - 2; n >= i; n--) {
                Val[n + 1] = Val[n];
            }
            Val[i] = new ScoreClass();
        }

        // スコア取得
        public ScoreClass this[int i] {
            set {
                Val[i] = value;
            }
            get {
                return Val[i];
            }
        }

        // デバック用文字列
        public override string ToString() {
            string str = "";
            foreach(var it in Val) {
                str += it.ToString() + "\n";
            }
            return str;
        }

        // 文字列データ化
        public string ToStringData() {
            string buf = "";
            foreach(var it in Val) {
                buf += it.Score.ToString();
                buf += ",";
                buf += it.Date.ToString();
                buf += ";";
            }
            return buf;
        }
        // 文字列データから復元
        public void FromStringData(string Dat) {
            var ScoreLst = Dat.Split(';');
            int Counter = 0;
            foreach(var it in ScoreLst) {
                var Data = it.Split(',');
                Val[Counter].Score = int.Parse(Data[0]);
                Val[Counter].Date = long.Parse(Data[1]);
                if(++Counter > 9) { break; }
            }
        }
    }
    // スコアリスト実体を宣言
    ScoreList SListInstance = new ScoreList();

    //----------------------------------
    // プレハブ標準の関数群
    void Start() {
        //        NameEntryField = GetComponentInChildren<UnityEngine.UI.InputField>();

        AWSStart();
        // ファイルからスコア読み出し
        Load();
    }

    void AWSStart() {
        GameObject AWSObj = this.InstantiateChild(AWSPrefabName);
        AWSObj.name = "AWS";
        AWS = AWSObj.GetComponent<DynamoConnecter>();
    }


    public void AddScore(int Score) {
        // スコアを追加
        Load();
        int r = SListInstance.Insert(Score) + 1;
        Debug.Log(
            (r == -1) ? ("Out of Ranking.") : ("Rank : " + r)
            + "\n" + SListInstance.ToString());
        Save();

    }

    #region // 未実装
    void SendHiScore() {

    }
    #endregion

    private string KeyGenerator() {
        string Key = SystemInfo.deviceUniqueIdentifier.ToString();
        Key.PadLeft(32, Key[0]);
        Key = Key.Substring(0, 32);
        return Key;
    }

    int Save() {
        // キーを作成
        string Key = KeyGenerator();
        var AesInstance = new AesCryptography(Key);

        // データを読みだして暗号化
        byte[] Dat = System.Text.Encoding.UTF8.GetBytes(SListInstance.ToStringData());
        byte[] Sav = AesInstance.Encrypt(Dat);

        // 書き込み
        var sw = File.Create(Application.persistentDataPath + "/save");
        sw.Dispose();
        File.WriteAllBytes(Application.persistentDataPath + "/save", Sav);
        return 0;
    }

    int Load() {
        try {
            // キーを作成
            string Key = KeyGenerator();
            var AesInstance = new AesCryptography(Key);

            // データを読みだして復号化
            byte[] Sav = File.ReadAllBytes(Application.persistentDataPath + "/save");
            byte[] Dat = AesInstance.Decrypt(Sav);

            // データセット
            SListInstance.FromStringData(System.Text.Encoding.UTF8.GetString(Dat));
        }
        catch(FileNotFoundException) { return -1; }        // ファイルが見当たらない場合
        catch(System.FormatException) { return -1; }       // ファイルデータ不正

        return 0;
    }

    private void Disp() {
        Canvas canvas = GetComponentInChildren<Canvas>();


        Vector3 Vec = new Vector3(0f, 0f, 0f);
        Vec.y = -(10 / 12f) + 5f;

        for(int n = 0; n < 10; n++) {
            Vec.y -= 10 / 12f;

            GameObject Obj = Instantiate(Resources.Load(ScorePlatePrefabName), Vec, Quaternion.identity) as GameObject;
            Obj.name = "ScorePlate" + n.ToString();
            //PlateInitializer Script = Obj.GetComponent<PlateInitializer>();
            //Script.Set("", SListInstance[n].Score);
        }

    }

    public void Send() {
        AWS.Add(SListInstance[0].Score, Name/*NameEntryField.text*/);
    }

}
