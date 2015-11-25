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

// 四角形構造体(原点を中心とする)
public struct Square
{
    private float _x;
    private float _y;
    private float _width;
    private float _height;
    private float _left;
    private float _right;
    private float _top;
    private float _bottom;

    public Square(float x, float y, float width, float height) {
        _x       = x;
        _y       = y;
        _width   = width;
        _height  = height;
        
        _left    = 0.0f;
        _right   = 0.0f;
        _top     = 0.0f;
        _bottom  = 0.0f;

        CalcArea();
    }

    public Square(Vector2 pos, Vector2 size) {
        _x       = pos.x;
        _y       = pos.y;
        _width   = size.x;
        _height  = size.y;

        _left    = 0.0f;
        _right   = 0.0f;
        _top     = 0.0f;
        _bottom  = 0.0f;

        CalcArea();
    }

    public float x {
        set { _x = value; CalcArea();}
        get { return _x;}
    }

    public float y {
        set { _y = value; CalcArea();}
        get { return _y;}
    }

    public float width {
        set { _width = value; CalcArea();}
        get { return _width;}
    }

    public float height {
        set { _height = value; CalcArea();}
        get { return _height;}
    }

    public float left {
        get { return _left; }
    }

    public float right {
        get { return _right; }
    }

    public float top {
        get { return _top; }
    }

    public float bottom {
        get { return _bottom; }
    }

    public override string ToString()
    {
        return "(x:" + _x.ToString() + ", y:" + _y.ToString() + ", width:" + _width.ToString() + ", height:" + _height.ToString() + ")";
    }

    public static Square zero {
        get { return new Square(0.0f,0.0f,0.0f,0.0f); }
    }

    private void CalcArea() {
        _left    = _x - _width * 0.5f;
        _right   = _x + _width * 0.5f;
        _top     = _y + _height * 0.5f;
        _bottom  = _y - _height * 0.5f;
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
