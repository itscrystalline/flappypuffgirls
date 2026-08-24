using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class ScoreDisplayController : MonoBehaviour
{
  private GameplayManager game;
  private TMP_Text[] children;

  void Start()
  {
    game = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameplayManager>();
    children = GetComponentsInChildren<TMP_Text>(true);

    game.onPipePass.AddListener(() => OnMilestone(5f));
    game.onPipeCriticalPass.AddListener(() => OnMilestone(10f));
  }

  void OnMilestone(float increaseFontSizeBy)
  {
    var clones = children.Select(t => Instantiate(t.gameObject, transform).GetComponent<TMP_Text>()).ToArray();
    StartCoroutine(DoScoreFade(clones, increaseFontSizeBy));
  }

  float EaseOutQuint(float x) => 1 - Mathf.Pow(1 - x, 5);
  IEnumerator DoScoreFade(TMP_Text[] texts, float increaseFontSizeBy)
  {
    var startingFontSize = texts[0].fontSize;
    for (var step = 1; step <= 15; step++)
    {
      foreach (var t in texts)
      {
        var ease = EaseOutQuint(step / 15f);
        t.fontSize = startingFontSize + (ease * increaseFontSizeBy);
        t.color = new Color(t.color.r, t.color.g, t.color.b, 1 - ease);
      }
      yield return new WaitForFixedUpdate();
    }
    foreach (var t in texts) Destroy(t.gameObject);
  }

  void Update()
  {
    foreach (var t in children) t.text = $"{Math.Round((game.localDifficulty - 1) * 10000)}".PadLeft(10, '0');
  }
}
