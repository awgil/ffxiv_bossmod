namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.D70Taquaru;

public enum OID : uint
{
    Boss = 0x1815, // R5.750, x1
    Voidzone = 0x1E9998, // R0.500, EventObj type, spawn during fight
    Helper = 0x233C, // R0.500, x12, 523 type
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    Douse = 7091, // Boss->self, 3.0s cast, range 8 circle
    Drench = 7093, // Boss->self, no cast, range 10+R 90-degree cone, 5.1s after pull, 7.1s after every 2nd Electrogenesis, 7.3s after every FangsEnd
    Electrogenesis = 7094, // Boss->location, 3.0s cast, range 8 circle
    FangsEnd = 7092, // Boss->player, no cast, single-target
}

class Douse(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 8, ActionID.MakeSpell(AID.Douse), m => m.Enemies(OID.Voidzone).Where(z => z.EventState != 7), 0.8f);

class DouseHaste(BossModule module) : BossComponent(module)
{
    private bool BossInVoidzone;

    public override void Update()
    {
        if (Module.FindComponent<Douse>()?.ActiveAOEs(0, Module.PrimaryActor).Any(z => z.Shape.Check(Module.PrimaryActor.Position, z.Origin, z.Rotation)) ?? false)
            BossInVoidzone = true;
        else
            BossInVoidzone = false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (BossInVoidzone && Module.PrimaryActor.TargetID == actor.InstanceID)
            hints.Add("Pull the boss out of the water puddle!");
        if (BossInVoidzone && Module.PrimaryActor.TargetID != actor.InstanceID && actor.Role == Role.Tank)
            hints.Add("Consider provoking and pulling the boss out of the water puddle.");
    }
}

class Electrogenesis(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Electrogenesis), 8);

class D70TaquaruStates : StateMachineBuilder
{
    public D70TaquaruStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Douse>()
            .ActivateOnEnter<DouseHaste>()
            .ActivateOnEnter<Electrogenesis>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "legendoficeman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 205, NameID = 5321)]
public class D70Taquaru(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -220), new ArenaBoundsCircle(25));
