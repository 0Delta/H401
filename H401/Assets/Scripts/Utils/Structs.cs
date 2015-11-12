using UnityEngine;

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
    public float x;
    public float y;
    public float width;
    public float height;
    public float left;
    public float right;
    public float top;
    public float bottom;

    public Square(float _x, float _y, float _width, float _height) {
        x       = _x;
        y       = _y;
        width   = _width;
        height  = _height;

        left    = x - width * 0.5f;
        right   = x + width * 0.5f;
        top     = y + height * 0.5f;
        bottom  = y - height * 0.5f;
    }

    public Square(Vector2 pos, Vector2 size) {
        x       = pos.x;
        y       = pos.y;
        width   = size.x;
        height  = size.y;

        left    = x - width * 0.5f;
        right   = x + width * 0.5f;
        top     = y + height * 0.5f;
        bottom  = y - height * 0.5f;
    }

    public override string ToString()
    {
        return "(x:" + x.ToString() + ", y:" + y.ToString() + ", width:" + width.ToString() + ", height:" + height.ToString() + ")";
    }

    public static Square zero {
        get { return new Square(0.0f,0.0f,0.0f,0.0f); }
    }
};