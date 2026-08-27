using UnityEngine;

public class PipeCriticalZone : MonoBehaviour
{
  void OnTriggerEnter2D()
  {
    GameplayManager.INSTANCE.pipeController.onPipeCriticalPass.Invoke();
  }
}
