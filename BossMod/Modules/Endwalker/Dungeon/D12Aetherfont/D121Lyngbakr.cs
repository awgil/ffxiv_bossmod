namespace BossMod.Endwalker.Dungeon.D12Aetherfont.D121Lyngbakr;

public enum OID : uint
{
    Boss = 0x3EEB, //R=7.6
    Helper = 0x233C,
    SmallCrystal = 0x1EB882, // R=0.5
    BigCrystal = 0x1EB883, // R=0.5
}

public enum AID : uint
{
    AutoAttack = 34517, // Boss->player, no cast, single-target
    BodySlam = 33335, // Boss->self, 3.0s cast, range 40 circle
    SonicBloop = 33345, // Boss->player, 5.0s cast, single-target, tankbuster
    ExplosiveFrequency = 33340, // Helper->self, 10.0s cast, range 15 circle
    ResonantFrequency = 33339, // Helper->self, 5.0s cast, range 8 circle
    TidalBreath = 33344, // Boss->self, 5.0s cast, range 40 180-degree cone
    Tidalspout = 33343, // Helper->player, 5.0s cast, range 6 circle
    Upsweep = 33338, // Boss->self, 5.0s cast, range 40 circle
    Floodstide = 33341, // Boss->self, 3.0s cast, single-target        
    Waterspout = 33342, // Helper->player, 5.0s cast, range 5 circle, spread
}

class Frequencies(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle smallcircle = new(8);
    private static readonly AOEShapeCircle bigcircle = new(15);
    private DateTime _activation1;
    private DateTime _activation2;
    private readonly List<Actor> _bigcrystals = [];
    private readonly List<Actor> _smallcrystals = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_smallcrystals.Count > 0)
            foreach (var c in _smallcrystals)
                yield return new(smallcircle, c.Position, default, _activation1);
        if (_bigcrystals.Count > 0 && _smallcrystals.Count == 0)
            foreach (var c in _bigcrystals)
                yield return new(bigcircle, c.Position, default, _activation2);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ResonantFrequency)
        {
            _activation1 = Module.CastFinishAt(spell);
            _smallcrystals.Add(caster);
        }
        if ((AID)spell.Action.ID == AID.ExplosiveFrequency)
        {
            _activation2 = Module.CastFinishAt(spell);
            _bigcrystals.Add(caster);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ResonantFrequency or AID.ExplosiveFrequency)
        {
            _smallcrystals.Remove(caster);
            _bigcrystals.Remove(caster);
        }
    }
}

class SonicBloop(BossModule module) : Components.SingleTargetCast(module, AID.SonicBloop);
class Waterspout(BossModule module) : Components.SpreadFromCastTargets(module, AID.Waterspout, 5);
class TidalBreath(BossModule module) : Components.StandardAOEs(module, AID.TidalBreath, new AOEShapeCone(40, 90.Degrees()));
class Tidalspout(BossModule module) : Components.StackWithCastTargets(module, AID.Tidalspout, 6);
class Upsweep(BossModule module) : Components.RaidwideCast(module, AID.Upsweep);
class BodySlam(BossModule module) : Components.RaidwideCast(module, AID.BodySlam);

class D121LyngbakrStates : StateMachineBuilder
{
    public D121LyngbakrStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SonicBloop>()
            .ActivateOnEnter<TidalBreath>()
            .ActivateOnEnter<Tidalspout>()
            .ActivateOnEnter<Waterspout>()
            .ActivateOnEnter<Upsweep>()
            .ActivateOnEnter<BodySlam>()
            .ActivateOnEnter<Frequencies>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "dhoggpt, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 822, NameID = 12336)]
public class D121Lyngbakr(WorldState ws, Actor primary) : BossModule(ws, primary, new(-322, 120), new ArenaBoundsCircle(20));
