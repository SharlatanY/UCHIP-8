﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityUtilities.GlobalHelpers.Paths;
using Random = System.Random;

namespace Chip8
{
  public class Chip8 : MonoBehaviour
  {
    //Output
    public AudioSource AudioSource;
    public Texture2D OutputTexture; //Texture the chip 8 emulator will render to

    /// <summary>
    /// If this is set to true, whenever a pixel would be drawn outside the screen (can only happen to the right and to the
    /// bottom since the min index is 0 and the coordinate 0/0 is in the upper left corner), the pixel will be "wrapped around".
    /// Example: max x index is 63 but pixel draw index is 64 -> pixel will be drawn at 0.
    /// </summary>
    private const bool DrawOverflowWrapAround = true;
    private bool[,] _virtualScreen;

    //Registers
    private readonly byte[] V = new byte[16];
    private ushort I; //memory address register

    //RAM
    private readonly byte[] RAM = new byte[4096]; //0x0 to 0xFFF
    private const ushort FontStartingAddress = 0x0;

    //Program counter
    private ushort _pcStartingAddress = 0x200;
    private ushort PC; //stores current execution address

    //Tick/CPU speed/timers
    private byte _delayTimer;

    private float _clockInHz = 500;
    private const float DelayTimerClockInHz = 60;
    private float _instructionTickIntervalInS;
    private const float DelayTimerTickIntervalInS = 1 / DelayTimerClockInHz;
    private const bool LegacyMode = true;

    /// <summary>
    /// Time reminder from last update call that wasn't enough for a "full tick".
    /// Example: Tick time is 3ms and between two Update() calls, 7ms pass.
    /// Two ticks will be execute (2x3ms =6ms) and the reminder will be 1ms (7ms-6ms).
    /// 5ms pass until the next Update() call but we'll now also add the reminder (5ms + 1ms from reminder)
    /// -> passed time is 6ms, two ticks are executed.
    /// This way, we get more consistent speed
    /// </summary>
    private float _instructionTickReminder;

    private float _delayTimerTickReminder;

    //Misc
    private readonly Random _random = new Random();
    private readonly Stack<ushort> _stack = new Stack<ushort>();
    private string _romPath = Path.Combine(GlobalPaths.FixedStreamingAssetPath, "rom.ch8");
    //private string _romPath = Path.Combine(GlobalPaths.FixedStreamingAssetPath, "BC_test.ch8");
    //private string _romPath = Path.Combine(GlobalPaths.FixedStreamingAssetPath, "pong.ch8");
    //private string _romPath = Path.Combine(GlobalPaths.FixedStreamingAssetPath, "brix.ch8");
    //private string _romPath = Path.Combine(GlobalPaths.FixedStreamingAssetPath, "test_opcode.ch8");

    private int _screenWidth, _screenHeight;

    private Color[] _outputTextureResetColorArray;

    private void Start()
    {
      _screenWidth = OutputTexture.width;
      _screenHeight = OutputTexture.height;
      _outputTextureResetColorArray = new Color[_screenWidth * _screenHeight];
      for (var i = 0; i < _outputTextureResetColorArray.Length; i++)
      {
        _outputTextureResetColorArray[i] = Color.black;
      }

      Reset();
    }

