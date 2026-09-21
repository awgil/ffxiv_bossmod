#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Global.Crucible.StrixPiece;

public enum OID : uint
{
    Boss = 0x4CB6, // R2.340, x1
    _Gen_TomePiece = 0x4CB7, // R1.050, x8
    _Gen_StrixPlume = 0x4CB8, // R1.500, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, x26, Helper type

    // all radius 6
    SlimeZone = 0x1EC0E0,
    ChickenZone = 0x1EC0E1,
    WindZone = 0x1E9582,
}

public enum AID : uint
{
    _Spell_Aero = 50929, // Boss->player, no cast, single-target
    _Weaponskill_Plummet = 48654, // Boss->self, 3.0s cast, single-target
    _Weaponskill_Plummet1 = 48655, // Helper->self, 7.0+0.5s cast, single-target
    _Weaponskill_Plummet2 = 48656, // Helper->self, 7.5s cast, range 10 circle
    _Spell_OnThePropertiesOfQuakes = 48657, // Boss->self, 8.0s cast, range 60 circle
    _Spell_MagicalMalletTheory = 48659, // Boss->self, 8.0s cast, single-target
    _Spell_MagicalMalletTheory1 = 48660, // Helper->player, no cast, range 8 circle
    _Ability_UltimateFocus = 48668, // Boss->self, 3.0s cast, single-target
    _Spell_OnThePropertiesOfDarkness = 48669, // Boss->self, 8.0s cast, range 100 circle
    _Spell_OnThePropertiesOfFloods = 48658, // Boss->self, 8.0s cast, range 60 circle
    _Weaponskill_WindfeatherWhisper = 48666, // Boss->self, 3.0s cast, single-target
    _Spell_AeroIII = 48667, // 4CB8->self, 12.0s cast, range 50 circle
}

public enum SID : uint
{
    _Gen_Levitation = 12, // none->player, extra=0x0
    _Gen_Transfiguration = 1608, // none->player, extra=0x1D3
    _Gen_DownForTheCount = 3908, // Helper->player, extra=0xEC7
    _Gen_MagicDamageUp = 5020, // Boss->Boss, extra=0x0
    _Gen_Bleeding = 3077, // none->player, extra=0x0
    _Gen_Bleeding1 = 3078, // none->player, extra=0x0
}

class Plummet(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Plummet2, 10, maxCasts: 6);

class Properties(BossModule module) : BossComponent(module)
{
    uint _goalID;
    DateTime _deadline;

    readonly List<Actor> Zones = [];

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var z in Zones)
        {
            var isSafe = z.OID == _goalID;
            Arena.AddCircle(z.Position, 6, isSafe ? ArenaColor.Safe : ArenaColor.Danger);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var z in Zones)
        {
            if (z.OID == _goalID)
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(z.Position, 6), _deadline);
            else if (z.OID != (uint)OID.WindZone)
                hints.AddForbiddenZone(ShapeDistance.Circle(z.Position, 6), DateTime.MaxValue);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Zones.FirstOrDefault(z => z.OID == _goalID) is { } z)
            hints.Add("Go to safe zone!", !actor.Position.InCircle(z.Position, 6));
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.SlimeZone or OID.ChickenZone or OID.WindZone)
            Zones.Add(actor);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008)
            Zones.Remove(actor);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Spell_OnThePropertiesOfQuakes:
                _goalID = (uint)OID.WindZone;
                _deadline = Module.CastFinishAt(spell, -1); // probably a bit of delay for levitation to activate
                break;
            case AID._Spell_MagicalMalletTheory:
                _goalID = (uint)OID.SlimeZone;
                _deadline = Module.CastFinishAt(spell); // helper cast happens about 0.8s after boss cast, again we want to move in early so the debuff applies
                break;
            case AID._Spell_OnThePropertiesOfFloods:
                _goalID = (uint)OID.ChickenZone;
                _deadline = Module.CastFinishAt(spell, -1);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Spell_MagicalMalletTheory1 or AID._Spell_OnThePropertiesOfQuakes or AID._Spell_OnThePropertiesOfFloods)
        {
            _goalID = 0;
            _deadline = default;
        }
    }
}

class OnThePropertiesOfDarkness(BossModule module) : Components.RaidwideCast(module, AID._Spell_OnThePropertiesOfDarkness)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Casters.FirstOrDefault(c => c.FindStatus(SID._Gen_MagicDamageUp) != null) != null)
            hints.Add("Dispel!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        foreach (var c in Casters)
            if (c.FindStatus(SID._Gen_MagicDamageUp) != null && c.PendingDispels.Count == 0 && hints.FindEnemy(c) is { } e)
                e.ShouldBeDispelled = true;
    }
}

class StrixPieceStates : StateMachineBuilder
{
    public StrixPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Plummet>()
            .ActivateOnEnter<Properties>()
            .ActivateOnEnter<OnThePropertiesOfDarkness>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1091, NameID = 14596)]
public class StrixPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, -420), new ArenaBoundsCircle(20));

