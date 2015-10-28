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
};