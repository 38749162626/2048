using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro; // 如果使用旧版 Text，请改为 using UnityEngine.UI;

[System.Serializable]
public class RecordData
{
    public int score;
    public string date;
    public bool passed;
}

public class RecordManager : MonoBehaviour
{
    [Header("UI 引用")]
    public Transform content;               // Scroll View 的 Content
    public GameObject itemPrefab;           // 带 Item 脚本的预制件
    public GameObject panel;                // 排行榜面板根对象
    public CanvasGroup canvasGroup;         // panel 上的 CanvasGroup 组件
    public TextMeshProUGUI passCountText;   // 显示通关次数的文本（可拖入 Text 或 TextMeshPro）

    [Header("动画设置")]
    [Range(0.1f, 2f)] public float fadeDuration = 0.3f;

    [Header("外部依赖")]
    public TileBoard tileBoard;

    public static RecordManager Instance { get; private set; }

    private const string PREFS_KEY = "LeaderboardRecords";
    private List<RecordData> records = new List<RecordData>();

    public int PassCount { get; private set; }

    [System.Serializable]
    private class RecordListWrapper
    {
        public List<RecordData> items;
    }

    private Coroutine fadeCoroutine;
    private bool isVisible = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 初始化 CanvasGroup 完全隐藏
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        // 保持面板激活，仅由 CanvasGroup 控制可见性
        if (panel != null && !panel.activeSelf)
            panel.SetActive(true);

        LoadAndRefresh();
    }

    /// <summary>
    /// 显示排行榜（淡入），锁定游戏操作。
    /// </summary>
    public void Show()
    {
        if (isVisible) return;
        isVisible = true;

        RefreshUI();
        UpdatePassCountDisplay();    // 打开时刷新通关次数显示

        if (tileBoard != null)
            tileBoard.waiting = true;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeIn());
    }

    /// <summary>
    /// 隐藏排行榜（淡出），恢复游戏操作。
    /// </summary>
    public void Hide()
    {
        if (!isVisible) return;
        isVisible = false;

        if (tileBoard != null)
            tileBoard.waiting = false;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOut());
    }

    /// <summary>
    /// 返回键回调：根据当前状态切换排行榜显隐。
    /// </summary>
    public void Back(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (isVisible) Hide();
        else Show();
    }

    // ========== 渐变动画 ==========
    private IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        if (canvasGroup == null) yield break;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }

    // ========== 通关次数显示 ==========
    /// <summary>
    /// 更新通关次数 UI 文本（PassCount 变化时自动调用）。
    /// </summary>
    private void UpdatePassCountDisplay()
    {
        if (passCountText != null)
            passCountText.text = PassCount.ToString();
    }

    // ========== 记录管理 ==========
    public static void AddRecord(int score, bool passed)
    {
        if (Instance == null)
        {
            Debug.LogError("RecordManager 实例不存在。");
            return;
        }
        Instance.AddRecordInternal(score, passed);
    }

    private void AddRecordInternal(int score, bool passed)
    {
        records.Add(new RecordData
        {
            score = score,
            date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            passed = passed
        });

        if (passed)
        {
            PassCount++;
            UpdatePassCountDisplay();   // 通关次数变化时立即刷新 UI
        }

        SortRecords();
        SaveRecords();
        RefreshUI();
    }

    public void LoadAndRefresh()
    {
        LoadRecords();
        SortRecords();
        UpdatePassCount();              // 内部会重新统计并更新显示
        RefreshUI();
    }

    private void LoadRecords()
    {
        string json = PlayerPrefs.GetString(PREFS_KEY, "");
        if (!string.IsNullOrEmpty(json))
        {
            var wrapper = JsonUtility.FromJson<RecordListWrapper>(json);
            records = wrapper?.items ?? new List<RecordData>();
        }
        else records = new List<RecordData>();
    }

    private void SaveRecords()
    {
        var wrapper = new RecordListWrapper { items = records };
        PlayerPrefs.SetString(PREFS_KEY, JsonUtility.ToJson(wrapper));
        PlayerPrefs.Save();
    }

    private void SortRecords()
    {
        records.Sort((a, b) =>
        {
            int scoreCmp = b.score.CompareTo(a.score);
            if (scoreCmp != 0) return scoreCmp;
            return string.Compare(b.date, a.date, StringComparison.Ordinal);
        });
    }

    /// <summary>
    /// 从已有记录中重新计算通关次数，并更新 UI 文本。
    /// </summary>
    private void UpdatePassCount()
    {
        PassCount = 0;
        foreach (var rec in records)
            if (rec.passed) PassCount++;

        UpdatePassCountDisplay();   // 统计完成后更新 UI
    }

    public void RefreshUI()
    {
        if (content == null || itemPrefab == null) return;

        foreach (Transform child in content)
            Destroy(child.gameObject);

        for (int i = 0; i < records.Count; i++)
        {
            GameObject itemObj = Instantiate(itemPrefab, content);
            Item item = itemObj.GetComponent<Item>();
            if (item != null) item.Setup(i + 1, records[i]);
            else Debug.LogError("itemPrefab 上缺少 Item 脚本！");
        }
    }
}