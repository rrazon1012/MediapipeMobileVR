using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using Mediapipe.Unity;

public class Webcam : MonoBehaviour
{

  //UI screen output for ease of debugging
  public RawImage image;


  //video frame converted to texture, for ease of overlaying onto UI and for passing into a graph
  private WebCamTexture _camTexture;
  //setter and getter for the output texture
  public WebCamTexture camTexture
  {
    get => _camTexture; // == get { return _camTexture;}
    set
    {
      if (_camTexture != null) { _camTexture.Pause(); }
      _camTexture = value;
    }
  }


  //webcam device information stuff
  [SerializeField] private int _resWidth = 1920;
  [SerializeField] private int _resHeight = 1080;
  [SerializeField] private double _resFramerate = 30;

  private WebCamDevice? _webCam;
  public WebCamDevice? webCam
  {
    get => _webCam;
    set
    {
      if (_webCam is WebCamDevice webCamValue)
      {
        if(value is WebCamDevice valueofValue && valueofValue.name == webCamValue.name)
        {
          return; //webcam device is current device
        }
      }
      else if (value == null)
      {
        return; //webcam device is null, dont change
      }
      _webCam = value;
    }
  }

  public int textureWidth => !isPrepared ? 0 : camTexture.width;
  public int textureHeight => !isPrepared ? 0 : camTexture.height;

  public virtual bool isHorizontallyFlipped { get; set; } = false;
  public bool isVerticallyFlipped => isPrepared && camTexture.videoVerticallyMirrored;
  public bool isFrontFacing => isPrepared && (webCam is WebCamDevice valueOfWebCamDevice) && valueOfWebCamDevice.isFrontFacing;
  public RotationAngle rotation => !isPrepared ? RotationAngle.Rotation0 : (RotationAngle)camTexture.videoRotationAngle;


  //webcam device setup
  public static bool HasPermission = false;
  private bool _isInitialized = false;
  public bool isPrepared => camTexture != null;



  //all sources of video from webcamtexture class
  private WebCamDevice[] _camSources;
  public WebCamDevice[] camSources
  {
    get
    {
      if (_camSources == null)
      {
        _camSources = WebCamTexture.devices;
      }
      return _camSources;
    }
    set => _camSources = value;
  }


  // Start is called before the first frame update
  private IEnumerator Start()
  {
    //asks for permission from user if its on android and blocks start code
    yield return GetPermission();

    if (!HasPermission)
    {
      _isInitialized = true;
      yield break;
    }

    if (WebCamTexture.devices.Length > 0 && WebCamTexture.devices != null)
    {
      webCam = WebCamTexture.devices[0];
    }

    _isInitialized = true;
    /*if (_camTexture == null)
    {
      _camTexture = new WebCamTexture(1920, 1080, 30);
    }

    image.texture = _camTexture;

    if (!_camTexture.isPlaying)
    {
      Play();
    }*/
  }

  public IEnumerator Play()
  {
    yield return new WaitUntil(() => _isInitialized);
    if (!HasPermission)
    {
      throw new System.InvalidOperationException("No Permission to use camera");
    }
    InitializeWebCamTexture();
    camTexture.Play();
    yield return WaitForWebCamTexture();
  }

  public IEnumerator Resume()
  {
    if (!isPrepared)
    {
      throw new System.InvalidOperationException("WebCamTexture is not prepared yet");
    }
    if (!camTexture.isPlaying)
    {
      camTexture.Play();
    }
    yield return WaitForWebCamTexture();
  }

  public void Pause()
  {
    if (camTexture.isPlaying)
    {
      camTexture.Pause();
    }
  }

  public void Stop()
  {
    if (camTexture != null)
    {
      camTexture.Stop();
    }
    camTexture = null;
  }


  private IEnumerator GetPermission()
  {
    //if already has permission then exit.
    if (HasPermission)
    {
      yield break;
    }

    //requests for permission to access camera if it is being run in Android
#if UNITY_ANDROID
    if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
    {
      Permission.RequestUserPermission(Permission.Camera);
      yield return new WaitForSeconds(0.1f);
    }
#elif UNITY_IOS
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam)) {
          yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        }
#endif

#if UNITY_ANDROID
    if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
    {
      Debug.Log("Not permitted");
      yield break;
    }
#elif UNITY_IOS
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam)) {
          Logger.LogWarning(_TAG, "Not permitted to use WebCam");
          yield break;
        }
#endif
    HasPermission = true;
    yield return new WaitForEndOfFrame();
  }


  private void InitializeWebCamTexture()
  {
    // Stop();
    if (webCam is WebCamDevice valueOfWebCamDevice)
    {
      camTexture = new WebCamTexture(valueOfWebCamDevice.name, _resWidth, _resHeight, (int)_resFramerate);
      return;
    }
    throw new System.InvalidOperationException("No webvam device selected, Can't initialize");
  }

  private IEnumerator WaitForWebCamTexture()
  {
    const int timeoutFrame = 500;
    var count = 0;
    Debug.Log("Waiting for WebCamTexture to start");
    yield return new WaitUntil(() => count++ > timeoutFrame || camTexture.width > 16);

    if (camTexture.width <= 16)
    {
      throw new System.TimeoutException("Failed to start WebCam");
    }
  }

  public Texture GetCurrentTexture()
  {
    return camTexture;
  }
}
