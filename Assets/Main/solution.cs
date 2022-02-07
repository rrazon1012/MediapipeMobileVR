using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Unity;
using UnityEngine.UI;

namespace Mediapipe
{
  public class solution : MonoBehaviour
  {
    protected virtual string TAG => GetType().Name;

    protected Initializer initializer;
    protected Webcam camSource;
    protected bool isPaused;
    //public RawImage image;

    public GameObject[] cube;

    #region Debugging
    //Video screen that is being looked at
    [SerializeField] private ScreenUI _screen;
    //used for drawing the bounding box of the palm detected
    [SerializeField] private DetectionListAnnotationController _palmDetectionsAnnotationController;
    [SerializeField] private NormalizedRectListAnnotationController _handRectsFromPalmDetectionsAnnotationController;
    [SerializeField] private MultiHandLandmarkListAnnotationController _handLandmarksAnnotationController;
    //the graph being run that contains the nodes, calculators, inputs and outputs
    [SerializeField] private Graph _graphRunner;
    //frame from image/video source that is accessed as a frame
    [SerializeField] private TextureFramePool _textureFramePool;
    #endregion

    private Coroutine _coroutine; //coroutine for tracking the actual run
    public Graph.ModelComplexity modelComplexity = Graph.ModelComplexity.Lite; //complexity of the hand
    public int maxNumHands = 2; //maximum number of hands to be detected by mediapipe
    public long timeoutMillisec = 50; //timeout for packets

    //contains the information such as palmrect, handlandmarks, etc
    public MobileVRValue handValues { get; private set; }



    // Start is called before the first frame update
    private IEnumerator Start()
    {
      var initializerObj = GameObject.Find("Initializer"); //finds the initializer object in the scene

      if (initializerObj == null) //checks if it exists
      {
        Logger.LogError(TAG, "Initializer not found, play from the starting scene");
        yield break;
      }

      initializer = initializerObj.GetComponent<Initializer>();

      yield return new WaitUntil(() => initializer.isFinished);

      //get the webcam source
      var camObj = GameObject.Find("WebcamSource");

      if (camObj == null)
      {
        Logger.LogError(TAG, "Camera not found");
        Debug.Log("not found");
        yield break;
      }

      camSource = camObj.GetComponent<Webcam>();

      Play();
    }

    public void Play()
    {
      if(_coroutine != null)
      {
        Stop();
      }
      isPaused = false;
      _coroutine = StartCoroutine(Run());
    }

    /// <summary>
    ///   Pause the main program.
    /// <summary>
    public void Pause()
    {
      isPaused = true;
    }

    /// <summary>
    ///    Resume the main program.
    ///    If the main program has not begun, it'll do nothing.
    /// </summary>
    public void Resume()
    {
      isPaused = false;
    }

    /// <summary>
    ///   Stops the main program.
    /// </summary>
    public void Stop()
    {
      isPaused = true;
    }

    private IEnumerator Run()
    {
      var graphInitRequest = _graphRunner.WaitForInit();
      var imageSource = camSource;

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
      //Logger.LogInfo(TAG, $"Running Mode = {runningMode}");

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
      SetupAnnotationController(_handLandmarksAnnotationController, imageSource, true);
      /*SetupAnnotationController(_handRectsFromLandmarksAnnotationController, imageSource, true);*/

      while (true)
      {
        yield return new WaitWhile(() => isPaused);

        var textureFrameRequest = _textureFramePool.WaitForNextTextureFrame();
        yield return textureFrameRequest;
        var textureFrame = textureFrameRequest.result;

        // Copy current image to TextureFrame
        ReadFromImageSource(imageSource, textureFrame);

        _graphRunner.AddTextureFrameToInputStream(textureFrame).AssertOk();

        // TODO: copy texture before `textureFrame` is released
        _screen.ReadSync(textureFrame);

        // When running synchronously, wait for the outputs here (blocks the main thread).
        var value = _graphRunner.FetchNextValue();
        handValues = value;
        /*if (value.handRectsFromPalmDetections == null || value.handWorldLandmarks == null)
        {
          Debug.Log("Palm Detection returned null");
        }
        else
        {
          //Debug.Log(value.handWorldLandmarks[0].Landmark.ToString());
          //Debug.Log(value.handWorldLandmarks[0].Landmark[0].X);
          //Debug.Log(value.handLandmarks[0].Landmark[8].X);
          var screenOffset = new Vector2(value.handRectsFromPalmDetections[0].XCenter, value.handRectsFromPalmDetections[0].YCenter);
          var offset = value.handWorldLandmarks[0].Landmark[0];

          //Debug.Log("Offset X: " + offset.X + " Offset Y: " + offset.Y);

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

          for(var i = 1; i < 21; i++)
          {
            //scaling up the landmarks and applying a negate to account for the camera
            //Note: Y has a negative scale because 0,0 is at top left, otherwise it would 
            cube[i].transform.localPosition = new Vector3((value.handWorldLandmarks[0].Landmark[i].X) * scale * negate,
              (value.handWorldLandmarks[0].Landmark[i].Y) * -scale, (value.handWorldLandmarks[0].Landmark[i].Z) * scale) - bottomLeft;

            //Offset of the hand in world space relative to its offset in the image texture (video frame)
            cube[i].transform.position += new Vector3(screenOffset.x * 40 * negate, screenOffset.y * -20);
          }
          cube[0].transform.position = new Vector3(offset.X, offset.Y, offset.Z) * scale;

          Debug.Log("Index Tip " + value.handLandmarks[0].Landmark[8].Y);
          Debug.Log("Index MCP " + value.handLandmarks[0].Landmark[5].Y);
          if (value.handLandmarks[0].Landmark[8].Y < value.handLandmarks[0].Landmark[5].Y) 
          {
            Debug.Log("Index Finger is up");
          }
        }*/

        _palmDetectionsAnnotationController.DrawNow(value.palmDetections);
        _handRectsFromPalmDetectionsAnnotationController.DrawNow(value.handRectsFromPalmDetections);
        _handLandmarksAnnotationController.DrawNow(value.handLandmarks, value.handedness);

        yield return new WaitForEndOfFrame();
      }
    }

    protected static void SetupAnnotationController<T>(AnnotationController<T> annotationController, Webcam imageSource, bool expectedToBeMirrored = false) where T : HierarchicalAnnotation
    {
      annotationController.isMirrored = expectedToBeMirrored ^ imageSource.camTexture.videoVerticallyMirrored ^ imageSource.isFrontFacing;
      annotationController.rotationAngle = imageSource.rotation.Reverse();
    }

    protected static void ReadFromImageSource(Webcam imageSource, TextureFrame textureFrame)
    {
      var sourceTexture = imageSource.GetCurrentTexture();

      // For some reason, when the image is coiped on GPU, latency tends to be high.
      // So even when OpenGL ES is available, use CPU to copy images.
      var textureType = sourceTexture.GetType();

      if (textureType == typeof(WebCamTexture))
      {
        textureFrame.ReadTextureFromOnCPU((WebCamTexture)sourceTexture);
      }
      else if (textureType == typeof(Texture2D))
      {
        textureFrame.ReadTextureFromOnCPU((Texture2D)sourceTexture);
      }
      else
      {
        textureFrame.ReadTextureFromOnCPU(sourceTexture);
      }
    }
  }
}
