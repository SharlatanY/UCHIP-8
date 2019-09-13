using System;
using System.Linq;
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
    private const ushort FontStartingAddress = 0x0;

    //Program counter
    private static ushort pcStartingAddress = 0x200;
    private static ushort PC = pcStartingAddress; //stores current execution address

    //Misc
    private static float _clock = 500;

    private void Start()
    {
      InitializeRAM();
    }

    private void Reset()
    {
      InitializeRAM();
      PC = pcStartingAddress;

      //todo add other stuff to be reset as development progresses
    }

    private void InitializeRAM()
    {
      for (var i = 0; i < RAM.Length; i++)
      {
        RAM[i] = 0x0;
      }

      WriteFontIntoRAM(FontStartingAddress);
    }

    /// <summary>
    /// Writes the font sprites into the RAM
    /// </summary>
    /// <param name="startingAddress">Address at which first byte will be written</param>
    private void WriteFontIntoRAM(ushort startingAddress)
    {
      Array.Copy(Chip8Constants.Font, 0, RAM, (int)startingAddress, Chip8Constants.Font.Length );
    }


  }
}
