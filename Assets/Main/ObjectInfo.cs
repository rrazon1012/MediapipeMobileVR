using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Objects", menuName = "Object")]
public class ObjectInfo : ScriptableObject
{
  public enum ObjectType { fruit, bomb, powerup };

  public AudioClip objectSound;
  public string objectName;
  public ObjectType objectType; //0 = fruit(good), 1 = bomb(bad), 2 = powerup (if time permits)

  public int pointValue;
}
