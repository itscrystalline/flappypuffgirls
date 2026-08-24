using System;
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
  }

  void Update()
  {
    foreach (var t in children) t.text = $"{Math.Round((game.localDifficulty - 1) * 10000)}".PadLeft(12, '0');
  }
}
