using UnityEngine;

public class Pipe : MonoBehaviour
{
  public float logicalPosition = 0;
  public float openingSize = 2.0f;

  private Transform stemUpper, stemLower;
  private Transform baseUpper, baseLower;
  private BoxCollider2D criticalZoneUpper, criticalZoneLower;
  private BoxCollider2D hitbox;

  void Start()
  {
    stemUpper = transform.Find("PipeUpperStem");
    stemLower = transform.Find("PipeLowerStem");
    baseUpper = transform.Find("PipeUpper");
    baseLower = transform.Find("PipeLower");
    criticalZoneUpper = baseUpper.Find("CriticalZoneUpper").gameObject.GetComponent<BoxCollider2D>();
    criticalZoneLower = baseLower.Find("CriticalZoneLower").gameObject.GetComponent<BoxCollider2D>();
    hitbox = GetComponent<BoxCollider2D>();
  }

  void FixedUpdate()
  {
    var dist = logicalPosition - Manager.INSTANCE.playerDistance;
    if (dist < -100) Destroy(gameObject);

    var newTransform = transform.position;
    newTransform.x = dist;
    transform.position = newTransform;

    SetOpeningSize();
  }

  void SetOpeningSize()
  {
    var sidedOpeningSize = openingSize / 4f;
    stemUpper.transform.localPosition = new Vector2(0, 2.7896f + sidedOpeningSize);
    stemLower.transform.localPosition = new Vector2(0, -2.8315f - sidedOpeningSize);
    baseUpper.transform.localPosition = new Vector2(0, sidedOpeningSize);
    baseLower.transform.localPosition = new Vector2(0, -sidedOpeningSize);

    hitbox.offset = Vector2.zero;
    hitbox.size = new Vector2(0.2f, openingSize);

    criticalZoneUpper.offset = Vector2.zero;
    criticalZoneUpper.size = new Vector2(0.3f, openingSize * 0.1875f);
    criticalZoneLower.offset = Vector2.zero;
    criticalZoneLower.size = new Vector2(0.3f, openingSize * 0.1875f);
  }

  void OnTriggerEnter2D()
  {
    while (!Manager.INSTANCE) { }
    Manager.INSTANCE.pipesPassed += 1;
  }
}
