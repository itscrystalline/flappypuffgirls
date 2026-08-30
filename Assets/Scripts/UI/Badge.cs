using System;
using Coroutween;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Badge : MonoBehaviour, IUIElementAnim
{
  private GameplayManager game;

  private Vector2 originalPos;
  private RectTransform rect;
  const float NEW_Y = 300;
  [SerializeField]
  private Image[] images;
  [SerializeField]
  private TextController[] texts;

  public TextController currentScore;
  public TextController bestScore;
  public TextController resetText;

  public GameObject bronzeCoin;
  public GameObject silverCoin;
  public GameObject goldCoin;

  void Awake()
  {
    rect = GetComponent<RectTransform>();
    originalPos = rect.anchoredPosition;
  }

  void Start()
  {
    game = GameplayManager.INSTANCE;
    originalPos = rect.anchoredPosition;

    game.onMenu.AddListener(() => resetText.gameObject.SetActive(false));
    game.onPregame.AddListener(() => resetText.gameObject.SetActive(false));

    game.onJump.AddListener(() =>
    {
      if (game.state == GameState.Postgame)
      {
        _ = FadeOut();
        game.state = GameState.Pregame;
        game.onPregame.Invoke();
      }
    });
  }

  public async Awaitable FadeIn()
  {
    foreach (var img in images)
      img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
    foreach (var text in texts)
      text.Alpha = 1f;
    _ = RunDisplay();
    await Coroutines.RunOverTweened(1500, (tw) =>
    {
      rect.anchoredPosition = new Vector2(originalPos.x, Mathf.Lerp(NEW_Y, originalPos.y, tw));
    }, Tween.EaseOutBounce);
  }

  public async Awaitable FadeOut()
  {
    bronzeCoin.SetActive(false);
    silverCoin.SetActive(false);
    goldCoin.SetActive(false);
    await Coroutines.RunOver(150, (tw, _) =>
    {
      var alpha = Mathf.Lerp(1f, 0f, tw);
      foreach (var img in images)
        img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);
      foreach (var text in texts)
        text.Alpha = alpha;
    });
    rect.anchoredPosition = new Vector2(originalPos.x, NEW_Y);
    resetText.gameObject.SetActive(false);
  }

  public void FadeInImmeadiate()
  {
    foreach (var img in images)
      img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
    foreach (var text in texts)
      text.Alpha = 1f;
    rect.anchoredPosition = new Vector2(originalPos.x, originalPos.y);
    _ = RunDisplay();
  }

  public void FadeOutImmeadiate()
  {
    bronzeCoin.SetActive(false);
    silverCoin.SetActive(false);
    goldCoin.SetActive(false);
    foreach (var img in images)
      img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
    foreach (var text in texts)
      text.Alpha = 0f;
    rect.anchoredPosition = new Vector2(originalPos.x, NEW_Y);
    resetText.gameObject.SetActive(false);
  }

  async Awaitable RunDisplay()
  {
    resetText.Alpha = 0f;
    await Awaitable.WaitForSecondsAsync(0.5f);
    var curScore = game.Score;
    var highScore = game.HighScore;
    var max = Math.Max(curScore, highScore);
    await Coroutines.RunOverTweened(Math.Min(2500, max), tw =>
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
    await Awaitable.WaitForSecondsAsync(0.2f);

    resetText.gameObject.SetActive(true);
    _ = BlinkRestartText();
  }
  async Awaitable PlaceMedal(GameObject medal)
  {
    medal.SetActive(true);
    var image = medal.GetComponent<Image>();
    var rect = medal.GetComponent<RectTransform>();
    var randomAngle = Random.Range(-30, 30);
    // TODO: FIXME: what about the 3 different positions ???????????????????????????????
    // var randomOffset = new Vector2(Random.Range(-10, 10), Random.Range(-10, 10));
    _ = Coroutines.RunOverTweened(500, tw =>
    {
      rect.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(0, randomAngle, tw));
      // rect.anchoredPosition += randomOffset;
    }, Tween.EaseOutCubic);
    await Coroutines.RunOverTweened(500, tw =>
    {
      var twAlpha = Mathf.Lerp(0f, 1f, Math.Clamp(tw, 0f, 0.25f) * 4f);
      image.color = new Color(image.color.r, image.color.g, image.color.b, twAlpha);
      var twScale = Mathf.Lerp(4f, 1f, tw);
      medal.transform.localScale = new Vector2(twScale, twScale);
    }, Tween.EaseOutBounce);
    game.audioController.PlayMedalSound();
  }

  async Awaitable BlinkRestartText()
  {
    while (resetText.gameObject.activeInHierarchy)
    {
      await Coroutines.RunOverTweened(500, tw => resetText.Alpha = Mathf.Lerp(0, 1f, tw));
      if (!resetText.gameObject.activeInHierarchy) return;
      await Coroutines.RunOverTweened(500, tw => resetText.Alpha = Mathf.Lerp(1, 0f, tw));
    }
  }
}
