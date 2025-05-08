namespace BossMod.Dawntrail.Quest.TheProtectorAndTheDestroyer;

class ValorousAscension(BossModule module) : Components.RaidwideCast(module, AID.ValorousAscension);
class RendPower(BossModule module) : Components.StandardAOEs(module, AID.RendPower, new AOEShapeCone(40, 15.Degrees()), 6);
class SteadfastWill(BossModule module) : Components.SingleTargetCast(module, AID.SteadfastWill);
class Rush(BossModule module) : Components.ChargeAOEs(module, AID.Rush, 2.5f)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => base.ActiveAOEs(slot, actor).Take(4);
}
class StormlitShockwave(BossModule module) : Components.RaidwideCast(module, AID.StormlitShockwave);
class Electrobeam(BossModule module) : Components.StandardAOEs(module, AID.Electrobeam, new AOEShapeRect(40, 2));
class HolyBlade(BossModule module) : Components.StackWithCastTargets(module, AID.HolyBlade, 6);
class BastionBreaker(BossModule module) : Components.SpreadFromCastTargets(module, AID.BastionBreaker, 6);
class ThrownFlames(BossModule module) : Components.StandardAOEs(module, AID.ThrownFlames, new AOEShapeCircle(8));
class SearingSlash(BossModule module) : Components.StandardAOEs(module, AID.SearingSlash, new AOEShapeCircle(8));

class OtisOathbrokenStates : StateMachineBuilder
{
    public OtisOathbrokenStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BastionBreaker>()
            .ActivateOnEnter<ThrownFlames>()
            .ActivateOnEnter<SearingSlash>()
            .ActivateOnEnter<StormlitShockwave>()
            .ActivateOnEnter<Electrobeam>()
            .ActivateOnEnter<HolyBlade>()
            .ActivateOnEnter<SteadfastWill>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<ValorousAscension>()
            .ActivateOnEnter<RendPower>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70478, NameID = 13168)]
public class OtisOathbroken(WorldState ws, Actor primary) : BossModule(ws, primary, new(349, -14), new ArenaBoundsCircle(19.5f))
{
    protected override bool CheckPull() => WorldState.Actors.Any(x => !x.IsAlly && x.InCombat);
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = h.Actor.OID == (uint)OID.Boss ? 0 : 1;
    }
}
