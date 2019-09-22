using UnityEngine;

namespace Chip8
{
  public class ScreenRenderer : MonoBehaviour
  {
    public Texture ScreenTexture;

    private int _renderWidthInPx, _renderHeightInPx, _renderOffsetXInPx, _renderOffsetYInPx, _lastScreenWidth, _lastScreenHeight;

    // Update is called once per frame
    void Update()
    {
      // recalculate render coordinates if application window size changed
      if (_lastScreenWidth != Screen.width || _lastScreenHeight != Screen.height)
      {
        RecalculateTextureDrawCoordinates();
      }

      _lastScreenWidth = Screen.width;
      _lastScreenHeight = Screen.height;
    }
    private void OnPostRender()
    {
      GL.PushMatrix();
      //GL.LoadPixelMatrix(0, Screen.width, Screen.height, 0); //create "pixel perfect" matrix with 0/0 in lower left
      GL.LoadPixelMatrix(0, Screen.width, 0, Screen.height); //create "pixel perfect" matrix with 0/0 in upper left, which is what CHIP-8 works with

      Graphics.DrawTexture(
        new Rect(_renderOffsetXInPx, _renderOffsetYInPx, _renderWidthInPx, _renderHeightInPx),
        ScreenTexture);
      GL.PopMatrix();
    }

    /// <summary>
    /// Recalculates the coordinates used to correctly draw the texture
    /// </summary>
    private void RecalculateTextureDrawCoordinates()
    {
      //find nearest multiple of 64x32 for screen resolution and calculate exact values for drawing chip 8 screen (texture) centered and in the biggest possible format without any stretching
      if (Screen.width >= 2 * Screen.height)
      {
        //height will be maxed, width adjusted accordingly
        _renderHeightInPx = Screen.height - (Screen.height % Chip8Constants.ScreenResY);
        _renderWidthInPx = _renderHeightInPx * 2;
      }
      else
      {
        //width will be maxed, height adjusted accordingly
        _renderWidthInPx = Screen.width - (Screen.width % Chip8Constants.ScreenResX);
        _renderHeightInPx = _renderWidthInPx / 2;
      }

      _renderOffsetXInPx = (Screen.width - _renderWidthInPx) / 2;
      _renderOffsetYInPx = (Screen.height - _renderHeightInPx) / 2;
    }
  }
}
