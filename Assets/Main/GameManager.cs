using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

  public GameCanvas canvas;

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
  }
  #region game start menu events
  private void OnGameStart()
  {
    if (currTime <= 0)
    {
      canvas.HideEndScore();
      canvas.ShowRoundScore();

      currentScore = 0;
      timing = StartCoroutine(Time());
      spawner.Play();
    }
  }
  //private void OnGameHighScore()
  //{
  //}
  private void OnGameEnd()
  {
    //check if there already is a high score
    if (PlayerPrefs.HasKey("HighScore"))
    {
      if (currentScore > PlayerPrefs.GetInt("HighScore"))
      {
        PlayerPrefs.SetInt("HighScore", currentScore);
      }
    }
    else 
    {
      PlayerPrefs.SetInt("HighScore", currentScore);
    }

    //make the score UI popup
    highScore =  PlayerPrefs.GetInt("HighScore");

    canvas.ShowEndScore();
    canvas.HideRoundScore();
    //restart button
  }
  #endregion

  private void OnObjectHit(ObjectInfo info) {
    currentScore += info.pointValue;
  }
  //#endregion

  private IEnumerator Time() {
    currTime = gameTime;
    while (currTime > 0)
    {
      yield return new WaitForSeconds(1.0f);
      currTime--;
    }
  }
}
