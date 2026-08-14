namespace BossMod.Modules.Dawntrail.Dungeon.D13TheClyteum;

public enum OID : uint
{
    Malphas = 0x4C28,
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 50220, // Malphas->player, no cast, single-target
    RubbishDisposal = 48920, // Malphas->self, 5.0s cast, range 50 circle
    VoidDark = 50313, // Malphas->self, 5.0s cast, range 30 180.000-degree cone
    UnknownAbility = 48932, // Malphas->location, no cast, single-target
    ScrapMeddle = 48921, // Malphas->self, 2.3+0.7s cast, single-target
    Goekinesis = 48933, // Helper->self, 3.0s cast, range 80 width 4 rect
    PuppetStrings = 48922, // Malphas->self, 5.0+1.5s cast, single-target
    MetallicMiasma = 48936, // 4C2A->self, 0.5s cast, single-target
    MetallicMiasma1 = 48937, // Helper->self, 6.5s cast, range 60 30.000-degree cone
    CastOffHalo = 48934, // 4C29->self, 0.5s cast, single-target
    CastOffHalo1 = 48935, // Helper->self, 6.5s cast, range 7 circle
    GluttonousWireCastBar = 48929, // Malphas->self, 4.0+1.0s cast, single-target
    GluttonousWire = 48930, // Helper->players, 5.0s cast, range 6 circle
    PuppetPair = 48923, // Malphas->self, 5.0+1.5s cast, single-target
    StringUp = 48931, // Malphas->self, 5.0s cast, range 50 circle
    ShadowPlayCastBar = 50315, // Malphas->self, 4.2+0.8s cast, single-target
    ShadowPlay = 50314, // Helper->player, 5.0s cast, range 6 circle
    UnknownAbility1 = 48938, // 4C29->self, no cast, single-target
    UnknownAbility2 = 48941, // 4C2A->self, no cast, single-target
    PuppetMastery = 48925, // Malphas->self, 13.0+1.5s cast, single-target
    CastOffHalo2 = 48939, // 4C29->self, no cast, single-target
    MetallicMiasma2 = 48942, // 4C2A->self, no cast, single-target
    MetallicMiasma3 = 48943, // Helper->self, 14.5s cast, range 60 30.000-degree cone
    CastOffHalo3 = 48940, // Helper->self, 14.5s cast, range 7 circle
    WrathfulWire = 48928, // Helper->player, 5.0s cast, range 5 circle
}

[SkipLocalsInit]
sealed class RubbishDisposal(BossModule module) : Components.RaidwideCast(module, (uint)AID.RubbishDisposal);

sealed class VoidDark(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidDark, new AOEShapeCone(30f, 90f.Degrees()));

sealed class Goekinesis(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Goekinesis, new AOEShapeRect(80f, 2f));

//MetallicMiasma and CastOffHalo could *probably* be subtler -- look at adding a riskyWithSecondLeft value so that we're not pre-moving on the 15s casts?
sealed class MetallicMiasma(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.MetallicMiasma1, (uint)AID.MetallicMiasma3], new AOEShapeCone(60f, 15f.Degrees()));

sealed class CastOffHalo(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.CastOffHalo1, (uint)AID.CastOffHalo3], new AOEShapeCircle(7f));

sealed class ShadowPlay(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.ShadowPlay, 6f);

sealed class GluttonousWire(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.GluttonousWire, 6f, 4);

sealed class WrathfulWire(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.WrathfulWire, 5f);

//If there's a 'Stop Moving' version of this, I haven't seen it in 5 runs. Doesn't mean there isn't one, but we won't know until someone sees it.
sealed class StringUp(BossModule module) : Components.StayMove(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (caster.OID == (uint)OID.Malphas && spell.Action.ID == (uint)AID.StringUp)
        {
            foreach (var (slot, _) in Raid.WithSlot(false, true, true))
            {
                PlayerStates[slot] = new(Requirement.Move, WorldState.CurrentTime.AddSeconds(5d));
            }
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (caster.OID == (uint)OID.Malphas && spell.Action.ID == (uint)AID.StringUp)
        {
            foreach (var (slot, _) in Raid.WithSlot(false, true, true))
            {
                PlayerStates[slot] = default;
            }
        }
    }
}

sealed class D133MalphasStates : StateMachineBuilder
{
    public D133MalphasStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RubbishDisposal>()
            .ActivateOnEnter<VoidDark>()
            .ActivateOnEnter<Goekinesis>()
            .ActivateOnEnter<MetallicMiasma>()
            .ActivateOnEnter<CastOffHalo>()
            .ActivateOnEnter<ShadowPlay>()
            .ActivateOnEnter<GluttonousWire>()
            .ActivateOnEnter<WrathfulWire>()
            .ActivateOnEnter<StringUp>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified,
StatesType = typeof(D133MalphasStates),
ConfigType = null, // replace null with typeof(MalphasConfig) if applicable
ObjectIDType = typeof(OID),
ActionIDType = null, // replace null with typeof(AID) if applicable
StatusIDType = null, // replace null with typeof(SID) if applicable
TetherIDType = null, // replace null with typeof(TetherID) if applicable
IconIDType = null, // replace null with typeof(IconID) if applicable
PrimaryActorOID = (uint)OID.Malphas,
Contributors = "HerStolenLight",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.Dungeon,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1011u,
NameID = 14758u,
SortOrder = 3,
PlanLevel = 0)]
[SkipLocalsInit]
public sealed class D133Malphas(WorldState ws, Actor primary) : BossModule(ws, primary, new(760f, -803f), new ArenaBoundsCircle(20f));
