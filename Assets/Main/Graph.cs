using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using Mediapipe.Unity;
using Mediapipe;

using Stopwatch = System.Diagnostics.Stopwatch;

//abstract class for graphs, contains methods for handling packets
//as well as setting up the graph configurations
public class Graph : MonoBehaviour
{
  #region graph runner
  public enum ConfigType
  {
    None,
    CPU,
    GPU,
    OpenGLES,
  }

  protected string TAG => GetType().Name;

  //the text files containing the info for input/output streams as well as nodes and calculators
  [SerializeField] private TextAsset _cpuConfig = null;
  [SerializeField] private TextAsset _gpuConfig = null;
  [SerializeField] private TextAsset _openGlesConfig = null;

  [SerializeField] private long _timeoutMicrosec = 0;

  private static readonly GlobalInstanceTable<int, Graph> _InstanceTable = new GlobalInstanceTable<int, Graph>(5);
  private static readonly Dictionary<IntPtr, int> _NameTable = new Dictionary<IntPtr, int>();

  public InferenceMode inferenceMode => configType == ConfigType.CPU ? InferenceMode.CPU : InferenceMode.GPU;
  public ConfigType configType { get; private set; }

  //checks the set configuration type and returns the txt file as Text Asset
  public TextAsset config
  {
    get
    {
      switch (configType)
      {
        case ConfigType.CPU: return _cpuConfig;
        case ConfigType.GPU: return _gpuConfig;
        case ConfigType.OpenGLES: return _openGlesConfig;
        case ConfigType.None:
        default: return null;
      }
    }
  }

  //timeout values
  public long timeoutMicrosec
  {
    get => _timeoutMicrosec;
    private set => _timeoutMicrosec = value;
  }
  public long timeoutMillisec => timeoutMicrosec / 1000;

  //rotation of the image passed in
  public RotationAngle rotation { get; private set; } = 0;

  private Stopwatch _stopwatch;

  //calculator for the graph
  protected CalculatorGraph calculatorGraph { get; private set; }
  protected Timestamp currentTimestamp;
  #endregion
  
  public enum ModelComplexity
  {
    Lite = 0,
    Full = 1
  };

  public ModelComplexity modelComplexity = ModelComplexity.Lite;
  public int maxNumhands = 2;

#pragma warning disable IDE1006
  public UnityEvent<List<Detection>> OnPalmDetectionsOutput = new UnityEvent<List<Detection>>();
  public UnityEvent<List<NormalizedRect>> OnHandRectsFromPalmDetectionsOutput = new UnityEvent<List<NormalizedRect>>();
  public UnityEvent<List<LandmarkList>> OnHandWorldLandmarksOutput = new UnityEvent<List<LandmarkList>>();
  public UnityEvent<List<NormalizedLandmarkList>> OnHandLandmarksOutput = new UnityEvent<List<NormalizedLandmarkList>>();
  public UnityEvent<List<ClassificationList>> OnHandednessOutput = new UnityEvent<List<ClassificationList>>();
#pragma warning disable IDE1006

  //input streams for the graph, video, image, webcam input
  private const string _inputStreamName = "input_video";

  //output streams names from the graph
  private const string _palmDetectionStreamName = "palm_detections";
  private const string _handRectsFromLandmarksStreamName = "hand_rects_from_landmarks";
  private const string _HandWorldLandmarksStreamName = "hand_world_landmarks";
  private const string _HandLandmarksStreamName = "hand_landmarks";
  private const string _handednessStreamName = "handedness";

  //output streams for the graph
  private OutputStream<DetectionVectorPacket, List<Detection>> _palmDetectionsStream;
  private OutputStream<NormalizedRectVectorPacket, List<NormalizedRect>> _handRectsFromLandmarksStream;
  private OutputStream<NormalizedLandmarkListVectorPacket, List<NormalizedLandmarkList>> _handLandmarksStream;
  private OutputStream<LandmarkListVectorPacket, List<LandmarkList>> _handWorldLandmarksStream;
  private OutputStream<ClassificationListVectorPacket, List<ClassificationList>> _handednessStream;

