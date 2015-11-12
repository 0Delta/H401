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
    CAP = 0,    // 先端１つ
    HUB2_A,     // │
    HUB2_B,     // 鋭角
    HUB2_C,     // 鈍角
    HUB3_A,     // Ｙ
    HUB3_B,     // 卜
    //HUB3_C,     // ∋

    MAX,
};


public enum _eLinkDir {
    RU = 0,
    R,
    RD,
    LD,
    L,
    LU,

    MAX,

    NONE,       //方向なし、つまり根本の部分
}

//木の根元がどこか 走査に使用
public enum _eTreePath
{
    T1 = 0,
    T2,
    T3,
    T4,

    MAX,
}
public enum _eLevelState
{
    STAND,
    LIE,
}
public enum _eDebugState
{
    LEFT_LIE,
    RIGHT_LIE,
    RETURN,
}