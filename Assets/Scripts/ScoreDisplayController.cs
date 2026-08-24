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

    game.onPipePass.AddListener(OnMilestone);
    game.onPipeCriticalPass.AddListener(OnMilestone);
  }

  void OnMilestone()
  {
    var clones = children.Select(t => Instantiate(t.gameObject, transform).GetComponent<TMP_Text>()).ToArray();
    StartCoroutine(DoScoreFade(clones));
  }

  IEnumerator DoScoreFade(TMP_Text[] texts)
  {
    for (var step = 1; step <= 15; step++)
    {
      foreach (var t in texts)
      {
        t.fontSize++;
        t.color = new Color(t.color.r, t.color.g, t.color.b, t.color.a - 1f / 15f);
      }
      yield return null;
      yield return null;
      yield return null;
    }
    foreach (var t in texts) Destroy(t.gameObject);
    yield return 0;
  }

  void Update()
  {
    foreach (var t in children) t.text = $"{Math.Round((game.localDifficulty - 1) * 10000)}".PadLeft(12, '0');
  }
}