  protected long prevPalmDetectionMicrosec = 0;
  protected long prevHandRectsFromPalmDetectionsMicrosec = 0;
  protected long prevHandednessMicrosec = 0;

  public Status StartRun(Webcam imageSource)
  {
    InitializeOutputStreams();

    _palmDetectionsStream.StartPolling(true).AssertOk();
    _handRectsFromLandmarksStream.StartPolling(true).AssertOk();
    _handLandmarksStream.StartPolling(true).AssertOk();
    _handWorldLandmarksStream.StartPolling(true).AssertOk();
    _handednessStream.StartPolling(true).AssertOk();

    return calculatorGraph.StartRun(BuildSidePacket(imageSource));

  }

  public virtual void Stop()
  {
    #region stops the graph
    if (calculatorGraph == null) { return; }

    // TODO: not to call CloseAllPacketSources if calculatorGraph has not started.
    using (var status = calculatorGraph.CloseAllPacketSources())
    {
      if (!status.Ok())
      {
        Mediapipe.Logger.LogError(TAG, status.ToString());
      }
    }

    using (var status = calculatorGraph.WaitUntilDone())
    {
      if (!status.Ok())
      {
        Mediapipe.Logger.LogError(TAG, status.ToString());
      }
    }

    var _ = _NameTable.Remove(calculatorGraph.mpPtr);
    calculatorGraph.Dispose();
    calculatorGraph = null;

    if (_stopwatch != null && _stopwatch.IsRunning)
    {
      _stopwatch.Stop();
    }
    #endregion

    OnPalmDetectionsOutput.RemoveAllListeners();
    OnHandRectsFromPalmDetectionsOutput.RemoveAllListeners();
    OnHandLandmarksOutput.RemoveAllListeners();
    OnHandWorldLandmarksOutput.RemoveAllListeners();
    OnHandednessOutput.RemoveAllListeners();
  }

  public Status AddTextureFrameToInputStream(TextureFrame textureFrame)
  {
    return AddTextureFrameToInputStream(_inputStreamName, textureFrame);
  }

  //gets the next v
  public MobileVRValue FetchNextValue()
  {
    var _ = _palmDetectionsStream.TryGetNext(out var palmDetections);
    _ = _handRectsFromLandmarksStream.TryGetNext(out var handRectsFromPalmDetections);
    _ = _handWorldLandmarksStream.TryGetNext(out var handWorldLandmarks);
    _ = _handLandmarksStream.TryGetNext(out var handLandmarks);
    _ = _handednessStream.TryGetNext(out var handedness);

    OnPalmDetectionsOutput.Invoke(palmDetections);
    OnHandRectsFromPalmDetectionsOutput.Invoke(handRectsFromPalmDetections);
    OnHandednessOutput.Invoke(handedness);
    OnHandWorldLandmarksOutput.Invoke(handWorldLandmarks);
    OnHandLandmarksOutput.Invoke(handLandmarks);
    /*Debug.Log(palmDetections != null);
    Debug.Log(handRectsFromPalmDetections != null);
    Debug.Log(handedness != null);*/

    return new MobileVRValue(palmDetections, handRectsFromPalmDetections, handWorldLandmarks, handLandmarks, handedness);
  }

  //request the assets required for landmarks, handedness, palm detection
  protected IList<WaitForResult> RequestDependentAssets()
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
    _handRectsFromLandmarksStream = new OutputStream<NormalizedRectVectorPacket, List<NormalizedRect>>(calculatorGraph, _handRectsFromLandmarksStreamName);
    _handWorldLandmarksStream = new OutputStream<LandmarkListVectorPacket, List<LandmarkList>>(calculatorGraph, _HandWorldLandmarksStreamName);
    _handLandmarksStream = new OutputStream<NormalizedLandmarkListVectorPacket, List<NormalizedLandmarkList>>(calculatorGraph, _HandLandmarksStreamName);
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

  private SidePacket BuildSidePacket(Webcam imageSource)
  {
    var sidePacket = new SidePacket();

    SetImageTransformationOptions(sidePacket, imageSource, true);
    sidePacket.Emplace("model_complexity", new IntPacket((int)modelComplexity));
    sidePacket.Emplace("num_hands", new IntPacket(maxNumhands));

    return sidePacket;
  }

