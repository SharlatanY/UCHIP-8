﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
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
    private readonly Stack<ushort> _stack = new Stack<ushort>();
    private const string ROMPath = "Assets//StreamingAssets//rom.ch8";

    private Color[] _outputTextureResetColorArray;

    private void Start()
    {
      _outputTextureResetColorArray = new Color[OutputTexture.width * OutputTexture.height];
      for (var i = 0; i < _outputTextureResetColorArray.Length; i++)
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
      I = 0x0;

      _tickIntervalInS = 1 / _clockInHz;
      _tickReminder = 0;

      _stack.Clear();

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
      Array.Copy(Chip8Constants.Font, 0, RAM, startingAddress, Chip8Constants.Font.Length);
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

      // execute instruction
      ExecuteOpCode(opCode);

      //OutputTexture.SetPixel(0, 0, Color.white);
      //OutputTexture.Apply();
      //throw new NotImplementedException();
    }

    private void ExecuteOpCode(OpCode opCode)
    {
      var opCodeInvalid = false;
      switch (opCode.FirstDigitHex)
      {
        case "0":
          switch (opCode.Hex)
          {
            case "00E0":
              ClearScreen();
              break;
            case "00EE":
              ReturnFromSubroutine();
              break;
            default:
              opCodeInvalid = true;
              break;
          }
          break;
        case "1":
          JumpToAddress(opCode.NNN);
          break;
        case "2":
          CallSubroutine(opCode.NNN);
          break;
        case "3":
          SkipNextInstructionIfValEqualWithRegVal(opCode.X, opCode.NN);
          break;
        case "4":
          SkipNextInstructionIfValNotEqualWithRegVal(opCode.X, opCode.NN);
          break;
        case "5":
          if (opCode.LastDigitHex == "0")
            SkipNextInstructionIfRegValEqualWithOtherRegVal(opCode.X, opCode.Y);
          else
            opCodeInvalid = true;
          break;
        case "6":
          SetRegVal(opCode.X, opCode.NN);
          break;
        case "7":
          AddValToReg(opCode.X, opCode.NN);
          break;
        case "8":
          #region handle case 8
          switch (opCode.LastDigitHex)
          {
            case "0":
              CopyRegVal(opCode.X, opCode.Y);
              break;
            case "1":
              SetVxToVxOrVy(opCode.X, opCode.Y);
              break;
            case "2":
              SetVxToVxAndVy(opCode.X, opCode.Y);
              break;
            case "3":
              SetVxToVxXorVy(opCode.X, opCode.Y);
              break;
            case "4":
              AddRegToOtherRegWithCarry(opCode.X, opCode.Y);
              break;
            case "5:":
              SetVxToVxMinusVyWithBorrow(opCode.X, opCode.Y);
              break;
            case "6":
              OC8XY6(opCode.X, opCode.Y);
              break;
            case "7":
              SetVxToVyMinusVxWithBorrow(opCode.X, opCode.Y);
              break;
            case "8":
              OC8XYE(opCode.X, opCode.Y);
              break;
            default:
              opCodeInvalid = true;
              break;
          }
          break;
          #endregion
        case "9":
          if (opCode.LastDigitHex == "0")
            SkipNextInstructionIfRegValNotEqualWithOtherRegVal(opCode.X, opCode.Y);
          else
            opCodeInvalid = true;
          break;
        default:
          opCodeInvalid = true;
          break;
      }

      if (opCodeInvalid)
      {
        throw new ArgumentException($"Invalid OpCode: '{opCode.Hex}' at address {PC-2}", nameof(opCode)); //need to reduce PC by two to get actual address because PC was already directly set to address that will have to be read next
      }
    }

    /// <summary>
    /// Paints the whole screen texture black
    /// </summary>
    private void ClearScreen()
    {
      OutputTexture.SetPixels(_outputTextureResetColorArray);
      OutputTexture.Apply();
    }

    /// <summary>
    /// Sets the program counter to a specific address in the memory
    /// </summary>
    /// <param name="address"></param>
    private void JumpToAddress(ushort address)
    {
      PC = address;
    }

    /// <summary>
    /// Jumps to the start of a subroutine
    /// </summary>
    /// <param name="address"></param>
    private void CallSubroutine(ushort address)
    {
      _stack.Push(PC);
      PC = address;
    }

    /// <summary>
    /// Returns from the current subroutine and sets the PC to the next instruction after the initial call of the subroutine
    /// </summary>
    private void ReturnFromSubroutine()
    {
      PC = (ushort)(_stack.Pop() + 2); // +2 because the PC needs to be set to the next instruction after the original call to the subroutine. Else, the subroutine would be called again in the next tick.
    }

    /// <summary>
    /// Skips the next instruction if the value provided is equal to the value stored in a register.
    /// </summary>
    /// <param name="registerIndex">Index of register to compare the value to.</param>
    /// <param name="value">Value to compare with value in register</param>
    private void SkipNextInstructionIfValEqualWithRegVal(uint registerIndex, byte value)
    {
      if (V[registerIndex] == value)
        PC += 2;
    }

    /// <summary>
    /// Skips the next instruction if the value provided is NOT equal to the value stored in a register.
    /// </summary>
    /// <param name="registerIndex">Index of register to compare the value to.</param>
    /// <param name="value">Value to compare with value in register</param>
    private void SkipNextInstructionIfValNotEqualWithRegVal(uint registerIndex, byte value)
    {
      if (V[registerIndex] != value)
        PC += 2;
    }

    /// <summary>
    /// Skips the next instruction if the values of two specific registers are equal.
    /// </summary>
    /// <param name="registerIndex1">Index of first register to compare.</param>
    /// <param name="registerIndex2">Index of second register to compare.</param>
    private void SkipNextInstructionIfRegValEqualWithOtherRegVal(uint registerIndex1, uint registerIndex2)
    {
      if (V[registerIndex1] == V[registerIndex2])
        PC += 2;
    }

    /// <summary>
    /// Sets the value of a register
    /// </summary>
    /// <param name="registerIndex"></param>
    /// <param name="value"></param>
    private void SetRegVal(uint registerIndex, byte value)
    {
      V[registerIndex] = value;
    }

    /// <summary>
    /// Adds a value to the value that's already stored in a register.
    /// Overflows will wrap around.
    /// </summary>
    /// <param name="registerIndex"></param>
    /// <param name="value"></param>
    private void AddValToReg(uint registerIndex, byte value)
    {
      V[registerIndex] += value; // potential overflow intended
    }

    /// <summary>
    /// Sets the value of a register to that of another register.
    /// </summary>
    /// <param name="targetRegIndex">Register to copy value to.</param>
    /// <param name="sourceRegIndex">Register to copy value from.</param>
    private void CopyRegVal(uint targetRegIndex, uint sourceRegIndex)
    {
      V[targetRegIndex] = V[sourceRegIndex];
    }

    /// <summary>
    /// Register value V[x] is set to result of bitwise OR of V[x] and V[y]
    /// </summary>
    /// <param name="indexX">Index V[x]</param>
    /// <param name="indexY">Index V[y]</param>
    private void SetVxToVxOrVy(uint indexX, uint indexY)
    {
      V[indexX] = (byte) (V[indexX] | V[indexY]);
    }

    /// <summary>
    /// Register value V[x] is set to result of bitwise AND of V[x] and V[y]
    /// </summary>
    /// <param name="indexX">Index V[x]</param>
    /// <param name="indexY">Index V[y]</param>
    private void SetVxToVxAndVy(uint indexX, uint indexY)
    {
      V[indexX] = (byte) (V[indexX] & V[indexY]);
    }
    /// <summary>
    /// Register value V[x] is set to result of bitwise XOR of V[x] and V[y]
    /// </summary>
    /// <param name="indexX">Index V[x]</param>
    /// <param name="indexY">Index V[y]</param>
    private void SetVxToVxXorVy(uint indexX, uint indexY)
    {
      V[indexX] = (byte) (V[indexX] ^ V[indexY]);
    }

    /// <summary>
    /// Adds value of register V[y] to register V[x] and uses V[0xF] to store the carry flag (1 if overflow occured, else 0)
    /// </summary>
    /// <param name="indexX"></param>
    /// <param name="indexY"></param>
    private void AddRegToOtherRegWithCarry(uint indexX, uint indexY)
    {
      V[0xf] = (byte) (V[indexX] + V[indexY] > 0xff ? 1 : 0);
      V[indexX] += V[indexY];
    }

    /// <summary>
    /// Sets "V[x] -= V[y]" and uses V[0xF] to store the borrow flag (0 if underflow occured, else 1)
    /// </summary>
    /// <param name="indexX"></param>
    /// <param name="indexY"></param>
    private void SetVxToVxMinusVyWithBorrow(uint indexX, uint indexY)
    {
      V[0xf] = (byte) (V[indexX] > V[indexY] ? 1 : 0);
      V[indexX] -= V[indexY];
    }

    /// <summary>
    /// Store the value of register VY shifted right one bit in register VX
    /// Set register V[0xF] to the least significant bit prior to the shift.
    /// ATTENTION: There are different implementations for this method.
    /// Using my interpretation (the description there is ambiguous itself) of the one suggested by "mastering chip-8" (http://mattmik.com/files/chip8/mastering/chip8.html)!
    /// Some games (most newer games, in fact) probably won't work with this implementation. See discussions here: https://www.reddit.com/r/EmuDev/comments/8cbvz6/chip8_8xy6/, (https://www.reddit.com/r/EmuDev/comments/72dunw/chip8_8xy6_help/)
    /// </summary>
    private void OC8XY6(uint indexX, uint indexY)
    {
      V[0xf] = (byte)(V[indexY] & 0x1);
      V[indexX] = (byte) (V[indexY] >> 1);
    }

    /// <summary>
    /// Sets "V[x] = V[y] - V[x]"and uses V[0xF] to store the borrow flag (0 if underflow occured, else 1)
    /// </summary>
    /// <param name="indexX"></param>
    /// <param name="indexY"></param>
    private void SetVxToVyMinusVxWithBorrow(uint indexX, uint indexY)
    {
      V[0xf] = (byte)(V[indexY] > V[indexX] ? 1 : 0);
      V[indexX] = (byte)(V[indexY] - V[indexX]);
    }

    /// <summary>
    /// Store the value of register VY shifted left one bit in register VX
    /// Set register V[0xF] to the most significant bit prior to the shift.
    /// ATTENTION: There are different implementations for this method, just like with 8XY6.
    /// Using my interpretation (the description there is ambiguous itself) of the one suggested by "mastering chip-8" (http://mattmik.com/files/chip8/mastering/chip8.html)!
    /// Some games (most newer games, in fact) probably won't work with this implementation. See discussions here: https://www.reddit.com/r/EmuDev/comments/8cbvz6/chip8_8xy6/, (https://www.reddit.com/r/EmuDev/comments/72dunw/chip8_8xy6_help/)
    /// </summary>
    private void OC8XYE(uint indexX, uint indexY)
    {
      V[0xf] = (byte)(V[indexY] & 0x80);
      V[indexX] = (byte)(V[indexY] << 1);
    }

    /// <summary>
    /// Skips the next instruction if the values of two specific registers are NOT equal.
    /// </summary>
    /// <param name="registerIndex1">Index of first register to compare.</param>
    /// <param name="registerIndex2">Index of second register to compare.</param>
    private void SkipNextInstructionIfRegValNotEqualWithOtherRegVal(uint registerIndex1, uint registerIndex2)
    {
      if (V[registerIndex1] != V[registerIndex2])
        PC += 2;
    }
  }
}
