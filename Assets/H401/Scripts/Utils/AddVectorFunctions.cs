using UnityEngine;

public class AddVectorFunctions {
    public static float Vec2Cross(Vector2 lhs, Vector2 rhs) {
        return lhs.x * rhs.y - rhs.x * lhs.y;
    }
}
