namespace BossMod.Dawntrail.Dungeon.D06Alexandria.D063Eliminator;

public enum OID : uint
{
    Boss = 0x41CE, // R6.001, x1
    Helper = 0x233C, // R0.500, x20, Helper type
    EliminationClaw = 0x41CF, // R2.000, x1, add that casts line
    Elimbit = 0x41D0, // R2.000, x1, add that casts donut
    LightningGenerator = 0x41D1, // R3.000, x6, add that needs to be killed in intermission
}

public enum AID : uint
{
    AutoAttack = 36764, // Boss->player, no cast, single-target
    Teleport = 36763, // Boss->location, no cast, single-target
    Disruption = 36765, // Boss->self, 5.0s cast, range 60 circle, raidwide
    PartitionInstantR = 36766, // Boss->self, no cast, single-target, visual (instant cleave right after reconfigured)
    PartitionInstantL = 36767, // Boss->self, no cast, single-target, visual (instant cleave left after reconfigured)
    PartitionShortR = 36768, // Boss->self, 4.3+0.7s cast, single-target, visual (cleave right)
    PartitionShortL = 36769, // Boss->self, 4.3+0.7s cast, single-target, visual (cleave left)
    ReconfiguredPartitionR = 39247, // Boss->self, 1.2+5.6s cast, single-target, visual (right sword -> left cleave)
    ReconfiguredPartitionL = 39248, // Boss->self, 1.2+5.6s cast, single-target, visual (left sword -> right cleave)
    PartitionLongR = 39599, // Boss->self, 6.2+0.6s cast, single-target, visual (cleave right)
    PartitionLongL = 39600, // Boss->self, 6.2+0.6s cast, single-target, visual (cleave left)
    PartitionShortAOER = 39007, // Helper->self, 5.0s cast, range 40 180-degree cone (cleave right)
    PartitionShortAOEL = 39008, // Helper->self, 5.0s cast, range 40 180-degree cone (cleave left)
    PartitionLongAOER = 39238, // Helper->self, 7.0s cast, range 40 180-degree cone (cleave right)
    PartitionLongAOEL = 39249, // Helper->self, 7.0s cast, range 40 180-degree cone (cleave left)
    Electray = 39243, // Helper->player, 5.0s cast, range 6 circle spread
    OverexposureTargetSelect = 36778, // Helper->none, no cast, single-target, visual (target select for line stack)
    Overexposure = 36779, // Boss->self, 4.3+0.7s cast, single-target, visual (line stack)
    OverexposureAOE = 36780, // Helper->self, no cast, range 40 width 6 rect

    Subroutine1 = 36772, // Boss->self, 3.0s cast, single-target, visual (line add spawn)
    Terminate = 36773, // EliminationClaw->self, 6.2+0.6s cast, single-target
    TerminateAOE = 39615, // Helper->self, 7.0s cast, range 40 width 10 rect
    Subroutine1End = 36774, // Boss->self, no cast, single-target, visual (mechanic end)

    Subroutine2 = 36775, // Boss->self, 3.0s cast, single-target, visual (donut add spawn)
    HaloOfDestruction = 36776, // Elimbit->self, 6.4+0.4s cast, single-target, visual (donut)
    HaloOfDestructionAOE = 39616, // Helper->self, 7.0s cast, range 6-40 donut
    Subroutine2End = 36777, // Boss->self, no cast, single-target, visual (mechanic end)

    Subroutine3 = 36781, // Boss->self, 3.0s cast, single-target, visual (generators spawn or later mechanic start)
    LightningGeneratorCharge = 36791, // LightningGenerator->Boss, no cast, single-target, visual (add charges boss gauge)
    LightOfSalvation = 36782, // Elimbit->self, 6.0s cast, single-target, visual (proteans)
    LightOfSalvationVisual = 36783, // Helper->player, 5.9s cast, single-target, visual (proteans vfx at target)
    LightOfSalvationAOE = 36784, // Helper->self, no cast, range 40 width 6 rect, protean
    LightOfDevotion = 36785, // EliminationClaw->self, 5.0s cast, single-target, visual (line stack)
    LightOfDevotionTargetSelect = 36786, // Helper->none, no cast, single-target
    LightOfDevotionAOE = 36787, // Helper->self, no cast, range 40 width 6 rect, line stack
    Subroutine3End = 36788, // Boss->self, no cast, single-target, visual (mechanic end)

    HoloArk = 36789, // Boss->self, no cast, single-target, visual (raidwide/wipe depending on gauge)
    HoloArkAOE = 36790, // Helper->self, no cast, range 60 circle, raidwide
    Compression = 36792, // EliminationClaw->location, 5.3s cast, single-target, visual (knockback + aoe)
    CompressionAOE = 36793, // Helper->self, 6.0s cast, range 6 circle
    CompressionImpact = 36794, // Helper->self, 6.0s cast, range 60 circle, knockback 15
    Elimination = 36795, // Boss->self, 4.0s cast, single-target, visual (a series of raidwides followed by staggered lines)
    EliminationAOE = 36796, // Helper->self, no cast, range 60 circle, raidwide
    EliminationExplosion = 39239, // Helper->self, 8.5s cast, range 50 width 8 rect, staggered lines
}

