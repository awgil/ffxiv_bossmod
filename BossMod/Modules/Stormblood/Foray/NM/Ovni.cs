namespace BossMod.Stormblood.Foray.NM.Ovni;

public enum OID : uint
{
    Boss = 0x2685, // R16.000, x1
}

public enum AID : uint
{
    RockHard = 14786, // Boss->location, 3.0s cast, range 8 circle
    TorrentialTorment = 14785, // Boss->self, 5.0s cast, range 40+R 45-degree cone
    Fluorescence = 14789, // Boss->self, 3.0s cast, single-target
    PullOfTheVoid = 14782, // Boss->self, 3.0s cast, range 30 circle
    Megastorm = 14784, // Boss->self, 5.0s cast, range ?-40 donut
    IonShower = 14787, // Boss->player, 5.0s cast, single-target
    IonStorm = 14788, // Boss->players, no cast, range 20 circle
    VitriolicBarrage = 14790, // Boss->self, 4.0s cast, range 25+R circle
    ConcussiveOscillation = 14783, // Boss->self, 5.0s cast, range 24 circle
}

public enum SID : uint
{
    DamageUp = 1766, // Boss->Boss, extra=0x1
    Stun = 149, // Boss->player, extra=0x0
    Prey = 562, // Boss->player, extra=0x0
}

public enum IconID : uint
{
    IonShower = 111, // player->self
}

class PullOfTheVoid(BossModule module) : Components.KnockbackFromCastTarget(module, AID.PullOfTheVoid, 30, shape: new AOEShapeCircle(30), kind: Kind.TowardsOrigin, minDistanceBetweenHitboxes: true);
class Megastorm(BossModule module) : Components.StandardAOEs(module, AID.Megastorm, new AOEShapeDonut(5, 40));
class ConcussiveOscillation(BossModule module) : Components.StandardAOEs(module, AID.ConcussiveOscillation, new AOEShapeCircle(24));
class VitriolicBarrage(BossModule module) : Components.RaidwideCast(module, AID.VitriolicBarrage);
class RockHard(BossModule module) : Components.StandardAOEs(module, AID.RockHard, 8);
class TorrentialTorment(BossModule module) : Components.StandardAOEs(module, AID.TorrentialTorment, new AOEShapeCone(56, 22.5f.Degrees()));
class Fluorescence(BossModule module) : Components.DispelHint(module, (uint)SID.DamageUp);
class IonShower(BossModule module) : Components.GenericStackSpread(module, alwaysShowSpreads: true, raidwideOnResolve: false)
{
    private int _numCasts;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.IonShower && WorldState.Actors.Find(targetID) is { } target)
        {
            _numCasts = 0;
            Spreads.Add(new(target, 20, WorldState.FutureTime(5)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.IonStorm && ++_numCasts >= 3)
            Spreads.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Spreads.Any(s => s.Target == actor))
            // just gtfo from boss as far as possible
            hints.GoalZones.Add(p => (p - Module.PrimaryActor.Position).LengthSq() > 1600 ? 100 : 0);
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class OvniStates : StateMachineBuilder
{
    public OvniStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PullOfTheVoid>()
            .ActivateOnEnter<Megastorm>()
            .ActivateOnEnter<ConcussiveOscillation>()
            .ActivateOnEnter<VitriolicBarrage>()
            .ActivateOnEnter<RockHard>()
            .ActivateOnEnter<TorrentialTorment>()
            .ActivateOnEnter<IonShower>()
            .ActivateOnEnter<Fluorescence>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.EurekaNM, GroupID = 639, NameID = 1424, Contributors = "xan", SortOrder = 11)]
public class Ovni(WorldState ws, Actor primary) : BossModule(ws, primary, new(266.1068f, -97.09414f), new ArenaBoundsCircle(80, MapResolution: 1));
