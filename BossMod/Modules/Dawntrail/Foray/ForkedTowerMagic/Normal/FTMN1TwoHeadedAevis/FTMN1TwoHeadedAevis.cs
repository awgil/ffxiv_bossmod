namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN1TwoHeadedAevis;

sealed class StormsBreath(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.StormsBreathCast, 14f)
{
    // on visual cast since there are x2 instances of actual knockback
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            Casters.Add(new(Arena.Center, Distance, Module.CastFinishAt(spell), Shape, spell.Rotation, KnockbackKind, 0, [], caster.InstanceID, IgnoreImmunes));
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var kbs = ActiveKnockbacks(slot, actor);
        if (kbs.Length != 0)
        {
            var kb = kbs[0];
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                // slightly larger to avoid sus knockback
                hints.AddForbiddenZone(new SDKnockbackInAABBSquareAwayFromOrigin(Arena.Center, kb.Origin, 15f, 20f), act);
            }
        }
    }
}
sealed class ThunderfrostTempest(BossModule module) : Components.RaidwideCast(module, (uint)AID.ThunderfrostTempest)
{
    // don't show raidwide hint twice
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (Casters.Count == 0)
        {
            base.OnCastStarted(caster, spell);
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Casters.Count != 0)
        {
            base.OnCastFinished(caster, spell);
        }
    }
}
sealed class TwoTerrors(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TwoTerrors, new AOEShapeRect(40f, 5f));
//sealed class PoisonBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PoisonBreath, 18f);
sealed class PoisonBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PoisonBreath, 18f)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var knockbacks = Module.FindComponent<HissingReprise>();
        if (knockbacks == null || knockbacks.ActiveKnockbacks(slot, actor).Length == 0)
        {
            base.AddHints(slot, actor, hints);
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var knockbacks = Module.FindComponent<HissingReprise>();
        if (knockbacks == null || knockbacks.ActiveKnockbacks(slot, actor).Length == 0)
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}
//sealed class IceCluster(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IceCluster, 15f);
sealed class IceCluster(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IceCluster, 15f)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var knockbacks = Module.FindComponent<HissingReprise>();
        if (knockbacks == null || knockbacks.ActiveKnockbacks(slot, actor).Length == 0)
        {
            base.AddHints(slot, actor, hints);
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var knockbacks = Module.FindComponent<HissingReprise>();
        if (knockbacks == null || knockbacks.ActiveKnockbacks(slot, actor).Length == 0)
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}
//sealed class LightningCluster(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightningCluster, 15f);
sealed class LightningCluster(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightningCluster, 15f)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var knockbacks = Module.FindComponent<HissingReprise>();
        if (knockbacks == null || knockbacks.ActiveKnockbacks(slot, actor).Length == 0)
        {
            base.AddHints(slot, actor, hints);
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var knockbacks = Module.FindComponent<HissingReprise>();
        if (knockbacks == null || knockbacks.ActiveKnockbacks(slot, actor).Length == 0)
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}

sealed class BlazeLoop(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];

        var max = count > 2 ? 2 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        ref var aoe0 = ref aoes[0];
        aoe0.Color = Colors.Danger;
        aoe0.Risky = true;
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Blaze1 or (uint)AID.Blaze2 or (uint)AID.Blaze3)
        {
            var act = Module.CastFinishAt(spell);
            _aoes.Add(new(new AOEShapeCircle(5f), spell.LocXZ, activation: act, risky: false));
            _aoes.Add(new(new AOEShapeDonut(5f, 20f), spell.LocXZ, activation: act.AddSeconds(2.5d), risky: false));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Blaze1:
            case (uint)AID.Blaze2:
            case (uint)AID.Blaze3:
            case (uint)AID.Blazeloop:
                _aoes.RemoveAt(0);
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // try to position near origin for better dodging
        var active = ActiveAOEs(slot, actor);
        var count = active.Length;
        if (count != 0)
        {
            var aoe = active[0];
            hints.AddForbiddenZone(new AOEShapeDonut(7f, 20f), aoe.Origin, activation: aoe.Activation);
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}

sealed class ArcaneBeacon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArcaneBeacon, new AOEShapeRect(60f, 2.5f), 8);
sealed class Archaeofury1(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Archaeofury1, 6f);
sealed class Archaeofury2(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Archaeofury2, 6f);

[ModuleInfo(BossModuleInfo.Maturity.WIP,
    StatesType = typeof(TwoHeadedAevisStates),
    ConfigType = null, // replace null with typeof(TwoHeadedAevisConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = typeof(TetherID),
    IconIDType = typeof(IconID),
    PrimaryActorOID = (uint)OID.GreenHead,
    Contributors = "Equilius + gynorhino",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14489u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class TwoHeadedAevis(WorldState ws, Actor primary) : BossModule(ws, primary, new(-900f, 700f), new ArenaBoundsSquare(20f))
{
    private Actor? _blueHead;
    public Actor? BlueHead()
    {
        return _blueHead;
    }

    protected override void UpdateModule()
    {
        _blueHead ??= GetActor((uint)OID.BlueHead);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_blueHead);
    }
}
