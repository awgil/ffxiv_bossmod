﻿using BossMod.Autorotation;
using Dalamud.Game.ClientState.Objects.Types;

namespace BossMod;

sealed class IPCProvider : IDisposable
{
    private Action? _disposeActions;

    public IPCProvider(RotationModuleManager autorotation)
    {
        // TODO: this really needs to be reconsidered, this exposes implementation detail
        // for usecase description, see PR 330 - really AI itself should handle heal range
        Register("ActiveModuleComponentBaseList", () => autorotation.Bossmods.ActiveModule?.Components.Select(c => c.GetType().BaseType?.Name).ToList() ?? default);
        Register("ActiveModuleComponentList", () => autorotation.Bossmods.ActiveModule?.Components.Select(c => c.GetType().Name).ToList() ?? default);
        Register("ActiveModuleHasComponent", (string name) => autorotation.Bossmods.ActiveModule?.Components.Any(c => c.GetType().Name == name || c.GetType().BaseType?.Name == name) ?? false);

        Register("HasModule", (IGameObject obj) => ModuleRegistry.FindByOID(obj.DataId) != null);
        Register("HasModuleByDataId", (uint dataId) => ModuleRegistry.FindByOID(dataId) != null);
        Register("IsMoving", autorotation.ActionManager.InputOverride.IsMoving);
        Register("ForbiddenZonesCount", () => autorotation.Hints.ForbiddenZones.Count);
        //Register("InitiateCombat", () => autorotation.ClassActions?.UpdateAutoAction(CommonActions.AutoActionAIFight, float.MaxValue, true));
        //Register("SetAutorotationState", (bool state) => Service.Config.Get<AutorotationConfig>().Enabled = state);
    }

    public void Dispose() => _disposeActions?.Invoke();

    private void Register<TRet>(string name, Func<TRet> func)
    {
        var p = Service.PluginInterface.GetIpcProvider<TRet>("BossMod." + name);
        p.RegisterFunc(func);
        _disposeActions += p.UnregisterFunc;
    }

    private void Register<TRet, T1>(string name, Func<TRet, T1> func)
    {
        var p = Service.PluginInterface.GetIpcProvider<TRet, T1>("BossMod." + name);
        p.RegisterFunc(func);
        _disposeActions += p.UnregisterFunc;
    }

    //private void Register(string name, Action func)
    //{
    //    var p = Service.PluginInterface.GetIpcProvider<object>("BossMod." + name);
    //    p.RegisterAction(func);
    //    _disposeActions += p.UnregisterAction;
    //}

    //private void Register<T1>(string name, Action<T1> func)
    //{
    //    var p = Service.PluginInterface.GetIpcProvider<T1, object>("BossMod." + name);
    //    p.RegisterAction(func);
    //    _disposeActions += p.UnregisterAction;
    //}
}
