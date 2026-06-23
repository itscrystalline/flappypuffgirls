using UnityEngine;

public class Pipe : MonoBehaviour
{
  private Manager game;
  public float logicalPosition = 0;
  void Start()
  {
    while (!Manager.INSTANCE) { }
    game = Manager.INSTANCE;
  }
  void FixedUpdate()
  {
    var newTransform = transform.position;
    newTransform.x = logicalPosition - game.playerDistance;
    transform.position = newTransform;
  }
  void OnTriggerEnter2D()
  {
    game.pipesPassed += 1;
  }
}
