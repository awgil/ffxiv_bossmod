namespace BossMod;

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

    public bool KeepOnPhaseChange; // by default, all components are deactivated on phase change automatically (since phase change can happen at any time) - setting this to true prevents this

    public virtual void Init(BossModule module) { } // called at activation
    public virtual void Update(BossModule module) { } // called every frame - it is a good place to update any cached values
    public virtual void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints) { } // gather any relevant pieces of advice for specified raid member
    public virtual void AddGlobalHints(BossModule module, GlobalHints hints) { } // gather any relevant pieces of advice for whole raid
    public virtual void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // gather AI hints for specified raid member
    public virtual PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => PlayerPriority.Irrelevant; // determine how particular party member should be drawn; if custom color is left untouched, standard color is selected
    public virtual void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena) { } // called at the beginning of arena draw, good place to draw aoe zones
    public virtual void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena) { } // called after arena background and borders are drawn, good place to draw actors, tethers, etc.

    // world state event handlers
    public virtual void OnActorCreated(BossModule module, Actor actor) { }
    public virtual void OnActorDestroyed(BossModule module, Actor actor) { }
    public virtual void OnStatusGain(BossModule module, Actor actor, ActorStatus status) { } // note: also called for status-change events; if component needs to distinguish between lose+gain and change, it can use the fact that 'lose' is not called for change
    public virtual void OnStatusLose(BossModule module, Actor actor, ActorStatus status) { }
    public virtual void OnTethered(BossModule module, Actor source, ActorTetherInfo tether) { }
    public virtual void OnUntethered(BossModule module, Actor source, ActorTetherInfo tether) { }
    public virtual void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell) { } // note: action is always a spell; not called for player spells
    public virtual void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell) { } // note: action is always a spell; not called for player spells
    public virtual void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell) { } // note: action is always a spell; not called for player spells
    public virtual void OnEventIcon(BossModule module, Actor actor, uint iconID) { }
    public virtual void OnActorEState(BossModule module, Actor actor, ushort state) { }
    public virtual void OnActorEAnim(BossModule module, Actor actor, uint state) { }
    public virtual void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id) { }
    public virtual void OnActorNpcYell(BossModule module, Actor actor, ushort id) { }
    public virtual void OnActorModelStateChange(BossModule module, Actor actor, byte modelState, byte animState1, byte animState2) { }
    public virtual void OnEventEnvControl(BossModule module, byte index, uint state) { }
}
