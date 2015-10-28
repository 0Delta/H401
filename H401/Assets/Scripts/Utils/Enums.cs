// ノードのスライド方向
public enum _eSlideDir {
    LEFT = 0,       // 左
    RIGHT,          // 右
    LEFTUP,         // 左上
    LEFTDOWN,       // 左下
    RIGHTUP,        // 右上
    RIGHTDOWN,      // 右下

    NONE            // 無
};

public enum _eNodeType{
    CAP = 0,    // キャップ
    STRAIGHT,   // 直線
    KU,         // く
    KAESHI,     // 返し
    YAI,        // Ｙ
};

public enum _eLinkDir {
    RU = 0,
    R,
    RD,
    LD,
    L,
    LU,

    MAX,
}