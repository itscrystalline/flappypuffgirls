using UnityEngine;

public class Pipe : MonoBehaviour
{
  private Manager game;
  void Start()
  {
    while (!Manager.INSTANCE) { }
    game = Manager.INSTANCE;
  }
  void FixedUpdate()
  {
    var newTransform = transform.position;
    newTransform.x -= game.playerEffectiveSpeed() * Time.deltaTime;
    transform.position = newTransform;
  }
  void OnTriggerEnter2D()
  {
    game.pipesPassed += 1;
  }
}
