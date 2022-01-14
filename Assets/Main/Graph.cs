using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Mediapipe.Unity;
using Mediapipe;

using Stopwatch = System.Diagnostics.Stopwatch;

//abstract class for graphs, contains methods for handling packets
//as well as setting up the graph configurations
public abstract class Graph : MonoBehaviour
{
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

  private static readonly GlobalInstanceTable<int, GraphRunner> _InstanceTable = new GlobalInstanceTable<int, GraphRunner>(5);
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
}
