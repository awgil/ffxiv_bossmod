namespace BossMod.Dawntrail.Foray.FATE.InAPotOfBother;

public enum OID : uint
{
    CrescentFan = 0x4D8D, // R2.000, x2
    GreaterFan = 0x4D8E, // R3.200, x1
    CrescentHarpeia = 0x4E23, // R1.560, x3
    CrescentBigHorn = 0x4E26, // R4.600, x1
    CrescentAnkou = 0x4E80, // R5.200, x1
    CrescentBicephalus = 0x4E7E, // R2.850, x1
    CrescentSandSerpent = 0x4E20, // R3.450, x1
    CrescentWoolback = 0x4E25, // R4.500, x1
}

public enum AID : uint
{
    AutoAttack_ = 40542, // 4D8D/4D8E->player, no cast, single-target
    TightTornado1 = 50221, // 4D8D->self, 3.0s cast, range 15 width 4 rect
    TightTornado2 = 50222, // 4D8E->self, 3.0s cast, range 15 width 4 rect
    AeroIII = 50223, // 4D8E->self, 6.0s cast, range 20 circle
}

sealed class TightTornado(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.TightTornado1, (uint)AID.TightTornado2], new AOEShapeRect(15f, 2f));
sealed class TightTornadoKnockback(BossModule module) : Components.SimpleKnockbackGroups(module, [(uint)AID.TightTornado1, (uint)AID.TightTornado2], 10f, shape: new AOEShapeRect(15f, 2f));
sealed class AeroIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AeroIII, 20f)
{
    // draw for completion but no need to avoid
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var origin = spell.LocXZ;
            var rotation = spell.Rotation;
            Casters.Add(new(Shape, origin, rotation, Module.CastFinishAt(spell), default, false, caster.InstanceID, Shape.Distance(origin, rotation)));
        }
    }
}

[SkipLocalsInit]
sealed class InAPotOfBotherStates : StateMachineBuilder
{
    public InAPotOfBotherStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TightTornado>()
            .ActivateOnEnter<TightTornadoKnockback>()
            .ActivateOnEnter<AeroIII>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Dummy,
    StatesType = typeof(InAPotOfBotherStates),
    ConfigType = null, // replace null with typeof(GreaterFanConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.GreaterFan,
    Contributors = "",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.ForayFATE,
    GroupID = 1093u,
    NameID = 2073u,
    SortOrder = 2,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class InAPotOfBother(WorldState ws, Actor primary) : OpenWorldFate(ws, primary)
{
    // need to find something for OID.Boss to use as primary
    public static readonly uint[] Trash = [(uint)OID.GreaterFan, (uint)OID.CrescentFan];
    public Actor? GreaterFan;

    protected override void UpdateModule()
    {
        GreaterFan ??= GetActor((uint)OID.GreaterFan);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, Trash);
        Arena.Actor(GreaterFan);
    }
}
