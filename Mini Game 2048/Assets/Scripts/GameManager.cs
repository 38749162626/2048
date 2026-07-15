using System.Collections;
using TMPro;
using UnityEngine;
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

    private void Start()
    {
        NewGame();
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

        if(score > highscore)
        {
            PlayerPrefs.SetInt("highscore", score);
        }
    }

    private int LoadHighscore()
    {
        return PlayerPrefs.GetInt("highscore", 0);
    }
}
