# PS1Blend
Color blend layers like the Sony PlayStation 1 game console (PSX). PSX has four blend modes/operations. The calculation done per color channel (RGB) for each mode is as follows.
```
1:   N = B>>1 + F>>1   (average, 0.5B + 0.5F)
2:   N = B + F         (add)
3:   N = B - F         (subtract)
4:   N = B + F>>2      (add 1/4, B + 0.25F)

'B'  = value already in the framebuffer
'F'  = value being written to the framebuffer
'N'  = resulting value in the framebuffer (clamped to 0-31, 5-bit)
'>>' = bit shift right operator
```
Blend mode is specified by adding special tokens (**1, **2, **3 or **4) to a layer's name e.g. "Layer2 **3". Only visible layers below the selected layer are blended. The result of running the plugin is drawn to the selected layer.
Pixels with alpha < 128 in blended layers are ignored. Other alpha values are treated as fully opaque (alpha=255).

## Installation
Run the bat-file or place the plugin's dll-file in the Effects-folder in the Paint.NET installation folder.

## Example Output
![example](https://github.com/mechaskrom/PaintNet-PS1Blend/blob/main/example.png)
