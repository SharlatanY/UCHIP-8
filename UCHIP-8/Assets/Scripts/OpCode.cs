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
    /// The last three bytes of the opcode as ushort.
    /// </summary>
    public readonly ushort NNN;

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
      NNN = Convert.ToUInt16($"0x{Hex.Substring(1)}",16);
    }
  }
}
