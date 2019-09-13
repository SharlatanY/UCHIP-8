using System;
using System.IO;
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
    private const string ROMPath = "Assets//StreamingAssets//rom.ch8";

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
      ClearRAM();
      WriteFontIntoRAM(FontStartingAddress);
      WriteROMIntoRAM(pcStartingAddress);
    }

    /// <summary>
    /// Sets every byte in RAM to 0
    /// </summary>
    private void ClearRAM()
    {
      for (var i = 0; i < RAM.Length; i++)
      {
        RAM[i] = 0x0;
      }
    }

    /// <summary>
    /// Writes the font sprites into the RAM
    /// </summary>
    /// <param name="startingAddress">Address at which first byte will be written</param>
    private void WriteFontIntoRAM(ushort startingAddress)
    {
      Array.Copy(Chip8Constants.Font, 0, RAM, startingAddress, Chip8Constants.Font.Length );
    }

    /// <summary>
    /// Reads ROM from disk and loads it into the RAM
    /// </summary>
    /// <param name="startingAddress">Address at which first byte will be written</param>
    private void WriteROMIntoRAM(ushort startingAddress)
    {
      var binary = File.ReadAllBytes(ROMPath);
      Array.Copy(binary, 0, RAM, startingAddress, binary.Length);
    }
  }
}
