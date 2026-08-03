namespace BossMod.Shadowbringers.Dungeon.D09GrandCosmos.D093Lugus;

public enum OID : uint
{
    Boss = 0x2C13, // R4.0
    CrystalChandelier = 0x2C12, // R5.0
    VelvetDrapery = 0x2C0E, // R3.0
    GrandPiano = 0x2C0F, // R4.2
    PlushSofa = 0x2C11, // R3.0
    GildedStool = 0x2C10, // R1.0
    // furniture helpers used to check AOE clipping with furniture since it otherwise would only burn if the center is in shape
    FurnitureHelper1 = 0x2C16, // R1.0 (plush sofa, velvet drapery)
    FurnitureHelper2 = 0x2C18, // R0.8 (gilded stool)
    FurnitureHelper3 = 0x2C19, // R3.0 (chandelier)
    FurnitureHelper4 = 0x2C14, // R2.5 (velvet drapery)
    FurnitureHelper5 = 0x2C17, // R1.5 (grand piano)
    FurnitureHelper6 = 0x2C15, // R0.5 (plush sofa)
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    ScorchingLeft = 18275, // Boss->self, 5.0s cast, range 40 180-degree cone
    ScorchingRight = 18274, // Boss->self, 5.0s cast, range 40 180-degree cone

    BlackFlame = 18269, // Helper->players, no cast, range 6 circle
    OtherworldlyHeatVisual = 18267, // Boss->self, 5.0s cast, single-target
    OtherworldlyHeat = 18268, // Helper->self, 2.5s cast, range 10 width 4 cross
    CaptiveBolt = 18276, // Boss->player, 5.0s cast, single-target, tankbuster
    MortalFlameVisual = 18265, // Boss->self, 5.0s cast, single-target
    MortalFlame = 18266, // Helper->player, 5.5s cast, single-target

    FiresDomainVisual = 18270, // Boss->self, 8.0s cast, single-target
    FiresDomain1 = 18272, // Boss->player, no cast, width 4 rect charge
    FiresDomain2 = 18271, // Boss->player, no cast, width 4 rect charge
    FiresIre = 18273, // Boss->self, 2.0s cast, range 20 90-degree cone

    CullingBlade = 18277, // Boss->self, 6.0s cast, range 80 circle
    CullingBladeVisual = 18278, // Helper->self, no cast, range 80 circle
    Plummet = 18279 // Helper->self, 1.6s cast, range 3 circle
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

public enum SID : uint
{
    MortalFlame = 2136 // Helper->player, extra=0x0
}

class MortalFlame(BossModule module) : BossComponent(module)
{
    private BitMask burning;
    private DateTime _activation;

    public static List<Actor> Furniture(BossModule module)
    {
        var chandeliers = module.Enemies((uint)OID.CrystalChandelier);
        var count = chandeliers.Count;
        for (var i = 0; i < count; ++i)
        {
            if (chandeliers[i].IsTargetable)
            {
                return module.Enemies(D093Lugus.FurnitureB);
            }
        }
        return module.Enemies(D093Lugus.FurnitureA);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.MortalFlame)
        {
            burning[Raid.FindSlot(actor.InstanceID)] = true;
            _activation = status.ExpireAt;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.MortalFlame)
            burning[Raid.FindSlot(actor.InstanceID)] = false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!burning[slot])
            return;
        var furniture = Furniture(Module);
        var count = furniture.Count;
        var forbidden = new ShapeDistance[count];
        for (var i = 0; i < count; ++i)
        {
            var h = furniture[i];
            forbidden[i] = new SDInvertedCircle(h.Position, 0.8f);
        }
        if (forbidden.Length != 0)
        {
            hints.AddForbiddenZone(new SDIntersection(forbidden), _activation);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (burning[slot])
            hints.Add("Pass flames debuff to furniture!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (burning[pcSlot])
        {
            var furniture = Furniture(Module);
            var count = furniture.Count;
            for (var i = 0; i < count; ++i)
            {
                var a = furniture[i];
                Arena.ZoneCircleOutline(a.Position, a.HitboxRadius, Colors.Safe);
            }
        }
    }
}

class BlackFlame(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private static readonly AOEShapeCircle circle = new(6f);
    private static readonly AOEShapeCross cross = new(10f, 2f);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.BlackFlame)
        {
            var activation = WorldState.FutureTime(4d);
            var primary = Module.PrimaryActor;
            CurrentBaits.Add(new(primary, actor, circle, activation));
            CurrentBaits.Add(new(primary, actor, cross, activation, default, Angle.AnglesCardinals[1]));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.BlackFlame)
            CurrentBaits.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (!IsBaitTarget(actor))
        {
            return;
        }

