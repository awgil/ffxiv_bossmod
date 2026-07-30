namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.DD70Yaquaru;

public enum OID : uint
{
    Boss = 0x1815, // R5.75
    Voidzone = 0x1E9998 // R0.5
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target

    Douse = 7091, // Boss->self, 3.0s cast, range 8 circle
    Drench = 7093, // Boss->self, no cast, range 10+R 90-degree cone, 5.1s after pull, 7.1s after every 2nd Electrogenesis, 7.3s after every FangsEnd
    Electrogenesis = 7094, // Boss->location, 3.0s cast, range 8 circle
    FangsEnd = 7092 // Boss->player, no cast, single-target
}

public enum SID : uint
{
    Heavy = 14
}

class Douse(BossModule module) : Components.VoidzoneAtCastTarget(module, 8f, (uint)AID.Douse, GetVoidzones, 0.8d)
{
    public static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.Voidzone);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

class DousePuddle(BossModule module) : BossComponent(module)
{
    private readonly Actor[] puddles = Douse.GetVoidzones(module);

    private bool BossInPuddle
    {
        get
        {
            var len = puddles.Length;
            for (var i = 0; i < len; ++i)
            {
                if (Module.PrimaryActor.Position.InCircle(puddles[i].Position, 8f + Module.PrimaryActor.HitboxRadius))
                    return true;
            }
            return false;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        // indicate on minimap how far boss needs to be pulled
        if (BossInPuddle)
            Arena.ZoneCircleOutline(Module.PrimaryActor.Position, Module.PrimaryActor.HitboxRadius, Colors.Danger);
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
            var primary = Module.PrimaryActor;
            var hitbox = primary.HitboxRadius;
            var effPuddleSize = 8f + hitbox;
            var tankDist = hints.FindEnemy(primary)?.TankDistance ?? 2f;
            // yaquaru tank distance seems to be around 2-2.5y, but from testing, 3y minimum is needed to move it out of the puddle, either because of rasterization shenanigans or netcode
            var effTankDist = hitbox + tankDist + 1f;

            var len = puddles.Length;
            var puddlez = new ShapeDistance[len];
            for (var i = 0; i < len; ++i)
            {
                puddlez[i] = new SDCircle(puddles[i].Position, effPuddleSize + effTankDist);
            }
            var closest = new SDUnion(puddlez);
            hints.GoalZones.Add(p => closest.Distance(p) > 0f ? 1000f : 0f);
        }
    }
}

class Electrogenesis(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Electrogenesis, 8f);

class FangsEnd(BossModule module) : BossComponent(module)
{
    private BitMask _heavy;

    public override void Update()
    {
        for (var i = 0; i < 4; ++i)
        {
            var player = Raid[i];
            if (player == null)
                continue;

            if (player.FindStatus((uint)SID.Heavy) is ActorStatus st && (st.ExpireAt - WorldState.CurrentTime).TotalSeconds > 8d)
                _heavy.Set(i);
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Heavy)
            _heavy.Clear(Raid.FindSlot(actor.InstanceID));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.ShouldCleanse |= _heavy;
    }
}

class DD70YaquaruStates : StateMachineBuilder
{
    public DD70YaquaruStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Douse>()
            .ActivateOnEnter<DousePuddle>()
            .ActivateOnEnter<Electrogenesis>()
            .ActivateOnEnter<FangsEnd>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "legendoficeman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 205, NameID = 5321)]
public class DD70Yaquaru(WorldState ws, Actor primary) : BossModule(ws, primary, SharedBounds.ArenaBounds607080.Center, SharedBounds.ArenaBounds607080);
