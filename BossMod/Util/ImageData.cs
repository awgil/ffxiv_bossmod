using Dalamud.Interface.Textures;
using Dalamud.Utility;
using Lumina.Data.Files;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace BossMod;

public readonly record struct ImageData(int Width, int Height, byte[] Data)
{
    public static readonly ImageData Empty = new(0, 0, []);

    public readonly byte[] Data = Data;

    public RawImageSpecification ImageSpecification => RawImageSpecification.Rgba32(Width, Height);

    public ImageData Tint(uint tint, uint bias)
    {
        var output = new byte[Data.Length];

        var inputSpan = MemoryMarshal.Cast<byte, Vector128<byte>>((ReadOnlySpan<byte>)Data);
        var outputSpan = MemoryMarshal.Cast<byte, Vector128<byte>>((Span<byte>)output);

        var biasVec = Vector128.Create(bias).AsByte();
        var addendVec = Vector128.Create(tint).AsByte();
        var subtrahendVec = Vector128.Max(~addendVec, ~biasVec) - ~biasVec;
        addendVec = Vector128.Max(addendVec, biasVec) - biasVec;

        for (var i = 0; i < outputSpan.Length; ++i)
            outputSpan[i] = Vector128.Max(addendVec + Vector128.Min(inputSpan[i], ~addendVec), subtrahendVec) -
                            subtrahendVec;

        return new(Width, Height, output);
    }

    public static ImageData FromTexFile(TexFile tex) => new(tex.Header.Width, tex.Header.Height, tex.GetRgbaImageData());
}
