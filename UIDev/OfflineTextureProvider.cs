using BossMod;
using Dalamud.Bindings.ImGui;
using Dalamud.Game;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Plugin.Services;
using ImGuiScene;
using Lumina.Data.Files;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace UIDev;

internal class OfflineTextureProvider(IRenderer render) : ITextureProvider
{
    public readonly IRenderer Renderer = render;

    private const string IconFileFormat = "ui/icon/{0:D3}000/{1}{2:D6}.tex";
    private const string HighResolutionIconFileFormat = "ui/icon/{0:D3}000/{1}{2:D6}_hr1.tex";

    public nint ConvertToKernelTexture(IDalamudTextureWrap wrap, bool leaveWrapOpen = false) => throw new NotImplementedException();
    public IDalamudTextureWrap CreateEmpty(RawImageSpecification specs, bool cpuRead, bool cpuWrite, string? debugName = null) => throw new NotImplementedException();
    public Task<IDalamudTextureWrap> CreateFromExistingTextureAsync(IDalamudTextureWrap wrap, TextureModificationArgs args = default, bool leaveWrapOpen = false, string? debugName = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<IDalamudTextureWrap> CreateFromImageAsync(ReadOnlyMemory<byte> bytes, string? debugName = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<IDalamudTextureWrap> CreateFromImageAsync(Stream stream, bool leaveOpen = false, string? debugName = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<IDalamudTextureWrap> CreateFromImGuiViewportAsync(ImGuiViewportTextureArgs args, string? debugName = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public IDalamudTextureWrap CreateFromRaw(RawImageSpecification specs, ReadOnlySpan<byte> bytes, string? debugName = null) => throw new NotImplementedException();
    public Task<IDalamudTextureWrap> CreateFromRawAsync(RawImageSpecification specs, ReadOnlyMemory<byte> bytes, string? debugName = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<IDalamudTextureWrap> CreateFromRawAsync(RawImageSpecification specs, Stream stream, bool leaveOpen = false, string? debugName = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public IDalamudTextureWrap CreateFromTexFile(TexFile file) => CreateOfflineFromTexFile(file);

    private unsafe OfflineTextureWrap CreateOfflineFromTexFile(TexFile file)
    {
        var buffer = file.TextureBuffer;
        fixed (byte* raw = buffer.RawData)
        {
            return new OfflineTextureWrap(Renderer.CreateTexture(raw, buffer.Width, buffer.Height, buffer.NumBytes / (buffer.Width * buffer.Height)));
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
            cached = new OfflineSharedImmediateTexture(Renderer, CreateOfflineFromTexFile(Service.LuminaGameData!.GetFile<TexFile>(path)!));
            _cachedFromGame.Add(path, cached);
        }

        return cached;
    }

    private readonly Dictionary<string, ISharedImmediateTexture> _cachedFromGame = [];

    public ISharedImmediateTexture GetFromGameIcon(in GameIconLookup lookup) => TryGetIconPath(lookup, out var path) ? GetFromGame(path) : throw new InvalidDataException($"icon {lookup} not found");
    public ISharedImmediateTexture GetFromManifestResource(Assembly assembly, string name) => throw new NotImplementedException();
    public string GetIconPath(in GameIconLookup lookup) => TryGetIconPath(lookup, out var path) ? path : throw new FileNotFoundException();
    public IEnumerable<IBitmapCodecInfo> GetSupportedImageDecoderInfos() => throw new NotImplementedException();
    public bool IsDxgiFormatSupported(int dxgiFormat) => throw new NotImplementedException();
    public bool IsDxgiFormatSupportedForCreateFromExistingTextureAsync(int dxgiFormat) => throw new NotImplementedException();
    public bool TryGetFromGameIcon(in GameIconLookup lookup, [NotNullWhen(true)] out ISharedImmediateTexture? texture) => throw new NotImplementedException();
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
}

internal record class OfflineTextureWrap(TextureWrap Wrap) : IDalamudTextureWrap
{
    public ImTextureID Handle => new(Wrap.ImGuiHandle);

    public int Width => Wrap.Width;

    public int Height => Wrap.Height;

    public void Dispose() => Wrap.Dispose();
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
