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
    if (jump.WasPerformedThisFrame())
    {
      rb.AddForceY(game.playerJumpForce);
    }
  }
}
