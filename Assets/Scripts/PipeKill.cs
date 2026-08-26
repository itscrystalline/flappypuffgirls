using UnityEngine;

public class PipeKill : MonoBehaviour
{
  private GameplayManager game;
  void Start()
  {
    game = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameplayManager>();

    if (game.noClip)
    {
      var collider = GetComponent<BoxCollider2D>();
      collider.includeLayers = (LayerMask)0;
      collider.excludeLayers = (LayerMask)0b11111111_11111111_1111111_11111111;
    }
  }
  void OnCollisionEnter2D(Collision2D col)
  {
    if (!game.noClip)
      game.PlayerDied();
  }
}
