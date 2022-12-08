using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{

  public GameObject[] objects;
  public Transform[] spawnPoints;

  public float minTime = 0.2f;
  public float maxTime = 1.0f;
  
  // Start is called before the first frame update
  private void Start()
  {
    //routine = StartCoroutine(SpawnObjects());
    //EventSystem.current.GameStart();
  }

  //void Update()
  //{
  //  currTime -= Time.deltaTime;
  //}

  public void Play()
  {
    if (routine == null)
    {
      routine = StartCoroutine(SpawnObjects());
    }
  }

  public IEnumerator SpawnObjects()
  {
    while (GameManager.GM.currTime > 0)
    {
      //for adding delays between object spawns
      float delay = Random.Range(minTime, maxTime);

      yield return new WaitForSeconds(delay);

      int spawnIndex = Random.Range(0, spawnPoints.Length);

      var obj = Instantiate(objects[Random.Range(0, objects.Length)], spawnPoints[spawnIndex]);
      Destroy(obj, 5.0f);
    }
    EventSystem.current.GameEnd();

    routine = null;
  }
}
