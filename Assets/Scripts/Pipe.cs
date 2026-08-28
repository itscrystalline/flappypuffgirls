using UnityEngine;

public class Pipe : MonoBehaviour
{
  public float logicalPosition = 0;
  [SerializeField]
  [Range(0.5f, 12f)]
  private float _openingSize = 2.0f;
  public float OpeningSize
  {
    get => _openingSize; set
    {
      _openingSize = value;
      ApplyOpeningSize();
    }
  }

  private Transform stemUpper, stemLower;
  private Transform baseUpper, baseLower;
  private BoxCollider2D criticalZoneUpper, criticalZoneLower;
  private BoxCollider2D hitbox;

  private GameplayManager manager;

  void Awake()
  {
    ResolveChildren();
    manager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameplayManager>();
  }

  void OnValidate()
  {
    ResolveChildren();
    ApplyOpeningSize();
  }

  private void ResolveChildren()
  {
    if (stemUpper == null) stemUpper = transform.Find("PipeUpperStem");
    if (stemLower == null) stemLower = transform.Find("PipeLowerStem");
    if (baseUpper == null) baseUpper = transform.Find("PipeUpper");
    if (baseLower == null) baseLower = transform.Find("PipeLower");
    if (criticalZoneUpper == null && baseUpper != null)
      criticalZoneUpper = baseUpper.Find("CriticalZoneUpper").gameObject.GetComponent<BoxCollider2D>();
    if (criticalZoneLower == null && baseLower != null)
      criticalZoneLower = baseLower.Find("CriticalZoneLower").gameObject.GetComponent<BoxCollider2D>();
    if (hitbox == null) hitbox = GetComponent<BoxCollider2D>();
  }

  private void ApplyOpeningSize()
  {
    if (stemUpper == null) return;
    var sidedOpeningSize = _openingSize / 4f;
    stemUpper.transform.localPosition = new Vector2(0, 2.7896f + sidedOpeningSize);
    stemLower.transform.localPosition = new Vector2(0, -2.8315f - sidedOpeningSize);
    baseUpper.transform.localPosition = new Vector2(0, sidedOpeningSize);
    baseLower.transform.localPosition = new Vector2(0, -sidedOpeningSize);

    hitbox.offset = Vector2.zero;
    hitbox.size = new Vector2(0.2f, _openingSize);

    criticalZoneUpper.offset = Vector2.zero;
    criticalZoneUpper.size = new Vector2(0.3f, _openingSize * 0.1875f);
    criticalZoneLower.offset = Vector2.zero;
    criticalZoneLower.size = new Vector2(0.3f, _openingSize * 0.1875f);
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
