using UnityEngine;

public class Pipe : MonoBehaviour
{
  public float logicalPosition = 0;
  [SerializeField]
  private float _openingSize = 2.0f;
  public float OpeningSize
  {
    get => _openingSize; set
    {
      _openingSize = value;
      var sidedOpeningSize = value / 4f;
      stemUpper.transform.localPosition = new Vector2(0, 2.7896f + sidedOpeningSize);
      stemLower.transform.localPosition = new Vector2(0, -2.8315f - sidedOpeningSize);
      baseUpper.transform.localPosition = new Vector2(0, sidedOpeningSize);
      baseLower.transform.localPosition = new Vector2(0, -sidedOpeningSize);

      hitbox.offset = Vector2.zero;
      hitbox.size = new Vector2(0.2f, value);

      criticalZoneUpper.offset = Vector2.zero;
      criticalZoneUpper.size = new Vector2(0.3f, value * 0.1875f);
      criticalZoneLower.offset = Vector2.zero;
      criticalZoneLower.size = new Vector2(0.3f, value * 0.1875f);
    }
  }

  private Transform stemUpper, stemLower;
  private Transform baseUpper, baseLower;
  private BoxCollider2D criticalZoneUpper, criticalZoneLower;
  private BoxCollider2D hitbox;

  private GameplayManager manager;

  void Awake()
  {
    stemUpper = transform.Find("PipeUpperStem");
    stemLower = transform.Find("PipeLowerStem");
    baseUpper = transform.Find("PipeUpper");
    baseLower = transform.Find("PipeLower");
    criticalZoneUpper = baseUpper.Find("CriticalZoneUpper").gameObject.GetComponent<BoxCollider2D>();
    criticalZoneLower = baseLower.Find("CriticalZoneLower").gameObject.GetComponent<BoxCollider2D>();
    hitbox = GetComponent<BoxCollider2D>();
    manager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameplayManager>();
  }

  void FixedUpdate()
  {
    var dist = logicalPosition - manager.playerDistance;
    if (dist < -100) Destroy(gameObject);

    var newTransform = transform.position;
    newTransform.x = dist;
    transform.position = newTransform;
  }

  void OnTriggerEnter2D()
  {
    manager.pipeController.onPipePass.Invoke();
  }
}
