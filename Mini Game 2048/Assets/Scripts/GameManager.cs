using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public TileBoard board;
    public CanvasGroup gameOver;
    public Button newGameButton;
    public TextMeshProUGUI scoreText;
    public RectTransform scoreRectTrans;
    public TextMeshProUGUI highscoreText;

    private int score;
    private int windowedWidth;   // 记忆的窗口宽度
    private int windowedHeight;  // 记忆的窗口高度
    private int lastWidth;       // 上一次检测到的窗口宽度（用于避免重复SetResolution）
    private int lastHeight;      // 上一次检测到的窗口高度

    private void Awake()
    {
        // 初始化窗口记忆为当前屏幕分辨率（作为默认值）
        windowedWidth = Screen.currentResolution.width;
        windowedHeight = Screen.currentResolution.height;
        lastWidth = Screen.width;
        lastHeight = Screen.height;
    }

    private void Start()
    {
        Application.targetFrameRate = Screen.currentResolution.refreshRate;

        // 启动时进入无边框全屏（适配屏幕原生分辨率）
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);

        NewGame();
    }

    private void Update()
    {
        // 仅在窗口化模式下检测尺寸变化
        if (!Screen.fullScreen)
        {
            // 如果窗口尺寸发生变化（玩家拖拽了边缘）
            if (Screen.width != lastWidth || Screen.height != lastHeight)
            {
                // 更新记忆
                windowedWidth = Screen.width;
                windowedHeight = Screen.height;
                lastWidth = Screen.width;
                lastHeight = Screen.height;

                // 将渲染分辨率设为当前窗口实际像素尺寸（保持1:1，画面清晰）
                Screen.SetResolution(lastWidth, lastHeight, false);
            }
        }
    }

    public void NewGame()
    {
        SetScore(0);
        highscoreText.text = LoadHighscore().ToString();

        gameOver.alpha = 0f;
        scoreRectTrans.localScale = Vector3.one;
        newGameButton.interactable = true;
        gameOver.interactable = false;

        board.ClearBoard();
        board.CreateTile();
        board.CreateTile();
        board.enabled = true;
    }

    public void GameOver()
    {
        board.enabled = false;
        newGameButton.interactable = false;
        gameOver.interactable = true;

        StartCoroutine(Fade(gameOver, 1f, 1f));
    }

    public void Escape(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Application.Quit();
        }
    }

    public void FullScreen(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (Screen.fullScreen)
            {
                // ----- 退出全屏，进入窗口化 -----
                // 1. 切换为窗口模式
                Screen.fullScreenMode = FullScreenMode.Windowed;
                // 2. 使用记忆的窗口尺寸恢复窗口大小（渲染分辨率也同步）
                Screen.SetResolution(windowedWidth, windowedHeight, false);
                // 3. 更新 last 变量，避免 Update 误触发
                lastWidth = windowedWidth;
                lastHeight = windowedHeight;
            }
            else
            {
                // ----- 进入全屏（无边框） -----
                // 1. 先记录当前窗口尺寸（作为下次退出时的记忆）
                windowedWidth = Screen.width;
                windowedHeight = Screen.height;
                // 2. 切换到无边框全屏，渲染分辨率设为屏幕原生尺寸（消除黑边）
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
                // 3. 更新 last 变量
                lastWidth = Screen.width;
                lastHeight = Screen.height;
            }
        }
    }

    private IEnumerator Fade(CanvasGroup canvasGroup, float to, float delay)
    {
        yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        float duration = 0.5f;
        float from = canvasGroup.alpha;

        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    private IEnumerator PopText()
    {
        float duration = 0.2f;
        float elapsed = 0f;

        scoreRectTrans.localScale = Vector3.one;

        while (elapsed < duration)
        {
            scoreRectTrans.localScale = Vector3.Lerp(scoreRectTrans.localScale, new Vector3(1.3f, 1.3f, 1.3f), duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        scoreRectTrans.localScale = new Vector3(1.3f, 1.3f, 1.3f);

        duration = 0.1f;
        elapsed = 0f;

        while (elapsed < duration)
        {
            scoreRectTrans.localScale = Vector3.Lerp(scoreRectTrans.localScale, Vector3.one, duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        scoreRectTrans.localScale = Vector3.one;
    }

    public void IncreaseScore(int points)
    {
        SetScore(score + points);
        StartCoroutine(PopText());
    }

    private void SetScore(int score)
    {
        this.score = score;
        scoreText.text = score.ToString();
        SaveHighscore();
    }

    private void SaveHighscore()
    {
        int highscore = LoadHighscore();
        if (score > highscore)
        {
            PlayerPrefs.SetInt("highscore", score);
        }
    }

    private int LoadHighscore()
    {
        return PlayerPrefs.GetInt("highscore", 0);
    }
}