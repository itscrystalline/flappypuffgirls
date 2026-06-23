#nullable enable

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class Manager : MonoBehaviour
{
  [SerializeField]
  private float playerBaseSpeed = 5;
  public float playerJumpForce = 15;
  public bool noClip = false;
  public GameObject[] pipePrefabs = new GameObject[] { };

  // State
  public double localDifficulty = 1.0;
  public float playerDistance = 0.0f;
  public int pipesPassed = 0;

  private GameObject? player = null;
  private Pipe? lastPipe = null;
  private float viewportWidth = 0.0f;

  public float PlayerEffectiveSpeed()
  {
    return PlayerEffectiveSpeed(localDifficulty);
  }
  public float PlayerEffectiveSpeed(double difficulty)
  {
    return (float)(playerBaseSpeed + Math.Pow(difficulty, 1.05));
  }


  [HideInInspector]
  public static Manager? INSTANCE = null;
  private InputAction? reset;

  void Awake()
  {
    if (INSTANCE != null)
    {
      Destroy(gameObject);
      return;
    }
    DontDestroyOnLoad(gameObject);
    INSTANCE = this;
    reset = InputSystem.actions.FindAction("Reset");
  }

  void Start()
  {
    if (SceneManager.GetActiveScene().name == "Game")
    {
      player = GameObject.FindGameObjectWithTag("Player");
      var wallColliders = GameObject.FindGameObjectsWithTag("PlayWall").Select(g => g.GetComponent<BoxCollider2D>()).ToArray();
      var camera = Camera.main;
      var viewportHeight = camera.orthographicSize;
      var viewportWidth = viewportHeight * camera.aspect;
      this.viewportWidth = viewportWidth;
      Debug.Log($"camera w/h: {viewportWidth * 2} x {viewportHeight * 2}");

      if (wallColliders.Length < 4)
      {
        Debug.LogError("Less than 4 wall colliders!");
        return;
      }

      var left = (new Vector2(-viewportWidth, -viewportHeight), new Vector2(-0.5f, viewportHeight));
      var right = (new Vector2(0.5f, -viewportHeight), new Vector2(viewportWidth, viewportHeight));
      var top = (new Vector2(-viewportWidth, viewportHeight), new Vector2(viewportWidth, viewportHeight + 0.5f));
      var bottom = (new Vector2(-viewportWidth, -viewportHeight - 0.5f), new Vector2(viewportWidth, -viewportHeight));

      foreach ((BoxCollider2D box, (Vector2, Vector2) corners) in wallColliders.Zip(new (Vector2, Vector2)[] { left, right, bottom, top }, (a, b) => (a, b)))
      {
        box.offset = CentroidOf(corners);
        box.size = SizeOf(corners);
      }
    }
  }

  void Update()
  {
    if (reset!.WasPerformedThisFrame() && SceneManager.GetActiveScene().name == "Game")
    {
      ResetGame();
    }
  }
  void FixedUpdate()
  {
    if (SceneManager.GetActiveScene().name == "Game")
    {
      localDifficulty += 0.1 * Time.deltaTime;
      playerDistance += PlayerEffectiveSpeed() * Time.deltaTime;
      SpawnNextPipe();
    }
  }

  public void ResetGame()
  {
    localDifficulty = 1.0;
    playerDistance = 0.0f;
    pipesPassed = 0;
    player!.transform.position = Vector3.zero;
    player!.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, playerJumpForce);
  }

  private void SpawnNextPipe()
  {
    if (lastPipe && lastPipe.logicalPosition - playerDistance > viewportWidth) return;
    if (pipePrefabs.Length == 0) return;


    var pipe = Instantiate(pipePrefabs[Random.Range(0, pipePrefabs.Length)]).GetComponent<Pipe>();
    if (!lastPipe)
    {
      pipe.logicalPosition = viewportWidth;
      pipe.transform.position = new Vector2(viewportWidth, 0);
    }
    else
    {
      pipe.logicalPosition = lastPipe.logicalPosition + 10;
      pipe.transform.position = new Vector2(pipe.logicalPosition - playerDistance, 0);
    }
    lastPipe = pipe;

    SpawnNextPipe();
  }

  static Vector2 CentroidOf((Vector2, Vector2) vecs)
  {
    return new Vector2((vecs.Item2.x + vecs.Item1.x) / 2, (vecs.Item2.y + vecs.Item1.y) / 2);
  }
  static Vector2 SizeOf((Vector2, Vector2) vecs)
  {
    return new Vector2(Math.Abs(vecs.Item2.x - vecs.Item1.x), Math.Abs(vecs.Item2.y - vecs.Item1.y));
  }
}
