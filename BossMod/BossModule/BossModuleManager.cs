using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod
{
    // base class that creates and manages instances of proper boss modules in response to world state changes
    // derived class should perform rendering as appropriate
    public class BossModuleManager : IDisposable
    {
        public WorldState WorldState { get; init; }
        public BossModuleConfig Config { get; init; }
        public RaidCooldowns RaidCooldowns { get; init; }

        private bool _running = false;
        private Dictionary<uint, BossModule> _loadedModules = new(); // key = primary actor instance ID
        private List<BossModule> _activeModules = new();

        public BossModule? ActiveModule => _activeModules.FirstOrDefault();
        public IReadOnlyCollection<BossModule> ActiveModules => _activeModules;
        public IReadOnlyCollection<BossModule> LoadedModules => _loadedModules.Values;

        public BossModuleManager(WorldState ws, ConfigNode settings)
        {
            WorldState = ws;
            Config = settings.Get<BossModuleConfig>();
            RaidCooldowns = new(ws);

            Config.Modified += ConfigChanged;

            if (Config.Enable)
            {
                Startup();
                RefreshConfigOrModules();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Shutdown();
                RefreshConfigOrModules();
                Config.Modified -= ConfigChanged;
                RaidCooldowns.Dispose();
            }
        }

        public void Update()
        {
            if (!_running)
                return;

            try
            {
                foreach (var m in _activeModules)
                    m.Update();
            }
            catch (Exception ex)
            {
                Service.Log($"Boss module crashed: {ex}");
                Config.Enable = false;
                Shutdown();
                RefreshConfigOrModules();
            }
        }

        protected virtual void RefreshConfigOrModules() { }

        private void Startup()
        {
            if (_running)
                return;

            _running = true;
            WorldState.Actors.Added += ActorAdded;
            WorldState.Actors.Removed += ActorRemoved;
            WorldState.Actors.InCombatChanged += EnterExitCombat;

            if (Config.ShowDemo)
            {
                LoadDemo();
            }

            foreach (var a in WorldState.Actors)
            {
                var m = LoadModule(a);
                if (m != null && a.InCombat)
                {
                    ActivateModule(m);
                }
            }
        }

        private void Shutdown()
        {
            if (!_running)
                return;

            _running = false;
            WorldState.Actors.Added -= ActorAdded;
            WorldState.Actors.Removed -= ActorRemoved;
            WorldState.Actors.InCombatChanged -= EnterExitCombat;

            foreach (var m in _activeModules)
            {
                m.StateMachine.ActiveState = null;
                m.Reset();
            }
            _activeModules.Clear();

            foreach (var m in _loadedModules.Values)
                m.Dispose();
            _loadedModules.Clear();
        }

        private bool LoadDemo()
        {
            if (_running && !_loadedModules.ContainsKey(0))
            {
                _loadedModules[0] = new DemoModule(this, new(0, 0, "", ActorType.None, Class.None, new(), 0, false, 0));
                return true;
            }
            return false;
        }

        private BossModule? LoadModule(Actor actor)
        {
            var m = ModuleRegistry.CreateModule(actor.OID, this, actor);
            if (m != null)
            {
                Service.Log($"[BMM] Loading boss module '{m.GetType()}' for actor {actor.InstanceID:X} ({actor.OID:X}) '{actor.Name}'");
                _loadedModules[actor.InstanceID] = m;
            }
            return m;
        }

        public bool UnloadModule(uint instanceID)
        {
            var m = _loadedModules.GetValueOrDefault(instanceID);
            if (m == null)
                return false;

            DeactivateModule(m);

            Service.Log($"[BMM] Unloading boss module '{m.GetType()}' for actor {instanceID:X}");
            m.Dispose();
            _loadedModules.Remove(instanceID);
            return true;
        }

        public void ActivateModule(BossModule m)
        {
            Service.Log($"[BMM] Activating boss module '{m.GetType()}' for actor {m.PrimaryActor.InstanceID:X} ({m.PrimaryActor.OID:X}) '{m.PrimaryActor.Name}'");
            m.Reset();
            m.StateMachine.ActiveState = m.InitialState;
            _activeModules.Add(m);
        }

        public void DeactivateModule(BossModule m)
        {
            if (_activeModules.Remove(m))
            {
                Service.Log($"[BMM] Deactivating boss module '{m.GetType()}' for actor {m.PrimaryActor.InstanceID:X} ({m.PrimaryActor.OID:X}) '{m.PrimaryActor.Name}'");
                m.StateMachine.ActiveState = null;
                m.Reset();
            }
        }

        private void ActorAdded(object? sender, Actor actor)
        {
            if (LoadModule(actor) != null)
            {
                RefreshConfigOrModules();
            }
        }

        private void ActorRemoved(object? sender, Actor actor)
        {
            if (UnloadModule(actor.InstanceID))
            {
                RefreshConfigOrModules();
            }
        }

        private void EnterExitCombat(object? sender, Actor actor)
        {
            var m = _loadedModules.GetValueOrDefault(actor.InstanceID);
            if (m == null)
                return;

            if (actor.InCombat)
            {
                ActivateModule(m);
            }
            else
            {
                DeactivateModule(m);
            }
            RefreshConfigOrModules();
        }

        private void ConfigChanged(object? sender, EventArgs args)
        {
            if (Config.Enable)
                Startup();
            else
                Shutdown();

            if (Config.ShowDemo)
                LoadDemo();
            else
                UnloadModule(0);

            RefreshConfigOrModules();
        }
    }
}
