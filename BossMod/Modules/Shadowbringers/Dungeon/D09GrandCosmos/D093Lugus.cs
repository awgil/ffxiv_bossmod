namespace BossMod.Shadowbringers.Dungeon.D09GrandCosmos;

public enum OID : uint
{
    Boss = 0x2C13,
    Helper = 0x233C,
}

public enum AID : uint
{
    ScorchingRight = 18274, // Boss->self, 5.0s cast, range 40 180-degree cone
    ScorchingLeft = 18275, // Boss->self, 5.0s cast, range 40 180-degree cone
    BlackFlame = 18269, // Helper->players, no cast, range 6 circle
    OtherworldlyHeat1 = 18268, // Helper->self, 2.5s cast, range 10 width 4 cross
    CaptiveBolt = 18276, // Boss->player, 5.0s cast, single-target

    // no idea what the difference is between these
    FiresDomain1 = 18272, // Boss->player, no cast, width 4 rect charge
    FiresDomain2 = 18271, // Boss->player, no cast, width 4 rect charge

    FiresIre = 18273, // Boss->self, 2.0s cast, range 20 90-degree cone
    CullingBlade = 18277, // Boss->self, 6.0s cast, range 80 circle
    Plummet = 18279, // Helper->self, 1.6s cast, range 3 circle
}

public enum SID : uint
{
    MortalFlame = 2136, // Helper->player, extra=0x0/0x50/0xA0/0xF0/0x140
}

public enum IconID : uint
{
    BlackFlame = 25, // player
    MortalFlame = 195, // player
    Target1 = 50, // player
    Target2 = 51, // player
    Target3 = 52, // player
    Target4 = 53, // player
    Tankbuster = 218 // player
}

class CullingBlade(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.CullingBlade));
class CaptiveBolt(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.CaptiveBolt));
class Plummet(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Plummet), new AOEShapeCircle(3));
class ScorchingRight(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ScorchingRight), new AOEShapeCone(40, 90.Degrees()));
class ScorchingLeft(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ScorchingLeft), new AOEShapeCone(40, 90.Degrees()));
class OtherworldlyHeat(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.OtherworldlyHeat1), new AOEShapeCross(10, 2));
class FiresIre(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FiresIre), new AOEShapeCone(20, 45.Degrees()));
class BlackFlame(BossModule module) : BossComponent(module)
{
    private BitMask targets;
    private DateTime activation;

    private static readonly AOEShapeCross Shape = new(10, 2);

    private IEnumerable<Actor> Furniture => Raid.WithoutSlot().Where(x => x.Type == ActorType.Enemy);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == 25)
        {
            targets.Set(Raid.FindSlot(actor.InstanceID));
            if (activation == default)
                activation = WorldState.FutureTime(4.2f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.BlackFlame)
        {
            targets.Clear(Raid.FindSlot(spell.MainTargetID));
            if (!targets.Any())
                activation = default;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var (slot, player) in Raid.WithSlot().IncludedInMask(targets))
        {
            if (slot == pcSlot)
                Shape.Outline(Arena, player.Position, default, ArenaColor.Danger);
            else
                Shape.Draw(Arena, player.Position, default, ArenaColor.AOE);
        }

        if (targets[pcSlot])
            foreach (var furniture in Furniture)
                Arena.ZoneCircle(furniture.Position, furniture.HitboxRadius, ArenaColor.AOE);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (targets[slot])
            foreach (var ally in Furniture)
                hints.AddForbiddenZone(p => IntersectFurniture(ally, p) ? -1 : 1, activation);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (targets[slot])
            hints.Add("Bait away from furniture!", Furniture.Any(f => IntersectFurniture(f, actor.Position)));
    }

    private bool IntersectFurniture(Actor furniture, WPos player) => IntersectBubble(furniture, player, 2, 10) || IntersectBubble(furniture, player, 10, 2);

    // TODO replace with Intersect.CircleAARect
    private bool IntersectBubble(Actor furniture, WPos rectCenter, float halfWidth, float halfHeight)
    {
        var radius = furniture.HitboxRadius;

        var circleCenter = furniture.Position;
        var off1 = (rectCenter - circleCenter).Abs();
        if (off1.X > halfWidth + radius || off1.Z > halfHeight + radius)
            return false;

        if (off1.X <= halfWidth || off1.Z <= halfHeight)
            return true;

        return (off1 - new WDir(halfWidth, halfHeight)).Length() <= radius;
    }
}

class MortalFlame(BossModule module) : BossComponent(module)
{
    private readonly float[] Timers = Utils.MakeArray(PartyState.MaxAllies, 0f);

    private IEnumerable<Actor> Furniture => Raid.WithoutSlot().Where(x => x.Type == ActorType.Enemy);

