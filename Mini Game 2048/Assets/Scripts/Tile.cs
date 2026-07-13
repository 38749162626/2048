using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Tile : MonoBehaviour
{
    public TileState state { get; private set; }
    public TileCell cell { get; private set; }
    public int number { get; private set; }
    public bool locked {  get; set; }

    private Image background;
    private TextMeshProUGUI text;

    private RectTransform rectTransform;

    private void Awake()
    {
        background = GetComponent<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetState(TileState state, int number)
    {
        this.state = state;
        this.number = number;

        background.color = state.backgroundColor;
        text.color = state.textColor;
        text.text = number.ToString();
    }

    public void Spawn(TileCell cell)
    {
        if (this.cell != null)
        {
            this.cell.tile = null;
        }

        this.cell = cell;
        this.cell.tile = this;

        transform.position = cell.transform.position;

        StartCoroutine(SpawnAnimate());
    }

    private IEnumerator SpawnAnimate(float duration = 0.5f)
    {
        background.color = new Color(background.color.r, background.color.g, background.color.b, 1);
        rectTransform.localScale = Vector3.zero;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, Vector3.one, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rectTransform.localScale = Vector3.one;
    }

    public void MoveTo(TileCell cell)
    {
        if (this.cell != null)
        {
            this.cell.tile = null;
        }

        this.cell = cell;
        this.cell.tile = this;

        StartCoroutine(MoveAnimate(cell.transform.position));
    }

    private IEnumerator MoveAnimate(Vector3 to, bool merging = false, float duration = 0.1f)
    {
        float elapsed = 0f;

        Vector3 from = transform.position;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;

            Debug.Log(Vector3.Distance(transform.position, to));

            if (merging && Vector3.Distance(transform.position, to) < 95)
                Destroy(gameObject);

            yield return null;
        }

        transform.position = to;
    }

    public void Merge(TileCell cell)
    {
        if (this.cell != null)
        {
            this.cell.tile = null;
        }

        this.cell = null;
        cell.tile.locked = true;

        StartCoroutine(MoveAnimate(cell.transform.position, true));
    }

    public IEnumerator MergeAnimate(float duration = 0.1f)
    {
        rectTransform.localScale = Vector3.one;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, new Vector3(1.15f, 1.15f, 1.15f), elapsed / duration / 2);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;

        while (elapsed < duration)
        {
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, Vector3.one, elapsed / duration / 2);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rectTransform.localScale = Vector3.one;
    }
}
