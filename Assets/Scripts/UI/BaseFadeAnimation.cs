using System;
using System.Linq;
using Coroutween;
using UnityEngine;
using UnityEngine.UI;

public class BaseFadeAnimation : MonoBehaviour, IUIElementAnim
{
  private Image[] images = Array.Empty<Image>();
  private TextController[] texts = Array.Empty<TextController>();

  void Awake()
  {
    images = GetComponentsInChildren<Image>();
    texts = GetComponentsInChildren<TextController>();
  }

  public async Awaitable FadeIn()
  {
    await Coroutines.RunOverTweened(100, tw =>
    {
      var alpha = Mathf.Lerp(0f, 1f, tw);
      foreach (var img in images)
      {
        img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);
      }
      foreach (var text in texts)
      {
        text.Alpha = alpha;
      }
    });
  }

  public void FadeInImmeadiate()
  {
    foreach (var img in images)
    {
      img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
    }
    foreach (var text in texts)
    {
      text.Alpha = 1f;
    }
  }

  public async Awaitable FadeOut()
  {
    await Coroutines.RunOverTweened(100, tw =>
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
  }

  public void FadeOutImmeadiate()
  {
    foreach (var img in images)
    {
      img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
    }
    foreach (var text in texts)
    {
      text.Alpha = 0f;
    }
  }

}
