#nullable enable

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Coroutween;
using Random = UnityEngine.Random;
using System.Threading.Tasks;
using System.Threading;

[Flags]
public enum GameState
{
  Menu = 0b0000_0001,
  Pregame = 0b0000_0010,
  Playing = 0b0000_0100,
  Died = 0b0000_1000,
  Postgame = 0b0001_0000
}
public enum PlayerSprite
{
  Blossom,
  Bubbles,
  Buttercup
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
  public GameObject? dayBackground;
  public GameObject? nightBackground;
  [SerializeField]
  [Range(0f, 1f)]
  private float backgroundParallaxFactor = 0.5f;
  [SerializeField]
  private float dayNightTransitionInterval = 60f;
  [SerializeField]
  private float dayNightFadeDuration = 2f;

  // State
  [SerializeField]
  public GameState state = GameState.Menu;
  public double localDifficulty = 1.0;
  public float playerDistance = 0.0f;

  public PipeController? pipeController;
  public UIController? uiController;
  public AudioController? audioController;

  public GameObject? player;
  public PlayerSprite playerSprite = PlayerSprite.Blossom;
  [HideInInspector]
  public Vector2 viewportSize = new();
  public float playerSpeed = 0f;
  private (SpriteRenderer sprite, float width)[] dayLayer = Array.Empty<(SpriteRenderer, float)>();
  private (SpriteRenderer sprite, float width)[] nightLayer = Array.Empty<(SpriteRenderer, float)>();

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

  public UnityEvent onJump = new();
  public UnityEvent onReset = new();
  public UnityEvent onSwitchCharacter = new();


  [HideInInspector]
  public static GameplayManager? INSTANCE = null;
  private InputAction? reset;
  private InputAction? jump;
  private InputAction? switchCharacter;

  void Awake()
  {
    INSTANCE = this;
    reset = InputSystem.actions.FindAction("Reset");
    jump = InputSystem.actions.FindAction("Jump");
    switchCharacter = InputSystem.actions.FindAction("switchCharacter");
  }

  void Start()
  {
    var wallColliders = GameObject.FindGameObjectsWithTag("PlayWall").Select(g => g.GetComponent<BoxCollider2D>()).ToArray();
    var camera = Camera.main;
    var viewportHeight = camera.orthographicSize;
    var viewportWidth = viewportHeight * camera.aspect;
    viewportSize = new Vector2(viewportWidth, viewportHeight);
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

    SetupBackgrounds();

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

    onReset.AddListener(() => ResetGame(true));
    onJump.AddListener(() =>
    {
      if (state == GameState.Menu)
      {
        state = GameState.Pregame;
        onPregame.Invoke();
      }
    });

    _ = DayNightCycle();
    PullSelectedPlayer();

    onMenu.Invoke();
    onSwitchCharacter.Invoke();
  }

  void PullSelectedPlayer()
  {
    onSwitchCharacter.AddListener(() => PlayerPrefs.SetInt("Player", (int)playerSprite));
    if (PlayerPrefs.HasKey("Player"))
      playerSprite = (PlayerSprite)PlayerPrefs.GetInt("Player");
  }

  void Update()
  {
    if (reset!.WasPerformedThisFrame()) onReset.Invoke();
    if (jump!.WasPerformedThisFrame()) onJump.Invoke();

    if (switchCharacter!.WasPerformedThisFrame() && state == GameState.Menu)
    {
      playerSprite = (PlayerSprite)(((byte)playerSprite + 1) % Enum.GetNames(typeof(PlayerSprite)).Length);
      onSwitchCharacter.Invoke();
    }

    ScrollLayer(dayLayer, playerDistance);
    ScrollLayer(nightLayer, playerDistance);
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

  async Awaitable DayNightCycle()
  {
    var toNight = true;
    while (true)
    {
      await Awaitable.WaitForSecondsAsync(dayNightTransitionInterval);
      await CrossfadeBackgrounds(toNight);
      toNight = !toNight;
    }
  }

  async Awaitable CrossfadeBackgrounds(bool toNight)
  {
    await Coroutines.RunOverTweened((uint)(dayNightFadeDuration * 1000f), tw =>
    {
      var nightAlpha = toNight ? tw : 1f - tw;
      SetLayerAlpha(dayLayer, 1f - nightAlpha);
      SetLayerAlpha(nightLayer, nightAlpha);
    });
  }

  private void SetupBackgrounds()
  {
    dayLayer = SetupBackgroundLayer(dayBackground!);
    nightLayer = SetupBackgroundLayer(nightBackground!);
    SetLayerAlpha(dayLayer, 1f);
    SetLayerAlpha(nightLayer, 0f);
  }

  private (SpriteRenderer sprite, float width)[] SetupBackgroundLayer(GameObject backgroundBlueprint)
  {
    var screenWidth = viewportSize.x * 2;
    var screenHeight = viewportSize.y * 2;
    var sprite = backgroundBlueprint.GetComponent<SpriteRenderer>();
    var size = sprite.size;
    var scalingRatio = Math.Max(screenWidth / size.x, screenHeight / size.y);
    sprite.transform.localScale = new Vector2(scalingRatio, scalingRatio);
    var width = size.x * scalingRatio;

    var amountOfClones = (uint)Math.Ceiling(screenWidth / width) + 1;

    var layer = new (SpriteRenderer sprite, float width)[amountOfClones];
    layer[0] = (sprite, width);
    for (int i = 1; i < amountOfClones; i++)
    {
      var spriteNew = Instantiate(backgroundBlueprint).GetComponent<SpriteRenderer>();
      var position = sprite.transform.position;
      position.x = i * width;
      spriteNew.transform.position = position;
      layer[i] = (spriteNew, width);
    }
    return layer;
  }

  private void SetLayerAlpha((SpriteRenderer sprite, float width)[] layer, float alpha)
  {
    foreach (var (sprite, _) in layer)
    {
      var color = sprite.color;
      color.a = alpha;
      sprite.color = color;
    }
  }

  private void ScrollLayer((SpriteRenderer sprite, float width)[] layer, double distance)
  {
    if (layer.Length == 0) return;
    var width = layer[0].width;
    var scroll = (float)(distance * backgroundParallaxFactor % width);
    for (int i = 0; i < layer.Length; i++)
    {
      var position = layer[i].sprite.transform.position;
      position.x = i * width - scroll;
      layer[i].sprite.transform.position = position;
    }
  }

  async Awaitable PrepareStartGame()
  {
    var cts = new CancellationTokenSource();
    async Awaitable PrepareStartGameBefore(CancellationToken cancel)
    {
      await uiController!.FadeBackdrop(1500, 1f, Tween.EaseOutQuint, cancel);
      await Awaitable.WaitForSecondsAsync(0.5f, cancel);
    }

    await Task.WhenAny(onJump.AsTask(), PrepareStartGameBefore(cts.Token).AsTask());
    cts.Cancel();
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
