namespace BossMod.Global.Crucible.PiscodemonPiece;

public enum OID : uint
{
    Boss = 0x4B8A,
    Helper = 0x233C,
}

public enum AID : uint
{
    _Spell_Thunder = 50745, // Boss->player, no cast, single-target
    _Ability_VoidBlizzardIII = 46887, // Boss->self, 2.5+0.5s cast, single-target
    _Ability_VoidBlizzardIII1 = 46888, // Helper->location, 3.0s cast, single-target
    _Ability_VoidBlizzardIII2 = 49886, // Helper->location, 4.5s cast, range 5 circle
    _Ability_VoidBlizzardIII3 = 49887, // Boss->self, no cast, single-target
    _Spell_VoidBlizzardIII = 46889, // Helper->self, 6.0s cast, range 5 circle
    _Spell_VoidBlizzardIII1 = 46890, // Helper->self, no cast, range 5 circle
    _Ability_ClearMind = 46898, // Boss->self, 4.0s cast, single-target
    _Spell_ArcaneBlast = 46899, // Boss->self, 8.0s cast, range 100 circle
    _Ability_VoidFlareStar = 46893, // Boss->self, 2.5+0.5s cast, single-target
    _Spell_VoidFlareStar = 46894, // Helper->location, 3.0s cast, single-target
    _Spell_VoidFlareStar1 = 46895, // Helper->self, 6.0s cast, range 100 circle
    _Ability_VoidThunderIII = 46891, // Boss->self, 3.0s cast, single-target
    _Spell_VoidThunderIII = 46892, // Helper->self, 5.0s cast, range 50 width 10 cross
    _Ability_VoidAeroIII = 46896, // Boss->self, 5.2+0.8s cast, single-target
    _Spell_VoidAeroIII = 46897, // Helper->self, 6.0s cast, range 5-60 donut
}

public enum SID : uint
{
    _Gen_DamageUp = 1225, // Boss->Boss, extra=0x0
}

class VoidBlizzardIIIPuddle(BossModule module) : Components.StandardAOEs(module, AID._Ability_VoidBlizzardIII2, 5);
class VoidBlizzardIIIExaflare(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(5))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Spell_VoidBlizzardIII)
        {
            Lines.Add(new()
            {
                Next = spell.LocXZ,
                Advance = new(0, 7),
                Rotation = spell.Rotation,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 1.2f,
                ExplosionsLeft = 6,
                MaxShownExplosions = 3
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Spell_VoidBlizzardIII or AID._Spell_VoidBlizzardIII1)
        {
            var lines = Lines.FindIndex(l => l.Next.AlmostEqual(caster.Position, 1));
            if (lines >= 0)
                AdvanceLine(Lines[lines], caster.Position);
        }
    }
}

class ArcaneBlast(BossModule module) : Components.RaidwideCast(module, AID._Spell_ArcaneBlast)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Casters.Any(c => c.FindStatus(SID._Gen_DamageUp) != null))
            hints.Add("Dispel!");
    }
}

class VoidThunderIII(BossModule module) : Components.StandardAOEs(module, AID._Ability_VoidThunderIII, new AOEShapeCross(50, 5));
class VoidAeroIII(BossModule module) : Components.StandardAOEs(module, AID._Spell_VoidAeroIII, new AOEShapeDonut(5, 60));

// todo: falloff range for void flare star

class PiscodemonPieceStates : StateMachineBuilder
{
    public PiscodemonPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VoidBlizzardIIIPuddle>()
            .ActivateOnEnter<VoidBlizzardIIIExaflare>()
            .ActivateOnEnter<ArcaneBlast>()
            .ActivateOnEnter<VoidThunderIII>()
            .ActivateOnEnter<VoidAeroIII>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1088, NameID = 14535)]
public class PiscodemonPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, 0), new ArenaBoundsSquare(20));

