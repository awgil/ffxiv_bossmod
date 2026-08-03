namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE214TinyTerror;

public enum OID : uint
{
    TinyMage = 0x4C6D,
    Helper = 0x233C,
    TinyMageHelper = 0x4D55, // R1.000, x1
    TinyApprentice = 0x4C6E, // R1.000, x0 (spawn during fight)
    ArcaneSphereSmall = 0x4C74, // R1.000, x0 (spawn during fight)
    ArcaneSphereBig = 0x4C73, // R1.000, x0 (spawn during fight)
    FlareSphereGrow = 0x4C6F, // R0.700-1.904, x0 (spawn during fight)
    FlareSphere = 0x4C70, // R1.400, x0 (spawn during fight)
    HolySphereGrow = 0x4C71, // R0.700-1.904, x0 (spawn during fight)
    HolySphere = 0x4C72, // R1.400, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 48305, // TinyMage->player, no cast, single-target
    TinyWarp = 48331, // TinyMage->location, no cast, single-target
    SmallForOne = 48306, // TinyMage->self, 3.0s cast, single-target
    AllForOne = 50762, // TinyMage->self, 3.0s cast, single-target

    ArcaneAggregation = 48307, // 4C6E->self, 3.0s cast, single-target
    ArcaneAggregation1 = 49718, // 4C6E->self, 5.5s cast, single-target
    ArcaneAggregation2 = 49719, // 4C6E->self, 5.5s cast, single-target
    ArcaneAggregation3 = 48308, // 4C6E->self, 3.0s cast, single-target

    TinyThunderIIIRaidwide = 48329, // TinyMage->self, 5.0s cast, single-target
    TinyThunderIII = 48330, // Helper->self, no cast, ???

    TinyQuakeIII = 48322, // TinyMage->self, 3.5+0.5s cast, single-target
    TinyQuakeIIIInner = 48323, // Helper->self, 4.0s cast, range 10 circle
    TinyQuakeIIIMiddle = 48324, // Helper->self, 4.0s cast, range 10-20 donut
    TinyQuakeIIIOuter = 48325, // Helper->self, 4.0s cast, range 20-30 donut

    DiminutiveDualcast = 48317, // TinyMage->self, 5.5+0.5s cast, single-target
    TinyBlizzardIII = 48319, // Helper->self, 6.0s cast, range 40 60.000-degree cone
    TinyFireIII = 48318, // Helper->self, 6.0s cast, range 14 circle

    TinyMeteorCast = 48320, // TinyMage->self, 5.0s cast, single-target
    TinyMeteor = 48321, // Helper->location, 4.0s cast, range 6 circle

    Comet = 48327, // 4C74->self, 60.0s cast, range 60 circle
    Comet1 = 49061, // Helper->self, no cast, ???
    Meteor = 48326, // 4C73->self, 130.0s cast, single-target

    TinyFlare = 48313, // 4C6F/4C70->self, no cast, single-target
    TinyFlare1 = 48311, // Helper->self, 2.0s cast, range 18 circle
    TinyHoly = 48314, // 4C72/4C71->self, no cast, single-target
    TinyHoly1 = 48312, // Helper->self, 2.0s cast, range 50 circle
    TinyHoly2 = 49058, // Helper->self, no cast, ???

    Recharge = 48309, // 4C6E->self, 1.5s cast, single-target
    Recharge1 = 48310, // 4C6E->self, 1.5s cast, single-target
    Recharge2 = 49059, // 4C6E/TinyMage->self, no cast, single-target

    Ability = 49057, // 4D55->self, no cast, range ?-25 donut
    Ability1 = 50530, // 4C6E->self, no cast, single-target
    Spell1 = 50638, // 4C6E->self, no cast, single-target
}

public enum SID : uint
{
    Gen1 = 2552, // none->4C6E, extra=0x198
    Gen2 = 3445, // none->4C74/4C73, extra=0x15/0xA/0x1E
}

