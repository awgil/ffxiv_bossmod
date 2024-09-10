namespace BossMod.Stormblood.Dungeon.D04DomaCastle.D041MagitekRearguard;

public enum OID : uint
{
    Boss = 0x1BCC, // R3.500, x? 
    RearguardBit = 0x1BCF, // R0.900, x?
    FireHelper = 0x1BCD, // R0.500, x?, Helper type
    RearguardMine = 0x1BCE, // R0.900, x?
}

public enum AID : uint
{
    Attack = 872, // 1BCC/1BE1/1BDB->player, no cast, single-target
    CermetPile = 8349, // 1BCC->self, no cast, range 40+R width 6 rect
    GarleanFire = 8350, // 1BCC->self, 3.0s cast, single-target
    GarleanFire2 = 8351, // 1BCD->self, 3.0s cast, range 6 circle
    SelfDetonate = 8352, // 1BCE->self, 3.0s cast, range 6 circle
    MagitekRay = 8353, // 1BCF->self, 3.0s cast, range 45+R width 2 rect
}
class CermetPile(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CermetPile), new AOEShapeRect(40f + 3.5f, 3));
class GarleanFire(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circle = new(6);
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            for (var i = 0; i < _aoes.Count; i++)
                yield return _aoes[i] with { Color = ArenaColor.AOE };
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.GarleanFire2)
        {
            _aoes.Add(new(circle, caster.Position));
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID == AID.GarleanFire2)
        {
            for (var i = 0; i < _aoes.Count; i++)
                _aoes.RemoveAt(0);
        }
    }
};
class MagitekRay(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MagitekRay), new AOEShapeRect(45f + 3.5f, 1));
class SelfDetonate(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SelfDetonate), new AOEShapeCircle(6));
class Mines(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _bits = [];
    private static readonly AOEShapeRect rect = new(8, 1);
    private static readonly AOEShapeCircle circ = new(2f);
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var b in _bits)
        {
            yield return new AOEInstance(rect, b.Position + 2 * b.Rotation.ToDirection(), b.Rotation);
            yield return new AOEInstance(circ, b.Position) with { Color = ArenaColor.Danger };
        }
    }
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.RearguardMine)
        {
            _bits.Add(actor);
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((OID)caster.OID is OID.RearguardMine && (AID)spell.Action.ID is AID.SelfDetonate)
        {
            _bits.Remove(caster);
        }
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.RearguardMine)
        {
            _bits.Remove(actor);
        }
    }
};
class D041MagitekRearguardStates : StateMachineBuilder
{
    public D041MagitekRearguardStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CermetPile>()
            .ActivateOnEnter<GarleanFire>()
            .ActivateOnEnter<MagitekRay>()
            .ActivateOnEnter<SelfDetonate>()
            .ActivateOnEnter<Mines>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 241, NameID = 6200)]
public class D041MagitekRearguard(WorldState ws, Actor primary) : BossModule(ws, primary, new(124.5f, 17.4f), new ArenaBoundsRect(16, 16, Angle.FromDirection(new WPos(110.157f, -6.058f) - new WPos(148.156f, 3.132f))));
