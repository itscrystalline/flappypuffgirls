using System;
using System.Collections;
using UnityEngine;
using Coroutween;

public class ScoreDisplayController : MonoBehaviour
{
  private GameplayManager game;
  private TextController controller;
  private TextController effect;

  void Start()
  {
    game = GameplayManager.INSTANCE;
    controller = GetComponent<TextController>();
    effect = transform.Find("ScoreTextEffect").GetComponent<TextController>();

    game.pipeController.onPipePass.AddListener(() => OnMilestone(5f));
    game.pipeController.onPipeCriticalPass.AddListener(() =>
    {
      OnMilestone(10f);
    });

    effect.gameObject.SetActive(false);
  }

  void OnMilestone(float increaseFontSizeBy)
  {
    StartCoroutine(DoScoreFade(increaseFontSizeBy));
  }

  IEnumerator DoScoreFade(float increaseFontSizeBy)
  {
    effect.gameObject.SetActive(true);
    var startingFontSize = effect.Size;
    yield return Coroutines.RunOverTweened(250, (tweened) =>
    {
      effect.Size = startingFontSize + (tweened * increaseFontSizeBy);
      effect.Alpha = 1 - tweened;
    }, Tween.EaseOutQuint);
    effect.Alpha = 0;
    effect.Size = startingFontSize;
    effect.gameObject.SetActive(false);
  }

  void Update()
  {
    controller.Text = $"{Math.Round((game.localDifficulty - 1) * 10000)}".PadLeft(8, '0');
  }
}
