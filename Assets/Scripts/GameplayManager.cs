#nullable enable

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Coroutween;
using Random = UnityEngine.Random;

[Flags]
public enum GameState
{
  Menu = 0b0000_0001,
  Pregame = 0b0000_0010,
  Playing = 0b0000_0100,
  Died = 0b0000_1000,
  Postgame = 0b0001_0000
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
  public GameObject? backgroundDay = null, backgroundNight = null;

  // State
  [SerializeField]
  public GameState state = GameState.Menu;
  public double localDifficulty = 1.0;
  public float playerDistance = 0.0f;

  public PipeController? pipeController;
  public UIController? uiController;

  public GameObject? player;
  [HideInInspector]
  public float viewportWidth = 0.0f;
  public float playerSpeed = 0f;

  private uint? _highScore;
  public uint HighScore
  {
    get
    {
      if (_highScore == null)
      {
        if (PlayerPrefs.HasKey("HighScore"))
        {
          _highScore = (uint)PlayerPrefs.GetInt("HighScore");
        }
        else
        {
          PlayerPrefs.SetInt("HighScore", 0);
          _highScore = 0;
        }
      }
      return _highScore ?? 0;
    }
    set
    {
      _highScore = value;
      PlayerPrefs.SetInt("HighScore", (int)value);
    }
  }
  public uint Score { get => (uint)Math.Round((localDifficulty - 1) * 10000); }

  // Events
  public UnityEvent onMenu = new();
  public UnityEvent onPregame = new();
  public UnityEvent onPlay = new();
  public UnityEvent onDie = new();
  public UnityEvent onPostgame = new();


  [HideInInspector]
  public static GameplayManager? INSTANCE = null;
  private InputAction? reset;
  private InputAction? jump;

  void Awake()
  {
    INSTANCE = this;
    reset = InputSystem.actions.FindAction("Reset");
    jump = InputSystem.actions.FindAction("Jump");
  }

  void Start()
  {
    var wallColliders = GameObject.FindGameObjectsWithTag("PlayWall").Select(g => g.GetComponent<BoxCollider2D>()).ToArray();
    var camera = Camera.main;
    var viewportHeight = camera.orthographicSize;
    var viewportWidth = viewportHeight * camera.aspect;
    this.viewportWidth = viewportWidth;
    Debug.Log($"camera w/h: {viewportWidth * 2} x {viewportHeight * 2}");

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

    pipeController!.onPipeCriticalPass.AddListener(() => localDifficulty *= criticalDifficultyScaleFactor);

    void PrintState() => print(state);
    onMenu.AddListener(PrintState);
    onPregame.AddListener(PrintState);
    onPlay.AddListener(PrintState);
    onDie.AddListener(PrintState);
    onPostgame.AddListener(PrintState);

    onMenu.AddListener(() => player!.SetActive(false));
    onPregame.AddListener(() => _ = PrepareStartGame());
    onPlay.AddListener(StartGame);
    onDie.AddListener(() => _ = PlayerDied());
    onPostgame.AddListener(PostGame);

    onMenu.Invoke();
  }


  void Update()
  {
    if (reset!.WasPerformedThisFrame())
    {
      ResetGame(true);
    }

    if (jump!.WasPerformedThisFrame() && state == GameState.Menu)
    {
      state = GameState.Pregame;
      onPregame.Invoke();
    }
  }
  void FixedUpdate()
  {
    if (state == GameState.Playing || state == GameState.Menu)
    {
      if (state == GameState.Playing) localDifficulty *= ((difficultyScaleFactor - 1) * Time.fixedDeltaTime) + 1;
      playerSpeed = (float)(playerBaseSpeed + Math.Pow(localDifficulty, difficultySpeedScaleFactor));
    }
    playerDistance += playerSpeed * Time.fixedDeltaTime;
  }

  async Awaitable PrepareStartGame()
  {
    await uiController!.FadeBackdrop(750, 1f, Tween.EaseOutQuint);
    state = GameState.Playing;
    onPlay.Invoke();
    _ = uiController!.FadeBackdrop(250, 0f, Tween.EaseOutQuint);
    ResetGame(false);
  }
  void StartGame()
  {
    player!.SetActive(true);
  }
  async Awaitable PlayerDied()
  {
    _ = uiController!.FadeBackdrop(750, 0.9f, Tween.EaseOutCubic);
    var finalSpeed = playerSpeed;
    await Coroutines.RunOverTweened(1000, tw => playerSpeed = Mathf.Lerp(finalSpeed, 0, tw), Tween.EaseOutQuint);
    state = GameState.Postgame;
    onPostgame.Invoke();
  }
  void PostGame()
  {
    player!.SetActive(false);
  }

  public void ResetGame(bool toMenu)
  {
    localDifficulty = 1.0;
    playerDistance = 0.0f;
    pipeController!.Reset();
    player!.transform.position = Vector3.zero;
    player!.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, playerJumpForce);
    if (toMenu)
    {
      state = GameState.Menu;
      onMenu.Invoke();
    }
  }


  public float DifficultyScaledRandomRange(float min, float max, double difficulty, bool invert)
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


  static Vector2 CentroidOf((Vector2, Vector2) vecs)
  {
    return new Vector2((vecs.Item2.x + vecs.Item1.x) / 2, (vecs.Item2.y + vecs.Item1.y) / 2);
  }
  static Vector2 SizeOf((Vector2, Vector2) vecs)
  {
    return new Vector2(Math.Abs(vecs.Item2.x - vecs.Item1.x), Math.Abs(vecs.Item2.y - vecs.Item1.y));
  }
}
