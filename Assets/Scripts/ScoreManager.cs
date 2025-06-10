using DG.Tweening;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] MatchHandler matches;
    [SerializeField] TextMeshProUGUI scoreText;

    [Header("Pop Settings")]
    public float popScale = 1.5f;
    public float popDuration = 0.2f;
    public Ease popEase = Ease.OutBack;

    int _score;

    private void Start()
    {
        matches.OnTileDestroyed += AddScore;
    }
    public void AddScore(int score)
    {
        _score += score;
        PlayPop();
        scoreText.text = "    Score: " + _score.ToString();
    }

    private void PlayPop()
    {
        scoreText.transform.localScale = Vector3.one;

        var seq = DOTween.Sequence();
        seq
          .Append(scoreText.transform.DOScale(popScale, popDuration).SetEase(popEase))
          .Append(scoreText.transform.DOScale(1f, popDuration).SetEase(popEase));
    }
}
