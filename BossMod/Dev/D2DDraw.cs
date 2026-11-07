#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using SharpDX.Direct3D;
using System.Runtime.InteropServices;
using d2 = SharpDX.Direct2D1;
using d3 = SharpDX.Direct3D11;
using dx = SharpDX.DXGI;

namespace BossMod.Dev;

public sealed class D2DDraw : IDisposable
{
    public readonly d3.Device Device;
    private d3.Texture2DDescription Description;
    public readonly d2.DeviceContext D2DContext;

    public D2DDraw()
    {
        Device = new(DriverType.Hardware, d3.DeviceCreationFlags.BgraSupport);
        Description = new()
        {
            ArraySize = 1,
            MipLevels = 1,
            BindFlags = d3.BindFlags.RenderTarget | d3.BindFlags.ShaderResource,
            Width = 0,
            Height = 0,
            Format = dx.Format.B8G8R8A8_UNorm,
            SampleDescription = new(1, 0)
        };

        var dxgi = Device.QueryInterface<dx.Device>();
        var factory = new d2.Factory1();
        var d2dDevice = new d2.Device(factory, dxgi);

        D2DContext = new d2.DeviceContext(d2dDevice, d2.DeviceContextOptions.None)
        {
            AntialiasMode = d2.AntialiasMode.Aliased
        };
    }

    public IDalamudTextureWrap Draw(int width, int height, bool antialias, Action<d2.DeviceContext> drawFunc)
    {
        Description.Width = width;
        Description.Height = height;

        D2DContext.AntialiasMode = antialias ? d2.AntialiasMode.PerPrimitive : d2.AntialiasMode.Aliased;

        using var draw = new d3.Texture2D(Device, Description);
        var surface = draw.QueryInterface<dx.Surface>();
        var bitmap = new d2.Bitmap1(D2DContext, surface, new d2.BitmapProperties1(
        new d2.PixelFormat(Description.Format, d2.AlphaMode.Premultiplied), 96, 96, d2.BitmapOptions.Target));

        D2DContext.Target = bitmap;

        D2DContext.BeginDraw();
        D2DContext.Clear(new SharpDX.Color4(0, 0, 0, 0));

        drawFunc(D2DContext);

        D2DContext.EndDraw();

        var stagingDesc = draw.Description;
        stagingDesc.Usage = d3.ResourceUsage.Staging;
        stagingDesc.BindFlags = d3.BindFlags.None;
        stagingDesc.CpuAccessFlags = d3.CpuAccessFlags.Read;
        stagingDesc.OptionFlags = d3.ResourceOptionFlags.None;

        using var staging = new d3.Texture2D(Device, stagingDesc);

        Device.ImmediateContext.CopyResource(draw, staging);

        var data = Device.ImmediateContext.MapSubresource(staging, 0, d3.MapMode.Read, d3.MapFlags.None);

        var stride = width * 4;

        var buffer = new byte[stride * height];
        for (var y = 0; y < height; y++)
            Marshal.Copy(data.DataPointer + y * data.RowPitch, buffer, y * stride, stride);

        // if we enable antialiasing and then max the values of every nonempty pixel it kinda acts a little bit like the existing software rasterizer
        //for (var i = 0; i < buffer.Length; i++)
        //    if (buffer[i] > 0)
        //        buffer[i] = 0xff;

        return Service.Texture.CreateFromRaw(RawImageSpecification.Bgra32(width, height), buffer);
    }

    public void Dispose()
    {
        Device.Dispose();
        D2DContext.Dispose();
    }
}
