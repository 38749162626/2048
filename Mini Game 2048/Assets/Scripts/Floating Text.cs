using UnityEngine;
using System.Collections;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public RectTransform rectTrans;
    public TextMeshProUGUI textMeshPro;
    public CanvasGroup canvasGroup;

    public float lifetime = 1.5f;
    private float moveSpeed = 15f;

    private float timer = 0f;
    private Vector3 moveDirection;

    private void Start()
    {
        moveDirection = new Vector3(Random.Range(-0.9f, 0.9f), 1, 0);
        rectTrans = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        textMeshPro = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        rectTrans.anchoredPosition += (Vector2)(moveDirection * moveSpeed * Time.deltaTime);

        float progress = timer / lifetime;
        canvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
    }

    public IEnumerator SetText(string content)
    {
        if (textMeshPro == null && rectTrans == null)
        {
            yield return null;
        }
        textMeshPro.text = content;
        rectTrans.anchoredPosition = new Vector3(70, 233, 0) + new Vector3(Random.Range(-20, 20), Random.Range(-20, 20), 0);
    }
}