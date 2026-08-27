using System.Collections;
using Coroutween;
using UnityEngine;

public class StartBtn : MonoBehaviour, IUIElementAnim
{
  private Vector2 originalPos;
  const float NEW_Y = -361;
  void Start()
  {
    originalPos = transform.localPosition;
  }

  public IEnumerator FadeIn()
  {
    yield return Coroutines.RunOverTweened(250, (tw) =>
    {
      transform.localPosition = new Vector2(originalPos.x, Mathf.Lerp(NEW_Y, originalPos.y, tw));
    }, Tween.EaseOutElastic);
  }

  public IEnumerator FadeOut()
  {
    yield return Coroutines.RunOverTweened(250, (tw) =>
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
