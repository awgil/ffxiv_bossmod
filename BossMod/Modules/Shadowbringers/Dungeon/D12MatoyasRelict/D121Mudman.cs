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

class StoneAge(BossModule module) : Components.RaidwideCast(module, (uint)AID.StoneAge);
class HardRock(BossModule module) : Components.SingleTargetCast(module, (uint)AID.HardRock);
class MudVoidzone(BossModule module) : Components.Voidzone(module, 5f, GetVoidzone)
{
    private static List<Actor> GetVoidzone(BossModule module) => module.Enemies((uint)OID.MudVoidzone);
}
class Quagmire(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Quagmire, 6f);
class FallingRock(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.FallingRock, 6f, 4, 4);

class BrittleBreccia(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly Angle a135 = 135f.Degrees();
    private static readonly AOEShape[] _shapes = [new AOEShapeCone(6.5f, a135), new AOEShapeDonutSector(6.5f, 12.5f, a135), new AOEShapeDonutSector(12.5f, 18.5f, a135)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BrittleBreccia1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell), spell.Rotation);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.BrittleBreccia1 => 0,
                (uint)AID.BrittleBreccia2 => 1,
                (uint)AID.BrittleBreccia3 => 2,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(1.5d), spell.Rotation);
        }
    }
}

class RockyRoll(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeRect rect1 = new(60f, 2f), rect2 = new(60f, 3f), rect3 = new(60f, 4f);
    private readonly List<WPos> activeHoles = [with(4)];

    public override void OnMapEffect(byte index, uint state)
    {
        var pos = index switch
        {
            0x0A => new(-202.627f, -162.627f),
            0x0B => new(-157.373f, -162.627f),
            0x0C => new(-202.627f, -117.373f),
            0x0D => new(-157.373f, -117.373f),
            _ => new WPos(),
        };
        if (pos != default)
        {
            if (state == 0x00020001u)
            {
                activeHoles.Add(pos);
            }
            else if (state == 0x00080004u)
            {
                activeHoles.Remove(pos);
            }
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Mudball)
        {
            CurrentBaits.Add(new(source, WorldState.Actors.Find(tether.Target)!, rect1, WorldState.FutureTime(8.2d)));
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Mudball)
        {
            var count = CurrentBaits.Count;
            for (var i = 0; i < count; ++i)
            {
                if (CurrentBaits[i].Source == source)
                {
                    CurrentBaits.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        var count = activeHoles.Count;
        for (var i = 0; i < count; ++i)
        {
            Arena.ZoneCircleOutlineUnclipped(activeHoles[i], 5f, Colors.Safe, 5f);
        }
    }

    public override void Update()
    {
        var count = CurrentBaits.Count;
        if (count == 0)
        {
            return;
        }
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        var activation = WorldState.FutureTime(9.7d);

        for (var i = 0; i < count; ++i)
        {
            ref var b = ref baits[i];
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
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (IsBaitTarget(actor))
            hints.Add("Bait into a hole!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var baits = ActiveBaitsOn(actor);
        var count = baits.Count;
        if (count == 0)
            return;
        var actHolesCount = activeHoles.Count;
        var forbidden = new ShapeDistance[actHolesCount];
        var b = baits[0];
        for (var i = 0; i < actHolesCount; ++i)
            forbidden[i] = new SDInvertedRect(b.Source.Position, activeHoles[i], 1f);
        if (actHolesCount != 0)
            hints.AddForbiddenZone(new SDIntersection(forbidden));
    }
}

class D121MudmanStates : StateMachineBuilder
{
    public D121MudmanStates(BossModule module) : base(module)
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
public class D121Mudman(WorldState ws, Actor primary) : BossModule(ws, primary, new(-180f, -140f), new ArenaBoundsCircle(19.5f));
