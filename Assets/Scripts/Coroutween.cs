using System;
using UnityEngine;

namespace Coroutween
{
  public static class Tween
  {
    public static float Identity(float x) => x;
    public static float EaseOutQuint(float x) => 1 - Mathf.Pow(1 - x, 5);
    public static float EaseOutCubic(float x) => 1 - Mathf.Pow(1 - x, 3);
    public static float EaseOutElastic(float x) => x == 0f ? 0f : x == 1f ? 1f
      : Mathf.Pow(2, -10 * x) * Mathf.Sin((x * 10 - 0.75f) * (2f * (float)Math.PI / 3f)) + 1;
    public static float EaseOutBounce(float x)
    {
      const float n1 = 7.5625f;
      const float d1 = 2.75f;

      if (x < 1 / d1)
      {
        return n1 * x * x;
      }
      else if (x < 2 / d1)
      {
        return n1 * (x -= 1.5f / d1) * x + 0.75f;
      }
      else if (x < 2.5 / d1)
      {
        return n1 * (x -= 2.25f / d1) * x + 0.9375f;
      }
      else
      {
        return n1 * (x -= 2.625f / d1) * x + 0.984375f;
      }
    }

  }

  public static class Coroutines
  {
    public static async Awaitable RunOver(uint milliseconds, Action<float, uint> runOnProgress)
    {
      if (milliseconds == 0)
      {
        runOnProgress(1f, 0);
        return;
      }

      double start = Time.timeAsDouble;

      while (true)
      {
        await Awaitable.NextFrameAsync();
        double elapsed = (Time.timeAsDouble - start) * 1000.0;
        if (elapsed >= milliseconds)
        {
          runOnProgress(1f, milliseconds);
          break;
        }
        runOnProgress((float)elapsed / milliseconds, (uint)elapsed);
      }
    }
    public static async Awaitable RunOverTweened(uint milliseconds, Action<float> runOnProgressTweened, Func<float, float> tweenFunction = null)
    {
      tweenFunction ??= Tween.Identity;
      await RunOver(milliseconds, (progress, _) => runOnProgressTweened(tweenFunction(progress)));
    }
  }
}
