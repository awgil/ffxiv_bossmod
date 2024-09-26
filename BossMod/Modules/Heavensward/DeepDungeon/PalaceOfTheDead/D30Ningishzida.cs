namespace BossMod.Heavensward.DeepDungeon.PalaceoftheDead.D30Ningishzida;

public enum OID : uint
{
    Boss = 0x16AC, // R4.800, x1
    BallofFirePuddle = 0x1E8D9B, // R0.500, x0 (spawn during fight), EventObj type
    BallofIcePuddle = 0x1E8D9C, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 6501, // Boss->player, no cast, range 6+R ?-degree cone
    Dissever = 6426, // Boss->self, no cast, range 6+R 90-degree cone
    BallOfFire = 6427, // Boss->location, no cast, range 6 circle
    BallOfIce = 6428, // Boss->location, no cast, range 6 circle
    FearItself = 6429, // Boss->self, 6.0s cast, range 54+R circle
}

class Dissever(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.Dissever), new AOEShapeCone(10.8f, 45.Degrees()), activeWhileCasting: false);
class BallofFire(BossModule module) : Components.PersistentVoidzone(module, 6, m => m.Enemies(OID.BallofFirePuddle).Where(z => z.EventState != 7));
class BallofIce(BossModule module) : Components.PersistentVoidzone(module, 6, m => m.Enemies(OID.BallofIcePuddle).Where(z => z.EventState != 7));
class FearItself(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FearItself), new AOEShapeDonut(5, 50));

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($" Bait the boss away from the middle of the arena. \n {Module.PrimaryActor.Name} will cast x2 Fire Puddles & x2 Ice Puddles, after the 4th puddle is dropped, run to the middle.");
    }
}

class D30NingishzidaStates : StateMachineBuilder
{
    public D30NingishzidaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Dissever>()
            .ActivateOnEnter<BallofFire>()
            .ActivateOnEnter<BallofIce>()
            .ActivateOnEnter<FearItself>()
            .ActivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 176, NameID = 5012)]
public class D30Ningishzida(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -235), new ArenaBoundsCircle(25));
