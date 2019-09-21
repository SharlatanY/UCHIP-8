# Known Issues
## Opcodes with multiple, different implementations 
There are some CHIP-8 opcodes that have multiple definitions/whose definitions changed over time.
I'm trying to always implement what I believe to be oldest/first known implementation (it's really not easy to figure out what the "original" implementation was with the resources Google brings up).
However, since a lot of newer games also rely on newer implementations of those opcodes, a lot of newer games will not work with this emulator.

Affected opcodes, as far as I could gather, are:

 - 8XY6
 - 8XYE
 - FX55
 - FX65

For further information see:

 - https://www.reddit.com/r/EmuDev/comments/8cbvz6/chip8_8xy6/
 - https://www.reddit.com/r/EmuDev/comments/72dunw/chip8_8xy6_help/