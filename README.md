# CHIP-8
![](https://raw.githubusercontent.com/SharlatanY/UCHIP-8/master/Docs/Img/header.jpg)
 Usage
 ---

 1. Select ROM by putting your ROM into subfolder *Chip8_Data\StreamingAssets\* and rename it to "rom.ch8" (case matters!)
 2. Start program

FAQ
---
**Q:** How do I configure the controls?
**A:** Controls can be configured at the start screen. What game uses which buttons in the end is up to the game.
The standard buttons used for the game coming with the emulator (Brix) are "Q" and "E"

**Q:** How do I reset the game?
**A:** Unfortunately, you currently have to close and restart the program.

Known Issues
---
### Some graphics flicker!
Some graphics flicker heavily, e.g. the players and balls in "Brix" or "Pong". This isn't a bug but a consequence of how Chip-8 works. This behavior is also present in "real", original versions of CHIP-8

### Opcodes with multiple, different implementations 
There are some CHIP-8 opcodes that have multiple definitions/whose definitions changed over time.
I'm trying to always implement what I believe to be oldest/first known implementation (it's really not easy to figure out what the "original" implementation was with the resources Google brings up).
For some opcodes I have already also implemented the new version but the flag, which codes to read can currently only be set when compiling the code yourself.

Since a lot of newer games also rely on newer implementations of those opcodes, a lot of newer games will not yet work with this emulator.

Affected opcodes, as far as I could gather, are:

 - 8XY6 (new version implemented as wellbut disabled)
 - 8XYE (new version implemented as well but disabled)
 - FX55
 - FX65

For further information see:

 - https://www.reddit.com/r/EmuDev/comments/8cbvz6/chip8_8xy6/
 - https://www.reddit.com/r/EmuDev/comments/72dunw/chip8_8xy6_help/