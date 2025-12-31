namespace BossMod.Dawntrail.Dungeon.D12Mistwake.D122Amdusias;

public enum OID : uint
{
    Boss = 0x4A77, // R3.960, x1
    Helper = 0x233C, // R0.500, x15, Helper type
    Thunderhead = 0x18D6, // R0.500, x2
    PoisonCloud = 0x4A78, // R1.200, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 46839, // Boss->player, no cast, single-target
    Unk = 45357, // Boss->location, no cast, single-target
    StaticCharge1 = 45335, // Boss->self, no cast, single-target
    StaticCharge2 = 45333, // Boss->self, no cast, single-target
    ThunderclapConcertoCast1 = 45341, // Boss->self, 5.0+0.5s cast, single-target
    ThunderclapConcerto1 = 45342, // Helper->self, 5.5s cast, range 40 300-degree cone
    ThunderclapConcertoCast2 = 45336, // Boss->self, 5.0+0.5s cast, single-target
    ThunderclapConcerto2 = 45337, // Helper->self, 5.5s cast, range 40 300-degree cone
    BioIICast = 45344, // Boss->self, 5.0s cast, single-target
    BioII = 45345, // Helper->location, 5.0s cast, range 20 circle
    GallopingThunderPre = 45348, // Helper->location, 1.5s cast, width 5 rect charge
    GallopingThunderCast = 45346, // Boss->location, 10.0s cast, single-target
    GallopingThunderCharge = 45347, // Boss->location, no cast, width 5 rect charge
    Burst = 45349, // PoisonCloud->self, 2.5s cast, range 9 circle
    ThunderIVCast = 45350, // Boss->self, 4.4+0.6s cast, single-target
    ThunderIV = 45351, // Helper->self, 5.0s cast, range 70 circle
    ShockboltCast = 45355, // Boss->self, 4.4+0.6s cast, single-target
    Shockbolt = 45356, // Helper->player, 5.0s cast, single-target
    Thunder = 45343, // Helper->player, 5.0s cast, range 5 circle
    ThunderIIICast = 45352, // Boss->self, 4.4+0.6s cast, single-target
    ThunderIIIFirst = 45353, // Helper->players, 5.0s cast, range 6 circle
    ThunderIIIRest = 45354, // Helper->players, no cast, range 6 circle
}

class ThunderclapConcerto(BossModule module) : Components.GroupedAOEs(module, [AID.ThunderclapConcerto1, AID.ThunderclapConcerto2], new AOEShapeCone(40, 150.Degrees()));

class BurstPredict(BossModule module) : Components.GenericAOEs(module, AID.Burst)
{
    private readonly List<List<(Actor Source, DateTime Activation)>> _sources = [];
    private readonly HashSet<ulong> _casters = [];

    private bool _gallopMode;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var i = 0;
        foreach (var grp in _sources.Take(_gallopMode ? 2 : int.MaxValue))
        {
            var color = i == 0 && _gallopMode ? ArenaColor.Danger : ArenaColor.AOE;
            foreach (var src in grp)
                yield return new AOEInstance(new AOEShapeCircle(9), src.Source.Position, default, src.Activation, color);
            i++;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.GallopingThunderPre)
        {
            _gallopMode = true;
            var from = caster.Position;
            var to = spell.LocXZ;
            var balls = Module.Enemies(OID.PoisonCloud).Where(c => c.Position.InRect(from, to - from, 2.5f + 1.2f) && !_casters.Contains(c.InstanceID));
            var next = _sources.Count == 0 ? Module.CastFinishAt(spell, 13.7f) : _sources[^1][0].Activation.AddSeconds(2.1f);
            _sources.Add([]);
            foreach (var ball in balls)
            {
                _sources[^1].Add((ball, next));
                _casters.Add(ball.InstanceID);
            }
            _sources.RemoveAll(s => s.Count == 0);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _casters.Remove(caster.InstanceID);
            if (_sources.Count > 0)
            {
                if (_sources[0].Count > 0)
                    _sources[0].RemoveAt(0);
                if (_sources[0].Count == 0)
                    _sources.RemoveAt(0);
            }
            if (_sources.Count == 0)
                _gallopMode = false;
        }

        if ((AID)spell.Action.ID == AID.ThunderIVCast)
            foreach (var src in Module.Enemies(OID.PoisonCloud).Where(e => !e.IsDead))
                _sources.Add([(src, WorldState.FutureTime(5.9f))]);
    }
}

// 46.34 -> 60
class GallopingThunder(BossModule module) : Components.GenericAOEs(module, AID.GallopingThunderCharge)
{
    private readonly List<(WPos From, WPos To, DateTime Activate)> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var i = 0;
        foreach (var aoe in Enumerable.Reverse(_aoes))
        {
            var dir = aoe.To - aoe.From;
            var len = dir.Length();
            yield return new AOEInstance(new AOEShapeRect(len, 2.5f), aoe.From, dir.ToAngle(), aoe.Activate, ++i == _aoes.Count ? ArenaColor.Danger : ArenaColor.AOE);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.GallopingThunderPre)
        {
            var next = _aoes.Count == 0 ? Module.CastFinishAt(spell, 8.7f) : _aoes[^1].Activate.AddSeconds(2.1f);
            _aoes.Add((caster.Position, spell.LocXZ, next));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }
}

class ThunderSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID.Thunder, 5);

class ThunderIII(BossModule module) : Components.StackWithCastTargets(module, AID.ThunderIIIFirst, 5, 4)
{
    private int _numCasts;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ThunderIIIFirst or AID.ThunderIIIRest)
        {
            _numCasts++;
            if (_numCasts >= 3)
            {
                Stacks.Clear();
                _numCasts = 0;
            }
        }
    }
}

class BioII(BossModule module) : Components.RaidwideCast(module, AID.BioII);
class Shockbolt(BossModule module) : Components.SingleTargetCast(module, AID.Shockbolt);
class ThunderIV(BossModule module) : Components.RaidwideCast(module, AID.ThunderIV);

class D122AmdusiasStates : StateMachineBuilder
{
    public D122AmdusiasStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ThunderclapConcerto>()
            .ActivateOnEnter<BurstPredict>()
            .ActivateOnEnter<GallopingThunder>()
            .ActivateOnEnter<ThunderSpread>()
            .ActivateOnEnter<ThunderIII>()
            .ActivateOnEnter<BioII>()
            .ActivateOnEnter<Shockbolt>()
            .ActivateOnEnter<ThunderIV>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1064, NameID = 14271)]
public class D122Amdusias(WorldState ws, Actor primary) : BossModule(ws, primary, new(281, -285), new ArenaBoundsCircle(19.5f));

