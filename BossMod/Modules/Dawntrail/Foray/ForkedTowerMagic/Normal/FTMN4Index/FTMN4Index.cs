namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN4Index;

sealed class ArenaChange(BossModule module) : BossComponent(module)
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00)
        {
            switch (state)
            {
                case 0x00020001:
                    Arena.Bounds = Index.OmniElementsBounds;
                    Arena.Center = Index.OmniElementsCenter;
                    break;
                case 0x00080004:
                    Arena.Bounds = Index.InitialBounds;
                    Arena.Center = Index.InitialCenter;
                    break;
            }
        }
    }
}
sealed class OmniElementPanels(BossModule module) : BossComponent(module)
{
    // spawns elemental panels with rotation 0, 60, 120
    public List<Actor> Actors = [];

    public override void OnActorCreated(Actor actor)
    {
        switch (actor.OID)
        {
            case (uint)OID.OmniElementFire:
            case (uint)OID.OmniElementIce:
            case (uint)OID.OmniElementThunder:
                Actors.Add(actor);
                break;
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        switch (actor.OID)
        {
            case (uint)OID.OmniElementFire:
            case (uint)OID.OmniElementIce:
            case (uint)OID.OmniElementThunder:
                Actors.Remove(actor);
                break;
        }
    }
}
sealed class Flare(BossModule module) : Components.RaidwideCast(module, (uint)AID.Flare);
sealed class Bombs(BossModule module) : Components.Adds(module, (uint)OID.SummonedBomb, 2)
{
    private readonly IndexConfig _config = Service.Config.Get<IndexConfig>();
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (ActiveActors.Count != 0)
        {
            hints.Add("Kill the Bombs!");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        var actors = CollectionsMarshal.AsSpan(ActiveActors);
        var count = actors.Length;
        for (var i = 0; i < count; i++)
        {
            ref var actor = ref actors[i];
            Arena.ZoneCircleOutline(actor.Position, 2f);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ActiveActors.Count != 0)
        {
            hints.PrioritizeTargetsByOID((uint)OID.SummonedBomb, 2);
            // ignore forced targetting if current target is a PC
            if (_config.ForceAddTargeting && WorldState.Actors.Find(actor.TargetID) is var target && target?.Type != ActorType.Player && target?.OID != (uint)OID.SummonedBomb)
            {
                hints.ForcedTarget = ActiveActors.MinBy(actor.DistanceToHitbox);
            }
        }
        else if (_config.ForceBossTargeting && WorldState.Actors.Find(actor.TargetID) == null)
        {
            hints.ForcedTarget = Module.PrimaryActor;
        }
    }
}
sealed class Aim(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Aim, 11f)
{
    // resolves after shockwave and predict; ignore until predict AOEs are gone
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var knockbacks = Module.FindComponent<Shockwave>();
        var predict = Module.FindComponent<Predict>();
        if (knockbacks?.ActiveKnockbacks(slot, actor).Length == 0 && predict?.ActiveAOEs(slot, actor).Length == 0)
        {
            base.AddHints(slot, actor, hints);
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var knockbacks = Module.FindComponent<Shockwave>();
        var predict = Module.FindComponent<Predict>();
        if (knockbacks?.ActiveKnockbacks(slot, actor).Length == 0 && predict?.ActiveAOEs(slot, actor).Length == 0)
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}
sealed class RomeosBallad(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RomeosBallad, 15f)
{
    // resolves after shockwave and predict; ignore until predict AOEs are gone
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var knockbacks = Module.FindComponent<Shockwave>();
        var predict = Module.FindComponent<Predict>();
        if (knockbacks?.ActiveKnockbacks(slot, actor).Length == 0 && predict?.ActiveAOEs(slot, actor).Length == 0)
        {
            base.AddHints(slot, actor, hints);
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var knockbacks = Module.FindComponent<Shockwave>();
        var predict = Module.FindComponent<Predict>();
        if (knockbacks?.ActiveKnockbacks(slot, actor).Length == 0 && predict?.ActiveAOEs(slot, actor).Length == 0)
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}
sealed class ElementaryChemistry(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElementaryChemistry, new AOEShapeRect(15f, 7.5f));
sealed class ElementaryEvocation(BossModule module) : Components.GenericAOEs(module)
{
    // panels either have 0rot or positive
    private readonly OmniElementPanels _panels = module.FindComponent<OmniElementPanels>()!;
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeCone _cone = new(30f, 30f.Degrees());
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }

