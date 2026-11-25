namespace BossMod.Stormblood.Quest.MasterMusosai;

public enum OID : uint
{
    Boss = 0x1EAC, // R0.500
    FastBladeHelper = 0x1EAF, // R0.500
}

public enum AID : uint
{
    Bloodstain = 9501, // Boss->self, 2.7s cast, range 5 circle
    ShieldBash = 9502, // Boss->player, no cast, single-target
    FastBlade = 9503, // Boss->self, 2.7s cast, range 40 width 6 rect
    FastBladeRepeat = 9504, // FastBladeHelper->self, no cast, range 40 width 6 rect
    SanguineBlade = 9507, // Boss->self, 6.7s cast, range 40 180-degree cone
}

class Bloodstain(BossModule module) : Components.StandardAOEs(module, AID.Bloodstain, new AOEShapeCircle(5));
class SanguineBlade(BossModule module) : Components.StandardAOEs(module, AID.SanguineBlade, new AOEShapeCone(40, 90.Degrees()));

class FastBlade(BossModule module) : Components.GenericAOEs(module, AID.FastBladeRepeat)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeRect _shape = new(40, 3);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FastBlade)
            _aoe = new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 1.1f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.FastBladeRepeat)
            _aoe = null;
    }
}

class BeroaldStates : StateMachineBuilder
{
    public BeroaldStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Bloodstain>()
            .ActivateOnEnter<FastBlade>()
            .ActivateOnEnter<SanguineBlade>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68096, NameID = 6683)]
public class Beroald(ModuleInit init) : BossModule(init, new(-89, 2), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}
