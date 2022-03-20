using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object : MonoBehaviour
{
  public ObjectInfo objectInfo;
  public GameObject altPrefab; //alternate model used for when the object is collided with

  private Rigidbody rb;
  private float initialForce = 25f;

  public void Start()
  {
    rb = gameObject.GetComponent<Rigidbody>();
    rb.AddForce(transform.up * initialForce, ForceMode.Impulse);
  }

  private void Update()
  {
  }

  private void OnTriggerEnter(Collider other)
  {
    Debug.Log("Collision");
    if (other.gameObject.tag == "Sword")
    {
      //if(objectInfo.objectType == ObjectInfo.ObjectType.fruit) { }
      //else if(objectInfo.objectType == ObjectInfo.ObjectType.bomb) { }
      //else { }

      AudioSource.PlayClipAtPoint(objectInfo.objectSound, transform.position, 0.5f);

      EventSystem.current.ObjectHit(objectInfo);

      var obj = Instantiate(altPrefab, transform.position, transform.rotation);
      Destroy(obj, 5.0f);
      Destroy(this.gameObject);

    }
  }
}
