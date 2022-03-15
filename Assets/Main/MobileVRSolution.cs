using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Unity;
using Mediapipe;
using Logger = Mediapipe.Logger;

//base class for actually running the graph
public class MobileVRSolution : Solution
{
  //Video screen that is being looked at
  [SerializeField] private Mediapipe.Unity.Screen _screen;
  //used for drawing the bounding box of the palm detected
  [SerializeField] private DetectionListAnnotationController _palmDetectionsAnnotationController;
  [SerializeField] private NormalizedRectListAnnotationController _handRectsFromPalmDetectionsAnnotationController;
  //the graph being run that contains the nodes, calculators, inputs and outputs
  [SerializeField] private MobileVRGraph _graphRunner;
  //frame from image/video source that is accessed as a frame
  [SerializeField] private TextureFramePool _textureFramePool;

  private Coroutine _coroutine;

  public RunningMode runningMode = RunningMode.Sync;

  public GameObject cube;

  //the complexity of the hand
  public MobileVRGraph.ModelComplexity modelComplexity = MobileVRGraph.ModelComplexity.Lite;

  //maximum number of hands to be detected at a time
  public int maxNumHands = 2;

  //50 milliseconds for packet timeout
  public long timeoutMillisec = 50;

  public override void Play()
  {
    if (_coroutine != null)
    {
      Stop();
    }
    base.Play();
    _coroutine = StartCoroutine(Run());
  }

  public override void Pause()
  {
    base.Pause();
    ImageSourceProvider.ImageSource.Pause();
  }

  public override void Resume()
  {
    base.Resume();
    var _ = StartCoroutine(ImageSourceProvider.ImageSource.Resume());
  }

  public override void Stop()
  {
    base.Stop();
    StopCoroutine(_coroutine);
    ImageSourceProvider.ImageSource.Stop();
    _graphRunner.Stop();
  }

  private IEnumerator Run()
  {
    var graphInitRequest = _graphRunner.WaitForInit();
    var imageSource = ImageSourceProvider.ImageSource;

    yield return imageSource.Play();

    if (!imageSource.isPrepared)
    {
      Logger.LogError(TAG, "Failed to start ImageSource, exiting...");
      yield break;
    }
    // NOTE: The _screen will be resized later, keeping the aspect ratio.
    _screen.Initialize(imageSource);

    Logger.LogInfo(TAG, $"Model Complexity = {modelComplexity}");
    Logger.LogInfo(TAG, $"Max Num Hands = {maxNumHands}");
    Logger.LogInfo(TAG, $"Running Mode = {runningMode}");

    yield return graphInitRequest;
    if (graphInitRequest.isError)
    {
      Logger.LogError(TAG, graphInitRequest.error);
      yield break;
    }

    _graphRunner.StartRun(imageSource).AssertOk();

    // Use RGBA32 as the input format.
    // TODO: When using GpuBuffer, MediaPipe assumes that the input format is BGRA, so the following code must be fixed.
    _textureFramePool.ResizeTexture(imageSource.textureWidth, imageSource.textureHeight, TextureFormat.RGBA32);

    // The input image is flipped if it's **not** mirrored
    SetupAnnotationController(_palmDetectionsAnnotationController, imageSource, true);
    SetupAnnotationController(_handRectsFromPalmDetectionsAnnotationController, imageSource, true);
    /*SetupAnnotationController(_handLandmarksAnnotationController, imageSource, true);
    SetupAnnotationController(_handRectsFromLandmarksAnnotationController, imageSource, true);*/

    while (true)
    {
      yield return new WaitWhile(() => isPaused);

      var textureFrameRequest = _textureFramePool.WaitForNextTextureFrame();
      yield return textureFrameRequest;
      var textureFrame = textureFrameRequest.result;

      // Copy current image to TextureFrame
      ReadFromImageSource(imageSource, textureFrame);

      _graphRunner.AddTextureFrameToInputStream(textureFrame).AssertOk();

      if (runningMode == RunningMode.Sync)
      {
        // TODO: copy texture before `textureFrame` is released
        _screen.ReadSync(textureFrame);

        // When running synchronously, wait for the outputs here (blocks the main thread).
        var value = _graphRunner.FetchNextValue();
        if (value.handRectsFromPalmDetections == null)
        {
          Debug.Log("Palm Detection returned null");
        }
        else
        {
          Debug.Log(value.handRectsFromPalmDetections[0].ToString());
          /*cube.transform.position.x += value.handRectsFromPalmDetections[0].XCenter;
          cube.transform.position.y += value.handRectsFromPalmDetections[0].YCenter;*/
          cube.transform.position = new Vector3(value.handRectsFromPalmDetections[0].XCenter, value.handRectsFromPalmDetections[0].YCenter, 0);
        }
        _palmDetectionsAnnotationController.DrawNow(value.palmDetections);
        _handRectsFromPalmDetectionsAnnotationController.DrawNow(value.handRectsFromPalmDetections);
        //_handLandmarksAnnotationController.DrawNow(value.handLandmarks, value.handedness);
        //_handRectsFromLandmarksAnnotationController.DrawNow(value.handRectsFromLandmarks);
      }

      yield return new WaitForEndOfFrame();
    }
  }
}
