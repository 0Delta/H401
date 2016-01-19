
using System.Collections.Generic;
using System.Diagnostics;
using UniRx;

// デバック用ログシステム
namespace CustomDebugLog {
    [Conditional("DEBUG")]
    public class CDebugLog : System.Attribute
    {

        #region // 型宣言
        /// <summary>
        /// ログの種類です
        /// <see cref="Debug(string)"/>
        /// <see cref="Info(string)"/>
        /// <see cref="Warning(string)"/>
        /// <see cref="Error(string)"/>
        /// <see cref="Assert(string)"/>
        /// </summary>
        public enum LOGTYPE {

            DEBUG,
            INFO,
            WARNING,
            ERROR,
            ASSERT,
        }

        /// <summary>
        /// ログ一つを表現するクラス
        /// 開発者が使用する事はありません
        /// </summary>
        public class Log
        {
            readonly private LOGTYPE type = LOGTYPE.DEBUG;
            readonly private string message = "";

            /// <summary>
            /// ログの種類を返します
            /// </summary>
            /// <returns>LOGTYPE型で種類を返します</returns>
            public LOGTYPE GetLogType() {
                return type;
            }

            // コンストラクタ。デフォルトを禁止
            /// <summary>
            /// デフォルトコンストラクタは禁止されています。
            /// <see cref="Log(LOGTYPE, string)"/>
            /// </summary>
            private Log() { }
            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="type">ログの種類</param>
            /// <param name="message">ログとして残す文字列</param>
            public Log(LOGTYPE type, string message) {
                this.type = type;
                this.message = message;
            }

            /// <summary>
            /// [T]Message\n の形式で文字列化します
            /// </summary>
            public override string ToString() {
                return "[" + type.ToChar() + "]" + message;
            }
            /// <summary>
            /// Type,Message\n の形式で文字列化します
            /// CVSを作成するのに使用します
            /// </summary>
            public string ToStringCVS() {
                return type.ToString() + "\t" + message + "\n";
            }
        }
        #endregion

        private const bool FORCE_LOG = false;
        private const bool AUTO_EXPORT = true;
        private const int AUTO_EXPORT_RATE = 2;

        // 名前のリスト
        private static List<string> NameList = new List<string>();
        public static Dictionary<string,CDebugLog> InstanceList = new Dictionary<string, CDebugLog>();
        // 変数宣言
        private readonly string LogName = "";
        private List<Log> LogDat = new List<Log>();

        /// <summary>
        /// デフォルトコンストラクタは禁止されています。
        /// String型で名前を付けて下さい。
        /// <see cref="CDebugLog(string)"/>
        /// </summary>
        private CDebugLog() { }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="Name">名前を入力してください。出力時にはファイル名になります。</param>
        public CDebugLog(string Name)
        {
            // 必ず名前を付けて初期化する
            int RetryCount = 0;
            string LName = Name;
            // 同名がいるなら(1)とか付ける
            while (NameList.Contains(LName))
            {
                RetryCount++;
                LName = Name + "(" + RetryCount + ")";
            }
            // 名前登録
            LogName = LName;
            NameList.Add(LogName);
            InstanceList.Add(LogName, this);

            // 自動書き出し
            if (AUTO_EXPORT)
            {
                Observable
                    .EveryUpdate()
                    .DistinctUntilChanged(x => Count)
                    .ThrottleFrame(AUTO_EXPORT_RATE * 60)
                    .Subscribe(_ =>
                    {
                        Export();
                    });
            }
        }


