using UnityEngine;

public class AudioController : MonoBehaviour
{
  public AudioClip[] jumps;
  [Range(0f, 1f)]
  public float jumpVolume = 1f;
  public AudioClip[] thruPipes;
  [Range(0f, 1f)]
  public float thruPipesVolume = 1f;
  public AudioClip[] winds;
  [Range(0f, 1f)]
  public float windsVolume = 1f;
  public AudioClip die;
  [Range(0f, 1f)]
  public float dieVolume = 0.75f;
  // public AudioClip ding;
  // [Range(0f, 1f)]
  // public float dingVolume = 1f;
  public AudioClip medal;
  [Range(0f, 1f)]
  public float medalVolume = 1f;

  [Space(5f)]

  [SerializeField]
  private AudioSource source;
  private GameplayManager manager;

  void Start()
  {
    manager = GameplayManager.INSTANCE;
    manager.onJump.AddListener(() =>
    {
      if (manager.state == GameState.Playing)
        source.PlayOneShot(choose(jumps), jumpVolume);
    });
    manager.onDie.AddListener(() => source.PlayOneShot(die, dieVolume));

    manager.pipeController.onPipePass.AddListener(() => source.PlayOneShot(choose(thruPipes), thruPipesVolume));
    manager.pipeController.onPipeCriticalPass.AddListener(() => source.PlayOneShot(choose(thruPipes), thruPipesVolume));

    _ = PlayWindSounds();
  }

  async Awaitable PlayWindSounds()
  {
    while (isActiveAndEnabled)
    {
      await Awaitable.WaitForSecondsAsync(Random.Range(3f, 6f));
      source.PlayOneShot(choose(winds), windsVolume);
    }
  }

  public void PlayMedalSound() => source.PlayOneShot(medal, medalVolume);
  // public void PlayDing() => source.PlayOneShot(ding, dingVolume);

  AudioClip choose(AudioClip[] src) => src[Random.Range(0, src.Length)];
}
