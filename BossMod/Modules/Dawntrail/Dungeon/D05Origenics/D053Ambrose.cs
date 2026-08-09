namespace BossMod.Dawntrail.Dungeon.D05Origenics.D053Ambrose;

public enum OID : uint
{
    Boss = 0x417D, // R4.998
    Electrolance = 0x4180, // R1.38
    Superfluity = 0x417F, // R1.8
    OrigenicsEyeborg = 0x417E, // R4.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // Superfluity/OrigenicsEyeborg->player, no cast, single-target
    Teleport = 36439, // Boss->location, no cast, single-target

    PsychicWave = 36436, // Boss->self, 5.0s cast, range 80 circle

    OverwhelmingChargeVisual = 36435, // Boss->self, no cast, single-target
    OverwhelmingCharge1 = 39233, // Boss->self, 5.0s cast, range 26 180-degree cone
    OverwhelmingCharge2 = 39072, // Helper->self, 9.8s cast, range 26 180-degree cone

    PsychokinesisVisual1 = 36427, // Boss->self, 10.0s cast, single-target
    PsychokinesisVisual2 = 38929, // Boss->self, 8.0s cast, single-target
    Psychokinesis = 36428, // Helper->self, 10.0s cast, range 70 width 13 rect

    ExtrasensoryField = 36432, // Boss->self, 7.0s cast, single-target
    ExtrasensoryExpulsionWestEast = 36434, // Helper->self, 7.0s cast, range 15 width 20 rect
    ExtrasensoryExpulsionNorthSouth = 36433, // Helper->self, 7.0s cast, range 20 width 15 rect

    VoltaicSlash = 36437, // Boss->player, 5.0s cast, single-target
    PsychokineticCharge = 39055, // Boss->self, 7.0s cast, single-target

    Electrolance = 36429, // Boss->location, 6.0s cast, range 22 circle

    RushTelegraph = 38953, // Helper->location, 2.5s cast, width 10 rect charge
    Rush = 38954, // Electrolance->location, no cast, width 10 rect charge
    ElectrolanceAssimilationVisual = 36430, // Boss->self, 0.5s cast, single-target
    ElectrolanceAssimilation = 36431, // Helper->self, 1.0s cast, range 33 width 10 rect

    WhorlOfTheMind = 36438 // Helper->player, 5.0s cast, range 5 circle
}

sealed class PsychicWaveArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.PsychicWave && Arena.Bounds.Radius > 30f)
        {
            var center = Arena.Center;
            var shape = new AOEShapeCustom(center, [new Rectangle(center, 32.5f, 23.5f)], [new Rectangle(center, 15f, 19.5f)]);
            _aoe = [new(shape, center, default, Module.CastFinishAt(spell, 0.7d), shapeDistance: shape.Distance(center, default))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x28 && state == 0x00020001u)
        {
            Arena.Bounds = new ArenaBoundsRect(15f, 19.5f);
            _aoe = [];
        }
    }
}

sealed class PsychicWave(BossModule module) : Components.RaidwideCast(module, (uint)AID.PsychicWave);
sealed class Psychokinesis(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Psychokinesis, new AOEShapeRect(70f, 6.5f));

sealed class ExtrasensoryExpulsion(BossModule module) : Components.GenericKnockback(module, maxCasts: 1)
{
    public readonly List<Knockback> KBs = [with(4)];
    public readonly AOEShapeRect RectNS = new(20f, 7.5f);
    public readonly AOEShapeRect RectEW = new(15f, 10f);
    private OverwhelmingCharge? _aoe;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => CollectionsMarshal.AsSpan(KBs);

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        _aoe ??= Module.FindComponent<OverwhelmingCharge>();
        if (_aoe!.AOE.Length != 0)
        {
            ref var activeAOE = ref _aoe.AOE[0];
            if (activeAOE.Check(pos))
            {
                return true;
            }
        }
        return !Arena.InBounds(pos);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddSource(AOEShape shape) => KBs.Add(new(spell.LocXZ, 20f, Module.CastFinishAt(spell), shape, spell.Rotation, Kind.DirForward));
        var id = spell.Action.ID;
        if (id == (uint)AID.ExtrasensoryExpulsionNorthSouth)
        {
            AddSource(RectNS);
        }
        else if (id == (uint)AID.ExtrasensoryExpulsionWestEast)
        {
            AddSource(RectEW);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (KBs.Count != 0 && spell.Action.ID is (uint)AID.ExtrasensoryExpulsionNorthSouth or (uint)AID.ExtrasensoryExpulsionWestEast)
        {
            KBs.Clear();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = KBs.Count;
        if (KBs.Count != 0)
        {
            var forbidden = new List<ShapeDistance>(2);
            var kbs = CollectionsMarshal.AsSpan(KBs);
            for (var i = 0; i < count; ++i)
            {
                ref var recti = ref kbs[i];
                if (recti.Shape is AOEShapeRect rect && rect == RectNS)
                {
                    forbidden.Add(new SDInvertedRect(recti.Origin, recti.Direction, 19f, default, 7f));
                }
            }

            hints.AddForbiddenZone(new SDIntersection([.. forbidden]), kbs[0].Activation);
        }
    }
}

sealed class VoltaicSlash(BossModule module) : Components.SingleTargetCast(module, (uint)AID.VoltaicSlash);

sealed class OverwhelmingCharge(BossModule module) : Components.GenericAOEs(module)
{
    private readonly ExtrasensoryExpulsion _kb = module.FindComponent<ExtrasensoryExpulsion>()!;
    private readonly AOEShapeCone cone = new(26f, 90f.Degrees());
    private readonly AOEShapeRect rectAdj = new(19f, 7f); // the knockback rectangles are placed poorly with significant error from visuals plus half height of the arena is smaller than 20 knockback distance

