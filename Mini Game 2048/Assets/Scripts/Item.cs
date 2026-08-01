using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 排行榜单条记录显示组件，支持前三名特殊颜色。
/// </summary>
public class Item : MonoBehaviour
{
    [Header("UI 组件")]
    public TextMeshProUGUI rankText;     // 名次
    public TextMeshProUGUI scoreText;    // 分数
    public TextMeshProUGUI dateText;     // 日期
    public Image passedImage;            // 通关图标（通关时激活）


    private Color goldColor   = new Color(0.8962264f, 0.6230483f, 0.0295924f);       // 金色
    private Color silverColor = new Color(0.5f, 0.5f, 0.5f); // 银色
    private Color bronzeColor = new Color(0.6698113f, 0.4171349f, 0.1611338f);    // 铜色
    private Color defaultRankColor = new Color(0.4901961f, 0.454902f, 0.4235294f);               // 其他名次颜色

    /// <summary>
    /// 设置列表项数据。
    /// </summary>
    /// <param name="rank">名次（从1开始）</param>
    /// <param name="data">记录数据</param>
    public void Setup(int rank, RecordData data)
    {
        if (rankText != null)
        {
            rankText.text = "NO." + rank.ToString();

            // 根据名次设置颜色
            switch (rank)
            {
                case 1: rankText.color = goldColor; break;
                case 2: rankText.color = silverColor; break;
                case 3: rankText.color = bronzeColor; break;
                default: rankText.color = defaultRankColor; break;
            }
        }

        if (scoreText != null) scoreText.text = data.score.ToString();
        if (dateText != null) dateText.text = data.date;
        if (passedImage != null) passedImage.gameObject.SetActive(data.passed);
    }
}