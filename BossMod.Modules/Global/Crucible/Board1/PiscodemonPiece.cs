namespace BossMod.Global.Crucible.PiscodemonPiece;

public enum OID : uint
{
    Boss = 0x4B8A,
    Helper = 0x233C,
}

public enum AID : uint
{
    ThunderAuto = 50745, // Boss->player, no cast, single-target
    VoidBlizzardIIIBossCast = 46887, // Boss->self, 2.5+0.5s cast, single-target
    VoidBlizzardIIISpawn = 49886, // Helper->location, 4.5s cast, range 5 circle
    VoidBlizzardIIIBoss = 49887, // Boss->self, no cast, single-target
    VoidBlizzardIIIUnk = 46888, // Helper->location, 3.0s cast, single-target
    VoidBlizzardIIIFirst = 46889, // Helper->self, 6.0s cast, range 5 circle
    VoidBlizzardIIIRest = 46890, // Helper->self, no cast, range 5 circle
    ClearMind = 46898, // Boss->self, 4.0s cast, single-target
    ArcaneBlast = 46899, // Boss->self, 8.0s cast, range 100 circle
    VoidFlareStarBoss = 46893, // Boss->self, 2.5+0.5s cast, single-target
    VoidFlareStarSpawn = 46894, // Helper->location, 3.0s cast, single-target
    VoidFlareStar = 46895, // Helper->self, 6.0s cast, range 100 circle
    VoidThunderIIICast = 46891, // Boss->self, 3.0s cast, single-target
    VoidThunderIII = 46892, // Helper->self, 5.0s cast, range 50 width 10 cross
    VoidAeroIIICast = 46896, // Boss->self, 5.2+0.8s cast, single-target
    VoidAeroIII = 46897, // Helper->self, 6.0s cast, range 5-60 donut
}

public enum SID : uint
{
    DamageUp = 1225, // Boss->Boss, extra=0x0
}

class VoidBlizzardIIIPuddle(BossModule module) : Components.StandardAOEs(module, AID.VoidBlizzardIIISpawn, 5);
class VoidBlizzardIIIExaflare(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(5))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.VoidBlizzardIIIFirst)
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
        if ((AID)spell.Action.ID is AID.VoidBlizzardIIIFirst or AID.VoidBlizzardIIIRest)
        {
            var lines = Lines.FindIndex(l => l.Next.AlmostEqual(caster.Position, 1));
            if (lines >= 0)
                AdvanceLine(Lines[lines], caster.Position);
        }
    }
}

class ArcaneBlast(BossModule module) : Components.RaidwideCast(module, AID.ArcaneBlast)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Casters.Any(c => c.FindStatus(SID.DamageUp) != null && c.PendingDispels.Count == 0))
            hints.Add("Dispel!");
    }
}

class VoidThunderIII(BossModule module) : Components.StandardAOEs(module, AID.VoidThunderIII, new AOEShapeCross(50, 5));
class VoidAeroIII(BossModule module) : Components.StandardAOEs(module, AID.VoidAeroIII, new AOEShapeDonut(5, 60));

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

