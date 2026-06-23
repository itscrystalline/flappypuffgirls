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
    var dist = logicalPosition - game.playerDistance;
    if (dist < -1000) Destroy(gameObject);

    var newTransform = transform.position;
    newTransform.x = dist;
    transform.position = newTransform;
  }
  void OnTriggerEnter2D()
  {
    game.pipesPassed += 1;
  }
}
