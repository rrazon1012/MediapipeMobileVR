using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameCanvas : MonoBehaviour
{
  [SerializeField]
  private GameObject GameRoundCanvas;
  [SerializeField]
  private Text score;
  [SerializeField]
  private Text timer;

  [SerializeField]
  private GameObject restartButton;

  [SerializeField]
  private GameObject EndGameCanvas;
  [SerializeField]
  private Text EndScore;
  [SerializeField]
  private Text EndHighScore;

  // Update is called once per frame
  void Update()
  {
    //score
    score.text = "X " + GameManager.GM.currentScore;

    var time = GameManager.GM.currTime;

    //timer
    var min = Mathf.FloorToInt(time / 60.0f);
    var seconds = Mathf.FloorToInt(time % 60.0f);

    timer.text = min.ToString("00") + ":" + seconds.ToString("00");
  }

  public void ShowEndScore()
  {
    EndGameCanvas.SetActive(true);
    EndScore.text = "Score: " + GameManager.GM.currentScore;
    EndHighScore.text = "HighScore: " + GameManager.GM.highScore;
    restartButton.SetActive(true);
  }
  public void ShowRoundScore()
  {
    GameRoundCanvas.SetActive(true);
  }

  public void HideEndScore()
  {
    EndGameCanvas.SetActive(false);
    restartButton.SetActive(false);
  }

  public void HideRoundScore() 
  {
    GameRoundCanvas.SetActive(false);
  }
}
