using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
  public static GameManager GM;
  public ObjectSpawner spawner;

  public int currentScore = 0;
  public int highScore;

  public float gameTime = 90.0f;
  public float currTime = 0.0f;

  private Coroutine timing;

  private void Awake()
  {
    if (GM == null)
    {
      DontDestroyOnLoad(gameObject);
      GM = this;
    }
  }

  private void Start()
  {
    if (GM != this)
      GM = this;

    EventSystem.current.onGameStart += OnGameStart;
    EventSystem.current.onGameEnd += OnGameEnd;
    EventSystem.current.onObjectHit += OnObjectHit;

    OnGameStart();
  }
  #region game start menu events
  private void OnGameStart()
  {
    currentScore = 0;
    timing = StartCoroutine(Time());
    spawner.Play();
  }
  //private void OnGameHighScore()
  //{
  //}
  private void OnGameEnd()
  {
    //make the score UI popup
    //restart button
  }
  #endregion

  private void OnObjectHit(ObjectInfo info) {
    currentScore += info.pointValue;
  }
  //#endregion

  private IEnumerator Time() {
    currTime = gameTime;
    while (gameTime > 0)
    {
      yield return new WaitForSeconds(1.0f);
      currTime--;
    }
  }
}