    private void Update()
    {
      float passedTime;

      //update delay timer
      if (_delayTimer > 0)
      {
        passedTime = Time.deltaTime + _delayTimerTickReminder;
        var numToSubtractFromDelay = (int)(passedTime / DelayTimerTickIntervalInS);
        _delayTimerTickReminder = passedTime % DelayTimerTickIntervalInS;

        _delayTimer = (byte)(_delayTimer - numToSubtractFromDelay < 0 ? 0 : _delayTimer - numToSubtractFromDelay);
      }

      //calculate how many ticks to execute
      passedTime = Time.deltaTime + _instructionTickReminder;
      var numTicksToExecute = (int)(passedTime / _instructionTickIntervalInS);
      _instructionTickReminder = passedTime % _instructionTickIntervalInS;

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
      _virtualScreen = new bool[_screenWidth, _screenHeight];

      InitializeRAM();
      PC = _pcStartingAddress;
      I = 0x0;

      _delayTimer = 0x0;
      _instructionTickIntervalInS = 1 / _clockInHz;
      _instructionTickReminder = 0;
      _delayTimerTickReminder = 0;

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
      var binary = File.ReadAllBytes(_romPath);
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
      var opCode = new OpCode(leftByte, rightByte);

      // move program counter to next instruction to be executed on next tick (might be overriden if instruction turns out to be a jump or function call)
      PC += 2; //Instructions are two bytes long, so we need to move two bytes go get to the next instruction

      // execute instruction
      ExecuteOpCode(opCode);
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
              AddRegToOtherReg(opCode.X, opCode.Y);
              break;
            case "5":
              SetVxToVxMinusVy(opCode.X, opCode.Y);
              break;
            case "6":
              OC8XY6(opCode.X, opCode.Y);
              break;
            case "7":
              SetVxToVyMinusVxWithBorrow(opCode.X, opCode.Y);
              break;
            case "E":
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
        case "A":
          SetI(opCode.NNN);
          break;
        case "B":
          JumpToAddressPlusV0(opCode.NNN);
          break;
        case "C":
          OCCXNN(opCode.X, opCode.NN);
          break;
        case "D":
          DrawSprite(opCode.X, opCode.Y, opCode.N);
          break;
        case "E":
          switch (opCode.LastTwoDigitsHex)
          {
            case "9E":
              SkipIfKeyPressed(opCode.X);
              break;
            case "A1":
              SkipIfKeyNotPressed(opCode.X);
              break;
            default:
              opCodeInvalid = true;
              break;
          }
          break;
        case "F":
          #region handle case F
          switch (opCode.LastTwoDigitsHex)
          {
            case "07":
              SetRegToDelayTimer(opCode.X);
              break;
            case "0A":
              WaitForKeyDownAndStoreInReg(opCode.X);
              break;
            case "15":
              SetDelayTimerToReg(opCode.X);
              break;
            case "18":
              PlaySound(opCode.X);
              break;
            case "1E":
              AddRegValToI(opCode.X);
              break;
            case "29":
              SetIToCharacterSpriteAddress(opCode.X);
              break;
            case "33":
              OCFX33(opCode.X);
              break;
            case "55":
              WriteRegsToMemory(opCode.X);
              break;
            case "65":
              WriteMemoryToRegs(opCode.X);
              break;
            default:
              opCodeInvalid = true;
              break;
          }
          break;
        #endregion
        default:
          opCodeInvalid = true;
          break;
      }

      if (opCodeInvalid)
      {
        throw new ArgumentException($"Invalid OpCode: '{opCode.Hex}' at address {PC - 2}", nameof(opCode)); //need to reduce PC by two to get actual address because PC was already directly set to address that will have to be read next
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
      //no need to increase program counter after pop because on push, when address to return to is stored from PC, PC has already been increased
      PC = (ushort)(_stack.Pop());
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
      V[indexX] = (byte)(V[indexX] | V[indexY]);
    }

    /// <summary>
    /// Register value V[x] is set to result of bitwise AND of V[x] and V[y]
    /// </summary>
    /// <param name="indexX">Index V[x]</param>
    /// <param name="indexY">Index V[y]</param>
    private void SetVxToVxAndVy(uint indexX, uint indexY)
    {
      V[indexX] = (byte)(V[indexX] & V[indexY]);
    }
    /// <summary>
    /// Register value V[x] is set to result of bitwise XOR of V[x] and V[y]
    /// </summary>
    /// <param name="indexX">Index V[x]</param>
    /// <param name="indexY">Index V[y]</param>
    private void SetVxToVxXorVy(uint indexX, uint indexY)
    {
      V[indexX] = (byte)(V[indexX] ^ V[indexY]);
    }

    /// <summary>
    /// Adds value of register V[y] to register V[x] and uses V[0xF] to store the carry flag (1 if overflow occured, else 0)
    /// </summary>
    /// <param name="indexX"></param>
    /// <param name="indexY"></param>
    private void AddRegToOtherReg(uint indexX, uint indexY)
    {
      V[0xf] = (byte)(V[indexX] + V[indexY] > 0xff ? 1 : 0);
      V[indexX] += V[indexY];
    }

