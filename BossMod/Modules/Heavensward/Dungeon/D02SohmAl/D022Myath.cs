namespace BossMod.Heavensward.Dungeon.D02SohmAl.D022Myath;
public enum OID : uint
{
    Boss = 0xE91, // R4.900, x?
    RheumOfTheMountain = 0xE94, // R1.600, x?
    BloodOfTheMountain = 0xE95, // R1.600, x?
    ChymeOfTheMountain = 0xE92, // R3.000, x?
    Helper = 0x233C, // Helper
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Overbite = 3803, // E91->player, no cast, single-target
    RazorScales = 3804, // E91->self, 3.0s cast, range 60+R 60-degree cone
    PrimordialRoar = 3810, // E91->self, 3.0s cast, range 60+R circle
    MadDash = 3808, // E91->player, 5.0s cast, range 6 circle
    MadDashStack = 3809, // E91->players, 5.0s cast, range 6 circle
    TheLastSong = 4995, // E92->self, 12.0s cast, range 60 circle
}
public enum IconID : uint
{
    Spread = 140, // player
    Stackmarker = 62, // player
}

class RazorScales(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RazorScales), new AOEShapeCone(60, 30.Degrees()));
class PrimordialRoar(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.PrimordialRoar));
class MadDash(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.MadDash), 6);
class MadDashStack(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.MadDashStack), 6, 2);
class TheLastSong(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.TheLastSong));
class Adds(BossModule module) : Components.Adds(module, (uint)OID.ChymeOfTheMountain)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.ChymeOfTheMountain => 2,
                OID.Boss => 1,
                _ => 0
            };
    }
}
class D022MyathStates : StateMachineBuilder
{
    public D022MyathStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RazorScales>()
            .ActivateOnEnter<PrimordialRoar>()
            .ActivateOnEnter<MadDash>()
            .ActivateOnEnter<MadDashStack>()
            .ActivateOnEnter<TheLastSong>()
            .ActivateOnEnter<Adds>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 37, NameID = 3793)]
public class D022Myath(WorldState ws, Actor primary) : BossModule(ws, primary, new(158, -94), new ArenaBoundsCircle(30f));
