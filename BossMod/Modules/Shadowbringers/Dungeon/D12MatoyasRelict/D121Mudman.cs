namespace BossMod.Shadowbringers.Dungeon.D12MatoyasRelict.D121Mudman;

public enum OID : uint
{
    Boss = 0x300C, // R3.500, x1
    MudVoidzone = 0x1EB145, // R0.5
    MudmansDouble = 0x300D, // R3.5
    MudBubble1 = 0x300E, // R2.0-4.0
    MudBubble2 = 0x3009, // R4.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    HardRock = 21631, // Boss->player, 5.0s cast, single-target
    Quagmire = 21633, // Helper->location, 4.0s cast, range 6 circle

    PetrifiedPeat = 21632, // Boss->self, 4.0s cast, single-target
    PeatPelt = 21634, // Boss->self, 4.0s cast, single-target

    RockyRollVisual1 = 21639, // MudBubble2/MudBubble1->location, no cast, width 0 rect charge, bubble disappears
    RockyRollVisual2 = 21635, // MudBubble1->player, 8.0s cast, single-target
    RockyRoll1 = 21636, // MudBubble1->location, no cast, width 4 rect charge
    RockyRoll2 = 21637, // MudBubble1->location, no cast, width 6 rect charge
    RockyRoll3 = 21640, // MudBubble1->location, no cast, width 8 rect charge

    BrittleBrecciaVisual = 21645, // Boss->self, 4.0s cast, single-target
    BrittleBreccia1 = 21646, // Helper->self, 4.3s cast, range 6+R 270-degree cone
    BrittleBreccia2 = 21647, // Helper->self, 4.3s cast, range 12+R 270-degree donut segment
    BrittleBreccia3 = 21648, // Helper->self, 4.3s cast, range 18+R 270-degree donut segment

    StoneAgeVisual = 21649, // Boss->self, 5.0s cast, single-target
    StoneAge = 21650, // Helper->self, 5.3s cast, range 20 circle

    TasteDirt = 21641, // MudmansDouble->self, 7.5s cast, single-target

    FallingRockVisual = 21651, // Boss->self, 5.0s cast, single-target, stack
    FallingRock = 21652 // Helper->player, 5.0s cast, range 6 circle
}

public enum TetherID : uint
{
    Mudball = 7 // MudBubble1->player
}

class StoneAge(BossModule module) : Components.RaidwideCast(module, AID.StoneAge);
class HardRock(BossModule module) : Components.SingleTargetCast(module, AID.HardRock);
class MudVoidzone(BossModule module) : Components.PersistentVoidzone(module, 5, m => m.Enemies(OID.MudVoidzone));
class Quagmire(BossModule module) : Components.StandardAOEs(module, AID.Quagmire, 6);
class FallingRock(BossModule module) : Components.StackWithCastTargets(module, AID.FallingRock, 6, 4, 4);

class BrittleBreccia(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCone(6.5f, 135.Degrees()), new AOEShapeDonutSector(6.5f, 12.5f, 135.Degrees()), new AOEShapeDonutSector(12.5f, 18.5f, 135.Degrees())];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BrittleBreccia1)
            AddSequence(caster.Position, Module.CastFinishAt(spell), caster.Rotation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Sequences.Count > 0)
        {
            var order = (AID)spell.Action.ID switch
            {
                AID.BrittleBreccia1 => 0,
                AID.BrittleBreccia2 => 1,
                AID.BrittleBreccia3 => 2,
                _ => -1
            };
            AdvanceSequence(order, caster.Position, WorldState.FutureTime(1.5f), caster.Rotation);
        }
    }
}

class RockyRoll(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeRect rect1 = new(60, 2);
    private static readonly AOEShapeRect rect2 = new(60, 3);
    private static readonly AOEShapeRect rect3 = new(60, 4);
    private readonly List<WPos> activeHoles = [];

    private static readonly Dictionary<byte, WPos> holePositions = new()
    {
        { 0x0A, new(-202.627f, -162.627f) },
        { 0x0B, new(-157.373f, -162.627f) },
        { 0x0C, new(-202.627f, -117.373f) },
        { 0x0D, new(-157.373f, -117.373f) }
    };

    public override void OnMapEffect(byte index, uint state)
    {
        if (holePositions.TryGetValue(index, out var value))
        {
            if (state == 0x00020001)
                activeHoles.Add(value);
            else if (state == 0x00080004)
                activeHoles.Remove(value);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Mudball)
            CurrentBaits.Add(new(source, WorldState.Actors.Find(tether.Target)!, rect1, WorldState.FutureTime(8.2f)));
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Mudball)
            CurrentBaits.RemoveAll(x => x.Source == source);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var h in activeHoles)
            Arena.AddCircle(h, 5, ArenaColor.Safe, 5);
    }

    public override void Update()
    {
        if (CurrentBaits.Count == 0)
            return;

        for (var i = 0; i < CurrentBaits.Count; i++)
        {
            var b = CurrentBaits[i];
            var activation = WorldState.FutureTime(9.7f);
            if (b.Source.HitboxRadius is > 2 and <= 3 && b.Shape == rect1)
            {
                b.Shape = rect2;
                b.Activation = activation;
            }
            else if (b.Source.HitboxRadius > 3 && b.Shape == rect2)
            {
                b.Shape = rect3;
                b.Activation = activation;
            }
            CurrentBaits[i] = b;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (CurrentBaits.Any(x => x.Source == actor))
            hints.Add("Bait into a hole!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var forbidden = new List<Func<WPos, bool>>();
        foreach (var b in ActiveBaitsOn(actor))
            foreach (var h in activeHoles)
                forbidden.Add(ShapeContains.InvertedRect(b.Source.Position, h, 1));
        if (forbidden.Count > 0)
            hints.AddForbiddenZone(p => forbidden.All(f => f(p)));
    }
}

class MudmanStates : StateMachineBuilder
{
    public MudmanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BrittleBreccia>()
            .ActivateOnEnter<FallingRock>()
            .ActivateOnEnter<MudVoidzone>()
            .ActivateOnEnter<Quagmire>()
            .ActivateOnEnter<HardRock>()
            .ActivateOnEnter<StoneAge>()
            .ActivateOnEnter<RockyRoll>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 746, NameID = 9735)]
public class Mudman(WorldState ws, Actor primary) : BossModule(ws, primary, new(-180, -140), new ArenaBoundsCircle(19.5f));
