using UnityEngine;

public class Pipe : MonoBehaviour
{
  public float logicalPosition = 0;
  void FixedUpdate()
  {
    var dist = logicalPosition - Manager.INSTANCE.playerDistance;
    if (dist < -100) Destroy(gameObject);

    var newTransform = transform.position;
    newTransform.x = dist;
    transform.position = newTransform;
  }
  void OnTriggerEnter2D()
  {
    while (!Manager.INSTANCE) { }
    Manager.INSTANCE.pipesPassed += 1;
  }
}
