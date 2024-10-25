namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.D160Todesritter;

public enum OID : uint
{
    Boss = 0x181D, // R3.920, x1
    VoidsentDiscarnate = 0x18EF, // R1.000, x0 (spawn during fight)
    Actor1e86e0 = 0x1E86E0, // R2.000, x1, EventObj type
    Voidzone = 0x1E858E, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 7179, // Boss->players, no cast, range 8+R(11.92) 90?-degree cone
    Geirrothr = 7154, // Boss->self, no cast, range 6+R(9.92) 90?-degree cone
    HallOfSorrow = 7155, // Boss->location, no cast, range 9 circle
    Infatuation = 7090, // VoidsentDiscarnate->self, 6.5s cast, range 6+R(7) circle
    Valfodr = 7156, // Boss->player, 4.0s cast, width 6 rect charge + kb
}

class CleaveAuto(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.AutoAttack), new AOEShapeCone(11.92f, 45.Degrees()), activeWhileCasting: false);
class HallOfSorrow(BossModule module) : Components.PersistentVoidzone(module, 9, m => m.Enemies(OID.Voidzone).Where(z => z.EventState != 7));
class Infatuation(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Infatuation), new AOEShapeCircle(7));
class Valfodr(BossModule module) : Components.BaitAwayChargeCast(module, ActionID.MakeSpell(AID.Valfodr), 3);
class ValfodrKB(BossModule module) : Components.Knockback(module) // note actual knockback is delayed by upto 1.2s in replay
{
    private DateTime _activation;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (Module.FindComponent<Valfodr>()?.CurrentBaits.Count > 0)
            yield return new(Module.PrimaryActor.Position, 25, _activation, Module.FindComponent<Valfodr>()!.CurrentBaits[0].Shape, Angle.FromDirection(Module.FindComponent<Valfodr>()!.CurrentBaits[0].Target.Position - Module.PrimaryActor.Position), Kind: Kind.DirForward);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Valfodr)
        {
            _activation = Module.CastFinishAt(spell);
            StopAtWall = true;
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<HallOfSorrow>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || (Module.FindComponent<Infatuation>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false);
}

class D160TodesritterStates : StateMachineBuilder
{
    public D160TodesritterStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CleaveAuto>()
            .ActivateOnEnter<HallOfSorrow>()
            .ActivateOnEnter<Infatuation>()
            .ActivateOnEnter<Valfodr>()
            .ActivateOnEnter<ValfodrKB>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 214, NameID = 5438)]
public class D160Todesritter(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), new ArenaBoundsCircle(25));
