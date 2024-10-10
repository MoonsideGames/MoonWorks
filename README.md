# MoonWorks

MoonWorks is a free cross-platform game development framework. Its implementation is heavily inspired by [FNA](https://github.com/FNA-XNA/FNA), which is itself a free implementation of the Microsoft XNA API.

MoonWorks wraps native-code implementations in managed C# for clean high-level abstractions that still run fast. It's simple and it tries to stay out of your way as much as possible.

MoonWorks *does* provide the components you need to implement a functioning game: window management, input, graphics, 3D math utilities, and audio.

MoonWorks *does not* include things like a built-in physics engine, a GUI editor, or a standard rendering pipeline. These decisions and tools are better made per-team or per-project. In short, if you don't mind learning what a vertex buffer is, this framework is for you.

MoonWorks uses strictly Free Open Source Software. It will never have any kind of dependency on proprietary products.

## Documentation

API Reference: https://moonside.games/docs/moonworksapi/

High-level documentation is provided here: https://moonside.games/docs/moonworks/

The source is documented in doc comments that your preferred IDE can read.

Join our Discord! https://discord.gg/ujhwdkHmhN

## Dependencies

* [SDL3](https://github.com/flibitijibibo/SDL3-CS) - Window management, Input, Graphics
* [IRO](https://github.com/MoonsideGames/IRO) - Image Loading
* [FAudio](https://github.com/FNA-XNA/FAudio) - Audio
* [Wellspring](https://github.com/MoonsideGames/Wellspring) - Font Rendering
* [dav1dfile](https://github.com/MoonsideGames/dav1dfile) - Compressed Video

Prebuilt dependencies can be obtained here: https://moonside.games/files/moonlibs.tar.bz2

## License

MoonWorks is released under the zlib license. See LICENSE for details.

MoonWorks uses code from the FNA project, released under the Microsoft Public License. See fna.LICENSE for details. By extension it uses code from the Mono.Xna project, released under the MIT license. See monoxna.LICENSE for details.
