using BossMod.BLM;

namespace BossMod.Endwalker.Dungeon.D04KtisisHyperboreia.D042LadonLord;

public enum OID : uint
{
    Boss = 0x3425, // R=3.99
    Helper = 0x233C,
    PyricSphere = 0x3426, // R=0.7
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport = 25733, // Boss->location, no cast, single-target
    Inhale1 = 25732, // Boss->self, 4.0s cast, single-target
    Inhale2 = 25915, // Boss->self, no cast, single-target
    Intimidation = 25741, // Boss->self, 6.0s cast, range 40 circle, raidwide
    PyricBlast = 25742, // Boss->players, 4.0s cast, range 6 circle, stack
    PyricBreathFront = 25734, // Boss->self, 7.0s cast, range 40 120-degree cone
    PyricBreathLeft = 25735, // Boss->self, 7.0s cast, range 40 120-degree cone
    PyricBreathRight = 25736, // Boss->self, 7.0s cast, range 40 120-degree cone
    PyricBreathFront2 = 25737, // Boss->self, no cast, range 40 120-degree cone
    PyricBreathLeft2 = 25738, // Boss->self, no cast, range 40 120-degree cone
    PyricBreathRight2 = 25739, // Boss->self, no cast, range 40 120-degree cone
    PyricSphereVisual = 25744, // PyricSphere->self, 5.0s cast, single-target
    PyricSphere = 25745, // Helper->self, 10.0s cast, range 50 width 4 cross
    Scratch = 25743, // Boss->player, 5.0s cast, single-target, tankbuster
    SpawnSpheres = 25740, // Boss->self, no cast, ???
}

public enum SID : uint
{
    BreathCombo = 2195, // none->Boss, extra=0x144/0x145/0x149/0x146/0x148/0x147, seems to save the current breath combo or what is left of it
    MiddleHead = 2812, // none->Boss, extra=0x9F6
    LeftHead = 2813, // none->Boss, extra=0x177F
    RightHead = 2814, // none->Boss, extra=0x21A8
}

class PyricBreath(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly List<uint> buffs = [];
    private static readonly Angle angle = 120.Degrees();
    private static readonly AOEShapeCone cone = new(40, 60.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        for (var i = 0; i < _aoes.Count; i++)
            yield return new AOEInstance(_aoes[i].Shape, _aoes[i].Origin, _aoes[i].Rotation, _aoes[i].Activation, i == 0 && NumCasts > 0 ? ArenaColor.Danger : ArenaColor.AOE);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.MiddleHead or SID.RightHead or SID.LeftHead)
            buffs.Add(status.ID);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.PyricBreathFront or AID.PyricBreathLeft or AID.PyricBreathRight)
        {
            _aoes.Add(new(cone, caster.Position, spell.Rotation, spell.NPCFinishAt));
            if (buffs.Count == 2)
            {
                if (buffs[1] == (uint)SID.RightHead)
                    _aoes.Add(new(cone, caster.Position, spell.Rotation + (buffs[0] == (uint)SID.MiddleHead ? -angle : -2 * angle), spell.NPCFinishAt.AddSeconds(2.1f)));
                if (buffs[1] == (uint)SID.LeftHead)
                    _aoes.Add(new(cone, caster.Position, spell.Rotation + (buffs[0] == (uint)SID.MiddleHead ? angle : 2 * angle), spell.NPCFinishAt.AddSeconds(2.1f)));
                if (buffs[1] == (uint)SID.MiddleHead)
                    _aoes.Add(new(cone, caster.Position, spell.Rotation + (buffs[0] == (uint)SID.LeftHead ? -angle : angle), spell.NPCFinishAt.AddSeconds(2.1f)));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.PyricBreathFront or AID.PyricBreathLeft or AID.PyricBreathRight)
        {
            ++NumCasts;
            if (_aoes.Count > 0 && buffs.Count > 0)
            {
                _aoes.RemoveAt(0);
                buffs.RemoveAt(0);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.PyricBreathFront2 or AID.PyricBreathLeft2 or AID.PyricBreathRight2)
        {
            _aoes.Clear();
            buffs.Clear();
        }
    }
}

class PyricSphere(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PyricSphere), new AOEShapeCross(50, 2)); // we could draw this almost 5s earlier, but why bother with 10s cast time
class PyricBlast(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.PyricBlast), 6, 4);
class Scratch(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Scratch));
class Intimidation(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Intimidation));

class D042LadonLordStates : StateMachineBuilder
{
    public D042LadonLordStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PyricBreath>()
            .ActivateOnEnter<PyricSphere>()
            .ActivateOnEnter<PyricBlast>()
            .ActivateOnEnter<Scratch>()
            .ActivateOnEnter<Intimidation>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 787, NameID = 10398)]
public class D042LadonLord(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 48), new ArenaBoundsSquare(19.5f));
