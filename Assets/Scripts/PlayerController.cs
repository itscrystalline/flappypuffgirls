using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
  private GameplayManager game;
  private Rigidbody2D rb;

  [SerializeField]
  private Transform[] girls;
  private (Transform, Vector2)[] _girls;

  void Awake()
  {
    rb = gameObject.GetComponent<Rigidbody2D>();
    _girls = girls.Select(t => (t, CartesianToPolar(t.localPosition))).ToArray();
  }
  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start()
  {
    game = GameplayManager.INSTANCE;
    game.onJump.AddListener(() => rb.linearVelocity = new Vector2(0, game.playerJumpForce));
  }

  // Update is called once per frame
  void Update()
  {
    var velX = game.playerSpeed;
    var velY = rb.linearVelocityY;
    var normLook = new Vector2(velX, velY).normalized;
    var lookAngle = Mathf.Atan2(normLook.y, normLook.x);

    foreach ((var gt, var gtp) in _girls)
    {
      gt.localPosition = PolarToCartesian(new Vector2(gtp.x, gtp.y + lookAngle * 0.35f));
      gt.localEulerAngles = new Vector3(0, 0, lookAngle * Mathf.Rad2Deg);
    }

  }

  /// x -> r
  /// y -> θ
  Vector2 CartesianToPolar(Vector2 cartisian) =>
    new Vector2(
      x: Mathf.Sqrt(Mathf.Pow(cartisian.x, 2) + Mathf.Pow(cartisian.y, 2)),
      y: Mathf.Atan2(cartisian.y, cartisian.x)
    );
  Vector2 PolarToCartesian(Vector2 polar) => new Vector2(polar.x * Mathf.Cos(polar.y), polar.x * Mathf.Sin(polar.y));
}
