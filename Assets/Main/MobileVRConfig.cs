using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mediapipe.Unity.UI;

namespace Mediapipe.Unity.HandTracking.UI
{
  public class MobileVRConfig : ModalContents
  {

    private MobileVRSolution _solution;

    //Configuration to be set

    // Start is called before the first frame update
    void Start()
    {
      _solution = GameObject.Find("Solution").GetComponent<MobileVRSolution>(); //grabs the solution gameobject and sets the solution referenced
      InitializeContents();
    }

    private void InitializeContents()
    {
     /* InitializeModelComplexity();
      InitializeMaxNumHands();
      InitializeRunningMode();
      InitializeTimeoutMillisec();*/
    }
  }
}
