using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
  private InputAction jump;
  private Manager game;
  private Rigidbody2D rb;
  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start()
  {
    jump = InputSystem.actions.FindAction("Jump");
    rb = gameObject.GetComponent<Rigidbody2D>();
    while (!Manager.INSTANCE) { }
    game = Manager.INSTANCE;
  }

  // Update is called once per frame
  void Update()
  {
    var velX = game.PlayerEffectiveSpeed();
    var velY = rb.linearVelocityY;
    var normLook = new Vector2(velX, velY).normalized;
    var lookAngle = Mathf.Atan2(normLook.y, normLook.x) * Mathf.Rad2Deg;
    transform.rotation = Quaternion.Euler(0, 0, lookAngle - 90);

    if (jump.WasPerformedThisFrame())
    {
      rb.linearVelocity = new Vector2(0, game.playerJumpForce);
    }
  }
}
