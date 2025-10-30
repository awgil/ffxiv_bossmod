namespace BossMod.Stormblood.Dungeon.D03BardamsMettle.D032HunterOfBardam;

public enum OID : uint
{
    Boss = 0x1AA5, // R2.200, x1
    Bardam = 0x1AA3, // R15.000, x1
    ThrowingSpear = 0x1F49, // R1.25
    StarShard = 0x1F4A, // R2.4
    LoomingShadow = 0x1F4D, // R1.0
    WarriorOfBardam = 0x1AA4, // R1.1
    Helper = 0x18D6
}

public enum AID : uint
{
    Visual1 = 9450, // Helper->player, no cast, single-target
    Visual2 = 9451, // Helper->player, no cast, single-target
    Visual3 = 9611, // StarShard->self, no cast, single-target

    Magnetism = 7944, // Boss->self, no cast, range 40+R circle, pull 40 between hitboxes
    Tremblor1 = 9596, // Helper->self, 4.0s cast, range 10 circle
    Tremblor2 = 9595, // Helper->self, 4.0s cast, range 10-20 donut
    EmptyGaze = 7940, // Boss->self, 6.5s cast, range 40+R circle
    Travail = 7935, // Bardam->self, no cast, single-target
    Charge = 9599, // ThrowingSpear->self, 2.5s cast, range 45+R width 5 rect

    Sacrifice = 7937, // Helper->location, 7.0s cast, range 3 circle, tower
    DivinePunishment = 7938, // Helper->self, no cast, range 40+R circle

    BardamsRing = 9601, // Helper->self, no cast, range 10-20 donut, stack donuts, 5.3s delay
    CometFirst = 9597, // Helper->location, 4.0s cast, range 4 circle
    CometRest = 9598, // Helper->location, 1.5s cast, range 4 circle

    HeavyStrike = 9591, // Boss/WarriorOfBardam->self, 4.0s cast, single-target
    HeavyStrike1 = 9592, // Helper->self, 4.0s cast, range 6+R 270-degree cone
    HeavyStrike2 = 9593, // Helper->self, 4.0s cast, range 6+R-12+R 270-degree donut segment
    HeavyStrike3 = 9594, // Helper->self, 4.0s cast, range 12+R-18+R 270-degree donut segment

    CometImpact = 9600, // StarShard->self, 4.0s cast, range 9 circle
    ReconstructVisual = 7933, // Bardam->self, no cast, single-target
    Reconstruct = 7934, // Helper->location, 4.0s cast, range 5 circle

    Tremblor = 9605, // Boss->self, 3.5s cast, single-target
    MeteorImpact = 9602, // LoomingShadow->self, 30.0s cast, ???
}

public enum IconID : uint
{
    BardamsRing = 58, // player
    ChasingAOE = 197, // player
}

class CometFirst(BossModule module) : Components.StandardAOEs(module, AID.CometFirst, 4);
class CometRest(BossModule module) : Components.StandardAOEs(module, AID.CometRest, 4);

class MeteorImpact(BossModule module) : Components.CastLineOfSightAOE(module, AID.MeteorImpact, 50, false)
{
    private Actor? castActor;
    public override IEnumerable<Actor> BlockerActors() => Module.Enemies(OID.StarShard).Where(x => !x.IsDead);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            castActor = caster;
    }

    public override void Update()
    {
        if (BlockerActors().Count() == 1)
            Modify(castActor?.Position, [(BlockerActors().First().Position, 2)]);
    }
}

class Charge(BossModule module) : Components.StandardAOEs(module, AID.Charge, new AOEShapeRect(46.25f, 2.5f));
class EmptyGaze(BossModule module) : Components.CastGaze(module, AID.EmptyGaze);
class Sacrifice(BossModule module) : Components.CastTowers(module, AID.Sacrifice, 3);
class Reconstruct(BossModule module) : Components.StandardAOEs(module, AID.Reconstruct, 5);
class CometImpact(BossModule module) : Components.StandardAOEs(module, AID.CometImpact, new AOEShapeCircle(9));
class BardamsRing(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeDonut(10, 20), (uint)IconID.BardamsRing, AID.BardamsRing, 3.5f, true);

class Tremblor(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10), new AOEShapeDonut(10, 20)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Tremblor1)
            AddSequence(caster.Position, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Sequences.Count > 0)
        {
            var order = (AID)spell.Action.ID switch
            {
                AID.Tremblor1 => 0,
                AID.Tremblor2 => 1,
                _ => -1
            };
            AdvanceSequence(order, caster.Position, WorldState.FutureTime(1.5f));
        }
    }
}

class TremblorFinal(BossModule module) : Components.StandardAOEs(module, AID.Tremblor2, new AOEShapeDonut(10, 20))
{
    private readonly Tremblor _aoe = module.FindComponent<Tremblor>()!;

    public override void Update()
    {
        MaxCasts = _aoe.Sequences.Count != 0 ? 0 : 1;
    }
}

class HeavyStrike(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCone(6.5f, 135.Degrees()), new AOEShapeDonutSector(6.5f, 12.5f, 135.Degrees()), new AOEShapeDonutSector(12.5f, 18.5f, 135.Degrees())];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HeavyStrike)
            AddSequence(caster.Position, Module.CastFinishAt(spell, 1), spell.Rotation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Sequences.Count > 0)
        {
            var order = (AID)spell.Action.ID switch
            {
                AID.HeavyStrike1 => 0,
                AID.HeavyStrike2 => 1,
                AID.HeavyStrike3 => 2,
                _ => -1
            };
            AdvanceSequence(order, caster.Position, WorldState.FutureTime(1.3f), caster.Rotation);
        }
    }
}

class D032HunterOfBardamStates : StateMachineBuilder
{
    public D032HunterOfBardamStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CometFirst>()
            .ActivateOnEnter<CometRest>()
            .ActivateOnEnter<Tremblor>()
            .ActivateOnEnter<TremblorFinal>()
            .ActivateOnEnter<HeavyStrike>()
            .ActivateOnEnter<Charge>()
            .ActivateOnEnter<EmptyGaze>()
            .ActivateOnEnter<BardamsRing>()
            .ActivateOnEnter<Sacrifice>()
            .ActivateOnEnter<CometImpact>()
            .ActivateOnEnter<Reconstruct>()
            .ActivateOnEnter<MeteorImpact>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team (Malediktus), Ported by Herculezz (MeteorImpact rewritten with help from xan)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 240, NameID = 6180)]
public class D032HunterOfBardam(WorldState ws, Actor primary) : BossModule(ws, primary, new(-28.5f, -14), new ArenaBoundsCircle(19.5f))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = -1;
    }
}
