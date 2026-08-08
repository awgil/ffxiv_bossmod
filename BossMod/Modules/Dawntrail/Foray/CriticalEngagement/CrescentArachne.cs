namespace BossMod.Dawntrail.Foray.CriticalEngagement.CrescentArachne;

public enum OID : uint
{
    Boss = 0x4DFA, // R6.500, x1
    Helper = 0x233C, // R0.500, x19 (spawn during fight), Helper type
    ArachneDaughter = 0x4DFB, // R2.400, x0 (spawn during fight)
}

public enum AID : uint
{
    DeathWall = 50365, // 4DFC->self, no cast, range 20-30 donut
    AutoAttack = 50853, // Boss->player, no cast, single-target
    ImplosionCast = 50366, // Boss->self, 5.0s cast, single-target
    Implosion = 50367, // Helper->self, no cast, ???
    Summon = 50368, // Boss->self, 3.0s cast, single-target
    ArachnidWeb1 = 50369, // Boss->4DFB, 3.0s cast, single-target
    ArachnidWeb2 = 50370, // 4DFB->4DFB, no cast, single-target
    ArachnidFunnelFirst = 50371, // Boss->4DFB, 5.0s cast, width 20 rect charge, first charge
    ArachnidFunnelBoss = 50372, // Boss->location, no cast, width 20 rect charge, visual only
    ArachnidFunnelRest = 50680, // Helper->location, no cast, width 20 rect charge, other charges
    AutoAttackDaughter = 50635, // 4DFB->player, no cast, single-target
    ConformityBoss = 50376, // Boss->self, 3.0s cast, range 50 45-degree cone
    QueensOrders = 50647, // Boss->self, 3.0s cast, single-target
    ConformityAdds = 50377, // 4DFB->self, 3.0s cast, range 50 45-degree cone
    BedrockUpliftCast = 50378, // Boss->self, 4.7s cast, single-target
    BedrockUplift1 = 50379, // Helper->self, 5.0s cast, range 10 circle
    BedrockUplift2 = 50380, // Helper->self, 7.0s cast, range 10-20 donut
    BedrockUplift3 = 50381, // Helper->self, 9.0s cast, range 20-30 donut
    VenomEruption = 50375, // 4DFB->self, 12.0s cast, single-target, enrage
}

public enum SID : uint
{
    Unk2056 = 2056, // none->_Gen_ArachneDaughter, extra=0x291
}

public enum TetherID : uint
{
    WebBoss = 420, // 4DFB->Boss
    WebAdds = 408, // 4DFB->4DFB
}

class Implosion(BossModule module) : Components.RaidwideCastDelay(module, AID.ImplosionCast, AID.Implosion, 0.8f);

class ArachnidFunnel(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<(Actor From, Actor To, DateTime Activation)> _charges = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var ((f, t, a), i) in _charges.Select((c, i) => (c, i)).Take(3))
        {
            var dir = t.Position - f.Position;
            yield return new(new AOEShapeRect(dir.Length(), 10), f.Position, dir.ToAngle(), a, Color: i == 0 ? ArenaColor.Danger : ArenaColor.AOE);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.WebBoss && _charges.Count == 0)
            _charges.Add((Module.PrimaryActor, source, WorldState.FutureTime(8.2f)));

        if ((TetherID)tether.ID == TetherID.WebAdds && WorldState.Actors.Find(tether.Target) is { } tar)
        {
            var prev = _charges[^1].Activation;
            var delay = _charges.Count == 1 ? 2.2f : 1.5f;
            _charges.Add((tar, source, prev.AddSeconds(delay)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ArachnidFunnelFirst or AID.ArachnidFunnelRest && _charges.Count > 0)
            _charges.RemoveAt(0);
    }
}

class Conformity(BossModule module) : Components.GroupedAOEs(module, [AID.ConformityBoss, AID.ConformityAdds], new AOEShapeCone(50, 22.5f.Degrees()))
{
    readonly List<Actor> _dangerous = [];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Unk2056)
            _dangerous.Add(actor);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (IDs.Contains(spell.Action))
            _dangerous.Remove(caster);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        foreach (var d in _dangerous)
            hints.GoalZones.Add(hints.GoalSingleTarget(d.Position, 20, 0.1f));
    }
}

class BedrockUplift(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10), new AOEShapeDonut(10, 20), new AOEShapeDonut(20, 30)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BedrockUplift1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var seq = (AID)spell.Action.ID switch
        {
            AID.BedrockUplift1 => 0,
            AID.BedrockUplift2 => 1,
            AID.BedrockUplift3 => 2,
            _ => -1
        };

        if (seq >= 0)
            AdvanceSequence(seq, caster.Position, WorldState.FutureTime(2));
    }
}

class ArachneDaughter(BossModule module) : Components.Adds(module, (uint)OID.ArachneDaughter, 1, forbidDots: true);

class CrescentArachneStates : StateMachineBuilder
{
    public CrescentArachneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Implosion>()
            .ActivateOnEnter<ArachnidFunnel>()
            .ActivateOnEnter<Conformity>()
            .ActivateOnEnter<BedrockUplift>()
            .ActivateOnEnter<ArachneDaughter>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14840)]
public class CrescentArachne(WorldState ws, Actor primary) : CEModule(ws, primary, new(170, -136), new ArenaBoundsCircle(20));

