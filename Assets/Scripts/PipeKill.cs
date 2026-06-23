using UnityEngine;

public class PipeKill : MonoBehaviour
{
  private Manager game;
  void Start()
  {
    while (!Manager.INSTANCE) { }
    game = Manager.INSTANCE;

    if (game.noClip)
    {
      var collider = GetComponent<BoxCollider2D>();
      collider.includeLayers = 0;
      collider.excludeLayers = LayerMask.GetMask("Everything");
    }
  }
  void OnCollisionEnter2D(Collision2D col)
  {
    if (!game.noClip) game.ResetGame();
  }
}
