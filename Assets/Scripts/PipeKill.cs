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
      collider.includeLayers = (LayerMask)0;
      collider.excludeLayers = (LayerMask)0b11111111_11111111_1111111_11111111;
    }
  }
  void OnCollisionEnter2D(Collision2D col)
  {
    if (!game.noClip) game.ResetGame();
  }
}
