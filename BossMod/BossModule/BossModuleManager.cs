using System;
using System.Collections.Generic;

namespace BossMod
{
    // class that creates and manages instances of proper boss modules in response to world state changes
    public class BossModuleManager : IDisposable
    {
        public WorldState WorldState { get; init; }
        public RaidCooldowns RaidCooldowns { get; init; }
        public BossModuleConfig WindowConfig { get; init; }

        private bool _running = false;
        private bool _activeModuleOverridden = false;

        private List<BossModule> _loadedModules = new();
        public IReadOnlyList<BossModule> LoadedModules => _loadedModules;

        // drawn module among loaded modules; this can be changed explicitly if needed
        // usually we don't have multiple concurrently active modules, since this prevents meaningful cd planning, raid cooldown tracking, etc.
        // but it can theoretically happen e.g. around checkpoints and in typically trivial outdoor content
        // TODO: reconsider...
        private BossModule? _activeModule;
        public BossModule? ActiveModule
        {
            get => _activeModule;
            set {
                Service.Log($"[BMM] Active module override: from {_activeModule?.GetType().FullName ?? "<n/a>"} (manual-override={_activeModuleOverridden}) to {value?.GetType().FullName ?? "<n/a>"}");
                _activeModule = value;
                _activeModuleOverridden = true;
            }
        }

        public BossModuleManager(WorldState ws)
        {
            WorldState = ws;
            WindowConfig = Service.Config.Get<BossModuleConfig>();
            RaidCooldowns = new(ws);

            WindowConfig.Modified += ConfigChanged;

            if (WindowConfig.Enable)
            {
                Startup();
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
                WindowConfig.Modified -= ConfigChanged;
                RaidCooldowns.Dispose();
            }
        }

        public void Update()
        {
            if (!_running)
                return;

            // update all loaded modules, handle activation/deactivation
            int bestPriority = 0;
            BossModule? bestModule = null;
            bool anyModuleActivated = false;
            for (int i = 0; i < _loadedModules.Count; ++i)
            {
                var m = _loadedModules[i];
                m.StateMachine.PrepullTimer = WorldState.Client.CountdownRemaining ?? 10000;
                bool wasActive = m.StateMachine.ActiveState != null;
                bool isActive;
                try
                {
                    m.Update();
                    isActive = m.StateMachine.ActiveState != null;
                }
                catch (Exception ex)
                {
                    Service.Log($"Boss module {m.GetType()} crashed: {ex}");
                    wasActive = true; // force unload if exception happened before activation
                    isActive = false;
                }

                // unload module either if it became deactivated or its primary actor disappeared without ever activating
                if (!isActive && (wasActive || m.PrimaryActor.IsDestroyed))
                {
                    UnloadModule(i--);
                    continue;
                }

                // module remains loaded
                int priority = ModuleDisplayPriority(m);
                if (priority > bestPriority)
                {
                    bestPriority = priority;
                    bestModule = m;
                }

                if (!wasActive && isActive)
                {
                    Service.Log($"[BMM] Boss module '{m.GetType()}' for actor {m.PrimaryActor.InstanceID:X} ({m.PrimaryActor.OID:X}) '{m.PrimaryActor.Name}' activated");
                    anyModuleActivated |= true;
                }
            }

            var curPriority = ModuleDisplayPriority(_activeModule);
            if (bestPriority > curPriority && (anyModuleActivated || !_activeModuleOverridden))
            {
                Service.Log($"[BMM] Active module change: from {_activeModule?.GetType().FullName ?? "<n/a>"} (prio {curPriority}, manual-override={_activeModuleOverridden}) to {bestModule?.GetType().FullName ?? "<n/a>"} (prio {bestPriority})");
                _activeModule = bestModule;
                _activeModuleOverridden = false;
            }
        }

        protected virtual void OnModuleLoaded(BossModule module) { Service.Log($"[BMM] Boss module '{module.GetType()}' for actor {module.PrimaryActor.InstanceID:X} ({module.PrimaryActor.OID:X}) '{module.PrimaryActor.Name}' loaded"); }
        protected virtual void OnModuleUnloaded(BossModule module) { Service.Log($"[BMM] Boss module '{module.GetType()}' for actor {module.PrimaryActor.InstanceID:X} unloaded"); }

        private void Startup()
        {
            if (_running)
                return;

            Service.Log("[BMM] Starting up...");
            _running = true;

            if (WindowConfig.ShowDemo)
                _loadedModules.Add(CreateDemoModule());

            WorldState.Actors.Added += ActorAdded;
            foreach (var a in WorldState.Actors)
                ActorAdded(null, a);
        }

        private void Shutdown()
        {
            if (!_running)
                return;

            Service.Log("[BMM] Shutting down...");
            _running = false;
            WorldState.Actors.Added -= ActorAdded;

            _activeModule = null;
            foreach (var m in _loadedModules)
                m.Dispose();
            _loadedModules.Clear();
        }

        private void LoadModule(BossModule m)
        {
            _loadedModules.Add(m);
            OnModuleLoaded(m);
        }

        private void UnloadModule(int index)
        {
            var m = _loadedModules[index];
            OnModuleUnloaded(m);
            if (_activeModule == m)
            {
                _activeModule = null;
                _activeModuleOverridden = false;
            }
            m.Dispose();
            _loadedModules.RemoveAt(index);
        }

        private int ModuleDisplayPriority(BossModule? m)
        {
            if (m == null)
                return 0;
            if (m.StateMachine.ActiveState != null)
                return 4;
            if (m.PrimaryActor.InstanceID == 0)
                return 2; // demo module
            if (!m.PrimaryActor.IsDestroyed && !m.PrimaryActor.IsDead && m.PrimaryActor.IsTargetable)
                return 3;
            return 1;
        }

        private BossModule CreateDemoModule()
        {
            return new DemoModule(WorldState, new(0, 0, -1, "", ActorType.None, Class.None, new()));
        }

        private void ActorAdded(object? sender, Actor actor)
        {
            var m = ModuleRegistry.CreateModuleForActor(WorldState, actor);
            if (m != null)
            {
                LoadModule(m);
            }
        }

        private void ConfigChanged(object? sender, EventArgs args)
        {
            if (WindowConfig.Enable)
                Startup();
            else
                Shutdown();

            int demoIndex = _loadedModules.FindIndex(m => m is DemoModule);
            bool showDemo = WindowConfig.Enable && WindowConfig.ShowDemo;
            if (showDemo && demoIndex < 0)
                LoadModule(CreateDemoModule());
            else if (!showDemo && demoIndex >= 0)
                UnloadModule(demoIndex);
        }
    }
}
