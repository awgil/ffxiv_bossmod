namespace BossMod.Endwalker.Dungeon.D04Ktisis.D043Hermes;

public enum OID : uint
{
    Boss = 0x348A, // R4.200, x1
    Helper = 0x233C, // R0.500, x12, 523 type
    Meteor = 0x348C, // R2.400, x0 (spawn during fight)
    Karukeion = 0x348B, // R1.000, x8
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    CosmicKiss = 25891, // Meteor->self, 5.0s cast, range 40 circle
    Double = 25892, // Boss->self, 3.0s cast, single-target

    Hermetica1 = 25888, // Boss->self, 3.0s cast, single-target
    Hermetica2 = 25893, // Boss->self, 6.0s cast, single-target
    Hermetica3 = 25895, // Boss->self, 12.0s cast, single-target

    Meteor = 25890, // Boss->self, 3.0s cast, single-target
    Quadruple = 25894, // Boss->self, 3.0s cast, single-target
    Trismegistos = 25886, // Boss->self, 5.0s cast, range 40 circle // Raidwide

    TrueAeroVisual = 25899, // Boss->self, 5.0s cast, single-target // protean
    TrueAeroProtean = 25900, // Helper->player, no cast, range 40 width 6 rect
    TrueAeroBait = 25901, // Helper->self, 2.5s cast, range 40 width 6 rect

    TrueAeroII1 = 25896, // Boss->self, 5.0s cast, single-target
    TrueAeroII2 = 25897, // Helper->player, 5.0s cast, range 6 circle
    TrueAeroII3 = 25898, // Helper->location, 3.5s cast, range 6 circle

    TrueAeroIV1 = 25889, // 348B->self, 4.0s cast, range 50 width 10 rect
    TrueAeroIVLOS = 27836, // 348B->self, 4.0s cast, range 50 width 10 rect
    TrueAeroIV3 = 27837, // 348B->self, 10.0s cast, range 50 width 10 rect

    TrueBravery = 25907, // Boss->self, 5.0s cast, single-target

    TrueTornado1 = 25902, // Boss->self, 5.0s cast, single-target // Tankbuster
    TrueTornado2 = 25903, // Boss->self, no cast, single-target
    TrueTornado3 = 25904, // Boss->self, no cast, single-target
    TrueTornado4 = 25905, // Helper->player, no cast, range 4 circle //Tankbuster
    TrueTornadoAOE = 25906, // Helper->location, 2.5s cast, range 4 circle //LocationBaitedAOE 

    UnknownAbility = 25887, // Helper->player, no cast, single-target
}

public enum SID : uint
{
    Double = 661, // Boss->Boss, extra=0x0
    VulnerabilityUp = 1789, // Helper->player, extra=0x1
    Quadruple = 2732, // Boss->Boss, extra=0x0
}

public enum IconID : uint
{
    Tankbuster = 218, // player
    Spreadmarker = 139, // player
}

public enum TetherID : uint
{
    Tether_160 = 160, // Karukeion->Boss
}

class TrueBraveryInterruptHint(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.TrueBravery));
class Trismegistos(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Trismegistos));

class TrueTornado1(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.TrueTornado1));
class TrueTornado4(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.TrueTornado4));
class TrueTornadoAOE(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.TrueTornadoAOE), 4);

//class TrueAeroProtean(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, ActionID.MakeSpell(AID.TrueAeroProtean), new AOEShapeRect(20, 3));
class TrueAeroBait(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TrueAeroBait), new AOEShapeRect(20, 3));

class TrueAeroII2(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.TrueAeroII2), new AOEShapeCircle(6));
class TrueAeroII3(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.TrueAeroII3), 6);

class TrueAeroIV1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TrueAeroIV1), new AOEShapeRect(50, 5, 50));
class TrueAeroIV3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TrueAeroIV3), new AOEShapeRect(50, 5, 50));

class CosmicKiss(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CosmicKiss), new AOEShapeCircle(10));

class TrueAeroIVLOS(BossModule module) : Components.CastLineOfSightAOE(module, ActionID.MakeSpell(AID.TrueAeroIVLOS), 50, false)
{
    public override IEnumerable<Actor> BlockerActors() => Module.Enemies(OID.Meteor);
}

class D043HermesStates : StateMachineBuilder
{
    public D043HermesStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CosmicKiss>()
            .ActivateOnEnter<TrueAeroBait>()
            .ActivateOnEnter<TrueAeroII2>()
            .ActivateOnEnter<TrueAeroII3>()
            .ActivateOnEnter<TrueAeroIV1>()
            .ActivateOnEnter<TrueAeroIVLOS>()
            .ActivateOnEnter<TrueAeroIV3>()
            .ActivateOnEnter<TrueTornado1>()
            .ActivateOnEnter<TrueTornado4>()
            .ActivateOnEnter<TrueTornadoAOE>()
            .ActivateOnEnter<Trismegistos>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 787, NameID = 10363)]
public class D043Hermes(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -50), new ArenaBoundsCircle(20));
