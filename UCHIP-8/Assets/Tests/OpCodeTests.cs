using Chip8;
using NUnit.Framework;
namespace Tests
{
  public class OpCodeTests
  {
    [Test]
    public void Hex_Ok()
    {
      var opCode = new OpCode(0xb5, 0x24);
      Assert.AreEqual("B524", opCode.Hex);
    }

    [Test]
    public void FirstDigitHex_Ok()
    {
      var opCode = new OpCode(0xb5, 0x24);
      Assert.AreEqual("B", opCode.FirstDigitHex);
    }

    [Test]
    public void LastTwoDigitsHex_Ok()
    {
      var opCode = new OpCode(0xb5, 0x24);
      Assert.AreEqual("24", opCode.LastTwoDigitsHex);
    }

    [Test]
    public void LastDigitHex_Ok()
    {
      var opCode = new OpCode(0xb5, 0x24);
      Assert.AreEqual("4", opCode.LastDigitHex);
    }

    [Test]
    public void NNN_Ok()
    {
      var opCode = new OpCode(0x1C, 0x24);
      Assert.AreEqual(3108, opCode.NNN);
    }

    [Test]
    public void NN_Ok()
    {
      var opCode = new OpCode(0x33, 0x24);
      Assert.AreEqual(0x24, opCode.NN);
    }

    [Test]
    public void N_Ok()
    {
      var opCode = new OpCode(0xD3, 0xAC);
      Assert.AreEqual(0xC, opCode.N);
    }

    [Test]
    public void X_Ok()
    {
      var opCode = new OpCode(0x3C, 0x24);
      Assert.AreEqual(12, opCode.X);
    }

    [Test]
    public void Y_Ok()
    {
      var opCode = new OpCode(0x3C, 0xD4);
      Assert.AreEqual(13, opCode.Y);
    }
  }
}
