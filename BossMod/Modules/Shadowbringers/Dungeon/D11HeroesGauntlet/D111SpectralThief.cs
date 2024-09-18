namespace BossMod.Shadowbringers.Dungeon.D11HeroesGauntlet.D111SpectralThief;
public enum OID : uint
{
    Boss = 0x2DEC, // R0.875, x? 
    SpectralThiefHelper = 0x2DED, // R0.875, x?
    ChickenKnife = 0x2E71, // R1.000, x?
    DashHelper = 0x1EAED9, // Circles?
    Helper = 0x233C,

}
public enum AID : uint
{
    SpectralDream = 20427, // 2DEC->players, 4.0s cast, single-target
    SpectralWhirlwind = 20428, // 2DEC->self, 4.0s cast, range 60 circle
    SpectralGust = 21454, // 2DEC->self, no cast, single-target
    SpectralGust2 = 21455, // 233C->player, 6.0s cast, range 5 circle

    ChickenKnife = 20438, // 2DEC->self, 2.0s cast, single-target
    CowardsCunning = 20439, // 2E71->self, 3.0s cast, range 60 width 2 rect

    Dash = 20435, // 2DEC->self, 3.0s cast, single-target
    Shadowdash = 20436, // 2DEC->self, 3.0s cast, single-target
    VacuumBlade = 20577, // 233C->self, no cast, range 15 circle
    VacuumBlade2 = 20578, // 233C->self, no cast, range 15 circle
    Papercutter = 20433, // 233C->self, no cast, range 80 width 14 rect
    Papercutter2 = 20434, // 233C->self, no cast, range 80 width 14 rect

    A = 20437, // 2DEC->location, no cast, single-target
    B = 20429, // 2DEC->self, no cast, single-target
    C = 20501, // 2DED->location, no cast, single-target
    D = 20432, // 2DED->self, no cast, single-target
    E = 20431, // 2DEC->self, no cast, single-target
    F = 20430, // 2DED->self, no cast, single-target
}
public enum SID : uint
{
    DashStatus = 2193,
}
public enum IconID : uint
{
    SpectralGust = 169,
    TankBuster = 198,
}
public enum TetherID : uint
{
    Dash = 12,
}
class VacuumBlade(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.VacuumBlade), new AOEShapeCircle(15));
class VacuumBlade2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.VacuumBlade2), new AOEShapeCircle(15));
class SpectralDream(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.SpectralDream));
class SpectralWhirlwind(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SpectralWhirlwind));
class SpectralGust(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(5), (uint)IconID.SpectralGust, ActionID.MakeSpell(AID.SpectralGust2), centerAtTarget: true);
class CowardsCunning(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CowardsCunning), new AOEShapeRect(60, 1, 10));
class PapercutterTest(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            for (var i = 0; i < _aoes.Count; i++)
            {
                yield return _aoes[i] with { Color = ArenaColor.AOE };
            }
        }
    }
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.DashHelper)
        {
            _aoes.Add(new AOEInstance(new AOEShapeCircle(15), actor.Position));
            _aoes.Add(new AOEInstance(new AOEShapeCross(60, 7), actor.Position));
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Papercutter or AID.Papercutter2 or AID.VacuumBlade or AID.VacuumBlade2)
        {
            _aoes.Clear();
        }
    }
}
class PapercutterTest2(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    public readonly List<Actor> _daggers = [];
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            for (var i = 0; i < _aoes.Count; i++)
            {
                yield return _aoes[i] with { Color = ArenaColor.AOE };
            }
        }
    }
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.DashHelper)
        {
            _daggers.Add(actor);
        }
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.DashHelper)
        {
            _daggers.Clear();
        }
    }
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((OID)actor.OID is OID.Boss or OID.SpectralThiefHelper)
        {
            if ((SID)status.ID == SID.DashStatus)
            {
                foreach (var e in _daggers)
                {
                    if (status.Extra is 0xB0 or 0xB3) //Circle
                    {
                        _aoes.Add(new AOEInstance(new AOEShapeCircle(15), e.Position));
                    }
                    else if (status.Extra is 0xB1 or 0xB4) //Vert Rect
                    {
                        _aoes.Add(new AOEInstance(new AOEShapeRect(60, 7, 60), e.Position));
                    }
                    else if (status.Extra is 0xB2 or 0xB5) //Horizontal Rect
                    {
                        _aoes.Add(new AOEInstance(new AOEShapeRect(60, 7, 60), e.Position, 90.Degrees()));
                    }
                }
            }
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Papercutter or AID.Papercutter2 or AID.VacuumBlade or AID.VacuumBlade2)
        {
            _aoes.Clear();
        }
    }
}
class Papercutter(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Papercutter), new AOEShapeRect(80, 7));
class Papercutter2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Papercutter2), new AOEShapeRect(80, 7));
class D111SpectralThiefStates : StateMachineBuilder
{
    public D111SpectralThiefStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VacuumBlade>()
            .ActivateOnEnter<VacuumBlade2>()
            .ActivateOnEnter<SpectralDream>()
            .ActivateOnEnter<SpectralWhirlwind>()
            .ActivateOnEnter<SpectralGust>()
            .ActivateOnEnter<CowardsCunning>()
            .ActivateOnEnter<PapercutterTest2>()
            .ActivateOnEnter<Papercutter>()
            .ActivateOnEnter<Papercutter2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 737, NameID = 9505)]
public class D111SpectralThief(WorldState ws, Actor primary) : BossModule(ws, primary, new(-680f, 449.97f), new ArenaBoundsSquare(20));
