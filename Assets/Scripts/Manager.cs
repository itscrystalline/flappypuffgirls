using UnityEngine;

public class Manager : MonoBehaviour
{
  void Awake()
  {
    var managers = GameObject.FindGameObjectsWithTag("GameController");
    if (managers.Length > 1 && managers[0] != this)
    {
      Destroy(gameObject);
      return;
    }
    DontDestroyOnLoad(gameObject);
  }
}