        SortHelpers.SortAOEByActivation(_aoes);
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var max = count > 4 ? 4 : count;

        for (var i = 0; i < max; i++)
        {
            ref var aoe = ref aoes[i];
            aoe.Color = i < 2 ? Colors.Danger : default;
        }

        return aoes[..max];
    }

    public override void OnActorCreated(Actor actor)
    {
        var panelId = actor.OID switch
        {
            (uint)OID.SwirlingOrb => (uint)OID.OmniElementIce,
            (uint)OID.BallOfFire => (uint)OID.OmniElementFire,
            (uint)OID.BallOfLevin => (uint)OID.OmniElementThunder,
            _ => default
        };

        if (panelId == default)
        {
            return;
        }

        var ballRotation = actor.Rotation;
        Actor? targetPanel = null;
        var panels = CollectionsMarshal.AsSpan(_panels.Actors);
        var count = panels.Length;
        for (var i = 0; i < count; i++)
        {
            ref var panel = ref panels[i];
            if (panel.OID == panelId)
            {
                targetPanel = panel;
                break;
            }
        }

        if (targetPanel == null)
        {
            return;
        }

        var panelRotation = targetPanel.Rotation;
        var distance = ballRotation.DistanceToAngle(panelRotation);
        var degrees = distance.Deg;
        if (degrees < 0f)
        {
            // -30, -90, -150
            var delay = distance.AlmostEqual(-30f.Degrees(), 0.1f) ? 0 : distance.AlmostEqual(-90f.Degrees(), 0.1f) ? 1 : 2;
            var activation = WorldState.FutureTime(8d + 2.4d * delay);
            _aoes.Add(new(_cone, Module.PrimaryActor.Position, panelRotation, activation));
            _aoes.Add(new(_cone, Module.PrimaryActor.Position, panelRotation + 180f.Degrees(), activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.FireIV:
                case (uint)AID.BlizzardIV:
                case (uint)AID.ThunderIV:
                    _aoes.RemoveAt(0);
                    break;
            }
        }
    }
}
sealed class ElementaryExpansion(BossModule module) : Components.GenericAOEs(module)
{
    private readonly OmniElementPanels _panels = module.FindComponent<OmniElementPanels>()!;
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeCone _cone = new(30f, 30f.Degrees());
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }

        SortHelpers.SortAOEByActivation(_aoes);
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var max = count > 4 ? 4 : count;

        for (var i = 0; i < max; i++)
        {
            ref var aoe = ref aoes[i];
            aoe.Color = i < 2 ? Colors.Danger : default;
        }

        return aoes[..max];
    }

    // spawns actor for each elemental ring
    // creation vs renderflag only 0.04s apart, use creation
    // creation @109.856 -> 116.567 (6.7s)
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.ExpansionFire or (uint)OID.ExpansionIce or (uint)OID.ExpansionThunder)
        {
            var panelId = actor.OID switch
            {
                (uint)OID.ExpansionFire => (uint)OID.OmniElementFire,
                (uint)OID.ExpansionIce => (uint)OID.OmniElementIce,
                (uint)OID.ExpansionThunder => (uint)OID.OmniElementThunder,
                _ => default
            };

            if (panelId == default)
            {
                return;
            }

            var panels = CollectionsMarshal.AsSpan(_panels.Actors);
            var count = panels.Length;
            for (var i = 0; i < count; i++)
            {
                ref var panel = ref panels[i];
                if (panel.OID == panelId)
                {
                    var act = WorldState.FutureTime(6.7d);
                    var rotation = panel.Rotation;
                    _aoes.Add(new(_cone, Module.PrimaryActor.Position, rotation, act));
                    _aoes.Add(new(_cone, Module.PrimaryActor.Position, rotation + 180f.Degrees(), act));
                    break;
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.FireIV:
                case (uint)AID.BlizzardIV:
                case (uint)AID.ThunderIV:
                    _aoes.RemoveAt(0);
                    break;
            }
        }
    }
}

sealed class Shockwave(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Shockwave, 9f, shape: new AOEShapeCircle(15f), stopAfterWall: true)
{
    private readonly Aim _aim = module.FindComponent<Aim>()!;
    private readonly RomeosBallad _romeo = module.FindComponent<RomeosBallad>()!;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
        {
            return [];
        }

