using System;

namespace Chip8
{
  public class OpCode
  {
    /// <summary>
    /// Full opcode as hex. ATTENTION: Hex chars will be in upper case!
    /// </summary>
    public readonly string Hex;

    /// <summary>
    /// First digit of the opcode in hexadecimal. ATTENTION: Hex chars will be in upper case!
    /// </summary>
    public readonly string FirstDigitHex;

    /// <summary>
    /// Last digit of the opcode in hexadecimal. ATTENTION: Hex chars will be in upper case!
    /// </summary>
    public readonly string LastDigitHex;

    /// <summary>
    /// The last three nibbles/hex characters of the opcode as ushort.
    /// </summary>
    public readonly ushort NNN;

    /// <summary>
    /// The last byte/two hex characters of the opcode as byte.
    /// </summary>
    public readonly byte NN;

    /// <summary>
    /// The last nibble/hex characters of the opcode as byte (so, 4 most significant bits will always be 0).
    /// </summary>
    public readonly byte N;

    /// <summary>
    /// The value of the second hex character of the opcode.
    /// </summary>
    public readonly uint X;

    /// <summary>
    /// The value of the third hex character of the opcode.
    /// </summary>
    public readonly uint Y;

    /// <summary>
    /// Expects the two bytes that make up the op code.
    /// OpCode expected to be represented in big endian.
    /// </summary>
    /// <param name="leftByte"></param>
    /// <param name="rightByte"></param>
    public OpCode(byte leftByte, byte rightByte)
    {
      var bytes = new byte[] {leftByte, rightByte};
      Hex = BitConverter.ToString(bytes).Replace("-", string.Empty); ;
      FirstDigitHex = Hex[0].ToString();
      LastDigitHex = Hex[3].ToString();
      NNN = Convert.ToUInt16($"0x{Hex.Substring(1)}",16);
      NN = Convert.ToByte($"0x{Hex.Substring(2)}", 16);
      N = Convert.ToByte($"0x{Hex.Substring(3)}", 16);
      X = (uint) Convert.ToInt16($"0x{Hex.Substring(1, 1)}", 16);
      Y = (uint) Convert.ToInt16($"0x{Hex.Substring(2, 1)}", 16);
    }
  }
}