public enum TetherID : uint
{
    FlareHolyMergeTether = 415, // 4C72/4C70->4C72/4C70
    ArcaneSphereTether = 422, // 4C6E/TinyMage->4C74/4EBB
    CometMeteorTether = 60, // 4C74->4EBB
}

sealed class TinyThunderIII(BossModule module) : Components.RaidwideCast(module, (uint)AID.TinyThunderIIIRaidwide);

sealed class TinyQuake(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TinyQuakeIIIInner)
        {
            aoes.Add(new(new AOEShapeCircle(10.0f), caster.Position, caster.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID, risky: false));
        }

        if (spell.Action.ID == (uint)AID.TinyQuakeIIIMiddle)
        {
            aoes.Add(new(new AOEShapeDonut(10.0f, 20.0f), caster.Position, caster.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID, risky: false));
        }

        if (spell.Action.ID == (uint)AID.TinyQuakeIIIOuter)
        {
            aoes.Add(new(new AOEShapeDonut(20.0f, 30.0f), caster.Position, caster.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID, risky: false));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.TinyQuakeIIIInner or (uint)AID.TinyQuakeIIIMiddle or (uint)AID.TinyQuakeIIIOuter)
        {
            aoes.Sort((a, b) => a.Activation.CompareTo(b.Activation));
            if (aoes.Count > 0)
            {
                aoes.RemoveAll(aoe => aoe.ActorID == caster.InstanceID);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        int show = 0;
        var incomingAOEs = aoes.OrderBy(a => a.Activation).Take(2).ToList();
        foreach (ref var aoe in CollectionsMarshal.AsSpan(incomingAOEs))
        {
            aoe.Color = show == 0 ? Colors.Danger : Colors.AOE;
            aoe.Risky = show == 0;
            show++;
        }

        return CollectionsMarshal.AsSpan(incomingAOEs);
    }
}

