using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSystem : MonoBehaviour
{
  public static EventSystem current;

  public void Awake()
  {
    current = this;
  }

  public event Action onGameStart;
  public void GameStart()
  {
    onGameStart?.Invoke();
  }

  public event Action onFruitHit;
  public void FruitHit()
  {
    onFruitHit?.Invoke();
  }
  public event Action onBombHit;
  public void BombHit()
  {
    onFruitHit?.Invoke();
  }
}
