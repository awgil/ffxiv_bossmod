namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.DD170Yulunggu;

public enum OID : uint
{
    Boss = 0x181E, // R5.750, x1
    Voidzone = 0x1E9998 // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target

    Douse = 7158, // Boss->self, 2.0s cast, range 8 circle
    Drench = 7160, // Boss->self, no cast, range 10+R ?-degree cone
    Electrogenesis = 7161, // Boss->location, 3.0s cast, range 8 circle
    FangsEnd = 7159 // Boss->player, no cast, single-target
}

class Douse(BossModule module) : Components.VoidzoneAtCastTarget(module, 8f, (uint)AID.Douse, GetVoidzones, 0.8f)
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

class Drench(BossModule module) : Components.Cleave(module, (uint)AID.Drench, new AOEShapeCone(15.75f, 45f.Degrees()), activeWhileCasting: false);
class Electrogenesis(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Electrogenesis, 8f);

class DD170YulungguStates : StateMachineBuilder
{
    public DD170YulungguStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Douse>()
            .ActivateOnEnter<DousePuddle>()
            .ActivateOnEnter<Drench>()
            .ActivateOnEnter<Electrogenesis>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 215, NameID = 5449)]
public class DD170Yulunggu(WorldState ws, Actor primary) : BossModule(ws, primary, SharedBounds.ArenaBounds160170180190.Center, SharedBounds.ArenaBounds160170180190);
