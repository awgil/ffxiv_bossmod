using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    // different encounter mechanics can be split into independent components
    // individual components should be activated and deactivated when needed (typically by state machine transitions)
    // components can also have sub-components; typically these are created immediately by constructor
    public class BossComponent
    {
        // list of actor-specific hints (string + whether this is a "risk" type of hint)
        public class TextHints : List<(string, bool)>
        {
            public void Add(string text, bool isRisk = true) => base.Add((text, isRisk));
        }

        // list of actor-specific "movement hints" (arrow start/end pos + color)
        public class MovementHints : List<(WPos, WPos, uint)>
        {
            public void Add(WPos from, WPos to, uint color) => base.Add((from, to, color));
        }

        // list of global hints
        public class GlobalHints : List<string> { }

        // a set of player priorities; if there are several active components, non-player party member is drawn using color provided by component that returned highest priority (or default color for this priority)
        public enum PlayerPriority
        {
            Irrelevant, // player is completely irrelevant to any mechanics done by PC; it might be even not drawn at all, depending on configuration
            Normal, // player is drawn (it might be important to PC for proper positioning, e.g. so that it is not clipped by other mechanic), but is currently not particularly important
            Interesting, // player is drawn, and it is somewhat interesting for PC (e.g. it might currently be at risk of being clipped by PC's mechanic)
            Danger, // player is a source of danger to the player: might be risking failing a mechanic that would wipe a raid, or might be baiting nasty AOE, etc.
            Critical, // tracking this player's position is extremely important
        }

        public virtual void Init(BossModule module) { } // called at activation
        public virtual void Update(BossModule module) { } // called every frame - it is a good place to update any cached values
        public virtual void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints) { } // gather any relevant pieces of advice for specified raid member
        public virtual void AddGlobalHints(BossModule module, GlobalHints hints) { } // gather any relevant pieces of advice for whole raid
        public virtual PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => PlayerPriority.Irrelevant; // determine how particular party member should be drawn; if custom color is left untouched, standard color is selected
        public virtual void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena) { } // called at the beginning of arena draw, good place to draw aoe zones
        public virtual void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena) { } // called after arena background and borders are drawn, good place to draw actors, tethers, etc.

        // subcomponent management
        private List<BossComponent> _subcomponents = new();
        public IReadOnlyList<BossComponent> Subcomponents => _subcomponents;

        // note: use AddSubcomponent when adding things in constructor (i.e. pre Init call) and AddAndInitSubcomponent when adding things dynamically post init
        public T AddSubcomponent<T>() where T : BossComponent, new()
        {
            if (FindSubcomponent<T>() != null)
            {
                Service.Log($"[BossModule] Activating a component of type {typeof(T)} when another of the same type is already active; old one is deactivated automatically");
                RemoveSubcomponent<T>();
            }
            T comp = new();
            _subcomponents.Add(comp);
            return comp;
        }

        public T AddAndInitSubcomponent<T>(BossModule module) where T : BossComponent, new()
        {
            var comp = AddSubcomponent<T>();
            comp.InitRec(module);
            return comp;
        }

        public void RemoveSubcomponent<T>() where T : BossComponent
        {
            int count = _subcomponents.RemoveAll(x => x is T);
            if (count == 0)
                Service.Log($"[BossModule] Could not find a component of type {typeof(T)} to deactivate");
        }

        public void RemoveAllSubcomponents()
        {
            _subcomponents.Clear();
        }

        public T? FindSubcomponent<T>() where T : BossComponent
        {
            return _subcomponents.OfType<T>().FirstOrDefault();
        }

        // registration for events
        private Dictionary<uint, Action<BossModule, int, Actor, ulong, ushort, DateTime>> _partyStatusUpdate = new(); // status id -> (slot, actor, sourceID, extra, expire-at)
        protected void PartyStatusUpdate<SID>(SID sid, Action<BossModule, int, Actor, ulong, ushort, DateTime> callback) where SID : Enum => _partyStatusUpdate[(uint)(object)sid] = callback;

        private Dictionary<uint, Action<BossModule, Actor, ulong, ushort, DateTime>> _enemyStatusUpdate = new(); // status id -> (actor, sourceID, extra, expire-at)
        protected void EnemyStatusUpdate<SID>(SID sid, Action<BossModule, Actor, ulong, ushort, DateTime> callback) where SID : Enum => _enemyStatusUpdate[(uint)(object)sid] = callback;

        private Dictionary<uint, Action<BossModule, int, Actor>> _partyStatusLose = new(); // status id -> (slot, actor)
        protected void PartyStatusLose<SID>(SID sid, Action<BossModule, int, Actor> callback) where SID : Enum => _partyStatusLose[(uint)(object)sid] = callback;

        private Dictionary<uint, Action<BossModule, Actor>> _enemyStatusLose = new(); // status id -> (actor)
        protected void EnemyStatusLose<SID>(SID sid, Action<BossModule, Actor> callback) where SID : Enum => _enemyStatusLose[(uint)(object)sid] = callback;

        private Dictionary<uint, Action<BossModule, Actor, Actor>> _tether = new(); // tether id -> (source, target)
        protected void Tether<TID>(TID tid, Action<BossModule, Actor, Actor> callback) where TID : Enum => _tether[(uint)(object)tid] = callback;

        private Dictionary<uint, Action<BossModule, Actor, Actor>> _untether = new(); // tether id -> (source, target)
        protected void Untether<TID>(TID tid, Action<BossModule, Actor, Actor> callback) where TID : Enum => _untether[(uint)(object)tid] = callback;

        // this API is for private use or use by BossModule
        private void InitRec(BossModule module)
        {
            foreach (var s in _subcomponents)
                s.InitRec(module);

            Init(module);

            // execute callbacks for existing state
            if (_partyStatusUpdate.Count > 0)
            {
                foreach (var (slot, actor) in module.Raid.WithSlot(true))
                {
                    foreach (var s in actor.Statuses.Where(s => s.ID != 0))
                        _partyStatusUpdate.GetValueOrDefault(s.ID)?.Invoke(module, slot, actor, s.SourceID, s.Extra, s.ExpireAt);
                }
            }
            if (_enemyStatusUpdate.Count > 0)
            {
                foreach (var actor in module.WorldState.Actors.Where(a => a.Type is not ActorType.Player and not ActorType.Pet and not ActorType.Chocobo))
                {
                    foreach (var s in actor.Statuses.Where(s => s.ID != 0))
                        _enemyStatusUpdate.GetValueOrDefault(s.ID)?.Invoke(module, actor, s.SourceID, s.Extra, s.ExpireAt);
                }
            }
            if (_tether.Count > 0)
            {
                foreach (var actor in module.WorldState.Actors.Where(a => a.Tether.ID != 0))
                {
                    var target = module.WorldState.Actors.Find(actor.Tether.Target);
                    if (target != null)
                        _tether.GetValueOrDefault(actor.Tether.ID)?.Invoke(module, actor, target);
                }
            }

            foreach (var actor in module.WorldState.Actors.Where(a => a.Type is not ActorType.Player and not ActorType.Pet and not ActorType.Chocobo))
            {
                if (actor.CastInfo != null)
                    OnCastStarted(module, actor);
            }
        }

        public void UpdateRec(BossModule module)
        {
            foreach (var s in _subcomponents)
                s.UpdateRec(module);
            Update(module);
        }

        public void AddHintsRec(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            foreach (var s in _subcomponents)
                s.AddHintsRec(module, slot, actor, hints, movementHints);
            AddHints(module, slot, actor, hints, movementHints);
        }

        public void AddGlobalHintsRec(BossModule module, GlobalHints hints)
        {
            foreach (var s in _subcomponents)
                s.AddGlobalHintsRec(module, hints);
            AddGlobalHints(module, hints);
        }

        public (PlayerPriority, uint) CalcPriorityRec(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player)
        {
            uint color = 0;
            var highestPrio = CalcPriority(module, pcSlot, pc, playerSlot, player, ref color);
            foreach (var s in _subcomponents)
            {
                var (subPrio, subColor) = s.CalcPriorityRec(module, pcSlot, pc, playerSlot, player);
                if (subPrio > highestPrio)
                {
                    highestPrio = subPrio;
                    color = subColor;
                }
            }
            return (highestPrio, color);
        }

        public void DrawArenaBackgroundRec(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var s in _subcomponents)
                s.DrawArenaBackgroundRec(module, pcSlot, pc, arena);
            DrawArenaBackground(module, pcSlot, pc, arena);
        }

        public void DrawArenaForegroundRec(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var s in _subcomponents)
                s.DrawArenaForegroundRec(module, pcSlot, pc, arena);
            DrawArenaForeground(module, pcSlot, pc, arena);
        }

        public void HandlePartyStatusUpdate(BossModule module, int slot, Actor actor, int index)
        {
            foreach (var s in _subcomponents)
                s.HandlePartyStatusUpdate(module, slot, actor, index);
            var status = actor.Statuses[index];
            _partyStatusUpdate.GetValueOrDefault(status.ID)?.Invoke(module, slot, actor, status.SourceID, status.Extra, status.ExpireAt);
        }

        public void HandleEnemyStatusUpdate(BossModule module, Actor actor, int index)
        {
            foreach (var s in _subcomponents)
                s.HandleEnemyStatusUpdate(module, actor, index);
            var status = actor.Statuses[index];
            _enemyStatusUpdate.GetValueOrDefault(status.ID)?.Invoke(module, actor, status.SourceID, status.Extra, status.ExpireAt);
        }

        public void HandlePartyStatusLose(BossModule module, int slot, Actor actor, int index)
        {
            foreach (var s in _subcomponents)
                s.HandlePartyStatusLose(module, slot, actor, index);
            _partyStatusLose.GetValueOrDefault(actor.Statuses[index].ID)?.Invoke(module, slot, actor);
        }

        public void HandleEnemyStatusLose(BossModule module, Actor actor, int index)
        {
            foreach (var s in _subcomponents)
                s.HandleEnemyStatusLose(module, actor, index);
            _enemyStatusLose.GetValueOrDefault(actor.Statuses[index].ID)?.Invoke(module, actor);
        }

        public void HandleTethered(BossModule module, Actor source, Actor target)
        {
            foreach (var s in _subcomponents)
                s.HandleTethered(module, source, target);
            _tether.GetValueOrDefault(source.Tether.ID)?.Invoke(module, source, target);
        }

        public void HandleUntethered(BossModule module, Actor source, Actor target)
        {
            foreach (var s in _subcomponents)
                s.HandleUntethered(module, source, target);
            _untether.GetValueOrDefault(source.Tether.ID)?.Invoke(module, source, target);
        }

        // "old-style" world state event handlers; note that they are kept virtual, since old components override them directly - TODO remove that after refactoring is complete
        public virtual void OnCastStarted(BossModule module, Actor actor)
        {
            foreach (var s in _subcomponents)
                s.OnCastStarted(module, actor);
        }

        public virtual void OnCastFinished(BossModule module, Actor actor)
        {
            foreach (var s in _subcomponents)
                s.OnCastFinished(module, actor);
        }

        public virtual void OnEventCast(BossModule module, CastEvent info)
        {
            foreach (var s in _subcomponents)
                s.OnEventCast(module, info);
        }

        public virtual void OnEventIcon(BossModule module, ulong actorID, uint iconID)
        {
            foreach (var s in _subcomponents)
                s.OnEventIcon(module, actorID, iconID);
        }

        public virtual void OnEventEnvControl(BossModule module, uint featureID, byte index, uint state)
        {
            foreach (var s in _subcomponents)
                s.OnEventEnvControl(module, featureID, index, state);
        }
    }
}
