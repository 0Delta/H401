using UnityEngine;
﻿using System;

// int型のVector2構造体
public struct Vec2Int
{
    public int x;
    public int y;

    public Vec2Int(int _x, int _y) {x = _x; y = _y;}

    public static Vec2Int operator + (Vec2Int a, Vec2Int b) {
        return new Vec2Int(a.x + b.x, a.y + b.y);
    }
    public static Vec2Int operator - (Vec2Int a, Vec2Int b) {
        return new Vec2Int(a.x - b.x, a.y - b.y);
    }
    public static Vec2Int operator * (Vec2Int a, Vec2Int b) {
        return new Vec2Int(a.x * b.x, a.y * b.y);
    }
    public static Vec2Int operator / (Vec2Int a, Vec2Int b) {
        return new Vec2Int(a.x / b.x, a.y / b.y);
    }

    public override string ToString()
    {
        return "(" + x.ToString() + ", " + y.ToString() + ")";
    }

    public static Vec2Int zero {
        get { return new Vec2Int(0,0); }
    }
};

[System.Serializable] public struct FieldLevelInfo
{
    public string fieldName; 
    //ノードの出現率
    public float Ratio_Cap;
    public float Ratio_Path2;
    public float Ratio_Path3;
    public float Ratio_Path4;
    public Color lightColor;
    public Color NodeColor;
    public string BG_Path;

    //背景の画像パス
}

[System.Serializable] public struct TimeLevelInfo
{
    public float MaxRegainRatio;    //回復する最大値（割合）
    public float SlipRatio;         //現象割合（秒間）
    public float RegainPerNodes;   //ノード3つごとの回復割合
    public float RegainPerCap;      //パス１ノード１つごとの回復割合
    public float RegainPer2Path;    //パス２ノード１つごとの回復割合
    public float RegainPer3Path;    //パス３ノード１つごとの回復割合
    public float RegainPer4Path;    //パス４ノード１つごとの回復割合
}

//スコア情報
[System.Serializable] public struct ScoreInfo
{
    public int BasePoint;
    public float BonusPerCap;
    public float BonusPer2Path;
    public float BonusPer3Path;
    public float BonusPer4Path;
}

//枝のノード情報
public struct NodeCountInfo
{
    public int nodes;
    public int cap;
    public int path2;
    public int path3;
    public int path4;
}
//フィーバー制御情報
[System.Serializable]public struct FeverInfo
{
    public float gainRatio;
    public float decreaseRatio;
    public float MaxGainRatio;
    public float gainPerCap;
    public float gainPerPath2;
    public float gainPerPath3;
    public float gainPerPath4;
}

// フェードイン・アウトの時間
[System.Serializable]
public struct FadeTime {
    public float inTime;
    public float outTime;

    public FadeTime(float _inTime, float _outTime) { inTime = _inTime; outTime = _outTime; }
};

//structはNull非許容だったのでClassにした
[Serializable]
public class OptionIOData
{
    public float fMasterVol;
    public float fBGMVol;
    public float fSEVol;
    public bool bGyroEnable;
}