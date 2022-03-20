using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
  public static GameManager GM;
  public ObjectSpawner spawner;

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
    EventSystem.current.onFruitHit += OnFruitHit;
    EventSystem.current.onBombHit += OnBombHit;
  }
  #region game start menu events
  private void OnGameStart()
  {
  }
  private void OnGameHighScore() 
  {
  }
  private void OnGameEnd()
  {
  }
  #endregion

  #region game interactions
  private void OnFruitHit() { }
  private void OnBombHit() { }
  #endregion

  //private void OnPlayerRangeEnter()
  //{
  //  if (!endEntered)
  //  {
  //    endEntered = true;
  //    StartCoroutine(UIFade.FadeCanvas(endScreen, 0f, 1f, END_ANIM_DUR, 0f));
  //    Invoke(nameof(DisableCharacterMovement), END_ANIM_DUR);
  //  }
  //}

  //private void OnPlayerRangeExit()
  //{
  //  Debug.Log("Player range exit");
  //}

  //private void OnObjectRangeEnter()
  //{
  //  Debug.Log("Object range enter");
  //}

  //private void OnObjectRangeExit()
  //{
  //  Debug.Log("Object range exit");
  //}

  //private void OnPlayerDeath()
  //{
  //  Debug.Log("I die");
  //}
  //private void OnPlayerCaughtTrigger()
  //{
  //  ambienceMixer.PlayOneClip(3, 1f);
  //  player.transform.position = playerSpawn.transform.position;
  //  player.transform.rotation = Quaternion.identity;
  //  player.transform.GetComponent<NavMeshAgent>().Warp(playerSpawn.transform.position);

  //  enemy.transform.GetComponent<NavMeshAgent>().Warp(enemySpawn.transform.position);
  //  enemy.GetComponent<Enemy_AI>().Reset();
  //  enemy.GetComponent<VoidToggledEnemy>().Reset();

  //  StartCoroutine(UIFade.FadeCanvas(deathScreen, 1f, 1f, DEATH_ANIM_DUR, 0f));
  //  Invoke(nameof(DisableCharacterMovement), 0f);

  //  StartCoroutine(UIFade.FadeCanvas(deathScreen, 1f, 0f, 1f, DEATH_ANIM_DUR));
  //  Invoke(nameof(EnableCharacterMovement), DEATH_ANIM_DUR);

  //}

  //private void OnPlayerInteractEnd()
  //{
  //  //endEntered = true;
  //  StartCoroutine(UIFade.FadeCanvas(endScreen, 0f, 1f, END_ANIM_DUR, 0f));
  //  Invoke(nameof(DisableCharacterMovement), 0f);
  //}

  //private void OnRestrainOrderCheck()
  //{
  //  VoidController controller = player.GetComponent<VoidController>();
  //  controller.Repress();
  //  controller.Reveal(lockedWall, new Vector3(0f, 0f, 3f));//Use Vector3.zero if no offset
  //  lockedWall.SetActive(false);
  //}

  //private void EnableCharacterMovement()
  //{
  //  player.GetComponent<BaseMotor>().SetMovementLock(false);
  //  player.GetComponent<BaseMotor>().SetRotationLock(false);
  //}

  //private void DisableCharacterMovement()
  //{
  //  player.GetComponent<BaseMotor>().SetMovementLock(true);
  //  player.GetComponent<BaseMotor>().SetRotationLock(true);
  //}

  //private void PauseTime()
  //{
  //  Time.timeScale = 0;
  //}

  //private void ResumeTime()
  //{
  //  Time.timeScale = 1;
  //}

  //public void PauseGame()
  //{
  //  isPaused = true;
  //  PauseTime();
  //  //pauseScreen.SetActive(true);
  //}

  //public void Quit()
  //{
  //  Application.Quit();
  //}

  //public void OnKeyEnterTrigger()
  //{
  //  player.GetComponent<InteractionManager>().currentInteraction = null;
  //  player.GetComponent<InteractionManager>().interacting = false;

  //  //key.SetActive(false); //Gameobject.Destroy(key);
  //  //door.GetComponent<intr_Door>().LockOpen();
  //}

  //public void OnSpawnEnemy()
  //{
  //  enemy.SetActive(true);
  //}

  //public void OnEnemyPatrol()
  //{
  //  ambienceMixer.Loop();
  //  ambienceMixer.ChangeMusic(1, 0.02f, 0.2f);
  //}

  //public void OnEnemyChase()
  //{
  //  ambienceMixer.Loop();
  //  ambienceMixer.ChangeMusic(2, 0.02f, 0.2f);
  //}

}
