using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MobileVRController : Controller
{
  private Coroutine _coroutine;

  public GameObject[] cube;
  public Text gestureText;

  public override void Play()
  {
    if (_coroutine != null)
    {
      Stop();
    }
    base.Play();
    _coroutine = StartCoroutine(Run());
  }

  public override void Stop()
  {
    base.Stop();
  }

  public override void Pause()
  {
    base.Pause();
  }

  public override void Resume()
  {
    base.Resume();
  }

  private IEnumerator Run()
  {

    while (true)
    {
      //get the latest hand values from solution, can be null
      GetHandValues();

      //check if handValues are null
      if (handValues != null)
      {

        if (handValues.handRectsFromPalmDetections == null || handValues.handWorldLandmarks == null)
        {
          Debug.Log("Hand Is not in frame");
        }
        else
        {

          var screenOffset = new Vector2(handValues.handRectsFromPalmDetections[0].XCenter, handValues.handRectsFromPalmDetections[0].YCenter);
          var offset = handValues.handWorldLandmarks[0].Landmark[0];

          //scale factor for world landmarks
          var scale = 100;

          //negate specific axis, depends on if the image is mirrored or not
          //0,0 is at topleft
#if UNITY_EDITOR
          var negate = 1;
#else
          var negate = -1;
#endif
          //bottom left coordinate to offset the hand to start at the bottom left
          var bottomLeft = new Vector3(15 * negate, -8);
          var hand = handValues.handWorldLandmarks[0];

          var landmarks = new Vector3[21];

          //full landmark tracking represented by cubes
          for (var i = 0; i < 21; i++)
          {
            //////////////////////////////////////for cubes/////////////////////////////
            //scaling up the landmarks and applying a negate to account for the camera
            //Note: Y has a negative scale because 0,0 is at top left, otherwise it would 
            /*cube[i].transform.localPosition = new Vector3((handValues.handWorldLandmarks[0].Landmark[i].X) * scale * negate,
              (handValues.handWorldLandmarks[0].Landmark[i].Y) * -scale, (handValues.handWorldLandmarks[0].Landmark[i].Z) * scale) - bottomLeft;

            //Offset of the hand in world space relative to its offset in the image texture (video frame)
            cube[i].transform.position += new Vector3(screenOffset.x * 40 * negate, screenOffset.y * -20);*/
            /////////////////////////////////////////////////////////////////////////////

            landmarks[i] = new Vector3(handValues.handWorldLandmarks[0].Landmark[i].X * scale * negate,
              handValues.handWorldLandmarks[0].Landmark[i].Y * -scale, handValues.handWorldLandmarks[0].Landmark[i].Z * scale) - bottomLeft;
          }

          cube[5].transform.localPosition = new Vector3(handValues.handWorldLandmarks[0].Landmark[5].X * scale * negate,
              handValues.handWorldLandmarks[0].Landmark[5].Y * -scale, handValues.handWorldLandmarks[0].Landmark[5].Z * scale) - bottomLeft;
          cube[5].transform.position += new Vector3(screenOffset.x * 40 * negate, screenOffset.y * -20);

          //cube[0].transform.position = new Vector3(offset.X, offset.Y, offset.Z) * scale;

          GetGestures(landmarks);

          /*if (isThumbUp)
          {
            Debug.Log("Thumb Up");
          }
          if (isIndexUp)
          {
            Debug.Log("Index Up");
          }

          if (isMiddleUp)
          {
            Debug.Log("Middle Up");
          }
          
          if (isRingUp)
          {
            Debug.Log("Ring Up");
          }
          
          if (isPinkyUp)
          {
            Debug.Log("Pinky Up");
          }*/
          gestureText.text = "None";

          if (isIndexUp && isMiddleUp && !isRingUp && !isPinkyUp)
          {
            Debug.Log("Peace Sign");
            gestureText.text = "Peace Sign";
          }
          else if (isIndexUp && isPinkyUp && !isMiddleUp && !isRingUp) {
            Debug.Log("Rock and Roll");
            gestureText.text = "Rock and Roll";
          }
          else if (isIndexUp && !isMiddleUp && !isRingUp && !isPinkyUp)
          {
            gestureText.text = "One";
          }
          else if (!isIndexUp && !isMiddleUp && !isRingUp && !isPinkyUp)
          {
            gestureText.text = "Fist";
          }

          isThumbUp = false;
          isIndexUp = false;
          isMiddleUp = false;
          isRingUp = false;
          isPinkyUp = false;
          isUpright = true;

}
      }
      yield return new WaitForEndOfFrame();
    }
  }
}
