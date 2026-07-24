using UnityEngine;

public class UIAdaptiveManager : MonoBehaviour
{
    [Header("UI Elements (竖屏默认位置)")]
    public RectTransform Board;
    public RectTransform Titles;
    public RectTransform NewGameButton;
    public RectTransform Score;
    public RectTransform Best;

    [Header("横屏自定义位置 (anchoredPosition)")]
    public Vector2 customBoardPos;
    public Vector2 customTitlesPos;
    public Vector2 customNewGameButtonPos;
    public Vector2 customScorePos;
    public Vector2 customBestPos;

    // 竖屏初始位置（自动记录）
    private Vector2 defaultBoardPos;
    private Vector2 defaultTitlesPos;
    private Vector2 defaultNewGameButtonPos;
    private Vector2 defaultScorePos;
    private Vector2 defaultBestPos;

    // 当前屏幕方向（true=横屏，false=竖屏）
    private bool isLandscape = false;

    // 切换阈值（宽高比 >= 1 视为横屏，可根据需要调整）
#if UNITY_ANDROID || UNITY_IOS
    private const float LANDSCAPE_THRESHOLD = 1.0f;
#else
    private const float LANDSCAPE_THRESHOLD = 2.0f;
#endif

    void Start()
    {
        // 1. 记录初始位置（作为竖屏布局）
        if (Board != null) defaultBoardPos = Board.anchoredPosition;
        if (Titles != null) defaultTitlesPos = Titles.anchoredPosition;
        if (NewGameButton != null) defaultNewGameButtonPos = NewGameButton.anchoredPosition;
        if (Score != null) defaultScorePos = Score.anchoredPosition;
        if (Best != null) defaultBestPos = Best.anchoredPosition;

        // 2. 根据当前屏幕方向应用正确的布局
        float aspect = (float)Screen.width / Screen.height;
        isLandscape = (aspect >= LANDSCAPE_THRESHOLD);
        if (isLandscape)
            SwitchToCustom();
        else
            SwitchToDefault();
    }

    void Update()
    {
        // 每帧检测宽高比，判断方向是否变化
        float aspect = (float)Screen.width / Screen.height;
        bool currentLandscape = (aspect >= LANDSCAPE_THRESHOLD);

        // 仅当方向发生改变时才执行切换
        if (currentLandscape != isLandscape)
        {
            isLandscape = currentLandscape;
            if (isLandscape)
                SwitchToCustom();   // 横屏 → 自定义位置
            else
                SwitchToDefault();  // 竖屏 → 初始位置
        }
    }

    /// <summary>
    /// 切换到横屏自定义位置
    /// </summary>
    public void SwitchToCustom()
    {
        if (Board != null) Board.anchoredPosition = customBoardPos;
        if (Titles != null) Titles.anchoredPosition = customTitlesPos;
        if (NewGameButton != null) NewGameButton.anchoredPosition = customNewGameButtonPos;
        if (Score != null) Score.anchoredPosition = customScorePos;
        if (Best != null) Best.anchoredPosition = customBestPos;
    }

    /// <summary>
    /// 切换到竖屏初始位置
    /// </summary>
    public void SwitchToDefault()
    {
        if (Board != null) Board.anchoredPosition = defaultBoardPos;
        if (Titles != null) Titles.anchoredPosition = defaultTitlesPos;
        if (NewGameButton != null) NewGameButton.anchoredPosition = defaultNewGameButtonPos;
        if (Score != null) Score.anchoredPosition = defaultScorePos;
        if (Best != null) Best.anchoredPosition = defaultBestPos;
    }
}