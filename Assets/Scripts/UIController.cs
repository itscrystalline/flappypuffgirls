using System;
using System.Linq;
using Coroutween;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
  private GameplayManager manager;
  private UIElement[] uiElements = Array.Empty<UIElement>();
  private Image backdrop;

  void Awake()
  {
    uiElements = GameObject.FindGameObjectsWithTag("UI").Select(g => g.GetComponent<UIElement>()).ToArray();
    backdrop = GameObject.FindGameObjectWithTag("Backdrop").GetComponent<Image>();
  }
  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start()
  {
    manager = GameplayManager.INSTANCE;

    manager.onMenu.AddListener(UpdateUIState);
    manager.onPregame.AddListener(UpdateUIState);
    manager.onPlay.AddListener(UpdateUIState);
    manager.onDie.AddListener(UpdateUIState);
    manager.onPostgame.AddListener(UpdateUIState);

    manager.onMenu.AddListener(() => _ = FadeBackdrop(1000, 0.8f, Tween.EaseOutQuint));

    foreach (var e in uiElements)
    {
      if (e.associatedStates.HasFlag(GameState.Menu))
      {
        e.FadeInImmeadiate();
      }
      else
      {
        e.FadeOutImmeadiate();
      }
    }
  }

  void UpdateUIState()
  {
    foreach (var e in uiElements)
    {
      if (e.associatedStates.HasFlag(manager.state))
      {
        e.FadeIn();
      }
      else
      {
        e.FadeOut();
      }
    }
  }

  public async Awaitable FadeBackdrop(uint milliseconds, float targetAlpha, Func<float, float> tweenFunction)
  {
    Color startColor = backdrop.color;
    if (milliseconds == 0)
    {
      backdrop.color = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
      await Awaitable.NextFrameAsync();
    }

    float startAlpha = backdrop.color.a;
    await Coroutines.RunOverTweened(milliseconds, tw =>
    {
      backdrop.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(startAlpha, targetAlpha, tw));
    }, tweenFunction);
  }
}
