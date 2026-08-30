using UnityEngine;

public class PlayingAsSelector : MonoBehaviour
{
  private GameplayManager game;

  public TextController characterName;
  public GameObject blossom, bubbles, buttercup;

  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start()
  {
    game = GameplayManager.INSTANCE;
    game.onSwitchCharacter.AddListener(() =>
    {
      switch (game.playerSprite)
      {
        case PlayerSprite.Blossom:
          blossom.SetActive(true);
          bubbles.SetActive(false);
          buttercup.SetActive(false);
          break;
        case PlayerSprite.Bubbles:
          blossom.SetActive(false);
          bubbles.SetActive(true);
          buttercup.SetActive(false);
          break;
        case PlayerSprite.Buttercup:
          blossom.SetActive(false);
          bubbles.SetActive(false);
          buttercup.SetActive(true);
          break;
      }
      characterName.Text = game.playerSprite.ToString();
    });
  }
}
