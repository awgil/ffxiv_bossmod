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

        // this API is for private use or use by BossModule
        private void InitRec(BossModule module)
        {
            foreach (var s in _subcomponents)
                s.InitRec(module);

            Init(module);

            // execute callbacks for existing state
            foreach (var actor in module.WorldState.Actors)
            {
                if (actor.CastInfo != null)
                    OnCastStarted(module, actor);
                if (actor.Tether.ID != 0)
                    OnTethered(module, actor, actor.Tether);
                for (int i = 0; i < actor.Statuses.Length; ++i)
                    if (actor.Statuses[i].ID != 0)
                        OnStatusGain(module, actor, actor.Statuses[i]);
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

        // "old-style" world state event handlers; note that they are kept virtual, since old components override them directly - TODO remove that after refactoring is complete
        public virtual void OnStatusGain(BossModule module, Actor actor, ActorStatus status) // note: also called for status-change events; if component needs to distinguish between lose+gain and change, it can use the fact that 'lose' is not called for change
        {
            foreach (var s in _subcomponents)
                s.OnStatusGain(module, actor, status);
        }

        public virtual void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            foreach (var s in _subcomponents)
                s.OnStatusLose(module, actor, status);
        }

        public virtual void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            foreach (var s in _subcomponents)
                s.OnTethered(module, source, tether);
        }

        public virtual void OnUntethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            foreach (var s in _subcomponents)
                s.OnUntethered(module, source, tether);
        }

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
