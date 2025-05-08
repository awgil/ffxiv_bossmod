namespace BossMod.Endwalker.Hunt.RankA.Minerva;

public enum OID : uint
{
    Boss = 0x3609, // R6.000, x1
}

public enum AID : uint
{
    AutoAttack = 872,
    AntiPersonnelBuild = 27297, // Boss->self, 5.0s cast, single-target, visual
    RingBuild = 27298, // Boss->self, 5.0s cast, single-target, visual
    BallisticMissileCircle = 27299, // Boss->location, 3.5s cast, range 6 circle
    BallisticMissileDonut = 27300, // Boss->location, 3.5s cast, range 6-20 donut
    Hyperflame = 27301, // Boss->self, 5.0s cast, range 60 60-degree cone
    SonicAmplifier = 27302, // TODO: never seen one...
    HammerKnuckles = 27304, // Boss->player, 5.0s cast, single-target
    BallisticMissileMarkTarget = 27377, // Boss->player, no cast, single-target
    BallisticMissileCircleWarning = 27517, // Boss->player, 6.5s cast, single-target
    BallisticMissileDonutWarning = 27518, // Boss->player, 6.5s cast, single-target
}

class BallisticMissile(BossModule module) : Components.GenericAOEs(module)
{
    private AOEShape? _activeMissile;
    private Actor? _activeTarget;
    private WPos _activeLocation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activeMissile != null)
            yield return new(_activeMissile, _activeTarget?.Position ?? _activeLocation);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (!(Module.PrimaryActor.CastInfo?.IsSpell() ?? false))
            return;

        string hint = (AID)Module.PrimaryActor.CastInfo.Action.ID switch
        {
            AID.AntiPersonnelBuild or AID.RingBuild => "Select next AOE type",
            AID.BallisticMissileCircleWarning or AID.BallisticMissileDonutWarning => "Select next AOE target",
            _ => "",
        };
        if (hint.Length > 0)
            hints.Add(hint);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (caster != Module.PrimaryActor)
            return;
        switch ((AID)spell.Action.ID)
        {
            case AID.BallisticMissileCircleWarning:
                _activeMissile = new AOEShapeCircle(6);
                _activeTarget = WorldState.Actors.Find(spell.TargetID);
                break;
            case AID.BallisticMissileDonutWarning:
                _activeMissile = new AOEShapeDonut(6, 20);
                _activeTarget = WorldState.Actors.Find(spell.TargetID);
                break;
            case AID.BallisticMissileCircle:
            case AID.BallisticMissileDonut:
                _activeLocation = spell.LocXZ;
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (caster != Module.PrimaryActor)
            return;
        switch ((AID)spell.Action.ID)
        {
            case AID.BallisticMissileCircleWarning:
            case AID.BallisticMissileDonutWarning:
                _activeLocation = _activeTarget?.Position ?? new();
                _activeTarget = null;
                break;
            case AID.BallisticMissileCircle:
            case AID.BallisticMissileDonut:
                _activeMissile = null;
                break;
        }
    }
}

class Hyperflame(BossModule module) : Components.StandardAOEs(module, AID.Hyperflame, new AOEShapeCone(60, 30.Degrees()));
class SonicAmplifier(BossModule module) : Components.RaidwideCast(module, AID.SonicAmplifier);
class HammerKnuckles(BossModule module) : Components.SingleTargetCast(module, AID.HammerKnuckles);

class MinervaStates : StateMachineBuilder
{
    public MinervaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BallisticMissile>()
            .ActivateOnEnter<Hyperflame>()
            .ActivateOnEnter<SonicAmplifier>()
            .ActivateOnEnter<HammerKnuckles>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 10627)]
public class Minerva(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
