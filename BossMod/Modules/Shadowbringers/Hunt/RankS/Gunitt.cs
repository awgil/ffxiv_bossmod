namespace BossMod.Shadowbringers.Hunt.RankS.Gunitt;

public enum OID : uint
{
    Boss = 0x2852, // R=4.0
}

public enum AID : uint
{
    AutoAttack = 870, // 2852->player, no cast, single-target
    TheDeepSeeks = 17356, // 2852->player, 4.0s cast, single-target
    TheDeepReaches = 17357, // 2852->self, 4.0s cast, range 40 width 2 rect
    TheDeepBeckons = 17358, // 2852->self, 4.0s cast, range 40 circle
    Abordage = 17359, // 2852->players, no cast, width 8 rect charge, seems to target random player before stack marker, no telegraph?
    SwivelGun = 17361, // 2852->players, 5.0s cast, range 10 circle, stack marker, applies magic vuln up, 3 times in a row
    CoinToss = 17360, // 2852->self, 4.0s cast, range 40 circle, gaze, applies Seduced (forced march to boss)
    TheDeepRends = 17351, // 2852->self, 5.5s cast, range 20 60-degree cone
    TheDeepRends2 = 17352, // 2852->self, no cast, range 20 60-degree cone, seems to target 5 random players after first The Deep Rends, no telegraph?
}

public enum SID : uint
{
    MagicVulnerabilityUp = 1138, // Boss->player, extra=0x0
    Seduced = 227, // Boss->player, extra=0x0
}

public enum IconID : uint
{
    Stackmarker = 93, // player
}

class TheDeepSeeks(BossModule module) : Components.SingleTargetCast(module, AID.TheDeepSeeks);
class TheDeepReaches(BossModule module) : Components.StandardAOEs(module, AID.TheDeepReaches, new AOEShapeRect(40, 1));
class TheDeepBeckons(BossModule module) : Components.RaidwideCast(module, AID.TheDeepBeckons);
class CoinToss(BossModule module) : Components.CastGaze(module, AID.CoinToss);
class TheDeepRends(BossModule module) : Components.StandardAOEs(module, AID.TheDeepRends, new AOEShapeCone(20, 30.Degrees()));
class TheDeepRendsHint(BossModule module) : Components.CastHint(module, AID.TheDeepRends, "Targets 5 random players after initial hit");

class SwivelGun(BossModule module) : Components.GenericStackSpread(module)
{
    private BitMask _forbidden;

    public override void Update()
    {
        if (Stacks.Count > 0) //updating forbiddenplayers because debuffs can be applied after new stack marker appears
        {
            var Forbidden = Stacks[0];
            Forbidden.ForbiddenPlayers = _forbidden;
            Stacks[0] = Forbidden;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Stackmarker)
            Stacks.Add(new(actor, 10, activation: WorldState.FutureTime(5), forbiddenPlayers: _forbidden));
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.MagicVulnerabilityUp)
            _forbidden.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.MagicVulnerabilityUp)
            _forbidden.Clear(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.SwivelGun)
            Stacks.RemoveAt(0);
    }
}

class GunittStates : StateMachineBuilder
{
    public GunittStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TheDeepBeckons>()
            .ActivateOnEnter<TheDeepReaches>()
            .ActivateOnEnter<TheDeepRends>()
            .ActivateOnEnter<TheDeepRendsHint>()
            .ActivateOnEnter<CoinToss>()
            .ActivateOnEnter<SwivelGun>()
            .ActivateOnEnter<TheDeepSeeks>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 8895)]
public class Gunitt(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
