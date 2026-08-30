using Coroutween;
using UnityEngine;

public class StartBtn : MonoBehaviour, IUIElementAnim
{
  private Vector2 originalPos;
  private RectTransform rect;
  const float NEW_Y = -100;
  void Awake()
  {
    rect = GetComponent<RectTransform>();
    originalPos = rect.anchoredPosition;
  }

  public async Awaitable FadeIn() =>
    await Coroutines.RunOverTweened(1000, (tw) =>
    {
      rect.anchoredPosition = new Vector2(originalPos.x, Mathf.Lerp(NEW_Y, originalPos.y, tw));
    }, Tween.EaseOutElastic);

  public async Awaitable FadeOut() =>
    await Coroutines.RunOverTweened(500, (tw) =>
    {
      rect.anchoredPosition = new Vector2(originalPos.x, Mathf.Lerp(originalPos.y, NEW_Y, tw));
    }, Tween.EaseOutCubic);

  public void FadeInImmeadiate() => rect.anchoredPosition = new Vector2(originalPos.x, originalPos.y);
  public void FadeOutImmeadiate() => rect.anchoredPosition = new Vector2(originalPos.x, NEW_Y);
}
