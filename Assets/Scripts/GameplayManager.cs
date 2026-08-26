#nullable enable

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum GameState
{
  Menu,
  Pregame,
  Playing,
  Died
}

public class GameplayManager : MonoBehaviour
{
  [SerializeField]
  private float playerBaseSpeed = 6;
  public float playerJumpForce = 11;
  public double difficultyScaleFactor = 1.005;
  public double difficultySpeedScaleFactor = 1.1;
  public double criticalDifficultyScaleFactor = 1.01;
  public bool noClip = false;

  [SerializeField]
  [Range(0.3f, 1.8f)]
  private float difficultyScaledRandomBound1 = 1.1f;
  [SerializeField]
  [Range(0.5f, 2.0f)]
  private float difficultyScaledRandomBound2 = 1.3f;
  [SerializeField]
  [Range(-6f, 0f)]
  private float pipeHeightRandomBound1 = -2f;
  [SerializeField]
  [Range(0f, 6f)]
  private float pipeHeightRandomBound2 = 2f;

  public GameObject[] pipePrefabs = Array.Empty<GameObject>();
  public GameObject? backgroundDay = null, backgroundNight = null;

  // State
  [SerializeField]
  private GameState _state = GameState.Menu;
  public double localDifficulty = 1.0;
  public float playerDistance = 0.0f;
  public int pipesPassed = 0;

  private float minPipeGap = 0f;
  private GameObject? player = null;
  private Image? backdrop = null;
  private Pipe? lastPipe = null;
  private float viewportWidth = 0.0f;
  private Dictionary<GameState, GameObject[]> uiElements = new Dictionary<GameState, GameObject[]>();

  public float PlayerEffectiveSpeed() => PlayerEffectiveSpeed(localDifficulty);
  public float PlayerEffectiveSpeed(double difficulty)
  {
    return (float)(playerBaseSpeed + Math.Pow(difficulty, difficultySpeedScaleFactor));
  }

  // Events
  public UnityEvent onPipePass = new();
  public UnityEvent onPipeCriticalPass = new();


  [HideInInspector]
  public static GameplayManager? INSTANCE = null;
  private InputAction? reset;
  private InputAction? jump;


  public GameState State
  {
    get => _state; set
    {
      _state = value;
      foreach ((GameState g, GameObject[] elems) in uiElements) foreach (var e in elems) e.SetActive(g == value);
      switch (value)
      {
        case GameState.Menu:
          player!.SetActive(false);
          DoBackdropFade(0, 0.8313725490f, x => x);
          break;
        case GameState.Pregame:
          break;
        case GameState.Playing:
          player!.SetActive(true);
          break;
        case GameState.Died:
          print("Died!!");
          break;
      }
    }
  }

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
    player = GameObject.FindGameObjectWithTag("Player");
    player!.SetActive(false);

    jump = InputSystem.actions.FindAction("Jump");

    minPipeGap = playerJumpForce * playerJumpForce / (Mathf.Abs(Physics2D.gravity.y * player.GetComponent<Rigidbody2D>().gravityScale) * 2f);

    var wallColliders = GameObject.FindGameObjectsWithTag("PlayWall").Select(g => g.GetComponent<BoxCollider2D>()).ToArray();
    var camera = Camera.main;
    var viewportHeight = camera.orthographicSize;
    var viewportWidth = viewportHeight * camera.aspect;
    this.viewportWidth = viewportWidth;
    Debug.Log($"camera w/h: {viewportWidth * 2} x {viewportHeight * 2}");

    backdrop = GameObject.FindGameObjectWithTag("Backdrop").GetComponent<Image>();

    var left = (new Vector2(-viewportWidth, -viewportHeight), new Vector2(-0.5f, viewportHeight));
    var right = (new Vector2(0.5f, -viewportHeight), new Vector2(viewportWidth, viewportHeight));
    var top = (new Vector2(-viewportWidth, viewportHeight), new Vector2(viewportWidth, viewportHeight + 0.5f));
    var bottom = (new Vector2(-viewportWidth, -viewportHeight - 0.5f), new Vector2(viewportWidth, -viewportHeight));

    foreach ((BoxCollider2D box, (Vector2, Vector2) corners) in wallColliders.Zip(new (Vector2, Vector2)[] { left, right, bottom, top }, (a, b) => (a, b)))
    {
      box.offset = CentroidOf(corners);
      box.size = SizeOf(corners);
    }

    foreach ((var bkgrnd, int idx) in new[] { backgroundDay!, backgroundNight! }.Select((g, i) => (g, i)))
    {
      var sprite = bkgrnd.GetComponent<SpriteRenderer>();
      var height = sprite.size;
      var screenHeight = viewportHeight * 2;
      var scalingRatio = screenHeight / height.y;
      sprite.transform.localScale = new Vector2(scalingRatio, scalingRatio);
    }

    uiElements[GameState.Menu] = GameObject.FindGameObjectsWithTag("UIMenu");
    uiElements[GameState.Pregame] = GameObject.FindGameObjectsWithTag("UIPregame");
    uiElements[GameState.Playing] = GameObject.FindGameObjectsWithTag("UIGame");
    uiElements[GameState.Died] = GameObject.FindGameObjectsWithTag("UIPostgame");

