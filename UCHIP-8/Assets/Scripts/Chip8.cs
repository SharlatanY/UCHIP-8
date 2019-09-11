using System;
using UnityEngine;

namespace Chip8
{
  public class Chip8 : MonoBehaviour
  {
    public Texture OutputTexture; //Texture the chip 8 emulator will render to

    //Registers
    private static readonly byte[] V = new byte[16];
    private static ushort I; //memory address register

    //RAM
    private static readonly byte[] RAM = new byte[4096]; //0x0 to 0xFFF

    //Program counter
    private static ushort PC; //stores current execution address
  }
}
