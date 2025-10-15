namespace BossMod.Dawntrail.DeepDungeon.PilgrimsTraverse.D10OrnamentalLeafman;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x8 (spawn during fight), Helper type
    Boss = 0x460D, // R4.800, x1
    Shrublet = 0x460E, // R1.800, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 45130, // Boss/Shrublet->player, no cast, single-target
    Jump1 = 44052, // Boss->location, no cast, single-target
    BranchOut = 44051, // Boss->self, 3.0s cast, single-target
    HedgeMazingBoss = 44053, // Boss->self, 12.7+0.8s cast, single-target
    HedgeMazingShrub = 44054, // Shrublet->self, 12.7+0.8s cast, single-target
    HedgeMazingBossAOE = 44854, // Helper->self, 13.5s cast, range 14 circle
    HedgeMazingShrubAOE = 44855, // Helper->self, 13.5s cast, range 14 circle
    LeafmashIndicator = 44058, // Helper->location, no cast, single-target
    LeafmashCast = 44055, // Boss->location, 10.0+0.9s cast, single-target
    LittleLeafmash = 44059, // Shrublet->player, no cast, range 5 circle, enrage (applies stun)
    LeafmashAOE = 44057, // Helper->self, 1.9s cast, range 15 circle
    LeafmashJump = 44056, // Boss->location, no cast, single-target
}

class HedgeMazing(BossModule module) : Components.GroupedAOEs(module, [AID.HedgeMazingBossAOE, AID.HedgeMazingShrubAOE], new AOEShapeCircle(14));
class Shrublet(BossModule module) : Components.Adds(module, (uint)OID.Shrublet, 1, forbidDots: true);

class Leafmash(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(WPos Position, DateTime Activation)> _jumps = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _jumps.Select(j => new AOEInstance(new AOEShapeCircle(15), j.Position, default, j.Activation)).Take(3);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.LeafmashIndicator)
            _jumps.Add((spell.TargetXZ, WorldState.FutureTime(8.8f - 0.2f * _jumps.Count)));

        if ((AID)spell.Action.ID == AID.LeafmashAOE)
        {
            NumCasts++;
            if (_jumps.Count > 0)
                _jumps.RemoveAt(0);
        }
    }
}

class D10OrnamentalLeafmanStates : StateMachineBuilder
{
    public D10OrnamentalLeafmanStates(BossModule module) : base(module)
    {
        DeathPhase(0, P0);
    }

    private void P0(uint id)
    {
        // loop is about 55s
        for (var i = 0u; i <= 40; i++)
            Loop(id + 0x100 * i, i == 0 ? 7.5f : 10);

        Timeout(id + 0xFF0000, 10000, "???");
    }

    private void Loop(uint id, float delay)
    {
        CastStart(id, AID.BranchOut, delay)
            .ActivateOnEnter<HedgeMazing>();

        ComponentCondition<HedgeMazing>(id + 1, 22, h => h.NumCasts == 4, "Hedges")
            .ActivateOnEnter<Shrublet>()
            .DeactivateOnExit<HedgeMazing>();

        // encourage autorot not to hold when shrublets appear
        ComponentCondition<Shrublet>(id + 0x10, 1.1f, s => s.ActiveActors.Count() == 3, "Adds appear")
            .ActivateOnEnter<Leafmash>()
            .SetHint(StateMachine.StateHint.VulnerableStart);

        ComponentCondition<Leafmash>(id + 0x20, 19, l => l.NumCasts == 1, "Jump 1")
            .SetHint(StateMachine.StateHint.VulnerableEnd);
        ComponentCondition<Leafmash>(id + 0x21, 5.3f, l => l.NumCasts == 4, "Jump 4")
            .DeactivateOnExit<Shrublet>()
            .DeactivateOnExit<Leafmash>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1032, NameID = 13979)]
public class D10OrnamentalLeafman(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), new ArenaBoundsCircle(19.5f));

