using UnityEngine;

public class OfflineRankingMGR : MonoBehaviour {

    /// <summary>
    /// 数値の位置を調整する為の変数群
    /// </summary>
    [System.Serializable]
    private class ScorePositon {
        [SerializeField,Range(5,20)]
        public float WordHeight = 0f;
        [SerializeField, Range(10, 100)]
        public float HeadMargin = 0f;
        [SerializeField, Range(0, 30)]
        public float Margin = 0f;
        [SerializeField, Range(0, 100)]
        public float XPostiion = 0.0f;
    }
    [SerializeField]
    ScorePositon ScorePos = null;

    /// <summary>
    ///  順位の位置を調整するための変数群
    /// </summary>
    [System.Serializable]
    private class RankPositon {
        [SerializeField,Range(5,25)]
        public float WordHeight = 0f;
        [SerializeField, Range(0, 100)]
        public float XPostiion = 0.0f;
    }
    [SerializeField]
    RankPositon RankPos = null;

    /// <summary>
    /// トップのサイズ倍率を調整するための変数群
    /// </summary>
    [System.Serializable]
    private class RankZoom {
        private float[] ZoomVal = new float[10];

        [SerializeField, Range(1, 2)]
        float first = 1.0f;
        [SerializeField, Range(1, 2)]
        float secound = 1.0f;
        [SerializeField, Range(1, 2)]
        float Third = 1.0f;

        /// <summary>
        ///  初期化
        /// </summary>
        public void Start() {
            ZoomVal[0] = first;
            ZoomVal[1] = secound;
            ZoomVal[2] = Third;
            for(int n = 3; n < 10; n++) {
                ZoomVal[n] = 1.0f;
            }
        }

        /// <summary>
        /// getインデクサ
        /// </summary>
        /// <param name="idx">何位を取得するか</param>
        /// <returns>float型で倍率</returns>
        public float this[int idx]
        {
            get
            {
                if(idx < 1 || 10 < idx) {
                    return 0f;
                }
                return ZoomVal[idx - 1];
            }
        }
    }
    [SerializeField]
    RankZoom ZoomOption = null;
    
    /// <summary>
    /// トップのテキストカラーを調整するための変数群
    /// </summary>
    [System.Serializable]
    private class TextColor {
        private Color[] colors = new Color[10];

        [SerializeField]
        Color first;
        [SerializeField]
        Color secound;
        [SerializeField]
        Color Third;

        /// <summary>
        ///  初期化
        /// </summary>
        public void Start() {
            colors[0] = first;
            colors[1] = secound;
            colors[2] = Third;
            for(int n = 3; n < 10; n++) {
                colors[n] = Color.white;
            }
        }

        /// <summary>
        /// getインデクサ
        /// </summary>
        /// <param name="idx">何位を取得するか</param>
        /// <returns>Color型でテキストカラー</returns>
        public Color this[int idx]
        {
            get
            {
                if(idx < 1 || 10 < idx) {
                    return Color.clear;
                }
                return colors[idx - 1];
            }
        }
    }
    [SerializeField]
    TextColor highRankColor = null;

    // Use this for initialization
    void Start() {

        // スコア管理オブジェクトを取得
        var ScoreMgr = transform.parent.GetComponentInChildren<ScoreManager>();
        var localCanvas = GetComponentInChildren<RectTransform>();

        // 変数宣言
        ZoomOption.Start();
        string ScoreString = "";
        float Ypos = 0f;
        Ypos -= ScorePos.HeadMargin;
        highRankColor.Start();
        
        // スコアの表示
        for(int n = 1; n < 10; n++) {
            Ypos -= ScorePos.Margin * ZoomOption[n == 1 ? 10 : n - 1] + (ScorePos.WordHeight * ZoomOption[n]);  // マージン追加

            // 順位を描画する
            var Canv = ScoreWordMGR.DrawRank(n, localCanvas.transform, RankPos.WordHeight * ZoomOption[n], highRankColor[n]);
            var CanvRectTrans = Canv.GetComponentInChildren<RectTransform>();
            CanvRectTrans.anchorMax = new Vector2(0.5f, 1.0f);
            CanvRectTrans.anchorMin = new Vector2(0.5f, 1.0f);
            CanvRectTrans.pivot = new Vector2(0.0f, 0.5f);
            CanvRectTrans.anchoredPosition = new Vector2(-RankPos.XPostiion * ZoomOption[n], Ypos);

            // スコア数値を描画する
            var ScoreInt = ScoreManager.GetScore(n);                                              // スコアの値を取得
            if (ScoreInt < 0) { continue; }
            ScoreString = ScoreInt.ToString();
            Canv = ScoreWordMGR.Draw(ScoreString, localCanvas.transform, (ScorePos.WordHeight * ZoomOption[n]), highRankColor[n]);      // 描画
            CanvRectTrans = Canv.GetComponentInChildren<RectTransform>();                           // 位置を調整
            CanvRectTrans.anchorMax = new Vector2(0.5f, 1.0f);
            CanvRectTrans.anchorMin = new Vector2(0.5f, 1.0f);
            CanvRectTrans.pivot = new Vector2(1.0f, 0.5f);
            CanvRectTrans.anchoredPosition = new Vector2(ScorePos.XPostiion * ZoomOption[n], Ypos);

        }
    }
}
