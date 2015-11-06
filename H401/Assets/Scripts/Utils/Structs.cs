using System;

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
    //ノードの出現率
    public float Ratio_Cap;
    public float Ratio_Path2;
    public float Ratio_Path3;

//    private string BG_Path;

    //背景の画像パス
}

[System.Serializable] public struct TimeLevelInfo
{
    public float MaxRegainRatio;    //回復する最大値（割合）
    public float SlipRatio;         //現象割合（秒間）
    public float RegainPer3Nodes;   //ノード3つごとの回復割合
    public float RegainPerCap;      //パス１ノード１つごとの回復割合
    public float RegainPer2Path;    //パス２ノード１つごとの回復割合
    public float RegainPer3Path;    //パス３ノード１つごとの回復割合
}