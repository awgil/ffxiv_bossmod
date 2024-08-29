namespace BossMod.RealmReborn.Dungeon.D29TheKeeperoftheLake.D291Einhander;

public enum OID : uint
{
    Boss = 0x3927, // R2.600, x1
    Astraea = 0x3928, // R2.000, x6
    AuxiliaryCeruleumTank = 0x3929, // R1.500, x6
    LoyalBiast = 0xE56, // R2.700, x1
    LoyalDragonfly = 0xE55, // R0.800, x2
    // Pretty sure these are the birds that fly into the areana and drop the tanks down? Only thought I can have to WHO these are.
    Actor1e9768 = 0x1E9768, // R2.000, x1, EventObj type
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x2, EventObj type
    Actor1e8f2f = 0x1E8F2F, // R0.500, x2, EventObj type
    Actor1e975b = 0x1E975B, // R2.000, x1, EventObj type
    Actor1e976a = 0x1E976A, // R2.000, x1, EventObj type
    Actor1e975e = 0x1E975E, // R2.000, x0 (spawn during fight), EventObj type
    Actor1e975a = 0x1E975A, // R2.000, x0 (spawn during fight), EventObj type
    Actor1e9767 = 0x1E9767, // R2.000, x0 (spawn during fight), EventObj type

}

public enum AID : uint
{
    AeroBlast = 29273, // Boss->self, 4.0s cast, range 40 circle, done?
    AutoAttack = 870, // Boss->player, no cast, single-target
    CeruleumExplosion = 29275, // 3929->self, 8.0s cast, range 12 circle
    HeavySwing = 29620, // Boss->player, 5.0s cast, single-target
    MarkXLIIIMiniCannon = 29272, // Boss->location, 5.0s cast, range 31 circle 
    MarkXLIQuickFiringCannon = 29271, // Boss->self, 5.0s cast, range 40 width 4 rect, done?
    ResoundingScreech = 29270, // Boss->self, 3.0s cast, single-target, not necessary to input, summon's tanks around the arena
    UnknownAbility = 29646, // Boss->location, no cast, single-target
}

public enum IconID : uint
{
    RedTankBuster = 218, // player
}

class AeroBlast(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AeroBlast), "Raidwide");
class MarkXLILineCannon(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MarkXLIQuickFiringCannon), new AOEShapeRect(40, 2));
class MarkXLILocationCannon(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.MarkXLIIIMiniCannon), 17, "Proximity aoe, get out!");
class CerulemTankAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CeruleumExplosion), new AOEShapeCircle(12));
class HeavySwingTB(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.HeavySwing), "Tank Buster");

class Bomb1(BossModule module) : Components.Adds(module, (uint)OID.AuxiliaryCeruleumTank);

class D291EinhanderStates : StateMachineBuilder
{
    public D291EinhanderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AeroBlast>()
            .ActivateOnEnter<MarkXLILineCannon>()
            .ActivateOnEnter<MarkXLILocationCannon>()
            .ActivateOnEnter<CerulemTankAOE>()
            .ActivateOnEnter<HeavySwingTB>()
            .ActivateOnEnter<Bomb1>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 32, NameID = 3369)]
public class D291Einhander(WorldState ws, Actor primary) : BossModule(ws, primary, new(18.85f, -17), new ArenaBoundsCircle(20f));
