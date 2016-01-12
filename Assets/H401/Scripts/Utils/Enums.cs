// ノードのスライド方向
public enum _eSlideDir {
    LEFT = 0,       // 左
    RIGHT,          // 右
    LEFTUP,         // 左上
    LEFTDOWN,       // 左下
    RIGHTUP,        // 右上
    RIGHTDOWN,      // 右下

    NONE            // 無
}

public enum _eLinkDir : int{
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
    CHANGE,
}
public enum _eDebugState
{
    LEFT_LIE,
    RIGHT_LIE,
    RETURN,
}
public enum _eFeverState
{
    NORMAL,
    FEVER,
}
public enum _ePauseState
{
    GAME,
    PAUSE,
}