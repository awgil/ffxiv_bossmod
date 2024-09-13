namespace BossMod.Stormblood.Dungeon.D15TheGhimlytDark.D151MarkIIIBMagitekColossus;

public enum OID : uint
{
    Boss = 0x25DA, // R3.500, x1
    Helper = 0x233C, // R0.500, x10, Helper type
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    CeruleumVent = 14195, // Boss->self, 4.0s cast, range 40 circle
    Exhaust = 14192, // Boss->self, 3.0s cast, range 40+R width 10 rect
    JarringBlow = 14190, // Boss->player, 4.0s cast, single-target
    MagitekRay = 14191, // Boss->players, 5.0s cast, range 6 circle // stack
    MagitekSlashFirstAOE = 14196, // Boss->self, 5.0s cast, range 20+R 60-degree cone
    MagitekSlashFirstCCW = 14670, // Boss->self, no cast, range 20+R ?-degree cone, 60-degree cone (same as above)
    MagitekSlashFirstCW = 14197, // Boss->self, no cast, range 20+R ?-degree cone, 60-degree cone (same as above)
    MagitekSlashHelper = 14671, // Helper->self, no cast, range 20+R ?-degree cone
    WildFireBeam = 14193, // Boss->self, no cast, single-target
    WildFireBeamHelper = 14194, // Helper->player, 5.0s cast, range 6 circle
}

public enum IconID : uint
{
    StackMarker = 62, // player
    Icon198 = 198, // player
    Icon139 = 139, // player
    // one of these is CW, the other one is CCW
    Icon168 = 168, // Boss
    Icon167 = 167, // Boss 
}

class CeruleumVent(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.CeruleumVent));
class Exhaust(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Exhaust), new AOEShapeRect(43.5f, 5));
class JarringBlow(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.JarringBlow));
class MagitekRay(BossModule module) : Components.StackWithIcon(module, (uint)IconID.StackMarker, ActionID.MakeSpell(AID.MagitekRay), 6, 0, 2, 4);
class WindFireBeam(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.WildFireBeamHelper), 6);

class D151MarkIIIBMagitekColossusStates : StateMachineBuilder
{
    public D151MarkIIIBMagitekColossusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<JarringBlow>()
            .ActivateOnEnter<WindFireBeam>()
            .ActivateOnEnter<Exhaust>()
            .ActivateOnEnter<CeruleumVent>()
            .ActivateOnEnter<MagitekRay>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 611, NameID = 7855)]
public class D151MarkIIIBMagitekColossus(WorldState ws, Actor primary) : BossModule(ws, primary, new(-180.6f, 68.5f), new ArenaBoundsCircle(20));
