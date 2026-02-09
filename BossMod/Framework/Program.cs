using Autofac;
using BossMod;
using DalaMock.Core.DI;
using DalaMock.Core.Mocks;
using DalaMock.Core.Windows;
using DalaMock.Shared.Interfaces;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

var cnt = new MockContainer();
var ui = cnt.GetMockUi();
var loader = cnt.GetPluginLoader();
var pl = loader.AddPlugin(typeof(MockPlugin));
loader.StartPlugin(pl);
ui.Run();

internal class MockPlugin(IDalamudPluginInterface dalamud, IPluginLog pluginLog, ICommandManager cmd, IDataManager data) : Plugin(dalamud, pluginLog, cmd, data)
{
    public override void ConfigureContainer(ContainerBuilder containerBuilder)
    {
        base.ConfigureContainer(containerBuilder);
        containerBuilder.RegisterType<MockWindowSystem>().As<IWindowSystem>();
        containerBuilder.RegisterType<MockFileDialogManager>().As<IFileDialogManager>();
        containerBuilder.RegisterType<MockFont>().As<IFont>().SingleInstance();
    }
}
