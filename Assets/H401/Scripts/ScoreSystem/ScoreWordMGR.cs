using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// スコアに使用する文字スプライトの制御
/// </summary>
public class ScoreWordMGR {

    static Text Proto = null;

    public static Canvas Draw(string Word,Transform pear,float Height,Color color = default(Color),bool Bold = false) {

        // Canvas作る
        var pobj = new GameObject();
        var canv = pobj.AddComponent<Canvas>();
        canv.name = "NumberCanvas";
        canv.transform.SetParent(pear);

        // Canvasのサイズ調整
        Vector3 pSize = new Vector3(32f, 64f);
        pSize.x *= Word.Length;
        var ptrans = pobj.GetComponentInChildren<RectTransform>();
        ptrans.sizeDelta = pSize;

        if (Proto == null)
        {
            var ProtoObj = Resources.Load("Prefabs/ScoreSystem/TextProto") as GameObject;
            Proto = ProtoObj.GetComponent<Text>();
        }
        var text = pobj.AddComponent<Text>();
        text.font = Proto.font;
        text.text = Word;
        text.fontSize = 30;
        text.resizeTextForBestFit = true;
        text.alignment = TextAnchor.UpperRight;

        if (Bold)
        {
            text.fontStyle = FontStyle.Bold;
        }

        // テキストカラーを変更
        if (color != Color.clear) {
            text.color = color;
        }

        //for (int digit = 0; digit < Word.Length; digit++) {
        //    // 数字のスプライト作る
        //    if(Word[digit] < '0' || '9' < Word[digit]) {
        //        continue;
        //    }
        //    int Num = Word[digit] - '0';
        //    var obj = new GameObject();
        //    obj.name = "Num" + digit.ToString();
        //    obj.transform.SetParent(canv.transform);

        //    var trans = obj.AddComponent<RectTransform>();
        //    // サイズを設定
        //    trans.sizeDelta = NumSprite[Num].rect.size;
        //    // 位置を設定
        //    trans.anchorMax = new Vector2(0, 0.5f);
        //    trans.anchorMin = new Vector2(0, 0.5f);
        //    Vector2 anchoedPos = Vector2.zero;
        //    anchoedPos.x = (NumSprite[Num].rect.size.x / 2f) + (NumSprite[Num].rect.size.x * digit);
        //    trans.anchoredPosition = anchoedPos;

        //    var img = obj.AddComponent<Image>();
        //    img.sprite = NumSprite[Num];
        //}

        // 最後にCanvasごとサイズ調整
        ptrans.localScale = new Vector3(Height / ptrans.sizeDelta.y, Height / ptrans.sizeDelta.y, 1);
        return canv;
    }


    public static Canvas DrawRank(int Rank, Transform pear, float Height, Color color = default(Color)) {

        // Canvas作る
        var pobj = new GameObject();
        var canv = pobj.AddComponent<Canvas>();
        canv.name = "RankCanvas";
        canv.transform.SetParent(pear);

        // Canvasのサイズ調整
        Vector3 pSize = new Vector3(128f, 64f);
        //pSize.x += NumSprite[0].rect.size.x;
        var ptrans = pobj.GetComponentInChildren<RectTransform>();
        ptrans.sizeDelta = pSize;

        //// 数字のスプライト作る
        //int Num = Rank.ToString()[0] - '0';
        //var obj = new GameObject();
        //obj.name = "Num";
        //obj.transform.SetParent(canv.transform);

        //var trans = obj.AddComponent<RectTransform>();
        //// サイズを設定
        //trans.sizeDelta = NumSprite[Num].rect.size;
        //// 位置を設定
        //trans.anchorMax = new Vector2(0, 0.5f);
        //trans.anchorMin = new Vector2(0, 0.5f);
        //Vector2 anchoedPos = Vector2.zero;
        //anchoedPos.x = (NumSprite[Num].rect.size.x / 2f);
        //trans.anchoredPosition = anchoedPos;

        //var img = obj.AddComponent<Image>();
        //img.sprite = NumSprite[Num];

        //// 英語のスプライト作る
        //Num = Num > 4 ? 4 : Num;
        //Num -= 1;
        //obj = new GameObject();
        //obj.name = "Alp";
        //obj.transform.SetParent(canv.transform);

        //trans = obj.AddComponent<RectTransform>();
        //// サイズを設定
        //trans.sizeDelta = AlpSprite[Num].rect.size;
        //// 位置を設定
        //trans.anchorMax = new Vector2(1, 0.5f);
        //trans.anchorMin = new Vector2(1, 0.5f);
        //anchoedPos = Vector2.zero;
        //anchoedPos.x = -(AlpSprite[Num].rect.size.x / 2f);
        //trans.anchoredPosition = anchoedPos;

        //img = obj.AddComponent<Image>();
        //img.sprite = AlpSprite[Num];

        var text = pobj.AddComponent<Text>();

        if (Proto == null)
        {
            var ProtoObj = Resources.Load("Prefabs/ScoreSystem/TextProto") as GameObject;
            Proto = ProtoObj.GetComponent<Text>();
        }
        text.font = Proto.font;
        text.fontSize = 30;
        text.resizeTextForBestFit = true;
        switch (Rank)
        {
            case 1:
                text.text = "1st";
                break;
            case 2:
                text.text = "2nd";
                break;
            case 3:
                text.text = "3rd";
                break;
            default:
                text.text = Rank.ToString() +"th";
                break;
        }

        if(Rank < 4)
        {
            text.fontStyle = FontStyle.Bold;
        }

        // テキストカラーを変更
        if (color != Color.clear) {
            text.color = color;
        }

        // 最後にCanvasごとサイズ調整
        ptrans.localScale = new Vector3(Height / ptrans.sizeDelta.y, Height / ptrans.sizeDelta.y, 1);
        return canv;
    }
}
