namespace BossMod.Endwalker.Dungeon.D01TheTowerOifZot.D012Sanduruva;

public enum OID : uint
{
    Boss = 0x33EF, // R=2.5
    BerserkerSphere = 0x33F0, // R=1.5-2.5
}

public enum AID : uint
{
    AutoAttack = 871, // Boss->player, no cast, single-target
    Teleport = 25254,  // Boss->location, no cast, single-target
    ExplosiveForce = 25250, //Boss->self, 3.0s cast, single-target
    IsitvaSiddhi = 25257, // Boss->player, 4.0s cast, single-target
    ManusyaBerserk = 25249, // Boss->self, 3.0s cast, single-target
    ManusyaConfuse = 25253, // Boss->self, 3.0s cast, range 40 circle
    ManusyaStop = 25255, // Boss->self, 3.0s cast, range 40 circle
    PrakamyaSiddhi = 25251, // Boss->self, 4.0s cast, range 5 circle
    PraptiSiddhi = 25256, //Boss->self, 2.0s cast, range 40 width 4 rect
    SphereShatter = 25252, // BerserkerSphere->self, 2.0s cast, range 15 circle
}

public enum SID : uint
{
    ManusyaBerserk = 2651, // Boss->player, extra=0x0
    ManusyaStop = 2653, // none->player, extra=0x0
    TemporalDisplacement = 900, // none->player, extra=0x0
    ManusyaConfuse = 2652, // Boss->player, extra=0x1C6
    WhoIsShe = 2655, // none->Boss, extra=0x0
    WhoIsShe2 = 2654, // none->BerserkerSphere, extra=0x1A8
}

class IsitvaSiddhi(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.IsitvaSiddhi));

class SphereShatter(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;
    private readonly List<Actor> _casters = [];
    private static readonly AOEShapeCircle circle = new(15);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_casters.Count > 0)
            foreach (var c in _casters)
                yield return new(circle, c.Position, default, _activation, Risky: _activation.AddSeconds(-7) < WorldState.CurrentTime);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.BerserkerSphere)
        {
            _casters.Add(actor);
            if (NumCasts == 0)
                _activation = WorldState.FutureTime(10.8f);
            else
                _activation = WorldState.FutureTime(20);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SphereShatter)
        {
            _casters.Remove(caster);
            ++NumCasts;
        }
    }
}

class PraptiSiddhi(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PraptiSiddhi), new AOEShapeRect(40, 2));
class PrakamyaSiddhi(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PrakamyaSiddhi), new AOEShapeCircle(5));
class ManusyaConfuse(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.ManusyaConfuse), "Applies Manyusa Confusion");
class ManusyaStop(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.ManusyaStop), "Applies Manyusa Stop");

class D012SanduruvaStates : StateMachineBuilder
{
    public D012SanduruvaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ManusyaConfuse>()
            .ActivateOnEnter<IsitvaSiddhi>()
            .ActivateOnEnter<ManusyaStop>()
            .ActivateOnEnter<PrakamyaSiddhi>()
            .ActivateOnEnter<SphereShatter>()
            .ActivateOnEnter<PraptiSiddhi>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "dhoggpt, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 783, NameID = 10257)]
public class D012Sanduruva(WorldState ws, Actor primary) : BossModule(ws, primary, new(-258, -26), new ArenaBoundsCircle(20))
{
    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.Boss => 1,
                OID.BerserkerSphere => -1,
                _ => 0
            };
        }
    }
}
