// Name:
// Submenu:
// Author: mechaskrom
// Title: PS1Blend
// Version:
// Desc: PlayStation 1 blend layers
// Keywords: playstation|psx|ps1|blend|layer
// URL:
// Help:

// For help writing a Bitmap plugin: https://boltbait.com/pdn/CodeLab/help/tutorial/bitmap/

protected override void OnRender(IBitmapEffectOutput output)
{
    //OnRender may be called multiple times with different output bounds because
    //the area to draw is tiled and called multithreaded. So it's important to
    //keep drawing inside the output bounds.

    RectInt32 outputBounds = output.Bounds;
    using IBitmapLock<ColorBgra32> outputLock = output.LockBgra32();
        var outputRegion = outputLock.AsRegionPtr().OffsetView(-outputBounds.Location);

    IReadOnlyList<IBitmapEffectLayerInfo> layers = Environment.Document.Layers;
    //Do all layers below selected layer (this is where the output is drawn).
    for (int i = 0; i < Environment.SourceLayerIndex; i++)
    {
        IBitmapEffectLayerInfo layer = layers.ElementAt(i);

        //Ignore invisible layers.
        if (!layer.Visible) continue;

        Func<ColorBgra32,ColorBgra32,ColorBgra32> blendOp = parseBlendOp(layer.Name);
        IEffectInputBitmap<ColorBgra32> layerBitmap = layer.GetBitmapBgra32();
        using IBitmapLock<ColorBgra32> layerLock = layerBitmap.Lock(layerBitmap.Bounds());
            RegionPtr<ColorBgra32> layerRegion = layerLock.AsRegionPtr();

        for (int y = outputBounds.Top; y < outputBounds.Bottom; y++)
        {
            if (IsCancelRequested) return;

            for (int  x = outputBounds.Left; x < outputBounds.Right; x++)
            {
                // Get your source pixel
                ColorBgra32 srcPixel = layerRegion[x,y];

                if (srcPixel.A < 128) continue; //Ignore low alpha pixels.

                ColorBgra32 dstPixel = outputRegion[x,y];
                ColorBgra32 outputPixel = blendOp(dstPixel, srcPixel);
                outputPixel.A = 255;

                // Save your pixel to the output canvas
                outputRegion[x,y] = outputPixel;
            }
        }
    }
}

private const string TokenOp1 = "**1";
private const string TokenOp2 = "**2";
private const string TokenOp3 = "**3";
private const string TokenOp4 = "**4";
private const string TokenOpS3 = "**s3";

private static Func<ColorBgra32,ColorBgra32,ColorBgra32> parseBlendOp(string layerName)
{
    if (layerName.Contains(TokenOp1)) return blendOp1;
    if (layerName.Contains(TokenOp2)) return blendOp2;
    if (layerName.Contains(TokenOp3)) return blendOp3;
    if (layerName.Contains(TokenOp4)) return blendOp4;
    if (layerName.Contains(TokenOpS3)) return blendOpS3;
    return blendOpCopy;
}

private static ColorBgra32 blendOpCopy(ColorBgra32 dstCol, ColorBgra32 srcCol)
{
    return srcCol;
}

private static ColorBgra32 blendOp1(ColorBgra32 dstCol, ColorBgra32 srcCol)
{
    dstCol.R = blendOp1(dstCol.R, srcCol.R);
    dstCol.G = blendOp1(dstCol.G, srcCol.G);
    dstCol.B = blendOp1(dstCol.B, srcCol.B);
    return dstCol;
}

private static ColorBgra32 blendOp2(ColorBgra32 dstCol, ColorBgra32 srcCol)
{
    dstCol.R = blendOp2(dstCol.R, srcCol.R);
    dstCol.G = blendOp2(dstCol.G, srcCol.G);
    dstCol.B = blendOp2(dstCol.B, srcCol.B);
    return dstCol;
}

private static ColorBgra32 blendOp3(ColorBgra32 dstCol, ColorBgra32 srcCol)
{
    dstCol.R = blendOp3(dstCol.R, srcCol.R);
    dstCol.G = blendOp3(dstCol.G, srcCol.G);
    dstCol.B = blendOp3(dstCol.B, srcCol.B);
    return dstCol;
}

private static ColorBgra32 blendOp4(ColorBgra32 dstCol, ColorBgra32 srcCol)
{
    dstCol.R = blendOp4(dstCol.R, srcCol.R);
    dstCol.G = blendOp4(dstCol.G, srcCol.G);
    dstCol.B = blendOp4(dstCol.B, srcCol.B);
    return dstCol;
}

private static ColorBgra32 blendOpS3(ColorBgra32 dstCol, ColorBgra32 srcCol)
{
    dstCol.R = blendOpS3(dstCol.R, srcCol.R);
    dstCol.G = blendOpS3(dstCol.G, srcCol.G);
    dstCol.B = blendOpS3(dstCol.B, srcCol.B);
    return dstCol;
}

//The PlayStation 1 has four blend modes/operations.
//The calculation done per color channel (RGB) for each mode is as follows:
// 1:   N = B>>1 + F>>1   (average, 0.5B + 0.5F)
// 2:   N = B + F         (add)
// 3:   N = B - F         (subtract)
// 4:   N = B + F>>2      (add 1/4, B + 0.25F)
//
//'B'  = value already in the framebuffer
//'F'  = value being written to the framebuffer
//'N'  = resulting value in the framebuffer (clamped to 0-31, 5-bit)
//'>>' = bit shift right operator
//B, F and N are 5-bit so mask with 0xF8 (5-bit expanded to 8-bit).

private static byte blendOp1(int b, int f)
{
    b &= 0xF8;
    f &= 0xF8;
    return blendOpClip((b >> 1) + (f >> 1));
}

private static byte blendOp2(int b, int f)
{
    b &= 0xF8;
    f &= 0xF8;
    return blendOpClip(b + f);
}

private static byte blendOp3(int b, int f)
{
    b &= 0xF8;
    f &= 0xF8;
    return blendOpClip(b - f);
}

private static byte blendOp4(int b, int f)
{
    b &= 0xF8;
    f &= 0xF8;
    return blendOpClip(b + (f >> 2));
}

private static byte blendOpS3(int b, int f)
{
    //1.0 * B - 1.0 * F inverted
    //Special variant of op3 to make blending ripped layers using op3 easier.
    //This assumes that 'F' was ripped with a white (248 to 255) background.
    b &= 0xF8;
    f &= 0xF8;
    return blendOpClip(b - (248 - f));

    //Something similar is not needed for op 2 and 4 (just rip with a black background).
    //Op 1 would need it, but because data may be lost if the calculation is done in two
    //separate steps it's not possible? E.g. with values 24 and 136.
    //Normal op1:
    //   ((24 & 0xF8) >> 1) + ((136 & 0xF8) >> 1) = 12 + 68 = 80 -> 80 after 0xF8 masking.
    //
    //Special op1, ripping 'F' layer separately with black (0) background:
    //   ((0 & 0xF8) >> 1) + ((136 & 0xF8) >> 1) = 0 + 68 = 68 -> 64 after 0xF8 masking. Uh oh! Data lost!
    //Then blend the ripped 'F' layer with 'B':
    //   ((24 & 0xF8) >> 1) + 64 = 12 + 64 = 76 -> 72 after 0xF8 masking. Wrong!, correct value was 80.
}

private static byte blendOpClip(int v)
{
    if (v < 0) v = 0; else if (v > 255) v = 255;
    return (byte)(v & 0xF8);
}
