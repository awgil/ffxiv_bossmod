using BossMod.Endwalker.Criterion.C02AMR.C023Moko;

namespace BossMod.Stormblood.Dungeon.D04DomaCastle.D042MagitekHexadrone;

public enum OID : uint
{
    Boss = 0x1BD0, // R4.240, x? 
    MagitekHexadroneHelper = 0x1BD1, // R0.500, x?
    HexadroneBit = 0x1BD2, // R0.900, x10 / x16 --- ChainMine
    HexadroneBit2 = 0x1BD3, // R0.500, x8 --- ChainMine2
}

public enum AID : uint
{
    Attack = 8501, // 1BD0->player, no cast, single-target
    CircleOfDeath = 8354, // 1BD0->self, 3.0s cast, range 4+R circle
    TwoTonzeMagitekMissile = 8355, // 1BD0->players, no cast, range 6 circle
    ChainMine = 9287, // 1BD2/1BD6->self, no cast, range 50+R width 3 rect
    ChainMine2 = 8359, // 1BD3/1BD7->player, no cast, single-target
    MagitekMissiles = 8356, // 1BD0->self, 7.5s cast, single-target
    MagitekMissiles2 = 8357, // 1BD1->location, 8.0s cast, range 6 circle
    MagitekMissiles3 = 8358, // 1BD1->location, no cast, range 60 circle
}
public enum IconID : uint
{
    StackMarker = 62,
}
public enum TetherID : uint
{
    ChainMine = 60,
}
class CircleOfDeath(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CircleOfDeath), new AOEShapeCircle(4f + 4.24f));
class TwoTonzeMagitekMissile(BossModule module) : Components.IconStackSpread(module, 62, default, ActionID.MakeSpell(AID.TwoTonzeMagitekMissile), default, 6, 0, 0)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var s in ActiveStackTargets)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(s.Position, 6));
    }
}
class ChainMines(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _tethers = [];
    private static readonly AOEShapeRect rect = new(50, 1.8f, 5);
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var source in _tethers)
        {
            yield return new AOEInstance(rect, source.Position + 2 * source.Rotation.ToDirection(), source.Rotation);
        }
    }
    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.ChainMine)
            _tethers.Add(source);
    }
    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.ChainMine)
            _tethers.Remove(source);
    }
};
class MagitekMissile2(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID.MagitekMissiles2), 6, 1, 4)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((OID)caster.OID is OID.MagitekHexadroneHelper && (AID)spell.Action.ID is AID.MagitekMissiles2)
        {
            Towers.Add(new(caster.Position, Radius));
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((OID)caster.OID is OID.MagitekHexadroneHelper && (AID)spell.Action.ID is AID.MagitekMissiles2)
        {
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Towers.Any(t => !t.ForbiddenSoakers[slot] && !t.CorrectAmountInside(Module)))
        {
            foreach (var t in Towers)
            {
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(t.Position, Radius - 1));
            }
        }
    }
};
class D042MagitekHexadroneStates : StateMachineBuilder
{
    public D042MagitekHexadroneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CircleOfDeath>()
            .ActivateOnEnter<TwoTonzeMagitekMissile>()
            .ActivateOnEnter<ChainMines>()
            .ActivateOnEnter<MagitekMissile2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 241, NameID = 6203)]
public class D042MagitekHexadrone(WorldState ws, Actor primary) : BossModule(ws, primary, new(-240, 130.5f), new ArenaBoundsSquare(20));
