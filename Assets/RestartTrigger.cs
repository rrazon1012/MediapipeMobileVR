using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartTrigger : MonoBehaviour
{
  private void OnTriggerEnter(Collider other)
  {
    Debug.Log("Restart");
    if(other.gameObject.tag == "Sword") 
    {
      EventSystem.current.GameStart();
    }
  }
}
