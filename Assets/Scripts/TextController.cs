#nullable enable

using UnityEngine;
using TMPro;
using Coroutween;
using System;

public class TextController : MonoBehaviour, IUIElementAnim
{
  private TMP_Text[] children = Array.Empty<TMP_Text>();
  void Awake() => children = GetComponentsInChildren<TMP_Text>();

  private string? textInner;
  public string Text
  {
    get => textInner ?? children[0].text; set
    {
      textInner = value;
      foreach (var t in children) t.text = value;
    }
  }

  private float? sizeInner;
  public float Size
  {
    get => sizeInner ?? children[0].fontSize; set
    {
      sizeInner = value;
      foreach (var t in children) t.fontSize = value;
    }
  }

  private float? alphaInner;
  public float Alpha
  {
    get => alphaInner ?? children[0].color.a; set
    {
      alphaInner = value;
      foreach (var t in children) t.color = new Color(t.color.r, t.color.g, t.color.b, value);
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