  #region Graph Runner Methods
  protected void Start()
  {
    _InstanceTable.Add(GetInstanceID(), this);
  }

  protected void OnDestroy()
  {
    Stop();
  }

  public WaitForResult WaitForInit()
  {
    return new WaitForResult(this, Initialize());
  }

  public virtual IEnumerator Initialize()
  {
    configType = DetectConfigType();
    Mediapipe.Logger.LogInfo(TAG, $"Using {configType} config");

    if (configType == ConfigType.None)
    {
      throw new InvalidOperationException("Failed to detect config. Check if config is set to GraphRunner");
    }

    InitializeCalculatorGraph().AssertOk();
    _stopwatch = new Stopwatch();
    _stopwatch.Start();

    Mediapipe.Logger.LogInfo(TAG, "Loading dependent assets...");
    var assetRequests = RequestDependentAssets();
    yield return new WaitWhile(() => assetRequests.Any((request) => request.keepWaiting));

    var errors = assetRequests.Where((request) => request.isError).Select((request) => request.error).ToList();
    if (errors.Count > 0)
    {
      foreach (var error in errors)
      {
        Mediapipe.Logger.LogError(TAG, error);
      }
      throw new InternalException("Failed to prepare dependent assets");
    }
  }

  public Status AddPacketToInputStream<T>(string streamName, Packet<T> packet)
  {
    return calculatorGraph.AddPacketToInputStream(streamName, packet);
  }

  public Status AddTextureFrameToInputStream(string streamName, TextureFrame textureFrame)
  {
    currentTimestamp = GetCurrentTimestamp();

    if (configType == ConfigType.OpenGLES)
    {
      var gpuBuffer = textureFrame.BuildGpuBuffer(GpuManager.GlCalculatorHelper.GetGlContext());
      return calculatorGraph.AddPacketToInputStream(streamName, new GpuBufferPacket(gpuBuffer, currentTimestamp));
    }

    var imageFrame = textureFrame.BuildImageFrame();
    textureFrame.Release();

    return AddPacketToInputStream(streamName, new ImageFramePacket(imageFrame, currentTimestamp));
  }

  public void SetTimeoutMicrosec(long timeoutMicrosec)
  {
    this.timeoutMicrosec = (long)Mathf.Max(0, timeoutMicrosec);
  }

  public void SetTimeoutMillisec(long timeoutMillisec)
  {
    SetTimeoutMicrosec(1000 * timeoutMillisec);
  }

  protected static bool TryGetGraphRunner(IntPtr graphPtr, out Graph graphRunner)
  {
    var isInstanceIdFound = _NameTable.TryGetValue(graphPtr, out var instanceId);

    if (isInstanceIdFound)
    {
      return _InstanceTable.TryGetValue(instanceId, out graphRunner);
    }
    graphRunner = null;
    return false;
  }

  protected static Status InvokeIfGraphRunnerFound<T>(IntPtr graphPtr, IntPtr packetPtr, Action<T, IntPtr> action) where T : Graph
  {
    try
    {
      var isFound = TryGetGraphRunner(graphPtr, out var graphRunner);
      if (!isFound)
      {
        return Status.FailedPrecondition("Graph runner is not found");
      }
      var graph = (T)graphRunner;
      action(graph, packetPtr);
      return Status.Ok();
    }
    catch (Exception e)
    {
      return Status.FailedPrecondition(e.ToString());
    }
  }

  protected static Status InvokeIfGraphRunnerFound<T>(IntPtr graphPtr, Action<T> action) where T : Graph
  {
    return InvokeIfGraphRunnerFound<T>(graphPtr, IntPtr.Zero, (graph, ptr) => { action(graph); });
  }

  protected bool TryGetPacketValue<T>(Packet<T> packet, ref long prevMicrosec, out T value) where T : class
  {
    long currentMicrosec = 0;
    using (var timestamp = packet.Timestamp())
    {
      currentMicrosec = timestamp.Microseconds();
    }

    if (!packet.IsEmpty())
    {
      prevMicrosec = currentMicrosec;
      value = packet.Get();
      return true;
    }

    value = null;
    return currentMicrosec - prevMicrosec > timeoutMicrosec;
  }

