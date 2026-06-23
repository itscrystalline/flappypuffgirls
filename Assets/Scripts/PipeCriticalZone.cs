using UnityEngine;

public class PipeCriticalZone : MonoBehaviour
{
  private Manager game;
  void Start()
  {
    while (!Manager.INSTANCE) { }
    game = Manager.INSTANCE;
  }
  void OnTriggerEnter2D()
  {
    game.localDifficulty *= game.criticalDifficultyScaleFactor;
  }
}