    foreach ((GameState g, GameObject[] elems) in uiElements) foreach (var e in elems) e.SetActive(g == State);

    onPipePass.AddListener(() => pipesPassed += 1);
    onPipeCriticalPass.AddListener(() => localDifficulty *= criticalDifficultyScaleFactor);
  }

  void Update()
  {
    if (reset!.WasPerformedThisFrame())
    {
      ResetGame(true);
    }

    if (jump!.WasPerformedThisFrame() && State == GameState.Menu)
    {
      StartGame();
    }
  }
  void FixedUpdate()
  {
    if (State == GameState.Playing || State == GameState.Menu)
    {
      if (State == GameState.Playing) localDifficulty *= ((difficultyScaleFactor - 1) * Time.fixedDeltaTime) + 1;
      playerDistance += PlayerEffectiveSpeed() * Time.fixedDeltaTime;
    }
    SpawnNextPipe();
  }

  public void StartGame()
  {
    DoBackdropFade(750, 0f, x => 1 - Mathf.Pow(1 - x, 5));
    StartCoroutine(RunOver(750, (_, progress) =>
    {
      if (progress == 0)
      {
        State = GameState.Pregame;
      }
      else if (progress == 499)
      {
        State = GameState.Playing;
        Debug.Log("Started!");
        ResetGame(false);
      }
    }));
  }
  public void PlayerDied()
  {
    StartCoroutine(RunOver(1000, (_, i) =>
    {
      if (i == 999) State = GameState.Died;
    }));
    DoBackdropFade(750, 0.9f, x => 1 - Mathf.Pow(1 - x, 3));
  }

  public void ResetGame(bool toMenu = true)
  {
    localDifficulty = 1.0;
    playerDistance = 0.0f;
    pipesPassed = 0;
    player!.transform.position = Vector3.zero;
    player!.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, playerJumpForce);
    foreach (var pipe in GameObject.FindGameObjectsWithTag("Pipe")) Destroy(pipe);
    if (toMenu) State = GameState.Menu;
  }

  private void SpawnNextPipe()
  {
    if (lastPipe && lastPipe.logicalPosition - playerDistance > viewportWidth) return;
    if (pipePrefabs.Length == 0) return;

    var pipe = Instantiate(pipePrefabs[Random.Range(0, pipePrefabs.Length)]).GetComponent<Pipe>();
    var spawnHeight = Random.Range(pipeHeightRandomBound1, pipeHeightRandomBound2);
    pipe.openingSize = DifficultyScaledRandomRange(minPipeGap + player!.GetComponent<CircleCollider2D>().radius, 7.0f, localDifficulty, true);
    if (!lastPipe)
    {
      pipe.logicalPosition = viewportWidth;
      pipe.transform.position = new Vector2(viewportWidth, spawnHeight);
    }
    else
    {
      pipe.logicalPosition = lastPipe.logicalPosition + 10;
      pipe.transform.position = new Vector2(pipe.logicalPosition - playerDistance, spawnHeight);
    }
    lastPipe = pipe;

    SpawnNextPipe();
  }

  float DifficultyScaledRandomRange(float min, float max, double difficulty, bool invert)
  {
    var bound1 = Mathf.Clamp(Mathf.Pow((float)difficulty, difficultyScaledRandomBound1), min, max);
    var bound2 = Mathf.Clamp(Mathf.Pow((float)difficulty, difficultyScaledRandomBound2), min, max);
    if (invert)
    {
      return Random.Range(Mathf.Min(max - bound1, max - bound2), Mathf.Max(max - bound1, max - bound2));
    }
    else
    {
      return Random.Range(Mathf.Min(bound1, bound2), Mathf.Max(bound1, bound2));
    }
  }

  void DoBackdropFade(uint milliseconds, float targetAlpha, Func<float, float> tweenFunction, Action? callback = null)
  {
    Color startColor = backdrop!.color;
    if (milliseconds == 0)
    {
      backdrop!.color = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
      goto end;
    }

    IEnumerator DoBackdropFadeImpl(Color startColor, uint milliseconds, float targetAlpha, Func<float, float> tweenFunction)
    {
      float startAlpha = backdrop!.color.a;
      yield return RunOver(milliseconds, (progress, _) =>
      {
        backdrop!.color = new Color(startColor.r, startColor.g, startColor.b, startAlpha + ((targetAlpha - startAlpha) * tweenFunction(progress)));
      });
    }
    StartCoroutine(DoBackdropFadeImpl(startColor, milliseconds, targetAlpha, tweenFunction));

  end:
    callback?.Invoke();
  }

  static IEnumerator RunOver(uint milliseconds, Action<float, uint> runOnProgress)
  {
    for (uint i = 0; i < milliseconds; i++)
    {
      runOnProgress((float)i / milliseconds, i);
      yield return new WaitForSeconds(0.001f);
    }
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
