﻿using Chip8;
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
    }
}