        #region // 各種ログ書き込み用関数
        /// <summary>
        /// デバック用メッセージ
        /// </summary>
        /// <param name="message">デバック用コメント</param>
        /// <returns>何も返しません</returns>
        public void Debug(string message) {
            LogDat.Add(new Log(LOGTYPE.DEBUG, message));
            if (FORCE_LOG)
            {
                UnityEngine.Debug.Log(message);
            }
        }
        /// <summary>
        /// 情報用メッセージ
        /// </summary>
        /// <param name="message">プログラムの実行に支障をきたさない情報を記述します</param>
        /// <returns>何も返しません</returns>
        public void Info(string message) {
            LogDat.Add(new Log(LOGTYPE.INFO, message));
        }
        /// <summary>
        /// 警告
        /// </summary>
        /// <param name="message">プログラムが危険な状態、想定内の不具合等を記述します</param>
        /// <returns>何も返しません</returns>
        public void Warning(string message) {
            LogDat.Add(new Log(LOGTYPE.WARNING, message));
        }
        /// <summary>
        /// エラーメッセージ
        /// </summary>
        /// <param name="message">想定外の不具合に関する情報を記述します</param>
        /// <returns>何も返しません</returns>
        public void Error(string message) {
            LogDat.Add(new Log(LOGTYPE.ERROR, message));
        }
        /// <summary>
        /// 致命エラーメッセージ
        /// </summary>
        /// <param name="message">プログラムの実行が不可能になるレベルの不具合に関する情報を記述します</param>
        /// <returns>何も返しません</returns>
        public void Assert(string message) {
            LogDat.Add(new Log(LOGTYPE.ASSERT, message));
        }
        #endregion

        #region // 各種ログ吐き出し用関数
        /// <summary>
        /// [T]Message の形式で全てのログを吐き出します
        /// </summary>
        public override string ToString() {
            string ret = "";
            foreach(var it in LogDat) {
                ret += it.ToStringCVS();
            }
            return ret;
        }

        /// <summary>
        /// [T]Message の形式でログを吐き出します
        /// フィルターを掛けて表示
        /// </summary>
        /// <param name="filter">指定したレベル未満のログを非表示にします</param>
        /// <param name="Exclude">Trueを指定した場合、filter以外のログを非表示にします。</param>
        public string ToString(LOGTYPE filter, bool Exclude = false) {
            string ret = "";
            foreach(var it in LogDat) {
                if(Exclude) {
                    if(it.GetLogType() != filter) {
                        continue;
                    }
                } else {
                    if(it.GetLogType() < filter) {
                        continue;
                    }
                }
                ret += it.ToString();
            }
            return ret;
        }
        /// <summary>
        /// [T]Message の形式で全てのログを逆順で吐き出します
        /// </summary>
        public string ToStringReverse(int Lim = int.MaxValue) {
            string ret = "";
            LogDat.Reverse();
            int Limit = 0;
            foreach(var it in LogDat) {
                ret += it.ToStringCVS();
                if(++Limit > Lim) { break; };
            
            }
            LogDat.Reverse();
            return ret;
        }

        /// <summary>
        /// [T]Message の形式でログを逆順で吐き出します
        /// フィルターを掛けて表示
        /// </summary>
        /// <param name="filter">指定したレベル未満のログを非表示にします</param>
        /// <param name="Exclude">Trueを指定した場合、filter以外のログを非表示にします。</param>
        public string ToStringReverse(LOGTYPE filter, bool Exclude = false) {
            LogDat.Reverse();
            string ret = "";
            foreach(var it in LogDat) {
                if(Exclude) {
                    if(it.GetLogType() != filter) {
                        continue;
                    }
                } else {
                    if(it.GetLogType() < filter) {
                        continue;
                    }
                }
                ret += it.ToString();
            }
            LogDat.Reverse();
            return ret;
        }
        #endregion

        /// <summary>
        /// 格納されているログの総数を返します
        /// </summary>
        public int Count { get { return LogDat.Count; } }

        /// <summary>
        /// ログをファイルに書き出します
        /// </summary>
        public void Export()
        {
            System.IO.File.WriteAllText(UnityEngine.Application.persistentDataPath + "/" + LogName + ".log", ToStringReverse());
        }
    }

    /// <summary>
    /// LOGTYPEの拡張メソッド
    /// </summary>
    public static class LogTypeExtension {
        /// <summary>
        /// LOGTYPEの頭文字一文字をcharで返します。
        /// </summary>
        public static char ToChar(this CDebugLog.LOGTYPE type) {
            return type.ToString()[0];
        }
    }
}