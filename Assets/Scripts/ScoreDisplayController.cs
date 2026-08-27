using System;
using System.Collections;
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
    StartCoroutine(DoScoreFade(dupe, increaseFontSizeBy));
  }

  IEnumerator DoScoreFade(TextController dupe, float increaseFontSizeBy)
  {
    dupe.gameObject.SetActive(true);
    var startingFontSize = dupe.Size;
    yield return Coroutines.RunOverTweened(250, tw =>
    {
      dupe.Size = Mathf.Lerp(startingFontSize, startingFontSize + increaseFontSizeBy, tw);
      dupe.Alpha = 1 - tw;
    }, Tween.EaseOutQuint);
    Destroy(dupe.gameObject);
  }

  void Update()
  {
    controller.Text = $"{Math.Round((game.localDifficulty - 1) * 10000)}".PadLeft(8, '0');
  }
}
