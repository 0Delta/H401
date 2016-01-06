using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// スコアに使用する文字スプライトの制御
/// </summary>
public class ScoreWordMGR {

    static Sprite[] NumSprite = null;
    static Sprite[] AlpSprite = null;

    /// <summary>
    /// スプライトを読み込みます
    /// </summary>
    /// <returns>成功で0、失敗で-1以下が帰ります</returns>
    public int Load() {
        int ret = 0;
        NumSprite = Resources.LoadAll<Sprite>("Sprites/Ranking_Number");
        AlpSprite = Resources.LoadAll<Sprite>("Sprites/Ranking_Alphabet");

        if(NumSprite == null) {
            Debug.LogError("Failed Load NumSprite");
            ret--;
        }
        if(AlpSprite == null) {
            Debug.LogError("Failed Load AlpSprite");
            ret--;
        }
        if(NumSprite.Length != 11) {
            Debug.LogWarning("Multiple Sprite num is not correct : Num");
            ret--;
        }
        if(AlpSprite.Length != 4) {
            Debug.LogWarning("Multiple Sprite num is not correct : Alp");
            ret--;
        }
        return ret;
    }

    /// <summary>
    /// 数字のスプライトを取得します
    /// </summary>
    /// <param name="Index">0-9:対応した数値のスプライト　10:透明なスプライト</param>
    /// <returns>指定したスプライト、エラーでNULLが帰ります</returns>
    Sprite Number(int Index) {
        if(Index > NumSprite.Length || 0 > Index) {
            return null;
        }
        return NumSprite[Index];
    }

    /// <summary>
    /// 英語(st,nd,rd,th)のスプライトを取得します
    /// </summary>
    /// <param name="Index">1:st 2:nd 3:rd 4:th</param>
    /// <returns>指定したスプライト、エラーでNULLが帰ります</returns>
    Sprite Alphabet(int Index) {
        if(Index <= 0) {
            return null;
        } else {
            Index--;
        }
        if(Index > AlpSprite.Length) {
            return null;
        }
        return AlpSprite[Index];
    }

    public static Canvas Draw(string Word,Transform pear,float Height) {

        // Canvas作る
        var pobj = new GameObject();
        var canv = pobj.AddComponent<Canvas>();
        canv.name = "NumberCanvas";
        canv.transform.SetParent(pear);

        // Canvasのサイズ調整
        Vector3 pSize = NumSprite[0].rect.size;
        pSize.x *= Word.Length;
        var ptrans = pobj.GetComponentInChildren<RectTransform>();
        ptrans.sizeDelta = pSize;

        for(int digit = 0; digit < Word.Length; digit++) {
            // 数字のスプライト作る
            if(Word[digit] < '0' || '9' < Word[digit]) {
                continue;
            }
            int Num = Word[digit] - '0';
            var obj = new GameObject();
            obj.name = "Num" + digit.ToString();
            obj.transform.SetParent(canv.transform);

            var trans = obj.AddComponent<RectTransform>();
            // サイズを設定
            trans.sizeDelta = NumSprite[Num].rect.size;
            // 位置を設定
            trans.anchorMax = new Vector2(0, 0.5f);
            trans.anchorMin = new Vector2(0, 0.5f);
            Vector2 anchoedPos = Vector2.zero;
            anchoedPos.x = (NumSprite[Num].rect.size.x / 2f) + (NumSprite[Num].rect.size.x * digit);
            trans.anchoredPosition = anchoedPos;

            var img = obj.AddComponent<Image>();
            img.sprite = NumSprite[Num];
        }

        // 最後にCanvasごとサイズ調整
        ptrans.localScale = new Vector3(Height / ptrans.sizeDelta.y, Height / ptrans.sizeDelta.y, 1);
        return canv;
    }


    public static Canvas DrawRank(int Rank, Transform pear, float Height) {

        // Canvas作る
        var pobj = new GameObject();
        var canv = pobj.AddComponent<Canvas>();
        canv.name = "RankCanvas";
        canv.transform.SetParent(pear);

        // Canvasのサイズ調整
        Vector3 pSize = AlpSprite[0].rect.size;
        pSize.x += NumSprite[0].rect.size.x;
        var ptrans = pobj.GetComponentInChildren<RectTransform>();
        ptrans.sizeDelta = pSize;

        // 数字のスプライト作る
        int Num = Rank.ToString()[0] - '0';
        var obj = new GameObject();
        obj.name = "Num";
        obj.transform.SetParent(canv.transform);

        var trans = obj.AddComponent<RectTransform>();
        // サイズを設定
        trans.sizeDelta = NumSprite[Num].rect.size;
        // 位置を設定
        trans.anchorMax = new Vector2(0, 0.5f);
        trans.anchorMin = new Vector2(0, 0.5f);
        Vector2 anchoedPos = Vector2.zero;
        anchoedPos.x = (NumSprite[Num].rect.size.x / 2f);
        trans.anchoredPosition = anchoedPos;

        var img = obj.AddComponent<Image>();
        img.sprite = NumSprite[Num];

        // 英語のスプライト作る
        Num = Num > 4 ? 4 : Num;
        Num -= 1;
        obj = new GameObject();
        obj.name = "Alp";
        obj.transform.SetParent(canv.transform);

        trans = obj.AddComponent<RectTransform>();
        // サイズを設定
        trans.sizeDelta = AlpSprite[Num].rect.size;
        // 位置を設定
        trans.anchorMax = new Vector2(1, 0.5f);
        trans.anchorMin = new Vector2(1, 0.5f);
        anchoedPos = Vector2.zero;
        anchoedPos.x = -(AlpSprite[Num].rect.size.x / 2f);
        trans.anchoredPosition = anchoedPos;

        img = obj.AddComponent<Image>();
        img.sprite = AlpSprite[Num];

        // 最後にCanvasごとサイズ調整
        ptrans.localScale = new Vector3(Height / ptrans.sizeDelta.y, Height / ptrans.sizeDelta.y, 1);
        return canv;
    }
}
