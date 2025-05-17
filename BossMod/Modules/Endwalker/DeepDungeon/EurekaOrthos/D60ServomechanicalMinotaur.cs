namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.D60ServomechanicalMinotaur;

public enum OID : uint
{
    Boss = 0x3DA1, // R6.000, x?
    Helper = 0x233C, // R0.500, x?, Helper type
    BallOfLevin = 0x3DA2, // R1.300, x?
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    BullishSwing = 31875, // Boss->self, 5.0s cast, range 13 circle, PB Circle AOE
    BullishSwipeSingle = 32795, // Boss->self, 5.0s cast, range 40 90-degree cone, usually used after KB to try and catch off guard
    DisorientingGroan = 31876, // Boss->self, 5.0s cast, range 60 circle, 15 yalm KB
    Shock = 31874, // BallOfLevin->self, 2.5s cast, range 5 circle // Cast, but going to be a persistent void zone
    Thundercall = 31873, // Boss->self, 5.0s cast, range 60 circle

    // Boss telegraphs with the helper, then cast Swipes 1-4 in a pattern. (Either same pattern twice, or 1-4 -> 4->1)
    BullishSwipe1 = 31868, // Boss->self, no cast, range 40 ?-degree cone
    BullishSwipe2 = 31869, // Boss->self, no cast, range 40 ?-degree cone
    BullishSwipe3 = 31870, // Boss->self, no cast, range 40 ?-degree cone
    BullishSwipe4 = 31871, // Boss->self, no cast, range 40 ?-degree cone
    OctupleSwipe = 31872, // Boss->self, 10.8s cast, range 40 ?-degree cone // Windup Cast for N/E/S/W
    OctupleSwipeTelegraph = 31867, // Helper->self, 1.0s cast, range 40 ?-degree cone // Displays the order of the aoe's going off
}

class BullishSwing(BossModule module) : Components.StandardAOEs(module, AID.BullishSwing, new AOEShapeCircle(13));
class BullishSwipeSingle(BossModule module) : Components.StandardAOEs(module, AID.BullishSwipeSingle, new AOEShapeCone(40, 45.Degrees()));
class DisorientingGroan(BossModule module) : Components.KnockbackFromCastTarget(module, AID.DisorientingGroan, 15, true);
class Shock(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Dictionary<Actor, DateTime> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select(c => new AOEInstance(new AOEShapeCircle(5), c.Key.Position, default, c.Value));

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.BallOfLevin)
            Casters[actor] = WorldState.FutureTime(12.9f);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (Casters.ContainsKey(caster))
            Casters[caster] = Module.CastFinishAt(spell);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Shock)
            Casters.Remove(caster);
    }
}
class Thundercall(BossModule module) : Components.RaidwideCast(module, AID.Thundercall, "Raidwide + Summoning Orbs");

class OctupleSwipe(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCone cone = new(40, 45.Degrees());
    private static readonly HashSet<AID> castEnd = [AID.OctupleSwipe, AID.BullishSwipe1, AID.BullishSwipe2, AID.BullishSwipe3, AID.BullishSwipe4];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
            yield return _aoes[0] with { Color = ArenaColor.Danger };
        if (_aoes.Count > 1)
            for (var i = 1; i < Math.Clamp(_aoes.Count, 0, 3); ++i)
                yield return _aoes[i];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.OctupleSwipeTelegraph)
            _aoes.Add(new(cone, caster.Position, spell.Rotation, _aoes.Count == 0 ? Module.CastFinishAt(spell, 8.7f + 2 * _aoes.Count) : _aoes[0].Activation.AddSeconds(_aoes.Count * 2)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && castEnd.Contains((AID)spell.Action.ID))
            _aoes.RemoveAt(0);
    }
}

class D60ServomechanicalMinotaurStates : StateMachineBuilder
{
    public D60ServomechanicalMinotaurStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BullishSwipeSingle>()
            .ActivateOnEnter<BullishSwing>()
            .ActivateOnEnter<DisorientingGroan>()
            .ActivateOnEnter<OctupleSwipe>()
            .ActivateOnEnter<Shock>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 902, NameID = 12267)]
public class D60ServomechanicalMinotaur(WorldState ws, Actor primary) : BossModule(ws, primary, new(-600, -300), new ArenaBoundsCircle(20));
