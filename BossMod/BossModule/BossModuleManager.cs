using System;
using System.Collections.Generic;

namespace BossMod
{
    // base class that creates and manages instances of proper boss modules in response to world state changes
    // derived class should perform rendering as appropriate
    public class BossModuleManager : IDisposable
    {
        public WorldState WorldState { get; init; }
        public RaidCooldowns RaidCooldowns { get; init; }
        protected BossModuleConfig WindowConfig { get; init; }

        private bool _running = false;
        private bool _configOrModulesUpdated = false;
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
            protected set {
                _activeModule = value;
                _configOrModulesUpdated = true;
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
                m.StateMachine.PrepullTimer = PrepullTimer();
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
                Service.Log($"[BMM] Active module change: from {_activeModule?.GetType()} (prio {curPriority}, manual-override={_activeModuleOverridden}) to {bestModule?.GetType()} (prio {bestPriority})");
                _activeModule = bestModule;
                _configOrModulesUpdated = true;
                _activeModuleOverridden = false;
            }

            if (_configOrModulesUpdated)
            {
                RefreshConfigOrModules();
                _configOrModulesUpdated = false;
            }
        }

        protected virtual void RefreshConfigOrModules() { }
        protected virtual void OnModuleLoaded(BossModule module) { Service.Log($"[BMM] Boss module '{module.GetType()}' for actor {module.PrimaryActor.InstanceID:X} ({module.PrimaryActor.OID:X}) '{module.PrimaryActor.Name}' loaded"); }
        protected virtual void OnModuleUnloaded(BossModule module) { Service.Log($"[BMM] Boss module '{module.GetType()}' for actor {module.PrimaryActor.InstanceID:X} unloaded"); }
        protected virtual float PrepullTimer() => 0; // TODO: reconsider...

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

            _configOrModulesUpdated = true;
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

            RefreshConfigOrModules();
            _configOrModulesUpdated = false;
        }

        private void LoadModule(BossModule m)
        {
            _loadedModules.Add(m);
            _configOrModulesUpdated = true;
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
            _configOrModulesUpdated = true;
        }

        private int ModuleDisplayPriority(BossModule? m)
        {
            if (m == null)
                return 0;
            if (m.StateMachine.ActiveState != null)
                return 3;
            if (!m.PrimaryActor.IsDestroyed && !m.PrimaryActor.IsDead && m.PrimaryActor.IsTargetable)
                return 2;
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
            if (WindowConfig.ShowDemo && demoIndex < 0)
                LoadModule(CreateDemoModule());
            else if (!WindowConfig.ShowDemo && demoIndex >= 0)
                UnloadModule(demoIndex);

            _configOrModulesUpdated = true;
        }
    }
}
