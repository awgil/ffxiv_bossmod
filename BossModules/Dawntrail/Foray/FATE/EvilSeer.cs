namespace BossMod.Dawntrail.Foray.FATE.EvilSeer;

public enum OID : uint
{
    Boss = 0x4BA7, // R5.750, x1
    AccursedOrb = 0x4BA8, // R2.000, x5 (spawn during fight)
    EvilSeer = 0x4BAA, // R0.500, x2 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack1 = 47146, // Boss->player, no cast, single-target
    AllEyes = 47147, // Boss->self, 3.0+0.5s cast, range 30 circle
    SeeNoEvil = 47148, // Boss->self, 5.0s cast, range 30 circle
    ColdStare = 47149, // Boss->self, 4.0s cast, range 40 90-degree cone
    JettaturaCast = 47150, // Boss->self, 3.0s cast, single-target
    Jettatura = 47151, // EvilSeer->location, 4.0s cast, range 8 circle
    SinisterSight = 47152, // AccursedOrb->location, 5.0s cast, range 50 circle
    AutoAttack2 = 45338, // Boss->player, no cast, single-target
}

class SinisterSight : Components.CastGaze
{
    public SinisterSight(BossModule module) : base(module, AID.SinisterSight, false, 50)
    {
        DrawEyeRange = false;
    }
}
class SeeNoEvil : Components.CastGaze
{
    public SeeNoEvil(BossModule module) : base(module, AID.SeeNoEvil, false, 30)
    {
        DrawEyeRange = false;
    }
}

class Jettatura(BossModule module) : Components.StandardAOEs(module, AID.Jettatura, 8);
class ColdStare(BossModule module) : Components.StandardAOEs(module, AID.ColdStare, new AOEShapeCone(40, 45.Degrees()));
class AllEyes(BossModule module) : Components.RaidwideCast(module, AID.AllEyes);

class EvilSeerStates : StateMachineBuilder
{
    public EvilSeerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SinisterSight>()
            .ActivateOnEnter<Jettatura>()
            .ActivateOnEnter<ColdStare>()
            .ActivateOnEnter<AllEyes>()
            .ActivateOnEnter<SeeNoEvil>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14726)]
public class EvilSeer(WorldState ws, Actor primary) : BossModule(ws, primary, new(510, -30), new ArenaBoundsCircle(30));

