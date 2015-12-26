using UnityEngine;
using System.Collections;

public class ScoreWordMGR {
    
    Sprite[] NumSprite = null;
    Sprite[] AlpSprite = null;

    public int Load () {
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
	
}
