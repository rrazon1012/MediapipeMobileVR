using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mediapipe;
using Mediapipe.Unity;

public class MobileVRGraph : GraphRunner
{

  #region GRAPH INFO
  //determines if the model goes for (lite) accuracy or (full) reduced latency
  public enum ModelComplexity
  {
    Lite = 0,
    Full = 1
  };

  public ModelComplexity modelComplexity = ModelComplexity.Lite;
  public int maxNumHands = 2;

#pragma warning disable IDE1006
  public UnityEvent<List<Detection>> OnPalmDetectionsOutput = new UnityEvent<List<Detection>>();
  public UnityEvent<List<NormalizedRect>> OnHandRectsFromPalmDetectionsOutput = new UnityEvent<List<NormalizedRect>>();
  public UnityEvent<List<ClassificationList>> OnHandednessOutput = new UnityEvent<List<ClassificationList>>();
#pragma warning disable IDE1006

  //input streams for the graph, video, image, webcam input
  private const string _inputStreamName = "input_video";

  //output streams names from the graph
  private const string _palmDetectionStreamName = "palm_detections";
  private const string _handRectsFromPalmDetectionsStreamName = "hand_rects_from_palm_detections";
  private const string _handednessStreamName = "handedness";

  //output streams for the graph
  private OutputStream<DetectionVectorPacket, List<Detection>> _palmDetectionsStream;
  private OutputStream<NormalizedRectVectorPacket, List<NormalizedRect>> _handRectsFromPalmDetectionsStream;
  private OutputStream<ClassificationListVectorPacket, List<ClassificationList>> _handednessStream;

  protected long prevPalmDetectionMicrosec = 0;
  protected long prevHandRectsFromPalmDetectionsMicrosec = 0;
  protected long prevHandednessMicrosec = 0;

  #endregion

  //called to start running the graph synchronously
  public override Status StartRun(ImageSource imageSource)
  {
    InitializeOutputStreams();

    _palmDetectionsStream.StartPolling(true).AssertOk();
    _handRectsFromPalmDetectionsStream.StartPolling(true).AssertOk();
    _handednessStream.StartPolling(true).AssertOk();

    return calculatorGraph.StartRun(BuildSidePacket(imageSource));
  }

  public override void Stop()
  {
    base.Stop();
    OnPalmDetectionsOutput.RemoveAllListeners();
    OnHandRectsFromPalmDetectionsOutput.RemoveAllListeners();
    OnHandednessOutput.RemoveAllListeners();
  }

  //feeds given video frame converted to texture into the graph
  public Status AddTextureFrameToInputStream(TextureFrame textureFrame)
  {
    return AddTextureFrameToInputStream(_inputStreamName, textureFrame);
  }

  //gets the next v
  public MobileVRValue FetchNextValue()
  {
    var _ = _palmDetectionsStream.TryGetNext(out var palmDetections);
    _ = _handRectsFromPalmDetectionsStream.TryGetNext(out var handRectsFromPalmDetections);
    _ = _handednessStream.TryGetNext(out var handedness);

    OnPalmDetectionsOutput.Invoke(palmDetections);
    OnHandRectsFromPalmDetectionsOutput.Invoke(handRectsFromPalmDetections);
    OnHandednessOutput.Invoke(handedness);

    /*Debug.Log(palmDetections != null);
    Debug.Log(handRectsFromPalmDetections != null);
    Debug.Log(handedness != null);*/

    return null; //new MobileVRValue(palmDetections, handRectsFromPalmDetections, handedness);
  }

  //request the assets required for landmarks, handedness, palm detection
  protected override IList<WaitForResult> RequestDependentAssets()
  {
    return new List<WaitForResult>
    {
      WaitForHandLandmarkModel(),
      WaitForAsset("hand_recrop.bytes"),
      WaitForAsset("handedness.txt"),
      WaitForPalmDetectionModel(),
    };
  }

  //method to initialize the outputstreams for holding the values
  protected void InitializeOutputStreams()
  {
    _palmDetectionsStream = new OutputStream<DetectionVectorPacket, List<Detection>>(calculatorGraph, _palmDetectionStreamName);
    _handRectsFromPalmDetectionsStream = new OutputStream<NormalizedRectVectorPacket, List<NormalizedRect>>(calculatorGraph, _handRectsFromPalmDetectionsStreamName);
    _handednessStream = new OutputStream<ClassificationListVectorPacket, List<ClassificationList>>(calculatorGraph, _handednessStreamName);
  }

  private WaitForResult WaitForPalmDetectionModel()
  {
    switch (modelComplexity)
    {
      case ModelComplexity.Lite: return WaitForAsset("palm_detection_lite.bytes");
      case ModelComplexity.Full: return WaitForAsset("palm_detection_full.bytes");
      default: throw new InternalException($"Invalid model complexity: {modelComplexity}");
    }
  }

  private WaitForResult WaitForHandLandmarkModel()
  {
    switch (modelComplexity)
    {
      case ModelComplexity.Lite: return WaitForAsset("hand_landmark_lite.bytes");
      case ModelComplexity.Full: return WaitForAsset("hand_landmark_full.bytes");
      default: throw new InternalException($"Invalid model complexity: {modelComplexity}");
    }
  }

  private SidePacket BuildSidePacket(ImageSource imageSource)
  {
    var sidePacket = new SidePacket();

    SetImageTransformationOptions(sidePacket, imageSource, true);
    sidePacket.Emplace("model_complexity", new IntPacket((int)modelComplexity));
    sidePacket.Emplace("num_hands", new IntPacket(maxNumHands));

    return sidePacket;
  }
}
