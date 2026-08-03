namespace BossMod.Dawntrail.Quest.MSQ.TheWarmthOfTheFamily.Tturuhhetso;

public enum OID : uint
{
    Boss = 0x4638, // R5.95
    ScaleArmoredLeg = 0x4648, // R5.95
    BallOfFire = 0x4639, // R1.0
    Koana = 0x4630, // R0.5
    WukLamat = 0x4631, // R0.5
    Orbs = 0x463A, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    Ensnare = 40566, // Boss->location, 5.0s cast, range 6 circle
    Regenerate = 40583, // Boss->self, 4.0s cast, single-target

    SearingSwellVisual1 = 40577, // Boss->self, 4.0s cast, single-target
    SearingSwellVisual2 = 40578, // Boss->self, no cast, single-target
    SearingSwell = 40579, // Helper->self, 4.0s cast, range 40 45-degree cone

    FlameBlast = 40580, // BallOfFire->self, 2.0s cast, range 40 width 4 rect

    TriceraSnareVisual = 40573, // Boss->self, 5.0s cast, single-target, spread
    TriceraSnare = 40574, // Boss->player/WukLamat/Koana, no cast, range 6 circle

    PrimordialRoar1 = 41271, // Boss->self, 5.0s cast, range 40 circle
    PrimordialRoar2 = 40584, // Boss->self, 5.0s cast, range 40 circle

    Pull = 40664, // Helper->Boss, 1.2s cast, single-target, pull 30 between centers, after leg got destroyed

    RazingRoarVisual = 40586, // Boss->self, no cast, single-target
    RazingRoarPlayer = 41245, // Helper->player, no cast, single-target, knockback 30, dir forward
    RazingRoarWukLamat = 41246, // Helper->WukLamat, 0.5s cast, single-target, knockback 13, dir forward
    RazingRoarKoana = 41247, // Helper->Koana, 0.5s cast, single-target, down for the count

    ModelStateChange = 40570, // Boss->self, no cast, single-target

    FirestormVisual = 40575, // Boss->self, 5.0s cast, single-target
    Firestorm = 40576, // Helper->self, 5.0s cast, range 10 circle

    CandescentRayTB = 40571, // Boss->self/players, 8.0s cast, range 50 width 8 rect, shared tankbuster, only happens if playing duty with tank
    CandescentRayMarker = 40569, // Helper->player, no cast, single-target
    CandescentRayLineStack = 40572, // Boss->self/players, 8.0s cast, range 50 width 8 rect, shared tankbuster, only happens if not playing duty with tank

    Aethercall = 40581, // Boss->self, 5.0s cast, single-target
    CollectOrb = 40582, // Orbs->player/Koana/WukLamat, no cast, single-target
    BossOrb = 41117 // Orbs->Boss, no cast, single-target
}

public enum IconID : uint
{
    Spreadmarker = 140 // WukLamat/Koana/player->self
}

sealed class CandescentRayLineStack(BossModule module) : Components.LineStack(module, aidMarker: default, (uint)AID.CandescentRayLineStack, minStackSize: 3, maxStackSize: 3);
sealed class CandescentRayTB(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.CandescentRayTB, new AOEShapeRect(50f, 4f))
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Target == null)
            return;
        hints.Add("Stack with Wuk Lamat!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Target == null)
            return;
        var koanas = Module.Enemies((uint)OID.Koana);
        var wuks = Module.Enemies((uint)OID.WukLamat);
        var koana = koanas.Count != 0 ? koanas[0] : null;
        var wuk = wuks.Count != 0 ? wuks[0] : null;
        var primary = Module.PrimaryActor;
        if (koana != null)
            hints.AddForbiddenZone(new SDCone(primary.Position, 100f, primary.AngleTo(koana), Angle.Asin(4f / (koana.Position - primary.Position).Length())), Activation);
        if (wuk != null)
            hints.AddForbiddenZone(new SDInvertedRect(primary.Position, primary.AngleTo(wuk), 50f, default, 4f), Activation);
    }
}

