#nullable enable

using System;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class PipeController : MonoBehaviour
{
  private GameplayManager? manager;

  [SerializeField]
  [Range(-6f, 0f)]
  private float pipeHeightRandomBound1 = -2f;
  [SerializeField]
  [Range(0f, 6f)]
  private float pipeHeightRandomBound2 = 2f;

  public GameObject[] pipePrefabs = Array.Empty<GameObject>();
  public int pipesPassed = 0;
  private float minPipeGap = 0f;
  private Pipe? lastPipe = null;

  public UnityEvent onPipePass = new();
  public UnityEvent onPipeCriticalPass = new();

  void Start()
  {
    manager = GameplayManager.INSTANCE;
    minPipeGap = manager!.playerJumpForce * manager!.playerJumpForce / (Mathf.Abs(Physics2D.gravity.y * manager!.player!.GetComponent<Rigidbody2D>().gravityScale) * 2f);
    onPipePass.AddListener(() => pipesPassed += 1);
  }

  void FixedUpdate()
  {
    SpawnNextPipe();
  }

  private void SpawnNextPipe()
  {
    if (lastPipe != null && lastPipe.logicalPosition - manager!.playerDistance > manager!.viewportWidth) return;
    if (pipePrefabs.Length == 0) return;

    var pipe = Instantiate(pipePrefabs[Random.Range(0, pipePrefabs.Length)]).GetComponent<Pipe>();
    var spawnHeight = Random.Range(pipeHeightRandomBound1, pipeHeightRandomBound2);
    pipe.openingSize = manager!.DifficultyScaledRandomRange(minPipeGap + manager!.player!.GetComponent<CircleCollider2D>().radius, 7.0f, manager!.localDifficulty, true);
    if (!lastPipe)
    {
      pipe.logicalPosition = manager!.viewportWidth;
      pipe.transform.position = new Vector2(manager!.viewportWidth, spawnHeight);
    }
    else
    {
      pipe.logicalPosition = lastPipe.logicalPosition + 10;
      pipe.transform.position = new Vector2(pipe.logicalPosition - manager!.playerDistance, spawnHeight);
    }
    lastPipe = pipe;

    SpawnNextPipe();
  }

  public void Reset()
  {
    pipesPassed = 0;
    lastPipe = null;
    foreach (var pipe in GameObject.FindGameObjectsWithTag("Pipe")) Destroy(pipe);
  }
}
