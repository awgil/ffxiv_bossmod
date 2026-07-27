namespace BossMod.Dawntrail.Dungeon.D02WorqorZormor.D023Gurfurlur;

public enum OID : uint
{
    Boss = 0x415F, // R7.000, x1
    AuraSphere = 0x4162, // R1.0
    BitingWind = 0x4160, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    HeavingHaymakerVisual = 36269, // Boss->self, 5.0s cast, single-target
    HeavingHaymaker = 36375, // Helper->self, 5.3s cast, range 60 circle

    Stonework = 36301, // Boss->self, 3.0s cast, single-target
    LithicImpact = 36302, // Helper->self, 6.8s cast, range 4 width 4 rect
    GreatFlood = 36307, // Helper->self, 7.0s cast, range 80 width 60 rect

    Allfire1 = 36303, // Helper->self, 7.0s cast, range 10 width 10 rect
    Allfire2 = 36304, // Helper->self, 8.5s cast, range 10 width 10 rect
    Allfire3 = 36305, // Helper->self, 10.0s cast, range 10 width 10 rect

    VolcanicDrop = 36306, // Helper->player, 5.0s cast, range 6 circle

    SledgeHammerMarker = 36315, // Helper->player, no cast, single-target
    Sledgehammer1 = 36313, // Boss->self/players, 5.0s cast, range 60 width 8 rect, line stack
    Sledgehammer2 = 36314, // Boss->self, no cast, range 60 width 8 rect
    Sledgehammer3 = 39260, // Boss->self, no cast, range 60 width 8 rect

    ArcaneStomp = 36319, // Boss->self, 3.0s cast, single-target

    ShroudOfEons1 = 36321, // AuraSphere->player, no cast, single-target
    ShroudOfEons2 = 36322, // AuraSphere->Boss, no cast, single-target

    EnduringGlory = 36320, // Boss->self, 6.0s cast, range 60 circle

    Windswrath1 = 36310, // Helper->self, 7.0s cast, range 40
    Windswrath2 = 39074, // Helper->self, 15.0s cast, range 40 circle

    Whirlwind = 36311 // Helper->self, no cast, range 5 circle
}

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom square = new([new Square(D023Gurfurlur.ArenaCenter, 25f)], [new Square(D023Gurfurlur.ArenaCenter, 20f)]);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HeavingHaymaker && Arena.Bounds.Radius > 20f)
        {
            _aoe = [new(square, Arena.Center, default, Module.CastFinishAt(spell, 0.6d))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x18 && state == 0x00020001u)
        {
            Arena.Bounds = D023Gurfurlur.DefaultBounds;
            _aoe = [];
        }
    }
}

sealed class AuraSphere(BossModule module) : BossComponent(module)
{
    public static List<Actor> GetOrbs(BossModule module)
    {
        var orbs = module.Enemies((uint)OID.AuraSphere);
        var count = orbs.Count;
        if (count == 0)
            return [];

        var filteredorbs = new List<Actor>(count);
        for (var i = 0; i < count; ++i)
        {
            var z = orbs[i];
            if (!z.IsDead)
                filteredorbs.Add(z);
        }
        return filteredorbs;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        var orbs = GetOrbs(Module);
        var count = orbs.Count;
        if (count != 0)
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
                orbz[i] = new SDInvertedRect(o.Position + 0.5f * o.Rotation.ToDirection(), new WDir(default, 1f), 0.5f, 0.5f, 0.5f);
            }
            hints.AddForbiddenZone(new SDIntersection(orbz), DateTime.MaxValue);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var orbs = GetOrbs(Module);
        var count = orbs.Count;
        for (var i = 0; i < count; ++i)
            Arena.AddCircle(orbs[i].Position, 1f, Colors.Safe);
    }
}

sealed class SledgeHammer(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.SledgeHammerMarker, (uint)AID.Sledgehammer3, 4.9d);
sealed class HeavingHaymaker(BossModule module) : Components.RaidwideCast(module, (uint)AID.HeavingHaymaker);
sealed class LithicImpact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LithicImpact, new AOEShapeRect(4f, 2f));
sealed class Whirlwind(BossModule module) : Components.Voidzone(module, 5f, GetVoidzones, 7f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.BitingWind);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

sealed class GreatFlood(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.GreatFlood, 25f, kind: Kind.DirForward)
{
    private readonly Allfire _aoe = module.FindComponent<Allfire>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
            return;

        if (_aoe.AOEs.Count == 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            hints.AddForbiddenZone(new SDInvertedRect(c.Origin, c.Direction, 15f, default, 20f), c.Activation);
        }
    }
}

