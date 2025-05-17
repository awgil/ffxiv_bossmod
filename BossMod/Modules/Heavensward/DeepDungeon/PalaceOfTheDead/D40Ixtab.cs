namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.D40Ixtab;

public enum OID : uint
{
    Boss = 0x16B9, // R3.800, x1
    AccursedPoxVoidZone = 0x1E8EA9, // R0.500, x0 (spawn during fight), EventObj type
    NightmareBhoot = 0x1764, // R1.800, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 6498, // Boss->player, no cast, single-target

    AccursedPox = 6434, // Boss->location, 3.0s cast, range 8 circle
    AncientEruption = 6430, // Boss->location, 2.5s cast, range 4 circle
    Blizzard = 967, // NightmareBhoot->player, 1.0s cast, single-target
    EntropicFlame = 6431, // Boss->self, 3.0s cast, range 50+R width 8 rect
    Scream = 6433, // Boss->self, 3.0s cast, range 25 circle
    ShadowFlare = 6432, // Boss->self, 3.0s cast, range 25+R circle
}

class Adds(BossModule module) : Components.Adds(module, (uint)OID.NightmareBhoot)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.NightmareBhoot => 2,
                OID.Boss => 1,
                _ => 0
            };
    }
}
class AccursedPox(BossModule module) : Components.StandardAOEs(module, AID.AccursedPox, 8);
class AncientEruption(BossModule module) : Components.StandardAOEs(module, AID.AncientEruption, 4);
class AncientEruptionZone(BossModule module) : Components.PersistentInvertibleVoidzone(module, 4, m => m.Enemies(OID.AccursedPoxVoidZone).Where(z => z.EventState != 7));
class EntropicFlame(BossModule module) : Components.StandardAOEs(module, AID.EntropicFlame, new AOEShapeRect(53.8f, 4));
class Scream(BossModule module) : Components.RaidwideCast(module, AID.Scream, "Raidwide + Fear, Adds need to be dead by now");
class ShadowFlare(BossModule module) : Components.RaidwideCast(module, AID.ShadowFlare);

class D40IxtabStates : StateMachineBuilder
{
    public D40IxtabStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<AccursedPox>()
            .ActivateOnEnter<AncientEruption>()
            .ActivateOnEnter<AncientEruptionZone>()
            .ActivateOnEnter<EntropicFlame>()
            .ActivateOnEnter<Scream>()
            .ActivateOnEnter<ShadowFlare>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 177, NameID = 5025)]
public class D40Ixtab(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -226), new ArenaBoundsCircle(24));
