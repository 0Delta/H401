using System.IO;
using UnityEngine;
using RankingExtension;
using UniRx;

public class ScoreManager : MonoBehaviour {

    [SerializeField]
    public string Name;
    //private UnityEngine.UI.InputField NameEntryField;

    /// <summary>
    /// テーブルの定義
    /// </summary>
    private class ScoreClass {
        // 変数群
        public int Score { get; set; }
        public long Date { get; set; }

        /// <summary>
        /// デフォルトコンストラクタ 全ての値を不正値に
        /// </summary>
        public ScoreClass() {
            Score = -1;
            Date = -1;
        }

        /// <summary>
        /// コンストラクタ。正しく引数を与えて初期化。
        /// </summary>
        /// <param name="iName">プレイヤーネーム</param>
        /// <param name="iScore">スコア</param>
        public ScoreClass(string iName, int iScore) {
            Date = GetDate();
            Score = iScore;
        }

        /// <summary>
        /// [Debug]文字列変換
        /// </summary>
        /// <returns>見やすい形で文字列を返す</returns>
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

        /// <summary>
        /// 比較演算子
        /// </summary>
        /// <param name="Sc">自身</param>
        /// <param name="tgt">比較対象</param>
        /// <returns></returns>
        static public bool operator <(ScoreClass Sc, int tgt) {
            if(Sc.Score < tgt)
                return true;
            return false;
        }
        
        /// <summary>
        /// 比較演算子
        /// </summary>
        /// <param name="Sc">自身</param>
        /// <param name="tgt">比較対象</param>
        /// <returns></returns>
        static public bool operator >(ScoreClass Sc, int tgt) {
            if(Sc.Score >= tgt)
                return true;
            return false;
        }

        /// <summary>
        /// 日付データを取得する
        /// </summary>
        /// <returns>long型、YYMMDDHHMMSSで返す</returns>
        static public long GetDate() {
            return long.Parse(System.DateTime.Now.ToString("yyMMddHHmmss", System.Globalization.DateTimeFormatInfo.InvariantInfo));
        }
    };

    /// <summary>
    /// スコアのリスト
    /// </summary>
    private class ScoreList {
        // 実体データ
        private ScoreClass[] Val = new ScoreClass[10];

        // 初期化処理
        public ScoreList() {
            for(int i = 0; i < Val.Length; i++) {
                Val[i] = new ScoreClass();
            }
        }

        /// <summary>
        /// 挿入
        /// </summary>
        /// <param name="Score">追加するスコア</param>
        /// <returns>順位、ランク外で-1</returns>
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

        /// <summary>
        /// 後方シフト
        /// </summary>
        /// <param name="i">0を挿入するインデックス</param>
        private void BackShift(int i) {
            for(int n = Val.Length - 2; n >= i; n--) {
                Val[n + 1] = Val[n];
            }
            Val[i] = new ScoreClass();
        }

        /// <summary>
        /// スコア取得
        /// </summary>
        /// <param name="i">0から始まるインデックス</param>
        /// <returns>int型スコア</returns>
        public ScoreClass this[int i] {
            set {
                if (CheckIdx(i))
                {
                    Val[i] = value;
                }
            }
            get {
                if (CheckIdx(i))
                {
                    return Val[i];
                }
                return null;
            }
        }

        /// <summary>
        /// デバック用文字列
        /// </summary>
        /// <returns>string型</returns>
        public override string ToString() {
            string str = "";
            foreach(var it in Val) {
                str += it.ToString() + "\n";
            }
            return str;
        }

        /// <summary>
        /// 文字列データ化
        /// </summary>
        /// <returns>string型データ</returns>
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

        /// <summary>
        /// 文字列データから復元
        /// </summary>
        /// <param name="Dat">string型データ</param>
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

        /// <summary>
        /// インデックスの引数チェック
        /// </summary>
        /// <param name="i">渡す値</param>
        /// <returns>範囲外だとfalse、正常ならtrue</returns>
        private bool CheckIdx(int i)
        {
            if(i < 0 || Val.Length <= i)
            {
                return false;
            }
            return true;
        }

    }
    // スコアリスト実体を宣言
    static ScoreList SListInstance = new ScoreList();
    static bool Ready = false;

    /// <summary>
    /// スコアをファイルからロードする
    /// </summary>
    /// <returns>正常で0</returns>
    public static int Load()
    {
        if (Ready) { return 0; }
        try
        {
            // キーを作成
            string Key = KeyGenerator();

            // データを読みだして復号化
            var fl = new AESLoader(Key);
            if (fl.Load("save", System.Text.Encoding.UTF8) == 0)
            {
                // データセット
                SListInstance.FromStringData(fl.GetString());
            }
            Ready = true;
        }
        catch (FileNotFoundException) { return -1; }        // ファイルが見当たらない場合
        catch (System.FormatException) { return -1; }       // ファイルデータ不正

        return 0;
    }

    /// <summary>
    /// 暗号化用文字列を生成する
    /// </summary>
    /// <returns>機種ごとにユニークな暗号化文字列</returns>
    private static string KeyGenerator()
    {
        string Key = SystemInfo.deviceUniqueIdentifier.ToString();
        Key.PadLeft(32, Key[0]);
        Key = Key.Substring(0, 32);
        return Key;
    }

    /// <summary>
    /// スコアを暗号化してファイルにセーブする
    /// </summary>
    /// <returns>正常で0</returns>
    static int Save()
    {
        Observable
            .NextFrame()
            .Subscribe(_ => {
                // キーを作成
                string Key = KeyGenerator();

                // 書き込み
                try
                {
                    // データを読みだして暗号化
                    var fw = new AESWriter(Key);
                    fw.Save("save", SListInstance.ToStringData(), System.Text.Encoding.UTF8);
                }
                catch (System.IO.IsolatedStorage.IsolatedStorageException)
                {
                    //return -1;
                }
                //return 0;
            });
            return 0;
    }

    /// <summary>
    /// スコアの取得
    /// </summary>
    /// <param name="Rank">取得するランク。1-10</param>
    /// <returns>範囲外が指定されたら0</returns>
    public static int GetScore(int Rank)
    {
        if (Rank < 1 || 10 < Rank)
        {
            return 0;
        }
        var ScoreVal = SListInstance[Rank-1];
        if (ScoreVal != null)
        {
            return ScoreVal.Score;
        }
        return -1;
    }

    //----------------------------------
    // プレハブ標準の関数群
    void Start() {
        Load();         // ファイルからスコア読み出し
    }

    /// <summary>
    /// スコアを追加
    /// </summary>
    /// <param name="Score">スコア</param>
    public int AddScore(int Score) {
        Load();
        int r = SListInstance.Insert(Score) + 1;
        //Debug.Log(
        //    (r == -1) ? ("Out of Ranking.") : ("Rank : " + r)
        //    + "\n" + SListInstance.ToString());
        Save();
        return r;
    }
}
