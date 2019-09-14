﻿using System;
using System.IO;
using UnityEngine;

namespace Chip8
{
  public class Chip8 : MonoBehaviour
  {
    public Texture2D OutputTexture; //Texture the chip 8 emulator will render to

    //Registers
    private readonly byte[] V = new byte[16];
    private ushort I; //memory address register

    //RAM
    private readonly byte[] RAM = new byte[4096]; //0x0 to 0xFFF
    private const ushort FontStartingAddress = 0x0;

    //Program counter
    private ushort _pcStartingAddress = 0x200;
    private ushort PC; //stores current execution address

    //Tick/CPU speed
    private float _clockInHz = 500;
    private float _tickIntervalInS;
    /// <summary>
    /// Time reminder from last update call that wasn't enough for a "full tick".
    /// Example: Tick time is 3ms and between two Update() calls, 7ms pass.
    /// Two ticks will be execute (2x3ms =6ms) and the reminder will be 1ms (7ms-6ms).
    /// 5ms pass until the next Update() call but we'll now also add the reminder (5ms + 1ms from reminder)
    /// -> passed time is 6ms, two ticks are executed.
    /// This way, we get more consistent speed
    /// </summary>
    private float _tickReminder;

    //Misc
    private const string ROMPath = "Assets//StreamingAssets//rom.ch8";

    private Color[] _outputTextureResetColorArray; 

    private void Start()
    {
      _outputTextureResetColorArray = new Color[OutputTexture.width * OutputTexture.height];
      for(var i = 0; i < _outputTextureResetColorArray.Length; i++)
      {
        _outputTextureResetColorArray[i] = Color.black;
      }

      Reset();
    }

    private void Update()
    {
      //calculate how many ticks to execute
      var passedTime = Time.deltaTime + _tickReminder;
      var numTicksToExecute = (int)(passedTime / _tickIntervalInS);
      _tickReminder = passedTime % _tickIntervalInS;

      //execute instructions
      for (var i = 0; i < numTicksToExecute; i++)
      {
        ExecuteTick();
      }
    }

    private void OnDisable()
    {
      ClearScreen();
    }

    private void Reset()
    {
      InitializeRAM();
      PC = _pcStartingAddress;

      _tickIntervalInS = 1 / _clockInHz;
      _tickReminder = 0;

      //todo add other stuff to be reset as development progresses
    }

    private void InitializeRAM()
    {
      ClearRAM();
      WriteFontIntoRAM(FontStartingAddress);
      WriteROMIntoRAM(_pcStartingAddress);
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

    /// <summary>
    /// Executes one CPU tick/clock cycle
    /// </summary>
    private void ExecuteTick()
    {
      // read instruction
      var leftByte = RAM[PC];
      var rightByte = RAM[PC + 1];
      //var uShortOpCode = (ushort)(leftByte << 8 | rightByte);
      var opCode = new OpCode(leftByte, rightByte);

      // move program counter to next instruction to be executed on next tick (might be overriden if instruction turns out to be a jump or function call)
      PC += 2; //Instructions are two bytes long, so we need to move two bytes go get to the next instruction

      //todo execute instruction

      //OutputTexture.SetPixel(0, 0, Color.white);
      //OutputTexture.Apply();
      throw new NotImplementedException();
    }

    /// <summary>
    /// Paints the whole screen texture black
    /// </summary>
    private void ClearScreen()
    {
      OutputTexture.SetPixels(_outputTextureResetColorArray);
      OutputTexture.Apply();
    }
  }
}
