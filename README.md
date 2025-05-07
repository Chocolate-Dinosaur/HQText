# HQText
An easy to use plugin for Unity that renders complex text accurately.

![header-795b83534ca1dd3744757b4dd9b0303b](https://github.com/user-attachments/assets/4c28aa57-9e9e-49a3-b31f-94bdb38ffef4)

HQText is a Unity plugin developed in response to the growing need for accurate and comprehensive language support, including precise placement of diacritics, ligatures, and the proper handling of mixed bi-directional text elements. Although our primary focus has been on providing correct support for Arabic text within Unity, HQText is designed to accommodate most other languages just as effectively.

To achieve this, HQText employs a user-friendly and intuitive wrapper for Pango, an open-source library for laying out and rendering text. By default, HQText utilizes FreeType for font rendering, but it can also be configured to work with the native Windows (win32) backend.

While TextMeshPro is another option for supporting Arabic, it often experiences issues with diacritic placement, which can vary between fonts, and transparency artifacts arising from overlapping character ligatures. For those who find these problems unacceptable, HQText serves as an ideal alternative.

[Download the Unity plugin.](https://github.com/Chocolate-Dinosaur/HQText/releases)

## Features
- Accurate rendering of complex text
- Supports TTF and OTF fonts
- Arabic, Persian/Farsi, Hebrew, Urdu and more
- Correct rendering of diacritics and ligatures
- Markup (Rich text) support
- Supports ALL the render-pipelines: Built-in/URP/HDRP
- Also works in editor (no need to enter play mode)
- Supports visual effects via [UIFX](https://www.chocdino.com/products/uifx/bundle/about/)
- Includes full C# and C++ source code

## Requirements
- Supports all Unity versions from 2019.4.0 up to Unity 6.x
- Supports all render pipelines: Built-in, URP and HDRP
- 64-Bit Windows only (Windows 10 & 11)
- (other platform support on request)

## Dependencies
This work is made possible by [Pango](https://docs.gtk.org/Pango/), licensed under LGPL-2.1-or-later. Authors:  Owen Taylor, Behdad Esfahbod.

## Contributors

### Authors / Maintainers
- Shane Marks
- [Chocolate Dinosaur Ltd](https://www.chocdino.com/products/hqtext/about/)

### Contributors
- Kelly McCarter, Denzil Büchner, Ruan Moolman, Calvin Lichungu, Andrew Griffiths

### Additional Contributors
- Dr. Mustapha Saidi, for creating the Arabic text references.

## Links
- [Download the Unity plugin](https://github.com/Chocolate-Dinosaur/HQText/releases)
- [Visit the website](https://www.chocdino.com/products/hqtext/about/) for more information.

## Building

See [BUILD.md](BUILD.md) for build instructions.

## Support & Requests
Contact [Chocolate Dinosaur Ltd](https://www.chocdino.com/contact/) for more information.
