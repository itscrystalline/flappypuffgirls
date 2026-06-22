#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
  [SerializeField]
  private float playerBaseSpeed = 10;
  public float playerJumpForce = 850;

  // State
  public double localDifficulty = 1.0;
  public double playerDistance = 0.0;
  private List<GameObject> pipes = new();

  public float playerEffectiveSpeed()
  {
    return playerEffectiveSpeed(localDifficulty);
  }
  public float playerEffectiveSpeed(double difficulty)
  {
    return (float)(playerBaseSpeed + Math.Pow(difficulty, 1.05));
  }


  [HideInInspector]
  public static Manager? INSTANCE = null;

  void Awake()
  {
    if (INSTANCE != null)
    {
      Destroy(gameObject);
      return;
    }
    DontDestroyOnLoad(gameObject);
    INSTANCE = this;
  }

  void Start()
  {
    if (SceneManager.GetActiveScene().name == "Game")
    {
      var wallColliders = GameObject.FindGameObjectsWithTag("PlayWall").Select(g => g.GetComponent<BoxCollider2D>()).ToArray();
      var camera = Camera.main;
      var viewportHeight = camera.orthographicSize;
      var viewportWidth = viewportHeight * camera.aspect;
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

      wallColliders[0].offset = CentroidOf(left);
      wallColliders[0].size = SizeOf(left);
      wallColliders[1].offset = CentroidOf(right);
      wallColliders[1].size = SizeOf(right);
      wallColliders[2].offset = CentroidOf(top);
      wallColliders[2].size = SizeOf(top);
      wallColliders[3].offset = CentroidOf(bottom);
      wallColliders[3].size = SizeOf(bottom);

      pipes.AddRange(GameObject.FindGameObjectsWithTag("Pipe"));
    }
  }

  void FixedUpdate()
  {
    if (SceneManager.GetActiveScene().name == "Game")
    {
      foreach (var pipe in pipes)
      {
        var transform = pipe.transform.position;
        transform.x -= playerEffectiveSpeed() * Time.deltaTime;
        pipe.transform.position = transform;
      }
    }
  }

  Vector2 CentroidOf((Vector2, Vector2) vecs)
  {
    return new Vector2((vecs.Item2.x + vecs.Item1.x) / 2, (vecs.Item2.y + vecs.Item1.y) / 2);
  }
  Vector2 SizeOf((Vector2, Vector2) vecs)
  {
    return new Vector2(Math.Abs(vecs.Item2.x - vecs.Item1.x), Math.Abs(vecs.Item2.y - vecs.Item1.y));
  }


}
