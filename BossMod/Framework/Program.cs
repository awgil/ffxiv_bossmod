using Autofac;
using BossMod;
using BossMod.Mocks;
using DalaMock.Core.DI;
using DalaMock.Core.Mocks;
using DalaMock.Core.Windows;
using DalaMock.Shared.Extensions;
using DalaMock.Shared.Interfaces;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

var cnt = new MockContainer();
var ui = cnt.GetMockUi();
var loader = cnt.GetPluginLoader();
var pl = loader.AddPlugin(typeof(MockPlugin));
loader.StartPlugin(pl);
ui.Run();

internal class MockPlugin(
    IDalamudPluginInterface dalamud,
    IPluginLog log,
    ICommandManager commandManager,
    IDataManager dataManager,
    IDtrBar dtrBar,
    ICondition condition,
    IGameGui gameGui,
    IChatGui chatGui,
    ITextureProvider tex
) : Plugin(
    dalamud,
    log,
    commandManager,
    dataManager,
    dtrBar,
    condition,
    gameGui,
    chatGui,
    tex
)
{
    public override void ConfigureContainer(ContainerBuilder containerBuilder)
    {
        base.ConfigureContainer(containerBuilder);
        containerBuilder.RegisterType<MockWindowSystem>().As<IWindowSystem>();
        containerBuilder.RegisterType<MockFileDialogManager>().As<IFileDialogManager>();
        containerBuilder.RegisterType<MockFont>().As<IFont>().SingleInstance();
    }

    public override void ConfigureExtra(ContainerBuilder containerBuilder)
    {
        containerBuilder.RegisterSingletonSelfAndInterfaces<MockMovementOverride>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<MockActionManagerEx>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<MockWorldStateSync>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<MockWorldStateFactory>();
        containerBuilder.RegisterSingletonSelfAndInterfaces<MockHintExecutor>();

        containerBuilder.RegisterBuildCallback(scope =>
        {
            Service.Config.Modified.Subscribe(() =>
            {
                Service.Log("Config was modified.");
            });
        });

        /*
        containerBuilder.RegisterBuildCallback(scope =>
        {
            var ip = scope.Resolve<IDalamudPluginInterface>();
            var xlDir = new DirectoryInfo(ip.ConfigFile.DirectoryName!).Parent!;
            var assetDir = Path.Join(xlDir.FullName, "dalamudAssets/dev/UIRes");

            var noto = Path.Join(assetDir, "NotoSansCJKjp-Medium.otf");
            unsafe
            {
                ImGui.GetIO().Fonts.AddFontFromFileTTF(noto, 17.0f, null, ImGui.GetIO().Fonts.GetGlyphRangesJapanese());
                ImGui.GetIO().Fonts.Build();
            }
        });
        */
    }
}
