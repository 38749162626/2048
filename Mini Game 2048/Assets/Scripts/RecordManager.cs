using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// 单条排行榜记录的数据结构（公开，供 Item 使用）。
/// </summary>
[System.Serializable]
public class RecordData
{
    public int score;
    public string date;    // 格式 "yyyy-MM-dd HH:mm:ss"，添加时自动生成当地时间
    public bool passed;
}

/// <summary>
/// 排行榜管理器：数据的加载、保存、排序、UI刷新。
/// 挂载到排行榜面板的根 GameObject 上。
/// </summary>
public class RecordManager : MonoBehaviour
{
    [Header("UI 引用")]
    [Tooltip("Scroll View 的 Content 对象")]
    public Transform content;
    [Tooltip("列表项预制件（需挂载 Item 脚本）")]
    public GameObject itemPrefab;

    // 单例
    public static RecordManager Instance { get; private set; }

    private const string PREFS_KEY = "LeaderboardRecords";
    private List<RecordData> records = new List<RecordData>();

    /// <summary>
    /// 通关总次数（供外部读取）。
    /// </summary>
    public int PassCount { get; private set; }

    [System.Serializable]
    private class RecordListWrapper
    {
        public List<RecordData> items;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 如需跨场景保留，可取消下一行注释
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadAndRefresh();
    }

    /// <summary>
    /// 外部添加记录的静态入口。
    /// </summary>
    /// <param name="score">分数</param>
    /// <param name="passed">是否通关</param>
    public static void AddRecord(int score, bool passed)
    {
        if (Instance == null)
        {
            Debug.LogError("RecordManager 实例不存在，请确保场景中已挂载该脚本。");
            return;
        }
        Instance.AddRecordInternal(score, passed);
    }

    private void AddRecordInternal(int score, bool passed)
    {
        RecordData newRecord = new RecordData
        {
            score = score,
            date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), // 调用时的当地时间
            passed = passed
        };

        records.Add(newRecord);
        if (passed)
            PassCount++;   // 通关次数加1

        SortRecords();
        SaveRecords();
        RefreshUI();
    }

    /// <summary>
    /// 从 PlayerPrefs 加载并刷新整个排行榜。
    /// </summary>
    public void LoadAndRefresh()
    {
        LoadRecords();
        SortRecords();
        UpdatePassCount();   // 从已保存数据中重新统计通关次数
        RefreshUI();
    }

    private void LoadRecords()
    {
        string json = PlayerPrefs.GetString(PREFS_KEY, "");
        if (!string.IsNullOrEmpty(json))
        {
            RecordListWrapper wrapper = JsonUtility.FromJson<RecordListWrapper>(json);
            if (wrapper != null && wrapper.items != null)
                records = wrapper.items;
            else
                records = new List<RecordData>();
        }
        else
        {
            records = new List<RecordData>();
        }
    }

    private void SaveRecords()
    {
        RecordListWrapper wrapper = new RecordListWrapper { items = records };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(PREFS_KEY, json);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 排序：分数降序，分数相同按日期降序（最新排前）。
    /// </summary>
    private void SortRecords()
    {
        records.Sort((a, b) =>
        {
            int scoreCmp = b.score.CompareTo(a.score);
            if (scoreCmp != 0)
                return scoreCmp;
            return string.Compare(b.date, a.date, StringComparison.Ordinal);
        });
    }

    /// <summary>
    /// 根据 records 列表重新计算通关次数（用于加载存档后初始化）。
    /// </summary>
    private void UpdatePassCount()
    {
        PassCount = 0;
        foreach (var rec in records)
        {
            if (rec.passed)
                PassCount++;
        }
    }

    /// <summary>
    /// 刷新 Scroll View 内容。
    /// </summary>
    public void RefreshUI()
    {
        if (content == null || itemPrefab == null)
        {
            Debug.LogWarning("RecordManager: content 或 itemPrefab 未赋值。");
            return;
        }

        // 清空现有列表项
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // 按排序后的顺序生成列表
        for (int i = 0; i < records.Count; i++)
        {
            GameObject itemObj = Instantiate(itemPrefab, content);
            Item item = itemObj.GetComponent<Item>();
            if (item != null)
            {
                item.Setup(i + 1, records[i]);
            }
            else
            {
                Debug.LogError("itemPrefab 上未找到 Item 脚本！");
            }
        }
    }
        
    public void Back(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (RecordManager.Instance.gameObject.activeSelf)
            {
                RecordManager.Instance.gameObject.SetActive(false);
            }
        }
    }
}