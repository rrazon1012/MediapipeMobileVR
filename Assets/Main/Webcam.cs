using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
  private WebCamDevice[] _camSources;
  public WebCamDevice[] camSources
  {
    get => _camSources;
    set
    {
      if (_camSources == null)
      {
        _camSources = WebCamTexture.devices;
      }
      else
      {
        camSources = value;
      }
    }
  }

  // Start is called before the first frame update
  void Start()
  {
    if (_camTexture == null)
    {
      _camTexture = new WebCamTexture();
    }

    image.texture = _camTexture;


    if (!_camTexture.isPlaying)
    {
      _camTexture.Play();
    }
  }

  // Update is called once per frame
  void Update()
  {

  }
}
