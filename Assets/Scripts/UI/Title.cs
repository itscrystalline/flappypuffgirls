using Coroutween;
using UnityEngine;

public class Title : MonoBehaviour, IUIElementAnim
{
  private Vector2 originalPos = new();
  const float NEW_Y = 386;
  void Awake() => originalPos = transform.localPosition;

  public async Awaitable FadeIn()
  {
    await Coroutines.RunOverTweened(1500, (tw) =>
    {
      transform.localPosition = new Vector2(originalPos!.x, Mathf.Lerp(NEW_Y, originalPos!.y, tw));
    }, Tween.EaseOutElastic);
  }

  public async Awaitable FadeOut()
  {
    await Coroutines.RunOverTweened(500, (tw) =>
    {
      transform.localPosition = new Vector2(originalPos.x, Mathf.Lerp(originalPos.y, NEW_Y, tw));
    }, Tween.EaseOutElastic);
  }

  public void FadeInImmeadiate()
  {
    transform.localPosition = new Vector2(originalPos.x, originalPos.y);
  }

  public void FadeOutImmeadiate()
  {
    transform.localPosition = new Vector2(originalPos.x, NEW_Y);
  }
}