        var furniture = MortalFlame.Furniture(Module);
        var count = furniture.Count;
        var forbidden = new ShapeDistance[count * 2];
        var index = 0;

        for (var i = 0; i < count; ++i)
        {
            var p = furniture[i];
            {
                // AOE and hitboxes seem to be forbidden to intersect
                forbidden[index++] = new SDCross(p.Position, Angle.AnglesCardinals[1], cross.Length + p.HitboxRadius, cross.HalfWidth + p.HitboxRadius);
                forbidden[index++] = new SDCircle(p.Position, circle.Radius + p.HitboxRadius);
            }
        }
        if (forbidden.Length != 0)
            hints.AddForbiddenZone(new SDUnion(forbidden), CurrentBaits.Ref(0).Activation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (!IsBaitTarget(pc))
        {
            return;
        }
        var furniture = MortalFlame.Furniture(Module);
        var count = furniture.Count;
        for (var i = 0; i < count; ++i)
        {
            var a = furniture[i];
            Arena.ZoneCircleOutline(a.Position, a.HitboxRadius, Colors.Danger);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (IsBaitTarget(actor))
        {
            hints.Add("Bait away, avoid intersecting furniture hitboxes!");
        }
    }
}

class OtherworldlyHeat(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OtherworldlyHeat, new AOEShapeCross(10f, 2f));

sealed class Scorching(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ScorchingLeft, (uint)AID.ScorchingRight], new AOEShapeCone(40f, 90f.Degrees()));

class CullingBlade(BossModule module) : Components.RaidwideCast(module, (uint)AID.CullingBlade);
class CaptiveBolt(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.CaptiveBolt);
class FiresIre(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FiresIre, new AOEShapeCone(20f, 45f.Degrees()));
class Plummet(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Plummet, 3f);
class FiresDomainTether(BossModule module) : Components.StretchTetherDuo(module, default, default)
{
    private readonly WDir offset = new(default, 23.5f);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (IsBaitTarget(actor))
        {
            hints.AddForbiddenZone(new SDRect(Arena.Center + offset, Arena.Center - offset, 23.5f));
        }
    }
}

class FiresDomain(BossModule module) : Components.GenericBaitAway(module)
{
    private readonly AOEShapeRect rect = new(default, 2f);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (CurrentBaits.Count != 0 && spell.Action.ID is (uint)AID.FiresDomain1 or (uint)AID.FiresDomain2)
            CurrentBaits.RemoveAt(0);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is >= (uint)IconID.Target1 and <= (uint)IconID.Target4)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, rect));
    }

    public override void Update()
    {
        var count = CurrentBaits.Count;
        if (count == 0)
            return;
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        for (var i = 0; i < count; ++i)
        {
            ref var b = ref baits[i];
            b.Shape = new AOEShapeRect((b.Target.Position - b.Source.Position).Length(), 2f);
        }
    }
}

