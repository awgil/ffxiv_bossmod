namespace BossMod.Dawntrail.Dungeon.D07StrayboroughDeadwalk.D072JackInThePot;

public enum OID : uint
{
    Boss = 0x41CA, // R4.160, x1
    Helper = 0x233C, // R0.500, x?, Helper type
    SpectralSamovar = 0x41CB, // R2.880, x0 (spawn during fight)
    StrayPhantagenitrix = 0x41D2, // R2.100, x0 (spawn during fight)
    TeacupHelper = 0x41D5, // R1.500, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 37169, // Boss->player, no cast, single-target
    TroublingTeacups = 36716, // Boss->self, 3.0s cast, single-target, visual (spawn cups)
    TeaAwhirl = 36717, // Boss->self, 6.0s cast, single-target, visual (cup AOEs)
    TricksomeTreat = 36720, // StrayPhantagenitrix->self, 3.0s cast, range 19 circle
    ToilingTeapots = 36722, // Boss->self, 3.0s cast, single-target, visual
    Puppet = 36721, // StrayPhantagenitrix->location, 4.0s cast, single-target
    PipingPour = 36723, // SpectralSamovar->location, 2.0s cast, single-target, visual (puddle)
    MadTeaParty = 36724, // Helper->self, no cast, puddle DOT
    LastDrop = 36726, // Boss->player, 5.0s cast, single-target, tankbuster
    SordidSteam = 36725, // Boss->self, 5.0s cast, range 40 circle, raidwide
}

public enum TetherID : uint
{
    CupTether = 276, // cup->phantagenitrix
}

public enum SID : uint
{
    AreaOfInfluenceUp = 1909, // none->Helper, extra=0x1..0x4
}

class PipingPour(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _shape = new(8);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11DD && _aoes.Count > 0 && (OID)actor.OID == OID.SpectralSamovar)
            _aoes.RemoveAt(0);
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if (status.ID == (uint)SID.AreaOfInfluenceUp && status.Extra == 0x1)
            _aoes.Add(new(_shape, actor.Position));
    }
}

class TeaAwhirl(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _shape = new(19);
    private readonly List<Actor> _cups = [];
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.CupTether)
            _cups.Add(source);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is not (0x23 or 0x01))
            return;
        switch (state)
        {
            case 0x02000100:
                TryActivate(11.5f, [
                    (new(17, -163), new WPos?(new(17, -177)), [new(3.5f, -161.5f), new(30.5f, -178.5f)]),
                    (new(17, -153), new WPos?(new(10, -170)), [new(25.5f, -156.5f), new(20.5f, -178.5f)]),
                    (new(17, -153), new WPos?(new(17, -177)), [new(20.5f, -178.5f), new(3.5f, -161.5f)]),
                    (new(34, -170), null, [new(8.5f, -173.5f)]),
                    (new(0, -170), null, [new(25.5f, -166.5f)]),
                ]);
                break;
            case 0x10000800:
                TryActivate(14.5f, [
                    (new(0, -170), new WPos?(new(34, -170)), [new(8.5f, -156.5f), new(25.5f, -183.5f)]),
                    (new(0, -170), new WPos?(new(17, -187)), [new(3.5f, -178.5f), new(8.5f, -156.5f)]),
                    (new(17, -187), new WPos?(new(17, -153)), [new(30.5f, -161.5f), new(3.5f, -178.5f)]),
                ]);
                break;
            case 0x00100001:
                if (_cups.Count >= 2)
                    AddAOEs(WorldState.FutureTime(16), [_cups[0].Position, _cups[1].Position]);
                break;
            case 0x00400020:
                TryActivate(19, [
                    (new(0, -170), new WPos?(new(17, -163)), [new(5, -165), new(22, -182)]),
                    (new(17, -177), new WPos?(new(17, -153)), [new(5, -175), new(29, -175)]),
                ]);
                break;
        }
    }

    private void TryActivate(float delay, List<(WPos a, WPos? b, WPos[] explode)> patterns)
    {
        var act = WorldState.FutureTime(delay);
        foreach (var (a, b, explode) in patterns)
        {
            if (b is { } bb ? HasCup(a) && HasCup(bb) : HasCup(a))
            {
                AddAOEs(act, explode);
                return;
            }
        }
    }

    private bool HasCup(WPos p) => _cups.Any(c => c.Position.AlmostEqual(p, 0.5f));

    private void AddAOEs(DateTime activation, WPos[] positions)
    {
        foreach (var p in positions)
            _aoes.Add(new(_shape, p, default, activation));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TricksomeTreat)
        {
            _aoes.Clear();
            _cups.Clear();
        }
    }
}

class SordidSteam(BossModule module) : Components.RaidwideCast(module, AID.SordidSteam);
class LastDrop(BossModule module) : Components.SingleTargetCast(module, AID.LastDrop);

class D072JackInThePotStates : StateMachineBuilder
{
    public D072JackInThePotStates(BossModule module) : base(module)
    {
        // TODO: Sordid Steam never seen (two clears)
        TrivialPhase()
            .ActivateOnEnter<TeaAwhirl>()
            .ActivateOnEnter<PipingPour>()
            .ActivateOnEnter<SordidSteam>()
            .ActivateOnEnter<LastDrop>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 981, NameID = 12760)]
public class D072JackInThePot(ModuleInit init) : BossModule(init, new(17, -170), new ArenaBoundsCircle(19.5f));
