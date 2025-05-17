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

public enum SID : uint
{
    Heavy = 14
}

class DouseCast(BossModule module) : Components.StandardAOEs(module, AID.Douse, new AOEShapeCircle(8));
class DousePuddle(BossModule module) : BossComponent(module)
{
    private IEnumerable<Actor> Puddles => Module.Enemies(OID.Voidzone).Where(z => z.EventState != 7);
    private bool BossInPuddle => Puddles.Any(p => Module.PrimaryActor.Position.InCircle(p.Position, 8 + Module.PrimaryActor.HitboxRadius));

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var p in Puddles)
            Arena.ZoneCircle(p.Position, 8, ArenaColor.AOE);

        // indicate on minimap how far boss needs to be pulled
        if (BossInPuddle)
            Arena.AddCircle(Module.PrimaryActor.Position, Module.PrimaryActor.HitboxRadius, ArenaColor.Danger);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Module.PrimaryActor.TargetID == actor.InstanceID && BossInPuddle)
            hints.Add("Pull boss out of puddle!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.PrimaryActor.TargetID == actor.InstanceID && BossInPuddle)
        {
            var effPuddleSize = 8 + Module.PrimaryActor.HitboxRadius;
            var tankDist = hints.FindEnemy(Module.PrimaryActor)?.TankDistance ?? 2;
            // yaquaru tank distance seems to be around 2-2.5y, but from testing, 3y minimum is needed to move it out of the puddle, either because of rasterization shenanigans or netcode
            var effTankDist = Module.PrimaryActor.HitboxRadius + tankDist + 1;

            var puddles = Puddles.Select(p => ShapeContains.Circle(p.Position, effPuddleSize + effTankDist)).ToList();
            var closest = ShapeContains.Union(puddles);
            hints.GoalZones.Add(p => !closest(p) ? 1000 : 0);
        }
    }
}

class Electrogenesis(BossModule module) : Components.StandardAOEs(module, AID.Electrogenesis, 8);
class FangsEnd(BossModule module) : BossComponent(module)
{
    private BitMask _heavy;

    public override void Update()
    {
        for (var i = 0; i < 4; i++)
        {
            var player = Raid[i];
            if (player == null)
                continue;

            if (player.FindStatus(SID.Heavy) is ActorStatus st && (st.ExpireAt - WorldState.CurrentTime).TotalSeconds > 8)
                _heavy.Set(i);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Heavy)
            _heavy.Clear(Raid.FindSlot(actor.InstanceID));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.ShouldCleanse |= _heavy;
    }
}

class D70TaquaruStates : StateMachineBuilder
{
    public D70TaquaruStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DouseCast>()
            .ActivateOnEnter<DousePuddle>()
            .ActivateOnEnter<Electrogenesis>()
            .ActivateOnEnter<FangsEnd>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "legendoficeman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 205, NameID = 5321)]
public class D70Taquaru(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -220), new ArenaBoundsCircle(25));
