using System;
using UnityEngine;
using Coroutween;

public class ScoreDisplayController : MonoBehaviour
{
  private GameplayManager game;
  private TextController controller;

  void Start()
  {
    game = GameplayManager.INSTANCE;
    controller = GetComponent<TextController>();

    game.pipeController.onPipePass.AddListener(() => OnMilestone(5f));
    game.pipeController.onPipeCriticalPass.AddListener(() => OnMilestone(10f));
  }

  void OnMilestone(float increaseFontSizeBy)
  {
    var dupe = Instantiate(gameObject, transform.parent).GetComponent<TextController>();
    Destroy(dupe.gameObject.GetComponent<ScoreDisplayController>());
    _ = DoScoreFade(dupe, increaseFontSizeBy);
  }

  async Awaitable DoScoreFade(TextController dupe, float increaseFontSizeBy)
  {
    dupe.gameObject.SetActive(true);
    var startingFontSize = dupe.Size;
    await Coroutines.RunOverTweened(250, tw =>
    {
      dupe.Size = Mathf.Lerp(startingFontSize, startingFontSize + increaseFontSizeBy, tw);
      dupe.Alpha = 1 - tw;
    }, Tween.EaseOutQuint);
    Destroy(dupe.gameObject);
  }

  void Update()
  {
    controller.Text = $"{game.Score}".PadLeft(8, '0');
  }
}