sealed class DiminutiveDualcast(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    public bool middleActive = false; // better control logic for the knockback sphere

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TinyBlizzardIII)
        {
            aoes.Add(new(new AOEShapeCone(40.0f, 30.0f.Degrees()), caster.Position, caster.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
        }

        if (spell.Action.ID == (uint)AID.TinyFireIII)
        {
            aoes.Add(new(new AOEShapeCircle(14.0f), caster.Position, caster.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
            middleActive = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.TinyBlizzardIII or (uint)AID.TinyFireIII)
        {
            if (aoes.Count > 0)
            {
                aoes.RemoveAll(aoe => aoe.ActorID == caster.InstanceID);
            }

            if (spell.Action.ID == (uint)AID.TinyFireIII) {
                middleActive = false;
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var incomingAOEs = aoes.OrderBy(a => a.Activation).Take(4).ToList();
        var waveTimer = incomingAOEs.MinBy(a => a.Activation).Activation.AddSeconds(0.2f);

        foreach (ref var aoe in CollectionsMarshal.AsSpan(incomingAOEs))
        {
            if (aoe.Activation <= waveTimer)
            {
                aoe.Color = Colors.Danger;
                aoe.Risky = true;
            }
        }

        return CollectionsMarshal.AsSpan(incomingAOEs);
    }
}

sealed class TinyMeteor(BossModule module) : Components.GenericAOEs(module, (uint)AID.TinyMeteor)
{
    private readonly List<AOEInstance> aoes = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TinyMeteor)
        {
            aoes.Add(new(new AOEShapeCircle(6.0f), caster.Position, caster.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.TinyMeteor)
        {
            if (aoes.Count > 0)
            {
                aoes.RemoveAll(aoe => aoe.ActorID == caster.InstanceID);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var waveTimer = aoes.MinBy(a => a.Activation).Activation.AddSeconds(0.2f);

        foreach (ref var aoe in CollectionsMarshal.AsSpan(aoes))
        {
            if (aoe.Activation <= waveTimer)
            {
                aoe.Color = Colors.Danger;
                aoe.Risky = true;
            }
        }

        return CollectionsMarshal.AsSpan(aoes);
    }
}

sealed class Comet(BossModule module) : BossComponent(module)
{
    private readonly List<ArcaneSphere> arcaneSpheres = [];

    private class ArcaneSphere(Actor actor)
    {
        public Actor arcaneSphere = actor;
        public int mages = 0;
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.ArcaneSphereSmall)
        {
            arcaneSpheres.Add(new ArcaneSphere(actor));
        }
    }

    public override void OnActorDeath(Actor actor)
    {
        if (actor.OID == (uint)OID.ArcaneSphereSmall)
        {
            var sphere = arcaneSpheres.Find(a => a.arcaneSphere.InstanceID == actor.InstanceID);
            if (sphere != null)
            {
                arcaneSpheres.Remove(sphere);
            }
        }
    }

    public override void Update()
    {
        if (arcaneSpheres.Count == 0)
        {
            return;
        }

        foreach (var actor in WorldState.Actors)
        {
            if (actor.OID == (uint)OID.TinyApprentice)
            {
                var index = arcaneSpheres.FindIndex(sphere => actor.Position.AlmostEqual(sphere.arcaneSphere.Position, 4.0f));
                if (index < 0)
                {
                    return;
                }

                arcaneSpheres[index].mages++;
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (arcaneSpheres.Count == 0)
        {
            return;
        }

        var firstArcaneSphere = arcaneSpheres.MaxBy(a => a.mages);
        if (firstArcaneSphere != null)
        {
            Arena.ZoneCircleOutline(firstArcaneSphere.arcaneSphere.Position, 2.0f, Colors.Safe, 2.0f);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (arcaneSpheres.Count == 0)
        {
            return;
        }

        hints.Add("Attack the arcane sphere with the green circle around it!", false);
    }
}

sealed class FlareHolyMerge(BossModule module) : BossComponent(module) {
    private static readonly AOEShapeCircle flareShape = new(18.0f);
    private const float holyKnockBackDistance = 15.0f;

    private readonly record struct MergeCombination(WPos Origin, float Distance, bool IsFlare, DateTime Activation);
    private readonly List<MergeCombination> mergeCombinations = [];

    public override void OnTethered(Actor source, in ActorTetherInfo tether) {
        if (tether.ID == (uint)TetherID.FlareHolyMergeTether) {
            var sphere = WorldState.Actors.Find(tether.Target);
            if (sphere != null) {
                var midPoint = WPos.Lerp(source.Position, sphere.Position, 0.5f);
                var distance = (source.Position - sphere.Position).Length();
                mergeCombinations.Add(new(midPoint, distance, source.OID == (uint)OID.FlareSphere, default));
            }
        }

        if (mergeCombinations.Count == 4) {
            DateTime activationStart = WorldState.FutureTime(9.1f);
            mergeCombinations.Sort((a, b) => a.Distance.CompareTo(b.Distance));

            for (int i = 0; i < mergeCombinations.Count; i++) {
                mergeCombinations[i] = mergeCombinations[i] with { Activation = activationStart + TimeSpan.FromSeconds(3.0f * i) };
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID is (uint)AID.TinyFlare or (uint)AID.TinyHoly1) {
            if (mergeCombinations.Count > 0) {
                mergeCombinations.Sort((a, b) => a.Distance.CompareTo(b.Distance));
                mergeCombinations.RemoveAt(0);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        if (mergeCombinations.Count == 0) {
            return;
        }

        var nextCombinations = mergeCombinations.OrderBy(c => c.Distance).Take(2).ToList();

        for (int i = 0; i < nextCombinations.Count; i++) {
            var combination = nextCombinations[i];
            if (combination.IsFlare) {
                flareShape.Draw(Arena, combination.Origin, default, i == 0 ? Colors.Danger : Colors.AOE);
            }

            if (!combination.IsFlare) {
                var endPoint = Components.GenericKnockback.AwayFromSource(pc.Position, combination.Origin, holyKnockBackDistance);
                Components.GenericKnockback.DrawKnockback(pc, endPoint, Arena);
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc) {
        if (mergeCombinations.Count == 0) {
            return;
        }

        var nextCombinations = mergeCombinations.OrderBy(c => c.Distance).Take(2).ToList();

        foreach (var combination in nextCombinations) {
            if (!combination.IsFlare) {
                Arena.ZoneCircle(combination.Origin, 2.0f, Colors.Other7);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) {
        if (mergeCombinations.Count == 0) {
            return;
        }

        var nextCombinations = mergeCombinations.OrderBy(c => c.Distance).Take(2).ToList();
        foreach (var combination in nextCombinations) {
            if (combination.IsFlare && flareShape.Check(actor.Position, combination.Origin, default)) {
                hints.Add("GTFO from aoe!");
            }

            if (!combination.IsFlare) {
                var endPoint = Components.GenericKnockback.AwayFromSource(actor.Position, combination.Origin, holyKnockBackDistance);
                if (!Arena.InBounds(endPoint)) {
                    hints.Add("About to be knocked into wall!");
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        if (mergeCombinations.Count == 0) {
            return;
        }

        var nextCombinations = mergeCombinations.OrderBy(c => c.Distance).Take(2).ToList();
        var knockbackSetup = false;

        for (int i = 0; i < nextCombinations.Count; i++) {
            var combination = nextCombinations[i];
            if (combination.IsFlare) {
                hints.AddForbiddenZone(flareShape, combination.Origin, activation: combination.Activation);
            }

            // Safeguard so we don't try and solve both knockbacks at the same time, only happens if it knockback into knockback
            if (knockbackSetup) {
                return;
            }

            if (!combination.IsFlare) {
                var activation = combination.Activation;
                var circles = new (WPos Origin, float Radius)[2];
                for (var k = 0; k < 2 && nextCombinations.Count == 2; ++k) {
                    if (nextCombinations[k].IsFlare) {
                        circles[k] = (nextCombinations[k].Origin, flareShape.Radius);
                    }
                }

                hints.AddForbiddenZone(new SDKnockbackInAABBSquareAwayFromOriginPlusAOECirclesMixedRadii(Arena.Center, combination.Origin, 20f, 19f, circles, 2), activation);
                knockbackSetup = true;
            }
        }
    }
}

sealed class SphereGrowable(BossModule module) : BossComponent(module)
{
    private static readonly AOEShapeCircle flareShape = new(18.0f);
    private const float holyKnockBackDistance = 15.0f;
    private readonly List<Actor> mages = [];
    private Actor? orb = null;
    private int startIndex = -1;
    private int direction = 0;
    private WPos startPosition = default;
    private DateTime activation = default;
    private DiminutiveDualcast diminutiveDualcast = module.FindComponent<DiminutiveDualcast>()!;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.TinyApprentice)
        {
            mages.Add(actor);

            if (mages.Count == 4)
            {
                mages.Sort(delegate (Actor x, Actor y)
                {
                    var north = Angle.AnglesCardinals[2];
                    var xAngle = (x.Position - Arena.Center).ToAngle();
                    var yAngle = (y.Position - Arena.Center).ToAngle();

                    var xDeg = xAngle.AlmostEqual(north, 0.01f) ? 180f : xAngle.Deg;
                    var yDeg = yAngle.AlmostEqual(north, 0.01f) ? 180f : yAngle.Deg;

                    return xDeg < yDeg ? 1 : -1;
                });
            }
        }

        if (actor.OID is (uint)OID.FlareSphereGrow or (uint)OID.HolySphereGrow)
        {
            orb = actor;
            activation = WorldState.FutureTime(15.6);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.TinyFlare1 or (uint)AID.TinyHoly1)
        {
            orb = null;
            startIndex = -1;
            direction = 0;
            activation = default;
            startPosition = default;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var target = SolveExplosionMage();

        if (target == null || orb == null)
        {
            return;
        }

        if (orb.OID == (uint)OID.FlareSphereGrow)
        {
            flareShape.Draw(Arena, target.Position);
        }

        if (orb.OID == (uint)OID.HolySphereGrow)
        {
            var endPoint = Components.GenericKnockback.AwayFromSource(pc.Position, target.Position, holyKnockBackDistance);
            Components.GenericKnockback.DrawKnockback(pc, endPoint, Arena);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var target = SolveExplosionMage();

        if (target == null || orb == null)
        {
            return;
        }

        if (orb.OID == (uint)OID.HolySphereGrow)
        {
            Arena.ZoneCircle(target.Position, 2.0f, Colors.Other7);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var target = SolveExplosionMage();

        if (target == null || orb == null)
        {
            return;
        }

        if (orb.OID == (uint)OID.FlareSphereGrow && flareShape.Check(actor.Position, target.Position, default))
        {
            hints.Add("GTFO from aoe!");
        }

        if (orb.OID == (uint)OID.HolySphereGrow)
        {
            var endPoint = Components.GenericKnockback.AwayFromSource(actor.Position, target.Position, holyKnockBackDistance);
            if (!Arena.InBounds(endPoint))
            {
                hints.Add("About to be knocked into wall!");
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var target = SolveExplosionMage();

        if (target == null || orb == null)
        {
            return;
        }

        if (orb.OID == (uint)OID.FlareSphereGrow)
        {
            hints.AddForbiddenZone(flareShape, target.Position, activation: activation);
        }

        // If the sphere is a knockback we should wait until the final set of aoes
        if (diminutiveDualcast.middleActive == true) {
            return;
        }

        if (orb.OID == (uint)OID.HolySphereGrow) {
            hints.AddForbiddenZone(new SDKnockbackInCircleAwayFromOrigin(Arena.Center, target.Position, holyKnockBackDistance, 19.0f), activation);
        }
    }

    private Actor? SolveExplosionMage()
    {
        if (mages.Count == 0 || orb == null)
        {
            return null;
        }

        if (startIndex == -1)
        {
            var startAOE = mages.FindIndex(a => a.Position.AlmostEqual(orb.Position, 0.5f));
            if (startAOE < 0)
            {
                return null;
            }

            startIndex = startAOE;
            startPosition = orb.Position;
        }

        if (direction == 0)
        {
            var distanceMoved = orb.Position - startPosition;
            if (distanceMoved.Length() < 3.0f)
            {
                return null;
            }

            int bestIndex = -1;
            float bestDot = float.MinValue;

            for (int i = 0; i < mages.Count; i++)
            {
                if (i == startIndex)
                {
                    continue;
                }

                var mageDistance = (mages[i].Position - startPosition).Normalized();
                var dot = mageDistance.Dot(distanceMoved);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestIndex = i;
                }
            }

            if (bestIndex < 0)
            {
                return null;
            }

            direction = (bestIndex - startIndex + 4) % 4 == 1 ? 1 : -1;
        }

        return mages[(startIndex + 4 - direction) % 4];
    }
}

[SkipLocalsInit]
sealed class CE214TinyTerrorStates : StateMachineBuilder
{
    public CE214TinyTerrorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TinyThunderIII>()
            .ActivateOnEnter<TinyQuake>()
            .ActivateOnEnter<DiminutiveDualcast>()
            .ActivateOnEnter<TinyMeteor>()
            .ActivateOnEnter<Comet>()
            .ActivateOnEnter<FlareHolyMerge>()
            .ActivateOnEnter<SphereGrowable>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(CE214TinyTerrorStates),
    ConfigType = null, // replace null with typeof(TinyMageConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = typeof(TetherID),
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.TinyMage,
    Contributors = "Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14795u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class CE214TinyTerror(WorldState ws, Actor primary) : BossModule(ws, primary, new(152.000f, 716.000f), new ArenaBoundsCircle(20f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.ArcaneSphereSmall));
        Arena.Actors(Enemies((uint)OID.ArcaneSphereBig));
    }

    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 20f);
}