    public AOEInstance[] AOE = [];
    private readonly Angle a180 = 180f.Degrees();

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _kb.KBs.Count;
        var componentActive = count != 0 || actor.PendingKnockbacks.Count != 0;

        if (AOE.Length != 0)
        {
            if (componentActive)
            {
                var kbs = CollectionsMarshal.AsSpan(_kb.KBs);
                ref var aoe = ref AOE[0];
                for (var i = 0; i < count; ++i)
                {
                    ref readonly var kb = ref kbs[i];
                    if (aoe.Rotation.AlmostEqual(kb.Direction + a180, Angle.DegToRad))
                    {
                        return new AOEInstance[1] { new(rectAdj, kb.Origin, kb.Direction, kb.Activation, Colors.SafeFromAOE, false) };
                    }
                }
            }
            else
            {
                return AOE;
            }
        }
        else if (componentActive)
        {
            var kbs = CollectionsMarshal.AsSpan(_kb.KBs);
            for (var i = 0; i < count; ++i)
            {
                ref var recti = ref kbs[i];
                if (recti.Shape is AOEShapeRect rect && rect.LengthFront == 20f)
                {
                    return new AOEInstance[1] { new(rectAdj, recti.Origin, recti.Direction, recti.Activation, Colors.SafeFromAOE, false) };
                }
            }
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.OverwhelmingCharge1 or (uint)AID.OverwhelmingCharge2)
        {
            AOE = [new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.OverwhelmingCharge1 or (uint)AID.OverwhelmingCharge2)
        {
            AOE = [];
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var component = _kb.KBs.Count != 0;
        if (component && AOE.Length != 0)
        {
            ref var aoe = ref AOE[0];
            hints.AddForbiddenZone(aoe.Shape, aoe.Origin, aoe.Rotation + a180, aoe.Activation);
        }
        else
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var aoes = ActiveAOEs(slot, actor);
        var len = aoes.Length;
        if (len != 0)
        {
            base.AddHints(slot, actor, hints);

            var actorInSafespot = false;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var aoe = ref aoes[i];
                if (aoe.Shape == rectAdj && aoe.Check(actor.Position))
                {
                    actorInSafespot = true;
                    break;
                }
            }
            hints.Add("Wait inside safespot for knockback!", !actorInSafespot);
        }
    }
}

sealed class Electrolance(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Electrolance, 22f);
sealed class WhorlOfTheMind(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.WhorlOfTheMind, 5f);

sealed class Rush(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeRect rect = new(33f, 5f);
    private readonly List<AOEInstance> _aoes = [with(7)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        if (count > 1)
        {
            aoes[0].Color = Colors.Danger;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RushTelegraph)
        {
            var activation = Module.CastFinishAt(spell, 6.8d);
            var dir = spell.LocXZ - caster.Position;

            if (_aoes.Count < 7)
            {
                _aoes.Add(new(new AOEShapeRect(dir.Length(), 5f), caster.Position, Angle.FromDirection(dir), activation));
            }
            else
            {
                _aoes.Add(new(rect, new(190f, 19.5f), -180f.Degrees(), activation));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.Rush or (uint)AID.ElectrolanceAssimilation)
        {
            _aoes.RemoveAt(0);
        }
    }
}

sealed class D053AmbroseStates : StateMachineBuilder
{
    public D053AmbroseStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PsychicWaveArenaChange>()
            .ActivateOnEnter<PsychicWave>()
            .ActivateOnEnter<Psychokinesis>()
            .ActivateOnEnter<ExtrasensoryExpulsion>()
            .ActivateOnEnter<OverwhelmingCharge>()
            .ActivateOnEnter<VoltaicSlash>()
            .ActivateOnEnter<Electrolance>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<WhorlOfTheMind>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 825u, NameID = 12695u, SortOrder = 4)]
public sealed class D053Ambrose(WorldState ws, Actor primary) : BossModule(ws, primary, new(190f, 0f), new ArenaBoundsRect(32.5f, 24f))
{
    private static readonly uint[] adds = [(uint)OID.Superfluity, (uint)OID.OrigenicsEyeborg];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, adds);
    }
}
