namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.D140AhPuch;

public enum OID : uint
{
    Boss = 0x181B, // R3.800, x1
    DeepPalaceFollower = 0x1906, // R1.800, x0 (spawn during fight)
    AccursedPoxVoidZone = 0x1E8EA9, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 6498, // Boss->player, no cast, single-target
    AccursedPox = 7146, // Boss->location, 3.0s cast, range 8 circle
    AncientEruption = 7142, // Boss->location, 2.5s cast, range 4 circle
    Blizzard = 967, // DeepPalaceFollower->player, 1.0s cast, single-target
    EntropicFlame = 7143, // Boss->self, 3.0s cast, range 50+R width 8 rect
    Scream = 7145, // Boss->self, 3.0s cast, range 30 circle
    ShadowFlare = 7144, // Boss->self, 3.0s cast, range 25+R circle
}

class Adds(BossModule module) : Components.Adds(module, (uint)OID.DeepPalaceFollower)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.DeepPalaceFollower => 2,
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

class D140AhPuchStates : StateMachineBuilder
{
    public D140AhPuchStates(BossModule module) : base(module)
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

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 212, NameID = 5410)]
public class D140AhPuch(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -237), new ArenaBoundsCircle(24));
