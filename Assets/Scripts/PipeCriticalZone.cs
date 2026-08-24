using UnityEngine;

public class PipeCriticalZone : MonoBehaviour
{
  private GameplayManager game;
  void Start()
  {
    game = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameplayManager>();
  }
  void OnTriggerEnter2D()
  {
    game.onPipeCriticalPass.Invoke();
  }
}
