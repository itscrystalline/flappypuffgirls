using System;
using System.Collections;
using UnityEngine;

public class ScoreDisplayController : MonoBehaviour
{
  private GameplayManager game;
  private TextController controller;
  private TextController effect;

  void Start()
  {
    game = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameplayManager>();
    controller = GetComponent<TextController>();
    effect = transform.Find("ScoreTextEffect").GetComponent<TextController>();

    game.onPipePass.AddListener(() => OnMilestone(5f));
    game.onPipeCriticalPass.AddListener(() =>
    {
      OnMilestone(10f);
    });

    effect.gameObject.SetActive(false);
  }

  void OnMilestone(float increaseFontSizeBy)
  {
    StartCoroutine(DoScoreFade(increaseFontSizeBy));
  }

  float EaseOutQuint(float x) => 1 - Mathf.Pow(1 - x, 5);
  IEnumerator DoScoreFade(float increaseFontSizeBy)
  {
    effect.gameObject.SetActive(true);
    var startingFontSize = effect.Size;
    for (var step = 1; step <= 15; step++)
    {
      var ease = EaseOutQuint(step / 15f);
      effect.Size = startingFontSize + (ease * increaseFontSizeBy);
      effect.Alpha = 1 - ease;
      yield return new WaitForFixedUpdate();
    }
    effect.Alpha = 0;
    effect.Size = startingFontSize;
    effect.gameObject.SetActive(false);
  }

  void Update()
  {
    controller.Text = $"{Math.Round((game.localDifficulty - 1) * 10000)}".PadLeft(10, '0');
  }
}
