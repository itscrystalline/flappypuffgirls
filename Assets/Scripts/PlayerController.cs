using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
  private InputAction jump;
  private GameplayManager game;
  private Rigidbody2D rb;
  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start()
  {
    jump = InputSystem.actions.FindAction("Jump");
    rb = gameObject.GetComponent<Rigidbody2D>();
    game = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameplayManager>();
  }

  // Update is called once per frame
  void Update()
  {
    var velX = game.playerSpeed;
    var velY = rb.linearVelocityY;
    var normLook = new Vector2(velX, velY).normalized;
    var lookAngle = Mathf.Atan2(normLook.y, normLook.x) * Mathf.Rad2Deg;
    transform.rotation = Quaternion.Euler(0, 0, lookAngle);

    if (jump.WasPerformedThisFrame())
    {
      rb.linearVelocity = new Vector2(0, game.playerJumpForce);
    }
  }
}
