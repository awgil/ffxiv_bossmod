using System;
using System.Collections.Generic;

namespace BossMod
{
    class IPCProvider : IDisposable
    {
        private List<Action> _disposeActions = new();

        public IPCProvider(Autorotation autorotation)
        {
            Register("IsMoving", () => ActionManagerEx.Instance!.InputOverride.IsMoving());
            Register("ForbiddenZonesCount", () => autorotation.Hints.ForbiddenZones.Count);
            Register("InitiateCombat", () => autorotation.ClassActions?.UpdateAutoAction(CommonActions.AutoActionAIFight, float.MaxValue, true));
            Register("SetAutorotationState", (bool state) => Service.Config.Get<AutorotationConfig>().Enabled = state);
        }

        public void Dispose()
        {
            foreach (var a in _disposeActions)
                a();
        }

        private void Register<TRet>(string name, Func<TRet> func)
        {
            var p = Service.PluginInterface.GetIpcProvider<TRet>("BossMod." + name);
            p.RegisterFunc(func);
            _disposeActions.Add(p.UnregisterFunc);
        }

        private void Register(string name, Action func)
        {
            var p = Service.PluginInterface.GetIpcProvider<object>("BossMod." + name);
            p.RegisterAction(func);
            _disposeActions.Add(p.UnregisterAction);
        }

        private void Register<T1>(string name, Action<T1> func)
        {
            var p = Service.PluginInterface.GetIpcProvider<T1, object>("BossMod." + name);
            p.RegisterAction(func);
            _disposeActions.Add(p.UnregisterAction);
        }
    }
}
