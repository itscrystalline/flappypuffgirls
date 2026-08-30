using UnityEngine;

public class PlayerController : MonoBehaviour
{
  private GameplayManager game;
  private Rigidbody2D rb;

  [SerializeField]
  private GameObject blossom, buttercup, bubbles;
  private Transform selectedGirl = null;

  void Awake()
  {
    rb = gameObject.GetComponent<Rigidbody2D>();
  }
  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start()
  {
    game ??= GameplayManager.INSTANCE;
    game.onJump.AddListener(() => rb.linearVelocity = new Vector2(0, game.playerJumpForce));
  }

  void OnEnable()
  {
    game ??= GameplayManager.INSTANCE;
    switch (game.playerSprite)
    {
      case PlayerSprite.Blossom:
        blossom.SetActive(true);
        selectedGirl = blossom.transform;
        break;
      case PlayerSprite.Bubbles:
        bubbles.SetActive(true);
        selectedGirl = bubbles.transform;
        break;
      case PlayerSprite.Buttercup:
        buttercup.SetActive(true);
        selectedGirl = buttercup.transform;
        break;
    }
  }
  void OnDisable()
  {
    blossom.SetActive(false);
    bubbles.SetActive(false);
    buttercup.SetActive(false);
    selectedGirl = null;
  }

  // Update is called once per frame
  void Update()
  {
    var velX = game.playerSpeed;
    var velY = rb.linearVelocityY;
    var normLook = new Vector2(velX, velY).normalized;
    var lookAngle = Mathf.Atan2(normLook.y, normLook.x);

    selectedGirl.localEulerAngles = new Vector3(0, 0, lookAngle * Mathf.Rad2Deg * 0.75f);
  }

  // /// x -> r
  // /// y -> θ
  // Vector2 CartesianToPolar(Vector2 cartisian) =>
  //   new Vector2(
  //     x: Mathf.Sqrt(Mathf.Pow(cartisian.x, 2) + Mathf.Pow(cartisian.y, 2)),
  //     y: Mathf.Atan2(cartisian.y, cartisian.x)
  //   );
  // Vector2 PolarToCartesian(Vector2 polar) => new Vector2(polar.x * Mathf.Cos(polar.y), polar.x * Mathf.Sin(polar.y));
}
