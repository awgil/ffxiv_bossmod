namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.D50ServomechanicalChimera14X;

static class Shapes
{
    public static readonly AOEShape Dragon = new AOEShapeDonut(8, 40);
    public static readonly AOEShape Ram = new AOEShapeCircle(9.1f);
}

public enum OID : uint
{
    Boss = 0x3D9C,
    Helper = 0x233C,
    Cacophony = 0x3D9D, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    SongsOfIceAndThunder = 31851, // Boss->self, 5.0s cast, range 9 circle
    SongsOfThunderAndIce = 31852, // Boss->self, 5.0s cast, range ?-40 donut
    TheDragonsVoiceSong = 31853, // Boss->self, no cast, range ?-40 donut
    TheRamsVoiceSong = 31854, // Boss->self, no cast, range 9 circle
    LeftbreathedThunder = 31861, // Boss->self, 5.0s cast, range 40 180-degree cone
    RightbreathedCold = 31863, // Boss->self, 5.0s cast, range 40 180-degree cone
    ColdThunder = 31855, // Boss->player, 5.0s cast, width 8 rect charge
    ThunderousCold = 31856, // Boss->player, 5.0s cast, width 8 rect charge
    TheDragonsVoice = 32806, // Boss->self, no cast, range ?-40 donut
    TheRamsVoice = 32807, // Boss->self, no cast, range 9 circle
}

class Cacophony(BossModule module) : Components.PersistentVoidzone(module, 6, m => m.Enemies(OID.Cacophony).Where(e => e.EventState != 7), 6);

class RightbreathedCold(BossModule module) : Components.StandardAOEs(module, AID.RightbreathedCold, new AOEShapeCone(40, 90.Degrees()));
class LeftbreathedThunder(BossModule module) : Components.StandardAOEs(module, AID.LeftbreathedThunder, new AOEShapeCone(40, 90.Degrees()));

abstract class StretchTether(BossModule module, AID action) : Components.BaitAwayChargeCast(module, action, 4)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        foreach (var bait in ActiveBaitsOn(actor))
            hints.AddForbiddenZone(new AOEShapeCircle(15), bait.Source.Position, activation: bait.Activation);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        foreach (var bait in ActiveBaitsOn(actor))
            if (actor.Position.InCircle(bait.Source.Position, 15))
                hints.Add("Stretch tether!");
    }
}

class ThunderousColdTether(BossModule module) : StretchTether(module, AID.ThunderousCold);
class ColdThunderTether(BossModule module) : StretchTether(module, AID.ColdThunder);

class Charge(BossModule module) : Components.GenericAOEs(module)
{
    private record class PredictedAOE(AOEShape Shape, Actor Caster, DateTime Activation, DateTime WaitUntil);

    private readonly List<PredictedAOE> _aoes = [];

    private string hint = "";

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Where(a => a.WaitUntil < WorldState.CurrentTime).Take(1).Select(a => new AOEInstance(a.Shape, a.Caster.Position, default, a.Activation));

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ThunderousCold:
                hint = "In -> Out";
                _aoes.Add(new(Shapes.Dragon, caster, WorldState.FutureTime(3.1f), WorldState.FutureTime(1)));
                _aoes.Add(new(Shapes.Ram, caster, WorldState.FutureTime(6.2f), WorldState.FutureTime(1)));
                break;
            case AID.ColdThunder:
                hint = "Out -> In";
                _aoes.Add(new(Shapes.Ram, caster, WorldState.FutureTime(3.1f), WorldState.FutureTime(1)));
                _aoes.Add(new(Shapes.Dragon, caster, WorldState.FutureTime(6.2f), WorldState.FutureTime(1)));
                break;
            case AID.TheDragonsVoice:
            case AID.TheRamsVoice:
                if (_aoes.Count > 0)
                    _aoes.RemoveAt(0);
                if (_aoes.Count == 0)
                    hint = "";
                break;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (hint.Length > 0)
            hints.Add(hint);
    }
}

abstract class Song(BossModule module, AID First, AOEShape FirstShape, AID Second, AOEShape SecondShape, string hint, float delay = 3.05f) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(1);

    public override void Update()
    {
        if (Module.PrimaryActor.LastFrameMovement != default)
            for (var i = 0; i < _aoes.Count; i++)
                _aoes.Ref(i).Origin = Module.PrimaryActor.Position;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == First)
        {
            _aoes.Add(new(FirstShape, caster.Position, Activation: Module.CastFinishAt(spell)));
            _aoes.Add(new(SecondShape, caster.Position, Activation: Module.CastFinishAt(spell, delay)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)First || spell.Action.ID == (uint)Second)
            _aoes.RemoveAt(0);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_aoes.Count > 0)
            hints.Add(hint);
    }
}

class SongsOfIceAndThunder(BossModule module) : Song(module, AID.SongsOfIceAndThunder, Shapes.Ram, AID.TheDragonsVoiceSong, Shapes.Dragon, "Out -> In");
class SongsOfThunderAndIce(BossModule module) : Song(module, AID.SongsOfThunderAndIce, Shapes.Dragon, AID.TheRamsVoiceSong, Shapes.Ram, "In -> Out");

class ServomechanicalChimera14XStates : StateMachineBuilder
{
    public ServomechanicalChimera14XStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SongsOfIceAndThunder>()
            .ActivateOnEnter<SongsOfThunderAndIce>()
            .ActivateOnEnter<Charge>()
            .ActivateOnEnter<ThunderousColdTether>()
            .ActivateOnEnter<ColdThunderTether>()
            .ActivateOnEnter<LeftbreathedThunder>()
            .ActivateOnEnter<RightbreathedCold>()
            .ActivateOnEnter<Cacophony>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 901, NameID = 12265)]
public class ServomechanicalChimera14X(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), new ArenaBoundsCircle(19.5f));
