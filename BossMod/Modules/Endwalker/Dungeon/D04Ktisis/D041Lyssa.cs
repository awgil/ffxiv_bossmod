namespace BossMod.Endwalker.Dungeon.D04Ktisis.D041Lyssa;

public enum OID : uint
{
    Boss = 0x3323, // R=4.0
    Helper = 0x233C,
    IcePillar = 0x3324, // R2.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    FrigidStomp = 25181, // Boss->self, 5.0s cast, range 50 circle //Raidwide
    FrostbiteAndSeek = 25175, // Boss->self, 3.0s cast, single-target
    HeavySmash = 25180, // Boss->players, 5.0s cast, range 6 circle //Stack
    IcePillar = 25179, // IcePillar->self, 3.0s cast, range 4 circle //SelfAOE
    Icicall = 25178, // Boss->self, 3.0s cast, single-target //Ice Spawn
    PillarPierceAOE = 25375, // IcePillar->self, 5.0s cast, range 80 width 4 rect
    PunishingSliceVisual = 25176, // Boss->self, no cast, single-target
    PunishingSliceAOE = 25177, // Helper->self, 2.0s cast, range 50 width 50 rect
    SkullDasher = 25182, // Boss->player, 5.0s cast, single-target // Tankbuster
    Unknown = 28304, // Helper->self, no cast, single-target
}

public enum IconID : uint
{
    Tankbuster = 218, // player
    Stackmarker = 62, // player
}

class PillarPierceAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PillarPierceAOE), new AOEShapeRect(40, 2));
class PunishingSliceAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PunishingSliceAOE), new AOEShapeRect(50, 50));
class IcePillar(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.IcePillar), new AOEShapeCircle(4));

class HeavySmash(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.HeavySmash), 6, 8);
class SkullDasher(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.SkullDasher));
class FrigidStomp(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.FrigidStomp));

class D041LyssaStates : StateMachineBuilder
{
    public D041LyssaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PillarPierceAOE>()
            .ActivateOnEnter<PunishingSliceAOE>()
            .ActivateOnEnter<IcePillar>()
            .ActivateOnEnter<HeavySmash>()
            .ActivateOnEnter<SkullDasher>()
            .ActivateOnEnter<FrigidStomp>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 787, NameID = 10396)]
public class D041Lyssa(WorldState ws, Actor primary) : BossModule(ws, primary, new(-144, 49), new ArenaBoundsCircle(19.5f));
