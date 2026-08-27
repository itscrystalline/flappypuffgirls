#nullable enable

using UnityEngine;
using TMPro;
using Coroutween;

public class TextController : MonoBehaviour, IUIElementAnim
{
  private TMP_Text[] Children { get => GetComponentsInChildren<TMP_Text>(true); }

  private string? textInner;
  public string Text
  {
    get => textInner ?? Children[0].text; set
    {
      textInner = value;
      foreach (var t in Children) t.text = value;
    }
  }

  private float? sizeInner;
  public float Size
  {
    get => sizeInner ?? Children[0].fontSize; set
    {
      sizeInner = value;
      foreach (var t in Children) t.fontSize = value;
    }
  }

  private float? alphaInner;
  public float Alpha
  {
    get => alphaInner ?? Children[0].color.a; set
    {
      alphaInner = value;
      foreach (var t in Children) t.color = new Color(t.color.r, t.color.g, t.color.b, value);
    }
  }

  public async Awaitable FadeIn()
  {
    await Coroutines.RunOver(100, (a, _) =>
    {
      Alpha = a;
    });
  }

  public async Awaitable FadeOut()
  {
    await Coroutines.RunOver(100, (a, _) =>
    {
      Alpha = 1 - a;
    });
  }

  public void FadeInImmeadiate()
  {
    Alpha = 1f;
  }

  public void FadeOutImmeadiate()
  {
    Alpha = 0f;
  }
}