  protected bool TryConsumePacketValue<T>(Packet<T> packet, ref long prevMicrosec, out T value) where T : class
  {
    long currentMicrosec = 0;
    using (var timestamp = packet.Timestamp())
    {
      currentMicrosec = timestamp.Microseconds();
    }

    if (!packet.IsEmpty())
    {
      prevMicrosec = currentMicrosec;
      var statusOrValue = packet.Consume();

      value = statusOrValue.ValueOr();
      return true;
    }

    value = null;
    return currentMicrosec - prevMicrosec > timeoutMicrosec;
  }

  protected Timestamp GetCurrentTimestamp()
  {
    if (_stopwatch == null || !_stopwatch.IsRunning)
    {
      return Timestamp.Unset();
    }
    var microseconds = _stopwatch.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000);
    return new Timestamp(microseconds);
  }

  protected Status InitializeCalculatorGraph()
  {
    calculatorGraph = new CalculatorGraph();
    _NameTable.Add(calculatorGraph.mpPtr, GetInstanceID());

    // NOTE: There's a simpler way to initialize CalculatorGraph.
    //
    //     calculatorGraph = new CalculatorGraph(config.text);
    //
    //   However, if the config format is invalid, this code does not initialize CalculatorGraph and does not throw exceptions either.
    //   The problem is that if you call ObserveStreamOutput in this state, the program will crash.
    //   The following code is not very efficient, but it will return Non-OK status when an invalid configuration is given.
    try
    {
      var calculatorGraphConfig = GetCalculatorGraphConfig();
      var status = calculatorGraph.Initialize(calculatorGraphConfig);

      return !status.Ok() || inferenceMode == InferenceMode.CPU ? status : calculatorGraph.SetGpuResources(GpuManager.GpuResources);
    }
    catch (Exception e)
    {
      return Status.FailedPrecondition(e.ToString());
    }
  }

  protected virtual CalculatorGraphConfig GetCalculatorGraphConfig()
  {
    return CalculatorGraphConfig.Parser.ParseFromTextFormat(config.text);
  }

  protected void SetImageTransformationOptions(SidePacket sidePacket, Webcam imageSource, bool expectedToBeMirrored = false)
  {
    // NOTE: The origin is left-bottom corner in Unity, and right-top corner in MediaPipe.
    rotation = imageSource.rotation.Reverse();
    var inputRotation = rotation;
    var isInverted = Mediapipe.Unity.CoordinateSystem.ImageCoordinate.IsInverted(rotation);
    var shouldBeMirrored = imageSource.isHorizontallyFlipped ^ expectedToBeMirrored;
    var inputHorizontallyFlipped = isInverted ^ shouldBeMirrored;
    var inputVerticallyFlipped = !isInverted;

    if ((inputHorizontallyFlipped && inputVerticallyFlipped) || rotation == RotationAngle.Rotation180)
    {
      inputRotation = inputRotation.Add(RotationAngle.Rotation180);
      inputHorizontallyFlipped = !inputHorizontallyFlipped;
      inputVerticallyFlipped = !inputVerticallyFlipped;
    }

    Mediapipe.Logger.LogDebug($"input_rotation = {inputRotation}, input_horizontally_flipped = {inputHorizontallyFlipped}, input_vertically_flipped = {inputVerticallyFlipped}");

    sidePacket.Emplace("input_rotation", new IntPacket((int)inputRotation));
    sidePacket.Emplace("input_horizontally_flipped", new BoolPacket(inputHorizontallyFlipped));
    sidePacket.Emplace("input_vertically_flipped", new BoolPacket(inputVerticallyFlipped));
  }

  protected virtual ConfigType DetectConfigType()
  {
    if (GpuManager.IsInitialized)
    {
#if UNITY_ANDROID
      if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 && _openGlesConfig != null)
      {
        return ConfigType.OpenGLES;
      }
#endif
      if (_gpuConfig != null)
      {
        return ConfigType.GPU;
      }
    }
    return _cpuConfig != null ? ConfigType.CPU : ConfigType.None;
  }

  protected WaitForResult WaitForAsset(string assetName, string uniqueKey, long timeoutMillisec, bool overwrite = false)
  {
    return new WaitForResult(this, AssetLoader.PrepareAssetAsync(assetName, uniqueKey, overwrite), timeoutMillisec);
  }

  protected WaitForResult WaitForAsset(string assetName, long timeoutMillisec, bool overwrite = false)
  {
    return WaitForAsset(assetName, assetName, timeoutMillisec, overwrite);
  }

  protected WaitForResult WaitForAsset(string assetName, string uniqueKey, bool overwrite = false)
  {
    return new WaitForResult(this, AssetLoader.PrepareAssetAsync(assetName, uniqueKey, overwrite));
  }

  protected WaitForResult WaitForAsset(string assetName, bool overwrite = false)
  {
    return WaitForAsset(assetName, assetName, overwrite);
  }

  protected class OutputStream<TPacket, TValue> where TPacket : Packet<TValue>, new()
  {
    private readonly CalculatorGraph _calculatorGraph;

    private readonly string _streamName;
    private OutputStreamPoller<TValue> _poller;
    private TPacket _outputPacket;

    private string _presenceStreamName;
    private OutputStreamPoller<bool> _presencePoller;
    private BoolPacket _presencePacket;

    private bool canFreeze => _presenceStreamName != null;

    public OutputStream(CalculatorGraph calculatorGraph, string streamName)
    {
      _calculatorGraph = calculatorGraph;
      _streamName = streamName;
    }

    public Status StartPolling(bool observeTimestampBounds = false)
    {
      _outputPacket = new TPacket();

      var statusOrPoller = _calculatorGraph.AddOutputStreamPoller<TValue>(_streamName, observeTimestampBounds);
      var status = statusOrPoller.status;
      if (status.Ok())
      {
        _poller = statusOrPoller.Value();
      }
      return status;
    }

    public Status StartPolling(string presenceStreamName)
    {
      _presenceStreamName = presenceStreamName;
      var status = StartPolling(false);

      if (status.Ok())
      {
        _presencePacket = new BoolPacket();

        var statusOrPresencePoller = _calculatorGraph.AddOutputStreamPoller<bool>(presenceStreamName);
        status = statusOrPresencePoller.status;
        if (status.Ok())
        {
          _presencePoller = statusOrPresencePoller.Value();
        }
      }
      return status;
    }

    public Status AddListener(CalculatorGraph.NativePacketCallback callback, bool observeTimestampBounds = false)
    {
      return _calculatorGraph.ObserveOutputStream(_streamName, callback, observeTimestampBounds);
    }

    public bool TryGetNext(out TValue value)
    {
      if (HasNextValue())
      {
        value = _outputPacket.Get();
        return true;
      }
      value = default;
      return false;
    }

    public bool TryGetLatest(out TValue value)
    {
      if (HasNextValue())
      {
        var queueSize = _poller.QueueSize();

        // Assume that queue size will not be reduced from another thread.
        while (queueSize-- > 0)
        {
          if (!Next())
          {
            value = default;
            return false;
          }
        }
        value = _outputPacket.Get();
        return true;
      }
      value = default;
      return false;
    }

    private bool HasNextValue()
    {
      if (canFreeze)
      {
        if (!NextPresence() || _presencePacket.IsEmpty() || !_presencePacket.Get())
        {
          // NOTE: IsEmpty() should always return false
          return false;
        }
      }
      return Next() && !_outputPacket.IsEmpty();
    }

    private bool NextPresence()
    {
      return Next(_presencePoller, _presencePacket, _presenceStreamName);
    }

    private bool Next()
    {
      return Next(_poller, _outputPacket, _streamName);
    }

    private static bool Next<T>(OutputStreamPoller<T> poller, Packet<T> packet, string streamName)
    {
      if (!poller.Next(packet))
      {
        Mediapipe.Logger.LogWarning($"Failed to get next value from {streamName}, so there may be errors inside the calculatorGraph. See logs for more details");
        return false;
      }
      return true;
    }
  }
  #endregion
}