class FiresIreBait(BossModule module) : Components.GenericBaitAway(module)
{
    private readonly AOEShapeCone cone = new(20f, 45f.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is >= (uint)IconID.Target1 and <= (uint)IconID.Target4)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, cone));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (CurrentBaits.Count != 0 && spell.Action.ID is (uint)AID.FiresDomain1 or (uint)AID.FiresDomain2)
            CurrentBaits.RemoveAt(0);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CurrentBaits.Count == 0)
            return;
        var baitsNotOnActor = CollectionsMarshal.AsSpan(ActiveBaitsNotOn(actor));
        var countNoA = baitsNotOnActor.Length;

        for (var i = 0; i < countNoA; ++i)
        {
            ref var b = ref baitsNotOnActor[i];
            hints.AddForbiddenZone(cone, b.Target.Position - (b.Target.HitboxRadius + Module.PrimaryActor.HitboxRadius) * Module.PrimaryActor.DirectionTo(b.Target), b.Rotation);
        }

        var baitsOnActor = ActiveBaitsOn(actor);
        var countoA = baitsOnActor.Count;
        if (countoA != 0)
        {
            var b = baitsOnActor.Ref(0).Activation;
            var furniture = MortalFlame.Furniture(Module);
            List<Actor> party = [.. Raid.WithoutSlot(false, true, true)];
            party.Remove(actor);
            var total = furniture.Count + party.Count;
            var actors = new List<Actor>(total);
            actors.AddRange(furniture);
            actors.AddRange(party);
            for (var i = 0; i < total; ++i)
            {
                hints.AddForbiddenZone(new SDCircle(actors[i].Position, 10f), b);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var bait = ActiveBaitsOn(pc);
        if (bait.Count == 0)
            return;
        var furniture = MortalFlame.Furniture(Module);
        var countF = furniture.Count;
        for (var i = 0; i < countF; ++i)
        {
            var a = furniture[i];
            Arena.ZoneCircleOutline(a.Position, a.HitboxRadius, Colors.Danger);
        }

        cone.Outline(Arena, pc.Position - (pc.HitboxRadius + Module.PrimaryActor.HitboxRadius) * Module.PrimaryActor.DirectionTo(pc), bait.Ref(0).Rotation);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var baits = ActiveBaitsNotOn(pc);
        var count = baits.Count;
        if (count == 0)
            return;
        var baitss = CollectionsMarshal.AsSpan(baits);
        for (var i = 0; i < count; ++i)
        {
            ref var b = ref baitss[i];
            cone.Draw(Arena, b.Target.Position - (b.Target.HitboxRadius + Module.PrimaryActor.HitboxRadius) * Module.PrimaryActor.DirectionTo(b.Target), b.Rotation);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (IsBaitTarget(actor))
            hints.Add("Bait away, avoid intersecting furniture hitboxes!");
    }
}

class D093LugusStates : StateMachineBuilder
{
    public D093LugusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FiresDomainTether>()
            .ActivateOnEnter<FiresDomain>()
            .ActivateOnEnter<FiresIre>()
            .ActivateOnEnter<FiresIreBait>()
            .ActivateOnEnter<Scorching>()
            .ActivateOnEnter<OtherworldlyHeat>()
            .ActivateOnEnter<BlackFlame>()
            .ActivateOnEnter<CaptiveBolt>()
            .ActivateOnEnter<CullingBlade>()
            .ActivateOnEnter<Plummet>()
            .ActivateOnEnter<MortalFlame>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 692, NameID = 9046)]
public class D093Lugus(WorldState ws, Actor primary) : BossModule(ws, primary, new(default, -340f), new ArenaBoundsSquare(24.5f))
{
    public static readonly uint[] FurnitureA = [(uint)OID.FurnitureHelper1, (uint)OID.FurnitureHelper2, (uint)OID.FurnitureHelper4, (uint)OID.FurnitureHelper5, (uint)OID.FurnitureHelper6];
    public static readonly uint[] FurnitureB = [.. FurnitureA, (uint)OID.FurnitureHelper3];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        var chandeliers = Enemies((uint)OID.CrystalChandelier);
        var count = chandeliers.Count;
        for (var i = 0; i < count; ++i)
        {
            if (chandeliers[i].IsTargetable)
            {
                Arena.Actors(this, FurnitureB);
                return;
            }
        }
        Arena.Actors(this, FurnitureA, Colors.Object, true);
    }
}
