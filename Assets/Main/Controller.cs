using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Unity;
using Mediapipe;

public class Controller : MonoBehaviour
{
  public enum LANDMARK
  {
    WRIST = 0,
    THUMB_CMC,
    THUMB_MCP,
    THUMB_IP,
    THUMB_TIP,
    INDEX_FINGER_MCP,
    INDEX_FINGER_PIP,
    INDEX_FINGER_DIP,
    INDEX_FINGER_TIP,
    MIDDLE_FINGER_MCP,
    MIDDLE_FINGER_PIP,
    MIDDLE_FINGER_DIP,
    MIDDLE_FINGER_TIP,
    RING_FINGER_MCP,
    RING_FINGER_PIP,
    RING_FINGER_DIP,
    RING_FINGER_TIP,
    PINKY_FINGER_MCP,
    PINKY_FINGER_PIP,
    PINKY_FINGER_DIP,
    PINKY_FINGER_TIP
  }

  //booleans for fingers to determine simple hand gestures
  public bool isThumbUp = false;
  public bool isIndexUp = false;
  public bool isMiddleUp = false;
  public bool isRingUp = false;
  public bool isPinkyUp = false;

  public bool isUpright = true; //it is assumed that the hand is upwards, otherwise

  //boolean to check if the hand is currently being tracked
  protected bool isTracking;

  //contains the packet 
  protected solution solution;
  protected MobileVRValue handValues;
  // Start is called before the first frame update
  IEnumerator Start()
  {
    solution = GameObject.Find("Solution").GetComponent<solution>();

    if (solution == null) {
      Debug.Log("Solution not found");
      yield break;
    }

    //if the handvalue is null wait for it to be set to know that solution is continously fetching packets
    yield return new WaitWhile(() => solution.handValues == null);

    Play();
  }

  //begins tracking
  public virtual void Play()
  {
    isTracking = true;
  }

  public virtual void Pause() 
  {
    isTracking = false;
  }

  public virtual void Resume()
  {
    isTracking = true;
  }

  public virtual void Stop() 
  {
    isTracking = false;
  }

  public void GetHandValues()
  {
    handValues = solution.handValues;
  }
  public void GetGestures(Vector3[] landmarks)
  {
    if(landmarks[(int)LANDMARK.WRIST].y > landmarks[(int)LANDMARK.MIDDLE_FINGER_MCP].y)
    {
      isUpright = false;
      Debug.Log("Not Upright");
    }

    if (isUpright)
    {
      if (landmarks[(int)LANDMARK.THUMB_TIP].y > landmarks[(int)LANDMARK.THUMB_CMC].y)
      {
        isThumbUp = true;
      }
      if (landmarks[(int)LANDMARK.INDEX_FINGER_TIP].y > landmarks[(int)LANDMARK.INDEX_FINGER_MCP].y) 
      {
        isIndexUp = true;
      }
      if (landmarks[(int)LANDMARK.MIDDLE_FINGER_TIP].y > landmarks[(int)LANDMARK.MIDDLE_FINGER_MCP].y) 
      {
        isMiddleUp = true;
      }
      if (landmarks[(int)LANDMARK.RING_FINGER_TIP].y > landmarks[(int)LANDMARK.RING_FINGER_MCP].y) 
      {
        isRingUp = true;
      }
      if (landmarks[(int)LANDMARK.PINKY_FINGER_TIP].y > landmarks[(int)LANDMARK.PINKY_FINGER_MCP].y)
      {
        isPinkyUp = true;
      }
    }
    else
    {
      if (landmarks[(int)LANDMARK.THUMB_TIP].y < landmarks[(int)LANDMARK.THUMB_CMC].y)
      {
        isThumbUp = true;
      }
      if (landmarks[(int)LANDMARK.INDEX_FINGER_TIP].y < landmarks[(int)LANDMARK.INDEX_FINGER_MCP].y)
      {
        isIndexUp = true;
      }
      if (landmarks[(int)LANDMARK.MIDDLE_FINGER_TIP].y < landmarks[(int)LANDMARK.MIDDLE_FINGER_MCP].y)
      {
        isMiddleUp = true;
      }
      if (landmarks[(int)LANDMARK.RING_FINGER_TIP].y < landmarks[(int)LANDMARK.RING_FINGER_MCP].y)
      {
        isRingUp = true;
      }
      if (landmarks[(int)LANDMARK.PINKY_FINGER_TIP].y < landmarks[(int)LANDMARK.PINKY_FINGER_MCP].y)
      {
        isPinkyUp = true;
      }
    }

  }

  protected void resetGestures()
  {
    isThumbUp = false;
    isIndexUp = false;
    isMiddleUp = false;
    isRingUp = false;
    isPinkyUp = false;
    isUpright = true;
  }
}