    /// <summary>
    /// Sets "V[x] -= V[y]" and uses V[0xF] to store the borrow flag (0 if underflow occured, else 1)
    /// </summary>
    /// <param name="indexX"></param>
    /// <param name="indexY"></param>
    private void SetVxToVxMinusVy(uint indexX, uint indexY)
    {
      V[0xf] = (byte)(V[indexX] > V[indexY] ? 1 : 0);
      V[indexX] -= V[indexY];
    }

    /// <summary>
    /// Legacy Mode:
    /// Store the value of register VY shifted right one bit in register VX
    /// Set register V[0xF] to the least significant bit of VY.
    /// Modern Mode:
    /// Store the value of register VX shifted right one bit in register VX
    /// Set register V[0xF] to the least significant bit of VX prior to the shift.
    /// Attention:
    /// ATTENTION: There are different documentations and implementations out there for this method.
    /// Depending on <see cref="LegacyMode"/>, different code will be executed when calling this function
    /// Some games (most newer games, in fact) probably won't work with the legacy implementation and vice versa. See discussions here: https://www.reddit.com/r/EmuDev/comments/8cbvz6/chip8_8xy6/, (https://www.reddit.com/r/EmuDev/comments/72dunw/chip8_8xy6_help/)
    /// </summary>
    private void OC8XY6(uint indexX, uint indexY)
    {
      if (LegacyMode)
      {
        V[0xf] = (byte)(V[indexY] & 0x1);
        V[indexX] = (byte)(V[indexY] >> 1);
      }
      else
      {
        V[0xf] = (byte)(V[indexX] & 0x1);
        V[indexX] = (byte)(V[indexX] >> 1);
      }
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
    /// Legacy Mode:
    /// Store the value of register VY shifted left one bit in register VX
    /// Set register V[0xF] to the most significant bit of VY
    /// Modern Mode:
    /// Store the value of register VX shifted left one bit in register VX
    /// Set register V[0xF] to the most significant bit of VX prior to the shift.
    /// ATTENTION: There are different implementations for this method, just like with 8XY6.
    /// Depending on <see cref="LegacyMode"/>, different code will be executed when calling this function
    /// Some games (most newer games, in fact) probably won't work with the legacy implementation and vice versa. See discussions here: https://www.reddit.com/r/EmuDev/comments/8cbvz6/chip8_8xy6/, (https://www.reddit.com/r/EmuDev/comments/72dunw/chip8_8xy6_help/)
    /// </summary>
    private void OC8XYE(uint indexX, uint indexY)
    {
      if (LegacyMode)
      {
        V[0xf] = (byte)((V[indexY] & 0x80) >> 7);
        V[indexX] = (byte)(V[indexY] << 1);
      }
      else
      {
        V[0xf] = (byte)((V[indexX] & 0x80) >> 7);
        V[indexX] = (byte)(V[indexX] << 1);
      }
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

    /// <summary>
    /// Sets the value of the I (memory address) register.
    /// </summary>
    /// <param name="value"></param>
    private void SetI(ushort value)
    {
      I = value;
    }

    /// <summary>
    /// Takes a memory address and sets the program counter to said address plus the value saved in register V[0]
    /// </summary>
    /// <param name="address"></param>
    private void JumpToAddressPlusV0(ushort address)
    {
      PC = (ushort)(address + V[0]);
    }

    /// <summary>
    /// Sets VX to the result of a bitwise and operation on a random number (Typically: 0 to 255) and NN. 
    /// </summary>
    /// <param name=""></param>
    private void OCCXNN(uint registerIndex, byte nn)
    {
      V[registerIndex] = (byte)(_random.Next(16) & nn);
    }

    /// <summary>
    /// Draws a sprite at coordinate (V[registerIndexX], V[registerIndexY]) that has a width of 8 pixels and a height of n pixels.
    /// To get the sprite rows, n bytes of data are read starting from memory address stored in register I;
    /// If any set pixels are unset by this operation, V[F] is set to 1, else it will be set to 0.
    /// If <see cref="DrawOverflowWrapAround"/> is set to true, any pixels that would be drawn outside the screen will be wrapped
    /// around. If it is set to false, they simply won't be drawn at all.
    /// </summary>
    /// <param name="registerIndexX"></param>
    /// <param name="registerIndexY"></param>
    /// <param name="n"></param>
    private void DrawSprite(uint registerIndexX, uint registerIndexY, byte n)
    {
      // this method is written very inefficiently and could use a lot of optimization
      // since it's just a small side project/proof of concept, I'll leave it as is as
      var xUpperLeft = (uint)V[registerIndexX];
      var yUpperLeft = (uint)V[registerIndexY];

      if (DrawOverflowWrapAround)
      {
        // handle potential overflows for start drawing location
        xUpperLeft %= (uint)_screenWidth;
        yUpperLeft %= (uint)_screenHeight;
      }
      

      var pixelUnset = false;
      var baseAddress = I;
      for (var i = 0; i < n; i++)
      {
        var y = (int)yUpperLeft + i;

        var spriteLine = new BitArray(new[] { RAM[baseAddress + i] });
        var maxIndex = spriteLine.Length - 1;
        for (var j = 0; j <= maxIndex; j++)
        {
          var x = (int)xUpperLeft + j;
          if(DrawOverflowWrapAround)
            x %= _screenWidth; //handle overflow

          if (x < _screenWidth && y < _screenHeight) //can only happen if draw wrapp around is not activated
          {
            //The array created by BitArray() orders the bits from least to most significant bit.
            //Since we draw from left to right and have big endianness though, we have to draw
            //the most significant bit first and then descend until we reach the least significant one.
            //That's why we use spriteLine[maxIndex -j]
            var oldPixelVal = _virtualScreen[x, y];
            var newPixelVal = spriteLine[maxIndex - j] ^ oldPixelVal;
            _virtualScreen[x, y] = newPixelVal;

            var color = newPixelVal ? Color.white : Color.black;
            OutputTexture.SetPixel(x, y, color); //improvement idea: could first store all pixel changes in _virtualScreen, then set all pixels at once with OutputTexture.SetPixels(..) so we only have one call per sprite to set pixels on the texture

            if (oldPixelVal == true && newPixelVal == false)
              pixelUnset = true;
          }
        }

        OutputTexture.Apply();
        V[0xf] = (byte)(pixelUnset ? 1 : 0);
      }
    }

    /// <summary>
    /// Skips the next instruction if key stored in register V[registerIndex] is currently pressed.
    /// </summary>
    /// <param name="registerIndex">Key code in hex.</param>
    private void SkipIfKeyPressed(uint registerIndex)
    {
      var key = V[registerIndex].ToString("X");
      //potential bug? because this returns true as long as the button is pressed, so, when we execute multiple ticks in short succession that all check for a certain button, the press might be registered for all of them.
      //but that's what was in the specification I found...
      if (Input.GetButton(key.ToUpper()))
        PC += 2;
    }

    /// <summary>
    /// Skips the next instruction if key stored in register V[registerIndex] is currently NOT pressed.
    /// </summary>
    /// <param name="registerIndex">Key code in hex.</param>
    private void SkipIfKeyNotPressed(uint registerIndex)
    {
      var key = V[registerIndex].ToString("X");
      //potential bug? because this returns true as long as the button is pressed, so, when we execute multiple ticks in short succession that all check for a certain button, the press might be registered for all of them.
      //but that's what was in the specification I found...
      if (!Input.GetButton(key.ToUpper()))
        PC += 2;
    }

    /// <summary>
    /// Sets the value of register V[registerIndex] to the current delay timer value
    /// </summary>
    /// <param name="registerIndex"></param>
    private void SetRegToDelayTimer(uint registerIndex)
    {
      V[registerIndex] = _delayTimer;
    }

    /// <summary>
    /// Waits for a key to be pressed down and stores the key in register V[registerIndex].
    /// Until a key is pressed down, no other instructions will be executed.
    /// </summary>
    //potential bug 1: might also be that this should check for "key pressed" instead of "key down". Documentation unclear
    //potential bug 2: I didn't find a place that explicitly stated of timers like the delay timer should be paused as well. Major consensus in forums etc. seems to be "probably" not. 
    private void WaitForKeyDownAndStoreInReg(uint registerIndex)
    {
      var pressedKeys = GetKeysJustPressed();
      if (pressedKeys.Any())
      {
        var key = pressedKeys.First();
        V[registerIndex] = Convert.ToByte(Convert.ToByte($"0x{key}", 16));
      }
      else
      {
        //Still waiting for any key do be pressed down. Set PC back so this instruction will be executed again in the next tick.
        PC -= 2;
      }
    }

    /// <summary>
    /// Sets the value of the delay timer to the value stored in register V[registerIndex]
    /// </summary>
    /// <param name="registerIndex"></param>
    private void SetDelayTimerToReg(uint registerIndex)
    {
      _delayTimer = V[registerIndex];
    }

    /// <summary>
    /// Plays a sound for "1/60 * V[registerIndex]" seconds
    /// </summary>
    private void PlaySound(uint registerIndex)
    {
      var playDuration = 1f / 60f * V[registerIndex];
      AudioSource.time = 0;
      AudioSource.Play();
      AudioSource.SetScheduledEndTime(AudioSettings.dspTime + playDuration);
    }

    /// <summary>
    /// Adds the value stored in register V[<paramref name="registerIndex"/>] to value stored in address register I.
    /// If an overflow occurs, V[0xF] will be set to 1, else to 0.
    /// </summary>
    /// <param name="registerIndex"></param>
    private void AddRegValToI(uint registerIndex)
    {
      V[0xF] = (byte)((V[registerIndex] + I) > 0xFFFF ? 1 : 0);
      I += V[registerIndex];
    }

    /// <summary>
    /// Sets the value of address register I to the address off the sprite for the hex character stored in register V[registerIndex]
    /// </summary>
    /// <param name="registerIndex"></param>
    private void SetIToCharacterSpriteAddress(uint registerIndex)
    {
      var charNumber = V[registerIndex];

      I = (ushort)(FontStartingAddress + charNumber * 5); //*5 because every character sprite is made up out of 5 bytes.
    }

    /// <summary>
    /// Stores the binary-coded decimal representation of V[registerIndex], with the most significant of three digits at the address in I, 
    /// the middle digit at I plus 1, and the least significant digit at I plus 2. (In other words, take the decimal representation of VX, 
    /// place the hundreds digit in memory at location in I, the tens digit at location I+1, and the ones digit at location I+2.) 
    /// </summary>
    /// <param name="registerIndex"></param>
    private void OCFX33(uint registerIndex)
    {
      var valToEncode = V[registerIndex];
      RAM[I] = (byte)((valToEncode / 100) % 10);
      RAM[I + 1] = (byte)((valToEncode / 10) % 10);
      RAM[I + 2] = (byte)(valToEncode % 10);
    }

    /// <summary>
    /// Write all register values from register V[0] to (including) V[registerIndex] starting at address stored in address register I.
    /// I won't be modified.
    /// </summary>
    /// <param name="maxRegisterIndex"></param>
    private void WriteRegsToMemory(uint maxRegisterIndex)
    {
      for (var i = 0; i <= maxRegisterIndex; i++)
        RAM[I + i] = V[i];

      //potential bug: some documentations say that I should be set to I + registerIndex + 1 at the end of the operations, others say I should NOT be modified.
      //since more sources seem to indicate it should not be modified, I'm going with that
    }

    /// <summary>
    /// Fills registers V[0] to (including) V[registerIndex] with RAM data starting at address stored in address register I.
    /// I won't be modified.
    /// </summary>
    /// <param name="maxRegisterIndex"></param>
    private void WriteMemoryToRegs(uint maxRegisterIndex)
    {
      for (var i = 0; i <= maxRegisterIndex; i++)
        V[i] = RAM[I + i];

      //I = (ushort) (I + maxRegisterIndex + 1);
      //potential bug: some documentations say that I should be set to I + registerIndex + 1 at the end of the operations, others say I should NOT be modified.
      //since more sources seem to indicate it should not be modified, I'm going with that
    }

    /// <summary>
    /// Returns all keys that have been pressed down since the last update() call (= exclude keys that have already been pressed down for a longer time).
    /// </summary>
    /// <returns></returns>
    private string[] GetKeysJustPressed()
    {
      var newlyPressedKeys = new List<string>();
      foreach (var key in Chip8Constants.AvailableKeys)
      {
        if (Input.GetButtonDown(key))
          newlyPressedKeys.Add(key);
      }

      return newlyPressedKeys.ToArray();
    }
  }
}