        var knockbacks = new List<Knockback>();
        for (var i = 0; i < count; i++)
        {
            // what happens if player standing in 2 circles?
            ref var kb = ref Casters.Ref(i);
            if (!IsImmune(slot, kb.Activation) && Shape!.Check(actor.Position, kb.Origin, default))
            {
                knockbacks.Add(kb);
                break;
            }
        }
        // don't check isimmune, use calculatemovement and go from there
        return CollectionsMarshal.AsSpan(knockbacks);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        AddHints(slot, actor, hints, null);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        AddHints(slot, actor, null, hints);
    }

    private void AddHints(int slot, Actor actor, TextHints? textHints, AIHints? aiHints)
    {
        // what happens when player stands in intersecting circle of 2 spears/knockbacks?
        // AI spazzes when getting to area in between 2 spear circles
        // if player standing in intersection, gets knocked back twice; how to determine order? proximity?
        // have AI only do predict 1 knockback, mark the other 2 circles as forbidden
        var kbs = ActiveKnockbacks(slot, actor);
        if (kbs.Length != 0)
        {
            var kb = kbs[0];
            var polygon = ((ArenaBoundsCustom)Arena.Bounds).Polygon;
            var aim = _aim.ActiveAOEs(slot, actor);
            var romeo = _romeo.ActiveAOEs(slot, actor);

            if (aim.Length == 0 && romeo.Length == 0)
            {
                if (textHints != null)
                {
                    var sd = new SDKnockbackInComplexPolygonAwayFromOrigin(Arena.Center, kb.Origin, Distance, polygon);
                    if (sd.Contains(actor.Position))
                    {
                        textHints?.Add("About to be knocked into danger!");
                    }
                }
                else
                {
                    var sd = new SDKnockbackInComplexPolygonAwayFromOrigin(Arena.Center, kb.Origin, Distance + 1f, polygon);
                    aiHints?.AddForbiddenZone(sd, kb.Activation);
                }
            }
            else
            {
                var isAim = aim.Length != 0;
                var radius = isAim ? 11f : 15f;
                var aoes = isAim ? aim : romeo;
                var count = aoes.Length;
                var origins = new WPos[count];

                for (var i = 0; i < count; i++)
                {
                    var aoe = aoes[i];
                    origins[i] = aoe.Origin;
                }

                if (textHints != null)
                {
                    var sd = new SDKnockbackInComplexPolygonAwayFromOriginPlusAOECircles(Arena.Center, kb.Origin, Distance, polygon, origins, radius, origins.Length);
                    if (sd.Contains(actor.Position))
                    {
                        textHints?.Add("About to be knocked into danger!");
                    }
                }
                else
                {
                    // avoid circle from other 2 knockbacks
                    var kbCount = Casters.Count;
                    for (var i = 0; i < kbCount; i++)
                    {
                        ref var other = ref Casters.Ref(i);
                        var origin = other.Origin;
                        if (!origin.AlmostEqual(kb.Origin, 1f))
                        {
                            aiHints?.AddForbiddenZone(new SDCircle(origin, radius + 1f), kb.Activation);
                        }
                    }
                    var sd = new SDKnockbackInComplexPolygonAwayFromOriginPlusAOECircles(Arena.Center, kb.Origin, Distance + 1f, polygon, origins, radius + 1f, origins.Length);
                    aiHints?.AddForbiddenZone(sd, kb.Activation);
                }
            }
        }
    }
}
sealed class DuologyOfImplements(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeCone _cone = new(30f, 30f.Degrees());
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];

        var max = count > 3 ? 3 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);

        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Iainuki:
            case (uint)AID.WindSlash:
                _aoes.Add(new(_cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.Iainuki:
                case (uint)AID.WindSlash:
                    _aoes.RemoveAt(0);
                    break;
            }
        }
    }
}
sealed class AllConsumingFlames(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spread, (uint)AID.AllConsumingFlames, 6f, 5d);
sealed class Predict(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly List<(Actor Source, ulong TargetID)> _tethered = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return CollectionsMarshal.AsSpan(_aoes);
    }

    // predict tethers go out 1st, then actor created and AOE status gained 0.1s later
    // is this always the case, or can it be either or?
    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Predict)
        {
            _tethered.Add((source, tether.Target));
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Predict)
        {
            var tethered = CollectionsMarshal.AsSpan(_tethered);
            var count = tethered.Length;
            for (var i = 0; i < count; i++)
            {
                var tether = tethered[i];
                var targetId = tether.TargetID;
                if (actor.InstanceID == targetId)
                {
                    var target = WorldState.Actors.Find(targetId);
                    if (target == null)
                    {
                        return;
                    }

                    var source = tether.Source;
                    _aoes.Add(new(status.Extra == 0x44C ? new AOEShapeDonut(4f, 15f) : new AOEShapeCircle(10f), source.Position, activation: WorldState.FutureTime(10d)));
                    break;
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.Cleansing:
                case (uint)AID.Starfall:
                    _aoes.RemoveAt(0);
                    break;
            }
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.ForetoldPhenomenon)
        {
            _tethered.Clear();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoes.Count != 0)
        {
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var count = aoes.Length;
            for (var i = 0; i < count; i++)
            {
                ref var aoe = ref aoes[i];
                if (aoe.Shape is AOEShapeDonut)
                {
                    hints.GoalZones.Add(AIHints.GoalProximity(aoe.Origin, 3.5f, 1f));
                }
                else
                {
                    hints.AddForbiddenZone(aoe.Shape, aoe.Origin, activation: aoe.Activation);
                }
            }
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP,
    StatesType = typeof(IndexStates),
    ConfigType = typeof(IndexConfig),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = typeof(TetherID),
    IconIDType = typeof(IconID),
    PrimaryActorOID = (uint)OID.Index,
    Contributors = "gynorhino",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14717u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class Index(WorldState ws, Actor primary) : BossModule(ws, primary, InitialCenter, InitialBounds)
{
    // points using material id 0x00007004
    private static readonly WPos[] _arenaInitialPos = [
        new(7.50198f, -615.00610f),new(7.49990f, -600.00012f),new(-7.50010f, -600.00012f),new(-7.50079f, -600.00067f),
        new(-7.50276f, -615.00580f),new(-15.00425f, -628.00012f),new(-27.99880f, -635.50494f),new(-20.49879f, -648.49530f),
        new(-7.50275f, -640.99445f),new(7.50200f, -640.99408f),new(20.49863f, -648.49530f),new(27.99863f, -635.50494f),
        new(15.00408f, -628.00012f),new(15.00408f, -628.00012f)];

    private static readonly WPos[] _arenaFullPos = [
        new(27.99862f, -620.49530f),new(20.49862f, -607.50494f),new(7.50198f, -615.00610f),new(7.49990f, -600.00012f),
        new(-7.50010f, -600.00012f),new(-7.50079f, -600.00067f),new(-7.50276f, -615.00580f),new(-20.49881f, -607.50494f),
        new(-27.99881f, -620.49530f),new(-15.00425f, -628.00012f),new(-27.99880f, -635.50494f),new(-20.49879f, -648.49530f),
        new(-7.50275f, -640.99445f),new(-7.50076f, -656.00049f),new(0.73911f, -656.00031f),new(7.49962f, -656.00043f),
        new(7.49992f, -656.00012f),new(7.50200f, -640.99408f),new(20.49863f, -648.49530f),new(27.99863f, -635.50494f),
        new(15.00408f, -628.00012f),new(15.00408f, -628.00012f)];

    private static readonly WPos[] _innerHexPos = [new(-2.88752f, -623.00104f), new(0.62856f, -623.00043f), new(2.88607f, -623.00067f), new(5.77356f, -628.00012f), new(2.88633f, -633.00024f), new(-2.88692f, -633.00024f), new(-5.77374f, -628.00012f)];

    private static readonly PolygonCustom[] _arenaInitial = [new(_arenaInitialPos)];
    private static readonly PolygonCustom[] _arenaFull = [new(_arenaFullPos)];
    private static readonly PolygonCustom[] _innerHex = [new(_innerHexPos)];

    public static WPos InitialCenter = new(0f, -624.25f);
    public static readonly ArenaBoundsCustom InitialBounds = new(_arenaInitial, _innerHex, Offset: -1f);

    public static WPos OmniElementsCenter = new(0f, -628f);
    public static readonly ArenaBoundsCustom OmniElementsBounds = new(_arenaFull, _innerHex, Offset: -1f);
}
