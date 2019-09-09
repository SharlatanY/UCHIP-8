using System;
using UnityEngine;

namespace Chip8
{
  public class ScreenRenderer : MonoBehaviour
  {
    public Texture ScreenTexture;
    public float x, y, width, height;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }




    private void OnPostRender()
    {
      GL.PushMatrix();
      GL.LoadPixelMatrix(0, Screen.width, Screen.height, 0);

      Graphics.DrawTexture(
        new Rect(x, y, Screen.width, Screen.height), //todo replace with correct coordinates depending on aspect ration, resolution etc
        ScreenTexture);
      GL.PopMatrix();
    }

    private bool ResolutionChanged()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Recalculates the coordinates used to correctly draw the texture
    /// </summary>
    private void RecalculateTextureDrawCoordinates()
    {
      //todo find nearest multiple of 64x32 for screen resolution and calculate exact values for drawing screen (texture)
      throw new NotImplementedException();
    }
  }
}