    private void SetTimer(int slot, float timer)
    {
        if (slot < 0)
            return;

        Timers[slot] = timer;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.MortalFlame && actor.Type != ActorType.Enemy)
            SetTimer(Raid.FindSlot(actor.InstanceID), (float)(status.ExpireAt - WorldState.CurrentTime).TotalSeconds);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.MortalFlame)
            SetTimer(Raid.FindSlot(actor.InstanceID), 0);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Timers[slot] > 0)
        {
            if (Furniture.Any())
                hints.Add("Pass flame to furniture!");
            else
                hints.Add("RIP");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Timers[slot] > 0)
        {
            var furnitures = Furniture.Select(f => ShapeDistance.InvertedCircle(f.Position, 1)).ToList();
            if (furnitures.Count > 0)
                hints.AddForbiddenZone(ShapeDistance.Intersection(furnitures), WorldState.FutureTime(Timers[slot]));
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Timers[pcSlot] > 0)
            foreach (var f in Furniture)
                Arena.ZoneCircle(f.Position, 2, ArenaColor.SafeFromAOE);
    }
}

class FiresDomain(BossModule module) : BossComponent(module)
{
    public const float TetherLength = 16f;
    private readonly List<Actor> Baits = [];
    private DateTime NextCharge;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID is >= IconID.Target1 and <= IconID.Target4)
        {
            Baits.Add(actor);
            if ((IconID)iconID == IconID.Target1)
                NextCharge = WorldState.FutureTime(8.4f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FiresDomain1 or AID.FiresDomain2)
        {
            Baits.RemoveAt(0);
            NextCharge = WorldState.FutureTime(4.4f);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Baits.Count == 0)
            return;

        var baitOrder = Baits.IndexOf(pc);

        DrawCharge(Module.PrimaryActor, Baits[0], baitOrder == 0);
        if (baitOrder > 0)
            DrawCharge(Baits[baitOrder - 1], pc, true);
    }

    private void DrawCharge(Actor from, Actor to, bool isPlayer)
    {
        if (isPlayer)
            Arena.AddRect(from.Position, from.DirectionTo(to), (from.Position - to.Position).Length(), 0, 2, ArenaColor.Danger);
        else
            Arena.ZoneRect(from.Position, to.Position, 2, ArenaColor.AOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Baits.Count == 0)
            return;

        var baitAOE = ShapeDistance.Rect(Module.PrimaryActor.Position, Baits[0].Position, 2);

        var order = Baits.IndexOf(actor);
        if (order == 0)
        {
            hints.Add("Stretch tether!", (actor.Position - Module.PrimaryActor.Position).Length() < TetherLength);
            if (Raid.WithoutSlot().Exclude(actor).Any(a => baitAOE(a.Position) < 0))
                hints.Add("GTFO from raid!");
        }
        else
        {
            if (baitAOE(actor.Position) < 0)
                hints.Add("GTFO from charge!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var baitOrder = Baits.IndexOf(actor);
        if (baitOrder < 0)
            return;

        if (baitOrder == 0)
        {
            var source = Module.PrimaryActor;
            // stretch tether
            hints.AddForbiddenZone(ShapeDistance.Circle(source.Position, TetherLength), NextCharge);
            // don't clip any other party member with charge
            foreach (var p in Raid.WithoutSlot(excludeNPCs: true).Exclude(actor))
                hints.AddForbiddenZone(ShapeDistance.Cone(source.Position, 100, source.AngleTo(p), Angle.Asin(2f / (p.Position - source.Position).Length())), NextCharge);
        }
        else
        {
            // try to preposition away from previous party member in line
            hints.AddForbiddenZone(ShapeDistance.Circle(Baits[baitOrder - 1].Position, TetherLength), NextCharge.AddSeconds(4.4f * baitOrder));
            // stay out of boss's charge aoe
            hints.AddForbiddenZone(ShapeDistance.Rect(Module.PrimaryActor.Position, Baits[0].Position, 2), NextCharge);
        }
    }
}

class LugusStates : StateMachineBuilder
{
    public LugusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ScorchingRight>()
            .ActivateOnEnter<ScorchingLeft>()
            .ActivateOnEnter<BlackFlame>()
            .ActivateOnEnter<OtherworldlyHeat>()
            .ActivateOnEnter<MortalFlame>()
            .ActivateOnEnter<FiresDomain>()
            .ActivateOnEnter<FiresIre>()
            .ActivateOnEnter<CaptiveBolt>()
            .ActivateOnEnter<Plummet>()
            .ActivateOnEnter<CullingBlade>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 692, NameID = 9046)]
public class Lugus(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -340), new ArenaBoundsSquare(24.5f));
