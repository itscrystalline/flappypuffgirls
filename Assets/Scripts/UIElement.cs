#nullable enable

using System.Linq;
using UnityEngine;

class UIElement : MonoBehaviour
{
  public GameState associatedStates;
  private bool? active = null;

  private IUIElementAnim? animator;
  public void FadeIn()
  {
    if (active ?? false) return;
    animator?.FadeIn();
    active = true;
  }
  public void FadeOut()
  {
    if (!active ?? false) return;
    animator?.FadeOut();
    active = false;
  }
  public void FadeInImmeadiate()
  {
    if (active ?? false) return;
    animator?.FadeInImmeadiate();
    active = true;
  }
  public void FadeOutImmeadiate()
  {
    if (!active ?? false) return;
    animator?.FadeOutImmeadiate();
    active = false;
  }

  void Awake()
  {
    animator = GetComponents<MonoBehaviour>().OfType<IUIElementAnim>().FirstOrDefault();
  }
}

interface IUIElementAnim
{
  public void FadeInImmeadiate();
  public Awaitable FadeIn();
  public void FadeOutImmeadiate();
  public Awaitable FadeOut();
}
