namespace BossMod.Heavensward.DeepDungeon.PalaceoftheDead.D70Taquaru;

public enum OID : uint
{
    Boss = 0x1815, // R5.750, x1
    Voidzone = 0x1E9998, // R0.500, EventObj type, spawn during fight
    Helper = 0x233C, // R0.500, x12, 523 type
};

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    Douse = 7091, // Boss->self, 3.0s cast, range 8 circle
    Drench = 7093, // Boss->self, no cast, range 10+R 90-degree cone, 5.1s after pull, 7.1s after every 2nd Electrogenesis, 7.3s after every FangsEnd
    Electrogenesis = 7094, // Boss->location, 3.0s cast, range 8 circle
    FangsEnd = 7092, // Boss->player, no cast, single-target
};

class Douse : Components.PersistentVoidzoneAtCastTarget
{
    public Douse() : base(8, ActionID.MakeSpell(AID.Douse), m => m.Enemies(OID.Voidzone).Where(z => z.EventState != 7), 0.8f) { }
}

class DouseHaste : BossComponent
{
    private bool BossInVoidzone;
    public override void Update(BossModule module)
    {
        if (module.FindComponent<Douse>()?.ActiveAOEs(module, 0, module.PrimaryActor).Any(z => z.Shape.Check(module.PrimaryActor.Position, z.Origin, z.Rotation)) ?? false)
            BossInVoidzone = true;
        else
            BossInVoidzone = false;
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (BossInVoidzone && module.PrimaryActor.TargetID == actor.InstanceID)
            hints.Add("Pull the boss out of the water puddle!");
        if (BossInVoidzone && module.PrimaryActor.TargetID != actor.InstanceID && actor.Role == Role.Tank)
            hints.Add("Consider provoking and pulling the boss out of the water puddle.");
    }
}

class Drench : Components.GenericAOEs
{
    private DateTime _activation;
    private static readonly AOEShapeCone cone = new(15.75f, 45.Degrees());
    private bool Pulled;

    public override void Update(BossModule module)
    {
        if (!Pulled)
        {
            _activation = module.WorldState.CurrentTime.AddSeconds(5.1f);
            Pulled = true;
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(cone, module.PrimaryActor.Position, module.PrimaryActor.Rotation, _activation);
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Electrogenesis) // boss can move after cast started, so we can't use aoe instance, since that would cause outdated position data to be used
        {
            ++NumCasts;
            if (NumCasts % 2 == 0)
                _activation = module.WorldState.CurrentTime.AddSeconds(7.3f);
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Drench)
            _activation = default;
        if ((AID)spell.Action.ID == AID.FangsEnd)
            _activation = module.WorldState.CurrentTime.AddSeconds(7.1f);
    }
}

class Electrogenesis : Components.LocationTargetedAOEs
{
    public Electrogenesis() : base(ActionID.MakeSpell(AID.Electrogenesis), 8, "Get out of the AOE") { }
}

class D70TaquaruStates : StateMachineBuilder
{
    public D70TaquaruStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Douse>()
            .ActivateOnEnter<DouseHaste>()
            .ActivateOnEnter<Drench>()
            .ActivateOnEnter<Electrogenesis>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "legendoficeman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 205, NameID = 5321)]
public class D70Taquaru : BossModule
{
    public D70Taquaru(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-300, -220), 25)) { }
}
