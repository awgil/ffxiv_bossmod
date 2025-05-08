namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.D170Yulunggu;

public enum OID : uint
{
    Boss = 0x181E, // R5.750, x1
    Voidzone = 0x1E9998, // R0.500, x0 (spawn during fight), EventObj type
    Actor1E86E0 = 0x1E86E0, // R2.000, x1, EventObj type
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    Douse = 7158, // Boss->self, 2.0s cast, range 8 circle
    Drench = 7160, // Boss->self, no cast, range 10+R ?-degree cone
    Electrogenesis = 7161, // Boss->location, 3.0s cast, range 8 circle
    FangsEnd = 7159, // Boss->player, no cast, single-target
}

class Douse(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 8, AID.Douse, m => m.Enemies(OID.Voidzone).Where(z => z.EventState != 7), 0.8f);

class DouseHaste(BossModule module) : BossComponent(module)
{
    private bool _bossInVoidzone;

    public override void Update()
    {
        if (Module.FindComponent<Douse>()?.ActiveAOEs(0, Module.PrimaryActor).Any(z => z.Shape.Check(Module.PrimaryActor.Position, z.Origin, z.Rotation)) ?? false)
            _bossInVoidzone = true;
        else
            _bossInVoidzone = false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_bossInVoidzone && Module.PrimaryActor.TargetID == actor.InstanceID)
            hints.Add("Pull the boss out of the water puddle!");
        if (_bossInVoidzone && Module.PrimaryActor.TargetID != actor.InstanceID && actor.Role == Role.Tank)
            hints.Add("Consider provoking and pulling the boss out of the water puddle.");
    }
}

class Drench(BossModule module) : Components.Cleave(module, AID.Drench, new AOEShapeCone(15.75f, 45.Degrees()), activeWhileCasting: false);

class Electrogenesis(BossModule module) : Components.StandardAOEs(module, AID.Electrogenesis, 8);

class D170YulungguStates : StateMachineBuilder
{
    public D170YulungguStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Douse>()
            .ActivateOnEnter<DouseHaste>()
            .ActivateOnEnter<Drench>()
            .ActivateOnEnter<Electrogenesis>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 215, NameID = 5449)]
public class D170Yulunggu(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), new ArenaBoundsCircle(25));