sealed class Allfire(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(10f, 5f);
    private static readonly AOEShapeRect safespot = new(15f, 10f, invertForbiddenZone: true);
    public readonly List<AOEInstance> AOEs = [with(16)];
    private bool first = true;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];

        var aoes = CollectionsMarshal.AsSpan(AOEs);
        if (!first)
            return aoes;

        var max = count >= 12 ? 12 : count >= 8 ? 8 : count >= 4 ? 4 : count;
        var deadline = aoes[0].Activation.AddSeconds(1d);
        var color = Colors.Danger;
        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];
            if (aoe.Activation < deadline)
            {
                if (count > 4)
                    aoe.Color = color;
                aoe.Risky = true;
            }
            else
                aoe.Risky = false;
        }
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (first && spell.Action.ID is (uint)AID.Allfire1 or (uint)AID.Allfire2 or (uint)AID.Allfire3)
        {
            AOEs.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            if (AOEs.Count == 16)
            {
                SortHelpers.SortAOEByActivation(AOEs);
            }
        }
        else if (!first && spell.Action.ID == (uint)AID.GreatFlood)
        {
            AOEs.Add(new(safespot, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), Colors.SafeFromAOE));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (AOEs.Count != 0 && spell.Action.ID is (uint)AID.Allfire1 or (uint)AID.Allfire2 or (uint)AID.Allfire3 or (uint)AID.GreatFlood)
        {
            AOEs.RemoveAt(0);
            if (AOEs.Count == 0)
            {
                first = false;
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (AOEs.Count == 0)
        {
            return;
        }

        if (first)
        {
            base.AddHints(slot, actor, hints);
            return;
        }
        ref var aoe = ref AOEs.Ref(0);
        hints.Add("Wait inside safespot for knockback!", !aoe.Check(actor.Position));
    }
}

sealed class VolcanicDrop(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.VolcanicDrop, 6f);
sealed class EnduringGloryGreatFloodWindswrathRaidwide(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.EnduringGlory, (uint)AID.GreatFlood,
(uint)AID.Windswrath1, (uint)AID.Windswrath2]);

class Windswrath(BossModule module, uint aid) : Components.SimpleKnockbacks(module, aid, 15f);
sealed class Windswrath1(BossModule module) : Windswrath(module, (uint)AID.Windswrath1)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
        {
            return;
        }
        ref readonly var c = ref Casters.Ref(0);
        hints.AddForbiddenZone(new SDKnockbackInAABBSquareAwayFromOrigin(Arena.Center, c.Origin, 15f, 19f), c.Activation);
    }
}

sealed class Windswrath2(BossModule module) : Windswrath(module, (uint)AID.Windswrath2)
{
    private enum Pattern { None, EWEW, WEWE }
    private Pattern CurrentPattern;
    private static readonly Angle a15 = 15f.Degrees(), a165 = 165f.Degrees(), a105 = 105f.Degrees(), a75 = 75f.Degrees();
    private readonly Whirlwind _aoe = module.FindComponent<Whirlwind>()!;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.BitingWind)
        {
            var z = actor.PosRot.Z;
            if (z == -180f)
            {
                CurrentPattern = Pattern.EWEW;
            }
            else if (z == -210f)
            {
                CurrentPattern = Pattern.WEWE;
            }
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            if (aoes[i].Check(pos))
            {
                return true;
            }
        }
        return !Arena.InBounds(pos);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
            return;

        if (_aoe.ActiveAOEs(slot, actor).Length != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            var act = c.Activation;

            if ((act - WorldState.CurrentTime).TotalSeconds <= 3d)
            {
                var patternWEWE = CurrentPattern == Pattern.WEWE;
                var origin = c.Origin;
                var forbidden = new ShapeDistance[4]
                {
                    new SDInvertedCone(origin, 5f, patternWEWE ? a15 : -a15, a15),
                    new SDInvertedCone(origin, 5f, patternWEWE ? -a165 : a165, a15),
                    new SDInvertedCone(origin, 5f, patternWEWE ? a105 : -a105, a15),
                    new SDInvertedCone(origin, 5f, patternWEWE ? -a75 : a75, a15)
                };
                hints.AddForbiddenZone(new SDIntersection(forbidden), act);
            }
            else
            {
                hints.AddForbiddenZone(new SDInvertedCircle(c.Origin, 8f), act);
            }
        }
    }
}

sealed class D023GurfurlurStates : StateMachineBuilder
{
    public D023GurfurlurStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<HeavingHaymaker>()
            .ActivateOnEnter<Whirlwind>()
            .ActivateOnEnter<AuraSphere>()
            .ActivateOnEnter<LithicImpact>()
            .ActivateOnEnter<Allfire>()
            .ActivateOnEnter<GreatFlood>()
            .ActivateOnEnter<VolcanicDrop>()
            .ActivateOnEnter<SledgeHammer>()
            .ActivateOnEnter<Windswrath1>()
            .ActivateOnEnter<Windswrath2>()
            .ActivateOnEnter<EnduringGloryGreatFloodWindswrathRaidwide>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 824, NameID = 12705)]
public sealed class D023Gurfurlur(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(-54f, -195f);
    public static readonly ArenaBoundsSquare StartingBounds = new(24.5f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(20f);
}