sealed class SearingSwell(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SearingSwell, new AOEShapeCone(40f, 22.5f.Degrees()));
sealed class Ensnare(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Ensnare, 6f);
sealed class TriceraSnare(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, (uint)AID.TriceraSnare, 6f, 4.7f)
{
    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        // on phase change all pending spreads get cancelled
        Spreads.Clear();
    }
}

sealed class PrimordialRoar1(BossModule module) : Components.RaidwideCast(module, (uint)AID.PrimordialRoar1);
sealed class PrimordialRoar2(BossModule module) : Components.RaidwideCast(module, (uint)AID.PrimordialRoar2);

sealed class OrbCollecting(BossModule module) : BossComponent(module)
{
    public static List<Actor> GetOrbs(BossModule module)
    {
        var orbs = module.Enemies((uint)OID.Orbs);
        var count = orbs.Count;
        if (count == 0)
            return [];

        var filteredorbs = new List<Actor>(count);
        for (var i = 0; i < count; ++i)
        {
            var z = orbs[i];
            if (z.Tether.ID != 0)
                filteredorbs.Add(z);
        }
        return filteredorbs;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (GetOrbs(Module).Count != 0)
            hints.Add("Soak the orbs!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var orbs = GetOrbs(Module);
        var count = orbs.Count;
        if (count != 0)
        {
            var orbz = new ShapeDistance[count];
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Sprint), actor, ActionQueue.Priority.High);
            for (var i = 0; i < count; ++i)
            {
                var o = orbs[i];
                orbz[i] = new SDInvertedRect(o.Position + 0.5f * o.Rotation.ToDirection(), new WDir(0f, 1f), 0.5f, 0.5f, 0.5f);
            }
            hints.AddForbiddenZone(new SDIntersection(orbz), DateTime.MaxValue);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var orbs = GetOrbs(Module);
        var count = orbs.Count;
        for (var i = 0; i < count; ++i)
            Arena.ZoneCircleOutline(orbs[i].Position, 1f, Colors.Safe);
    }
}

sealed class FlameBlast(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(20f, 2f, 20f);
    private readonly List<AOEInstance> _aoes = [];
    private static readonly Angle a90 = 90f.Degrees();

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 10 ? 10 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];
            if (i < 5)
            {
                if (count > 5)
                    aoe.Color = Colors.Danger;
                aoe.Risky = true;
            }
            else
            {
                if (aoes[0].Rotation.AlmostEqual(aoes[5].Rotation, Angle.DegToRad))
                    aoe.Risky = false;
            }
        }
        return aoes[..max];
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.BallOfFire)
            _aoes.Add(new(rect, actor.Position, actor.Rotation + a90, WorldState.FutureTime(6.7d)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.FlameBlast)
            _aoes.RemoveAt(0);
    }
}

sealed class Firestorm(BossModule module) : Components.SimpleAOEGroupsByTimewindow(module, [(uint)AID.Firestorm], 10f, 1d, 10);

sealed class TturuhhetsoStates : StateMachineBuilder
{
    public TturuhhetsoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CandescentRayLineStack>()
            .ActivateOnEnter<CandescentRayTB>()
            .ActivateOnEnter<FlameBlast>()
            .ActivateOnEnter<Ensnare>()
            .ActivateOnEnter<TriceraSnare>()
            .ActivateOnEnter<PrimordialRoar1>()
            .ActivateOnEnter<PrimordialRoar2>()
            .ActivateOnEnter<OrbCollecting>()
            .ActivateOnEnter<Firestorm>()
            .ActivateOnEnter<SearingSwell>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70785, NameID = 13593)]
public sealed class Tturuhhetso(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(395.735f, -45.365f), 19.5f, 20)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.ScaleArmoredLeg));
    }
}
