using System;

namespace Chip8
{
  public class OpCode
  {
    /// <summary>
    /// Full opcode as hex
    /// </summary>
    public readonly string Hex;

    /// <summary>
    /// First digit of the opcode in hexadecimal
    /// </summary>
    public readonly char FirstDigitHex;

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
      FirstDigitHex = Hex[0];
    }
  }
}
