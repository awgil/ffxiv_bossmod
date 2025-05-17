namespace BossMod.Dawntrail.Hunt.RankA.Pkuucha;

public enum OID : uint
{
    Boss = 0x4580, // R4.340, x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    MesmerizingMarch = 39863, // Boss->self, 4.0s cast, range 12 circle
    StirringSamba = 39864, // Boss->self, 4.0s cast, range 40 180-degree cone
    GlidingSwoop = 39757, // Boss->self, 3.5s cast, range 18 width 16 rect
    MarchingSamba = 39797, // Boss->self, 5.0s cast, single-target, visual (followed by short circle + cone)
    MesmerizingMarchShort = 39755, // Boss->self, 1.5s cast, range 12 circle
    StirringSambaShort = 39756, // Boss->self, 1.0s cast, range 40 180-degree cone
    DeadlySwoop = 39799, // Boss->player, no cast, single-target, deadly damage on targets hit by sambas
    PeckingFlurryFirst = 39760, // Boss->self, 5.0s cast, range 40 circle, raidwide (first in series of 3)
    PeckingFlurryRest = 39761, // Boss->self, no cast, range 40 circle, raidwide (remaining ones)
}

class MesmerizingMarchStirringSamba(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shapeCircle = new(12);
    private static readonly AOEShapeCone _shapeFrontal = new(40, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.MesmerizingMarch:
                _aoes.Add(new(_shapeCircle, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
                break;
            case AID.StirringSamba:
                _aoes.Add(new(_shapeFrontal, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
                break;
            case AID.MarchingSamba:
                _aoes.Add(new(_shapeCircle, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 1.7f)));
                _aoes.Add(new(_shapeFrontal, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 5.7f)));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = (AID)spell.Action.ID switch
        {
            AID.MesmerizingMarch or AID.MesmerizingMarchShort => _shapeCircle,
            AID.StirringSamba or AID.StirringSambaShort => _shapeFrontal,
            _ => null
        };
        if (shape == null)
            return;

        if (_aoes.Count == 0)
        {
            ReportError($"Unexpected resolve: {spell.Action}");
            return;
        }

        if (_aoes[0].Shape != shape)
            ReportError($"Unexpected resolve: got {spell.Action}, expected {_aoes[0].Shape}");
        _aoes.RemoveAt(0);
    }
}

class GlidingSwoop(BossModule module) : Components.StandardAOEs(module, AID.GlidingSwoop, new AOEShapeRect(18, 8));
class PeckingFlurry(BossModule module) : Components.RaidwideCast(module, AID.PeckingFlurryFirst, "Raidwide x3");

class PkuuchaStates : StateMachineBuilder
{
    public PkuuchaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MesmerizingMarchStirringSamba>()
            .ActivateOnEnter<GlidingSwoop>()
            .ActivateOnEnter<PeckingFlurry>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13443)]
public class Pkuucha(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
