using System;
using System.Collections;
using UnityEngine;

namespace Coroutween
{
  public static class Tween
  {
    public static float Identity(float x) => x;
    public static float EaseOutQuint(float x) => 1 - Mathf.Pow(1 - x, 5);
    public static float EaseOutElastic(float x) => x == 0f ? 0f : x == 1f ? 1f
      : Mathf.Pow(2, -10 * x) * Mathf.Sin((x * 10 - 0.75f) * (2f * (float)Math.PI / 3f)) + 1;
  }

  public static class Coroutines
  {
    public static IEnumerator RunOver(uint milliseconds, Action<float, uint> runOnProgress)
    {
      for (uint i = 1; i <= milliseconds; i++)
      {
        runOnProgress((float)i / milliseconds, i);
        yield return new WaitForSeconds(0.001f);
      }
    }
    public static IEnumerator RunOverTweened(uint milliseconds, Action<float> runOnProgressTweened, Func<float, float> tweenFunction = null)
    {
      tweenFunction ??= Tween.Identity;
      yield return RunOver(milliseconds, (progress, _) => runOnProgressTweened(tweenFunction(progress)));
    }
  }
}
