using Mediapipe.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenUI : MonoBehaviour
{
  [SerializeField] private RawImage _screen;

  private Webcam _imageSource;

  public void Initialize(Webcam imageSource)
  {
    _imageSource = imageSource;

    _screen.rectTransform.sizeDelta = new Vector2(_imageSource.textureWidth, _imageSource.textureHeight);
    _screen.rectTransform.localEulerAngles = _imageSource.rotation.Reverse().GetEulerAngles();
    //_screen.uvRect = GetUvRect(RunningMode.Async);
    _screen.texture = imageSource.GetCurrentTexture();
  }

  public void ReadSync(TextureFrame textureFrame)
  {
    if (!(_screen.texture is Texture2D))
    {
      _screen.texture = new Texture2D(_imageSource.textureWidth, _imageSource.textureHeight, TextureFormat.RGBA32, false);
      _screen.uvRect = GetUvRect();
    }
    textureFrame.CopyTexture(_screen.texture);
  }

  private UnityEngine.Rect GetUvRect()
  {
    var rect = new UnityEngine.Rect(0, 0, 1, 1);

    if (_imageSource.isFrontFacing)
    {
      rect.x = 1;
      rect.width = -1;
    }
    return rect;
  }
}
