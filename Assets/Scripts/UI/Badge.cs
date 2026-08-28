using System;
using Coroutween;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Badge : MonoBehaviour, IUIElementAnim
{
  private GameplayManager game;

  private Vector2 originalPos;
  const float NEW_Y = 400;
  private Image[] images = Array.Empty<Image>();
  private TextController[] texts = Array.Empty<TextController>();

  public TextController currentScore;
  public TextController bestScore;
  public GameObject bronzeCoin;
  public GameObject silverCoin;
  public GameObject goldCoin;

  void Awake()
  {
    images = GetComponentsInChildren<Image>();
    texts = GetComponentsInChildren<TextController>();
  }
  void Start()
  {
    game = GameplayManager.INSTANCE;
    originalPos = transform.localPosition;
  }

  public async Awaitable FadeIn()
  {
    _ = RunDisplay();
    await Coroutines.RunOverTweened(500, (tw) =>
    {
      transform.localPosition = new Vector2(originalPos.x, Mathf.Lerp(NEW_Y, originalPos.y, tw));
    }, Tween.EaseOutBounce);
  }

  public async Awaitable FadeOut()
  {
    await Coroutines.RunOver(50, (tw, _) =>
    {
      var alpha = Mathf.Lerp(1f, 0f, tw);
      foreach (var img in images)
      {
        img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);
      }
      foreach (var text in texts)
      {
        text.Alpha = alpha;
      }
    });
    transform.localPosition = new Vector2(originalPos.x, NEW_Y);
  }

  public void FadeInImmeadiate()
  {
    transform.localPosition = new Vector2(originalPos.x, originalPos.y);
  }

  public void FadeOutImmeadiate()
  {
    transform.localPosition = new Vector2(originalPos.x, NEW_Y);
  }

  async Awaitable RunDisplay()
  {
    await Awaitable.WaitForSecondsAsync(0.1f);
    var curScore = game.Score;
    var highScore = game.HighScore;
    var max = Math.Max(curScore, highScore);
    await Coroutines.RunOverTweened(Math.Min(1000, max), tw =>
    {
      var cnt = (uint)(max * tw);
      currentScore.Text = $"{Math.Min(cnt, curScore)}".PadLeft(8, '0');
      bestScore.Text = $"{Math.Min(cnt, highScore)}".PadLeft(8, '0');
    }, Tween.EaseOutCubic);

    game.HighScore = Math.Max(game.Score, game.HighScore);

    if (game.Score >= 50000)
    {
      await PlaceMedal(goldCoin);
    }
    else if (game.Score >= 20000)
    {
      await PlaceMedal(silverCoin);
    }
    else if (game.Score >= 5000)
    {
      await PlaceMedal(bronzeCoin);
    }
  }
  async Awaitable PlaceMedal(GameObject medal)
  {
    medal.SetActive(true);
    var image = medal.GetComponent<Image>();
    var rect = medal.GetComponent<RectTransform>();
    var randomAngle = Random.Range(-30, 30);
    // TODO: FIXME: what about the 3 different positions ???????????????????????????????
    // var randomOffset = new Vector2(Random.Range(-10, 10), Random.Range(-10, 10));
    _ = Coroutines.RunOverTweened(250, tw =>
    {
      rect.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(0, randomAngle, tw));
      // rect.anchoredPosition += randomOffset;
    }, Tween.EaseOutCubic);
    await Coroutines.RunOverTweened(250, tw =>
    {
      if (tw <= 0.25f)
      {
        var twAlpha = Mathf.Lerp(0f, 1f, Math.Clamp(tw, 0f, 0.25f) * 4f);
        image.color = new Color(image.color.r, image.color.g, image.color.b, twAlpha);
      }
      var twScale = Mathf.Lerp(4f, 1f, tw);
      medal.transform.localScale = new Vector2(twScale, twScale);
    }, Tween.EaseOutBounce);
  }
}
