using BossMod;
using Dalamud.Bindings.ImGui;
using Dalamud.Game;
using Dalamud.Interface.ImGuiSeStringRenderer;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Plugin.Services;
using ImGuiScene;
using Lumina.Data.Files;
using SharpDX;
using SharpDX.Direct3D11;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Format = SharpDX.DXGI.Format;

namespace UIDev;

internal class OfflineTextureProvider(IRenderer render, Device device) : ITextureProvider
{
    public readonly IRenderer Renderer = render;
    public readonly Device Device = device;

    private const string IconFileFormat = "ui/icon/{0:D3}000/{1}{2:D6}.tex";
    private const string HighResolutionIconFileFormat = "ui/icon/{0:D3}000/{1}{2:D6}_hr1.tex";

    public nint ConvertToKernelTexture(IDalamudTextureWrap wrap, bool leaveWrapOpen = false) => throw new NotImplementedException();
    public IDalamudTextureWrap CreateEmpty(RawImageSpecification specs, bool cpuRead, bool cpuWrite, string? debugName = null) => throw new NotImplementedException();
    public Task<IDalamudTextureWrap> CreateFromExistingTextureAsync(IDalamudTextureWrap wrap, TextureModificationArgs args = default, bool leaveWrapOpen = false, string? debugName = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<IDalamudTextureWrap> CreateFromImageAsync(ReadOnlyMemory<byte> bytes, string? debugName = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<IDalamudTextureWrap> CreateFromImageAsync(Stream stream, bool leaveOpen = false, string? debugName = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<IDalamudTextureWrap> CreateFromImGuiViewportAsync(ImGuiViewportTextureArgs args, string? debugName = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<IDalamudTextureWrap> CreateFromRawAsync(RawImageSpecification specs, ReadOnlyMemory<byte> bytes, string? debugName = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<IDalamudTextureWrap> CreateFromRawAsync(RawImageSpecification specs, Stream stream, bool leaveOpen = false, string? debugName = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public IDalamudTextureWrap CreateFromTexFile(TexFile file) => CreateFromTexFileInternal(file);

    private OfflineTextureWrap CreateFromTexFileInternal(TexFile file)
    {
        var buffer = file.TextureBuffer;
        var (dxgiFormat, conversion) = TexFile.GetDxgiFormatFromTextureFormat(file.Header.Format, false);
        if (conversion != TexFile.DxgiFormatConversion.NoConversion || !IsDxgiFormatSupported(dxgiFormat))
        {
            dxgiFormat = (int)Format.B8G8R8A8_UNorm;
            buffer = buffer.Filter(0, 0, TexFile.TextureFormat.B8G8R8A8);
        }

        var spec = new RawImageSpecification(buffer.Width, buffer.Height, dxgiFormat);
        return CreateFromRawInternal(spec, buffer.RawData);
    }

    public IDalamudTextureWrap CreateFromRaw(RawImageSpecification specs, ReadOnlySpan<byte> bytes, string? debugName = null) => CreateFromRawInternal(specs, bytes, debugName);

    private OfflineTextureWrap CreateFromRawInternal(RawImageSpecification specs, ReadOnlySpan<byte> bytes, string? debugName = null)
    {
        var texd = new Texture2DDescription()
        {
            Width = specs.Width,
            Height = specs.Height,
            MipLevels = 1,
            ArraySize = 1,
            Format = (Format)specs.DxgiFormat,
            SampleDescription = new(1, 0),
            Usage = ResourceUsage.Immutable,
            BindFlags = BindFlags.ShaderResource,
            CpuAccessFlags = CpuAccessFlags.None,
            OptionFlags = ResourceOptionFlags.None
        };
        unsafe
        {
            fixed (byte* data = bytes)
            {
                var texture = new Texture2D(Device, texd, [new DataRectangle((nint)data, specs.Pitch)]);
                var viewdesc = new ShaderResourceViewDescription()
                {
                    Format = texd.Format,
                    Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D,
                    Texture2D = new() { MipLevels = texd.MipLevels }
                };
                var view = new ShaderResourceView(Device, texture, viewdesc);
                return new OfflineTextureWrap(view, texd.Width, texd.Height);
            }
        }
    }

    public Task<IDalamudTextureWrap> CreateFromTexFileAsync(TexFile file, string? debugName = null, CancellationToken cancellationToken = default) => Task.FromResult(CreateFromTexFile(file));
    public ISharedImmediateTexture GetFromFile(string path) => throw new NotImplementedException();
    public ISharedImmediateTexture GetFromFile(FileInfo file) => throw new NotImplementedException();
    public ISharedImmediateTexture GetFromFileAbsolute(string fullPath) => throw new NotImplementedException();
    public ISharedImmediateTexture GetFromGame(string path)
    {
        if (!_cachedFromGame.TryGetValue(path, out var cached))
        {
            cached = new OfflineSharedImmediateTexture(Renderer, CreateFromTexFileInternal(Service.LuminaGameData!.GetFile<TexFile>(path)!));
            _cachedFromGame.Add(path, cached);
        }

        return cached;
    }

    private readonly Dictionary<string, ISharedImmediateTexture> _cachedFromGame = [];

    public ISharedImmediateTexture GetFromGameIcon(in GameIconLookup lookup) => TryGetIconPath(lookup, out var path) ? GetFromGame(path) : throw new InvalidDataException($"icon {lookup} not found");
    public ISharedImmediateTexture GetFromManifestResource(Assembly assembly, string name) => throw new NotImplementedException();
    public string GetIconPath(in GameIconLookup lookup) => TryGetIconPath(lookup, out var path) ? path : throw new FileNotFoundException();
    public IEnumerable<IBitmapCodecInfo> GetSupportedImageDecoderInfos() => throw new NotImplementedException();
    public bool IsDxgiFormatSupported(int dxgiFormat) => Device.CheckFormatSupport((Format)dxgiFormat).HasFlag(FormatSupport.Texture2D);
    public bool IsDxgiFormatSupportedForCreateFromExistingTextureAsync(int dxgiFormat) => throw new NotImplementedException();
    public bool TryGetFromGameIcon(in GameIconLookup lookup, [NotNullWhen(true)] out ISharedImmediateTexture? texture)
    {
        texture = null;
        if (TryGetIconPath(lookup, out var path))
        {
            texture = GetFromGame(path);
            return true;
        }
        return false;
    }
    public bool TryGetIconPath(in GameIconLookup lookup, [NotNullWhen(true)] out string? path)
    {
        // 1. Item
        path = FormatIconPath(
            lookup.IconId,
            lookup.ItemHq ? "hq/" : string.Empty,
            lookup.HiRes);
        if (FileExists(path))
            return true;

        var languageFolder = lookup.Language switch
        {
            ClientLanguage.Japanese => "ja/",
            ClientLanguage.English => "en/",
            ClientLanguage.German => "de/",
            ClientLanguage.French => "fr/",
            _ => null,
        };

        if (languageFolder is not null)
        {
            // 2. Regular icon, with language, hi-res
            path = FormatIconPath(
                lookup.IconId,
                languageFolder,
                lookup.HiRes);
            if (FileExists(path))
                return true;

            if (lookup.HiRes)
            {
                // 3. Regular icon, with language, no hi-res
                path = FormatIconPath(
                    lookup.IconId,
                    languageFolder,
                    false);
                if (FileExists(path))
                    return true;
            }
        }

        // 4. Regular icon, without language, hi-res
        path = FormatIconPath(
            lookup.IconId,
            null,
            lookup.HiRes);
        if (FileExists(path))
            return true;

        // 4. Regular icon, without language, no hi-res
        if (lookup.HiRes)
        {
            path = FormatIconPath(
                lookup.IconId,
                null,
                false);
            if (FileExists(path))
                return true;
        }

        return false;
    }

    private static bool FileExists(string path) => Service.LuminaGameData!.FileExists(path);

    private static string FormatIconPath(uint iconId, string? type, bool highResolution)
    {
        var format = highResolution ? HighResolutionIconFileFormat : IconFileFormat;

        type ??= string.Empty;
        if (type.Length > 0 && !type.EndsWith('/'))
            type += "/";

        return string.Format(format, iconId / 1000, type, iconId);
    }

    IDrawListTextureWrap ITextureProvider.CreateDrawListTexture(string? debugName) => throw new NotImplementedException();
    Task<IDalamudTextureWrap> ITextureProvider.CreateFromClipboardAsync(string? debugName, CancellationToken cancellationToken) => throw new NotImplementedException();
    bool ITextureProvider.HasClipboardImage() => throw new NotImplementedException();
    public IDalamudTextureWrap CreateTextureFromSeString(ReadOnlySpan<byte> text, scoped in SeStringDrawParams drawParams = default, string? debugName = null) => throw new NotImplementedException();
}

internal class OfflineTextureWrap(nint native, int width, int height) : IDalamudTextureWrap
{

    private readonly IDisposable? _resource;

    public OfflineTextureWrap(ShaderResourceView view, int width, int height) : this(view.NativePointer, width, height)
    {
        _resource = view;
    }
    public OfflineTextureWrap(TextureWrap innerWrap) : this(innerWrap.ImGuiHandle, innerWrap.Width, innerWrap.Height)
    {
        _resource = innerWrap;
    }

    public ImTextureID Handle => new(native);

    public int Width => width;
    public int Height => height;
    public void Dispose()
    {
        _resource?.Dispose();
    }
}

internal class OfflineSharedImmediateTexture : ISharedImmediateTexture
{
    private readonly OfflineTextureWrap Wrap;
    private readonly OfflineTextureWrap Empty;

    public unsafe OfflineSharedImmediateTexture(IRenderer renderer, OfflineTextureWrap wrap)
    {
        Wrap = wrap;
        var bytes = new byte[16];
        fixed (byte* ptr = bytes)
        {
            Empty = new(renderer.CreateTexture(ptr, 4, 4, 1));
        }
    }

    [return: NotNullIfNotNull(nameof(defaultWrap))]
    public IDalamudTextureWrap? GetWrapOrDefault(IDalamudTextureWrap? defaultWrap = null) => TryGetWrap(out var tex, out _) ? tex : defaultWrap;
    public IDalamudTextureWrap GetWrapOrEmpty() => TryGetWrap(out var tex, out _) ? tex : Empty;
    public Task<IDalamudTextureWrap> RentAsync(CancellationToken cancellationToken = default) => Task.FromResult<IDalamudTextureWrap>(Wrap);
    public bool TryGetWrap([NotNullWhen(true)] out IDalamudTextureWrap? texture, out Exception? exception)
    {
        texture = Wrap;
        exception = null;
        return true;
    }
}
