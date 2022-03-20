using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
  //public GameObject[] objectPrefabs;
  //public string[] objectTypes;

  public GameObject[] objects;

  public Transform[] spawnPoints;

  public float minTime = 0.2f;
  public float maxTime = 1.0f;
  public float gameTime = 60.0f;
  public float currTime = 0;
  private Coroutine routine;
  // Start is called before the first frame update
  private void Start()
  {
    routine = StartCoroutine(SpawnObjects());
  }

  void Update()
  {
    currTime -= Time.deltaTime;
  }

  public IEnumerator SpawnObjects()
  {
    currTime = gameTime;
    while (currTime > 0)
    {
      Debug.Log(currTime);
      //for adding delays between object spawns
      float delay = Random.Range(minTime, maxTime);

      yield return new WaitForSeconds(delay);

      int spawnIndex = Random.Range(0, spawnPoints.Length);

      var obj = Instantiate(objects[Random.Range(0,objects.Length)], spawnPoints[spawnIndex]);
      Destroy(obj, 5.0f);
    }
  }
}
