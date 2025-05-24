namespace BossMod.Endwalker.Dungeon.D05Aitiascope.D053Amon;

public enum OID : uint
{
    Boss = 0x346E, // R=16.98
    Helper = 0x233C,
    YsaylesSpirit = 0x346F, // R2.000, x1
    Unknown = 0x1EB26D, // R0.500, x0 (spawn during fight), EventObj type
    Ice = 0x3470, // R6.000, x1
}

public enum AID : uint
{
    AutoAttack = 24712, // Boss->player, no cast, single-target
    Antistrophe = 25694, // Boss->self, 3.0s cast, single-target
    CurtainCall = 25702, // Boss->self, 32.0s cast, range 40 circle //Lethal Raidwide, needs to hide behind Ice

    DarkForte = 25700, // Boss->player, 5.0s cast, single-target //Tankbuster
    Entracte = 25701, // Boss->self, 5.0s cast, range 40 circle //Raidwide

    DreamsOfIce = 27756, // Helper->self, 14.7s cast, range 6 circle // summons ice to hide behind
    Epode = 25695, // Helper->self, 8.0s cast, range 70 width 12 rect

    EruptionForteVisual = 24709, // Boss->self, 3.0s cast, single-target
    EruptionForteAOE = 25704, // Helper->location, 4.0s cast, range 8 circle // BaitedAOE

    LeftFiragaForte = 25697, // Boss->self, 7.0s cast, range 40 width 20 rect
    RightFiragaForte = 25696, // Boss->self, 7.0s cast, range 40 width 20 rect

    Strophe = 25693, // Boss->self, 3.0s cast, single-target

    ThundagaForte1 = 25690, // Boss->location, 5.0s cast, range 40 circle //proximity-based AoE
    ThundagaForte2 = 25691, // Helper->self, 5.0s cast, range 20 45-degree cone
    ThundagaForte3 = 25692, // Helper->self, 11.0s cast, range 20 45-degree cone

    Unknown = 25703, // YsaylesSpirit->self, no cast, single-target
}

public enum IconID : uint
{
    Tankbuster = 218, // player
}

class Epode(BossModule module) : Components.StandardAOEs(module, AID.Epode, new AOEShapeRect(70, 6));

class EruptionForteAOE(BossModule module) : Components.StandardAOEs(module, AID.EruptionForteAOE, 8);

class FiragaForte(BossModule module) : Components.GroupedAOEs(module, [AID.LeftFiragaForte, AID.RightFiragaForte], new AOEShapeRect(40, 10));

class ThundagaForte1(BossModule module) : Components.StandardAOEs(module, AID.ThundagaForte1, 15);

class DarkForte(BossModule module) : Components.SingleTargetCast(module, AID.DarkForte);
class Entracte(BossModule module) : Components.RaidwideCast(module, AID.Entracte);

class CurtainCall(BossModule module) : Components.GenericLineOfSightAOE(module, AID.CurtainCall, 60, true)
{
    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 5)
        {
            switch (state)
            {
                // ice appears
                case 0x00020001:
                    Modify(Module.PrimaryActor.Position, [(Arena.Center, 6)], Module.CastFinishAt(Module.PrimaryActor.CastInfo));
                    break;
                // ice disappears - this just removes the blocker, cast is already finished at this point
                case 0x00080004:
                    Modify(null, []);
                    break;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            Modify(null, Blockers);
    }
}

class ThundagaForte(BossModule module) : Components.GenericAOEs(module, AID.Strophe)
{
    private readonly List<Actor> _castersThundagaForte2 = [];
    private readonly List<Actor> _castersThundagaForte3 = [];

    private static readonly AOEShape _shapeThundagaForte2 = new AOEShapeCone(20, 22.5f.Degrees());
    private static readonly AOEShape _shapeThundagaForte3 = new AOEShapeCone(20, 22.5f.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _castersThundagaForte2.Count > 0
            ? _castersThundagaForte2.Select(c => new AOEInstance(_shapeThundagaForte2, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo)))
            : _castersThundagaForte3.Select(c => new AOEInstance(_shapeThundagaForte3, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        CastersForSpell(spell.Action)?.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        CastersForSpell(spell.Action)?.Remove(caster);
    }

    private List<Actor>? CastersForSpell(ActionID spell) => (AID)spell.ID switch
    {
        AID.ThundagaForte2 => _castersThundagaForte2,
        AID.ThundagaForte3 => _castersThundagaForte3,
        _ => null
    };
}

class D053AmonStates : StateMachineBuilder
{
    public D053AmonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            // .ActivateOnEnter<DreamsOfIce>()
            .ActivateOnEnter<Epode>()
            .ActivateOnEnter<EruptionForteAOE>()
            .ActivateOnEnter<FiragaForte>()
            .ActivateOnEnter<CurtainCall>()
            .ActivateOnEnter<ThundagaForte1>()
            .ActivateOnEnter<ThundagaForte>()
            .ActivateOnEnter<DarkForte>()
            .ActivateOnEnter<Entracte>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 786, NameID = 10293)]
public class D053Amon(WorldState ws, Actor primary) : BossModule(ws, primary, new(11, -490), new ArenaBoundsCircle(20));