public enum IconID : uint
{
    Electray = 139, // player
    LightOfSalvation = 534, // Helper
}

class Disruption(BossModule module) : Components.RaidwideCast(module, AID.Disruption);
class PartitionShortR(BossModule module) : Components.StandardAOEs(module, AID.PartitionShortAOER, new AOEShapeCone(40, 90.Degrees()));
class PartitionShortL(BossModule module) : Components.StandardAOEs(module, AID.PartitionShortAOEL, new AOEShapeCone(40, 90.Degrees()));
class PartitionLongR(BossModule module) : Components.StandardAOEs(module, AID.PartitionLongAOER, new AOEShapeCone(40, 90.Degrees()));
class PartitionLongL(BossModule module) : Components.StandardAOEs(module, AID.PartitionLongAOEL, new AOEShapeCone(40, 90.Degrees()));
class HaloOfDestruction(BossModule module) : Components.StandardAOEs(module, AID.HaloOfDestructionAOE, new AOEShapeDonut(6, 40));

class Terminate(BossModule module) : Components.StandardAOEs(module, AID.TerminateAOE, new AOEShapeRect(40, 5))
{
    private readonly HaloOfDestruction? _halo = module.FindComponent<HaloOfDestruction>();
    private static readonly AOEShapeRect _shapeOverlap = new(40, 4);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // when overlapping with halo, halo always resolves first - but we still wanna stay closer to the edge
        foreach (var aoe in ActiveAOEs(slot, actor))
        {
            var shape = aoe.Shape;
            var origin = aoe.Origin;
            if (_halo?.Casters.Count > 0)
            {
                shape = _shapeOverlap;
                origin.Z += (origin.Z - Module.Center.Z) switch
                {
                    <= -10 => -1,
                    >= +10 => +1,
                    _ => 0
                };
            }
            hints.AddForbiddenZone(shape, origin, aoe.Rotation, aoe.Activation);
        }
    }
}

class Electray(BossModule module) : Components.SpreadFromCastTargets(module, AID.Electray, 6)
{
    private readonly HaloOfDestruction? _halo = module.FindComponent<HaloOfDestruction>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // only spread after aoes are done
        if (_halo == null || _halo.Casters.Count == 0)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class Overexposure(BossModule module) : Components.SimpleLineStack(module, 3, 40, AID.OverexposureTargetSelect, AID.OverexposureAOE, 5.1f);

class CompressionAOE(BossModule module) : Components.StandardAOEs(module, AID.CompressionAOE, new AOEShapeCircle(6));
class CompressionImpact(BossModule module) : Components.KnockbackFromCastTarget(module, AID.CompressionImpact, 15)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in Casters)
            hints.AddForbiddenZone(ShapeContains.InvertedCone(c.Position, 8, Angle.FromDirection(Module.Center - c.Position), 30.Degrees()), Module.CastFinishAt(c.CastInfo)); // just a hack...
    }
}

class LightningGenerator(BossModule module) : Components.Adds(module, (uint)OID.LightningGenerator);

class LightOfSalvation(BossModule module) : Components.BaitAwayCast(module, AID.LightOfSalvationVisual, new AOEShapeRect(40, 3), false, true)
{
    private readonly CompressionImpact? _impact = module.FindComponent<CompressionImpact>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // only show proteans after knockback is done
        if (_impact == null || _impact.Casters.Count == 0)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        // note: actual aoe happens slightly after visual cast; but if adds are killed quickly enough, it might not happen at all
        if ((AID)spell.Action.ID is AID.LightOfSalvationAOE or AID.Subroutine3End or AID.LightOfDevotionAOE)
            CurrentBaits.Clear();
    }
}

class LightOfDevotion(BossModule module) : Components.SimpleLineStack(module, 3, 40, AID.LightOfDevotionTargetSelect, AID.LightOfDevotionAOE, 5.6f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        // note: actual aoe happens slightly after visual cast; but if adds are killed quickly enough, it might not happen at all
        if ((AID)spell.Action.ID is AID.Subroutine3End)
        {
            Source = null;
        }
    }
}

class EliminationExplosion(BossModule module) : Components.StandardAOEs(module, AID.EliminationExplosion, new AOEShapeRect(50, 4), 4);

class D063EliminatorStates : StateMachineBuilder
{
    public D063EliminatorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Disruption>()
            .ActivateOnEnter<PartitionShortR>()
            .ActivateOnEnter<PartitionShortL>()
            .ActivateOnEnter<PartitionLongR>()
            .ActivateOnEnter<PartitionLongL>()
            .ActivateOnEnter<HaloOfDestruction>()
            .ActivateOnEnter<Terminate>()
            .ActivateOnEnter<Electray>()
            .ActivateOnEnter<Overexposure>()
            .ActivateOnEnter<CompressionAOE>()
            .ActivateOnEnter<CompressionImpact>()
            .ActivateOnEnter<LightningGenerator>()
            .ActivateOnEnter<LightOfSalvation>()
            .ActivateOnEnter<LightOfDevotion>()
            .ActivateOnEnter<EliminationExplosion>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 827, NameID = 12729)]
public class D063Eliminator(WorldState ws, Actor primary) : BossModule(ws, primary, new(-759, -648), new ArenaBoundsSquare(15));
