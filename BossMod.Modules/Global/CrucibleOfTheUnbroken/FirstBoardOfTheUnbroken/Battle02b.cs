namespace BossMod.Global.CrucibleOfTheUnbroken.FirstBoardOfTheUnbroken.Battle02b;

public enum OID : uint
{
    Boss = 0x4B8A,
    Helper = 0x233C,
}
public enum AID : uint
{
    Thunder = 50745, // Boss->player, no cast, single-target

    VoidBlizzardIIIAOEQuick = 46888, // Helper->location, 3.0s cast, single-target
    VoidBlizzardIIIAOESlow = 49886, // Helper->location, 4.5s cast, range 5 circle
    VoidBlizzardIIIExaflareFirst = 46889, // Helper->self, 6.0s cast, range 5 circle
    VoidBlizzardIIIExaflareRest = 46890, // Helper->self, no cast, range 5 circle
    VoidBlizzardIII = 46887, // Boss->self, 2.5+0.5s cast, single-target
    VoidBlizzardIII2 = 49887, // Boss->self, no cast, single-target

    ClearMind = 46898, // Boss->self, 4.0s cast, single-target
    ArcaneBlast = 46899, // Boss->self, 8.0s cast, range 100 circle
    VoidFlareStarVisual = 46893, // Boss->self, 2.5+0.5s cast, single-target
    VoidFlareStarVisual2 = 46894, // Helper->location, 3.0s cast, single-target
    VoidFlareStar = 46895, // Helper->self, 6.0s cast, range 100 circle
    VoidThunderIIIVisual = 46891, // Boss->self, 3.0s cast, single-target
    VoidThunderIII = 46892, // Helper->self, 5.0s cast, range 50 width 10 cross
    VoidAeroIIIVisual = 46896, // Boss->self, 5.2+0.8s cast, single-target
    VoidAeroIII = 46897, // Helper->self, 6.0s cast, range 5-60 donut
}
class VoidBlizzardAOEs(BossModule module) : Components.GroupedAOEs(module, [AID.VoidBlizzardIIIAOEQuick, AID.VoidBlizzardIIIAOESlow], new AOEShapeCircle(5f));
class VoidBlizzardExaflares(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(6))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.VoidBlizzardIIIExaflareFirst)
            Lines.Add(new()
            {
                Next = caster.Position,
                Advance = caster.Rotation.ToDirection() * 7.5f,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 1.1f,
                ExplosionsLeft = 6,
                MaxShownExplosions = 3
            });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.VoidBlizzardIIIExaflareFirst or AID.VoidBlizzardIIIExaflareRest)
        {
            NumCasts++;
            var l = Lines.FindIndex(l => l.Next.AlmostEqual(caster.Position, 1));
            if (l >= 0)
                AdvanceLine(Lines[l], caster.Position);
        }
    }
}

class ArcaneBlast(BossModule module) : Components.RaidwideCast(module, AID.ArcaneBlast);
class VoidFlareStar(BossModule module) : Components.StandardAOEs(module, AID.VoidFlareStar, new AOEShapeCircle(20));
class VoidThunderIII(BossModule module) : Components.StandardAOEs(module, AID.VoidThunderIII, new AOEShapeCross(50, 5f));
class VoidAeroIII(BossModule module) : Components.StandardAOEs(module, AID.VoidAeroIII, new AOEShapeDonut(5, 60));

class Battle02bStates : StateMachineBuilder
{
    public Battle02bStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VoidBlizzardAOEs>()
            .ActivateOnEnter<VoidBlizzardExaflares>()
            .ActivateOnEnter<ArcaneBlast>()
            .ActivateOnEnter<VoidFlareStar>()
            .ActivateOnEnter<VoidThunderIII>()
            .ActivateOnEnter<VoidAeroIII>();
    }
}

[ModuleInfo(Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1088, NameID = 14535)]
public class Battle02b(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, 0), new ArenaBoundsSquare(20));
