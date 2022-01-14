using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mediapipe;
using Mediapipe.Unity;

//represents the value extracted from each frame
public class MobileVRValue
{
  public readonly List<Detection> palmDetections;
  public readonly List<NormalizedRect> handRectsFromPalmDetections;
  public readonly List<ClassificationList> handedness;

  public MobileVRValue(List<Detection> palmDetections, List<NormalizedRect> handRectsFromPalmDetections, List<ClassificationList> handedness)
  {
    this.palmDetections = palmDetections;
    this.handRectsFromPalmDetections = handRectsFromPalmDetections;
    this.handedness = handedness;
  }

  //public readonly List<Detection> palmDetections;
  //public readonly List<NormalizedRect> handRectsFromPalmDetections;
  //public readonly List<NormalizedLandmarkList> handLandmarks;
  //public readonly List<LandmarkList> handWorldLandmarks;
  //public readonly List<NormalizedRect> handRectsFromLandmarks;
  //public readonly List<ClassificationList> handedness;

  //public HandTrackingValue(List<Detection> palmDetections, List<NormalizedRect> handRectsFromPalmDetections,
  //                         List<NormalizedLandmarkList> handLandmarks, List<LandmarkList> handWorldLandmarks,
  //                         List<NormalizedRect> handRectsFromLandmarks, List<ClassificationList> handedness)
  //{
  //  this.palmDetections = palmDetections;
  //  this.handRectsFromPalmDetections = handRectsFromPalmDetections;
  //  this.handLandmarks = handLandmarks;
  //  this.handWorldLandmarks = handWorldLandmarks;
  //  this.handRectsFromLandmarks = handRectsFromLandmarks;
  //  this.handedness = handedness;
  //}

}
