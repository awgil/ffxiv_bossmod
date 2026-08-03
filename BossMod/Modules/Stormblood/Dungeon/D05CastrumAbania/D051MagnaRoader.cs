namespace BossMod.Stormblood.Dungeon.D05CastrumAbania.D051MagnaRoader;

public enum OID : uint
{
    Boss = 0x1AA9, // R3.2
    MarkXLIIIMiniCannon = 0x1AAC, // R2.0
    TwelfthLegionTriarius = 0x1AAB, // R0.5
    TwelfthLegionOptio = 0x1AAA, // R0.5
    Helper = 0x18D6
}

public enum AID : uint
{
    AutoAttack1 = 872, // Boss->player, no cast, single-target
    AutoAttack2 = 870, // TwelfthLegionOptio->player, no cast, single-target

    MagitekFireII = 7957, // Boss->location, 3.0s cast, range 5 circle
    MagitekFireIII = 7958, // Boss->self, 3.0s cast, range 40+R circle

    WildSpeeVisual = 8318, // Boss->location, 6.0s cast, range 40+R width 6 rect
    HaywireVisual = 7959, // Boss->location, no cast, width 6 rect charge
    HaywireTelegraph = 7960, // Helper->location, 6.5s cast, range 40+R width 6 rect
    WildSpeed = 8184, // Helper->location, no cast, range 40+R width 6 rect

    MagitekPulseVisual1 = 7961, // MarkXLIIIMiniCannon->location, 3.0s cast, single-target
    MagitekPulseVisual2 = 8325, // TwelfthLegionTriarius->self, 3.0s cast, single-target
    MagitekPulse = 8336, // Helper->location, 3.0s cast, range 6 circle

    Wheel = 7956 // Boss->player, no cast, single-target, tankbuster
}

public enum SID : uint
{
    Fetters = 1399 // Helper->player/Helper/Boss, extra=0x0
}

class MagitekPulsePlayer(BossModule module) : BossComponent(module)
{
    private readonly WildSpeedHaywire _aoe = module.FindComponent<WildSpeedHaywire>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoe.AOEs.Count != 0)
        {
            var turrets = Module.Enemies((uint)OID.MarkXLIIIMiniCannon);
            Actor? closest = null;
            var minDistSq = float.MaxValue;

            var count = turrets.Count;
            for (var i = 0; i < count; ++i)
            {
                var turret = turrets[i];
                if (turret.IsTargetable)
                {
                    hints.GoalZones.Add(AIHints.GoalSingleTarget(turret, 1.5f, 5f));
                    var distSq = (actor.Position - turret.Position).LengthSq();
                    if (distSq < minDistSq)
                    {
                        minDistSq = distSq;
                        closest = turret;
                    }
                }
            }
            if (closest != null && minDistSq < 12.25f)
            {
                hints.ForcedTarget = closest;
                hints.InteractWithTarget = closest;
                hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.MagitekPulse), null, ActionQueue.Priority.High, targetPos: Module.PrimaryActor.PosRot.XYZ());
            }
        }
        else if (Module.PrimaryActor.FindStatus((uint)SID.Fetters) != null)
            hints.ForcedTarget = Module.PrimaryActor;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_aoe.AOEs.Count == 0)
            return;
        var turrets = Module.Enemies((uint)OID.MarkXLIIIMiniCannon);
        var count = turrets.Count;
        for (var i = 0; i < count; ++i)
        {
            if (turrets[i].IsTargetable)
            {
                hints.Add("Use the turrets to stun the boss!");
                return;
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_aoe.AOEs.Count == 0)
            return;
        var turrets = Module.Enemies((uint)OID.MarkXLIIIMiniCannon);
        var count = turrets.Count;
        for (var i = 0; i < count; ++i)
        {
            var turret = turrets[i];
            if (turret.IsTargetable)
                Arena.ZoneCircleOutline(turret.Position, 3f, Colors.Safe);
        }
    }
}

class WildSpeedHaywire(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [with(4)];
    private static readonly AOEShapeRect rect = new(40.5f, 3f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];

        var aoes = CollectionsMarshal.AsSpan(AOEs);
        if (count > 1)
            aoes[0].Color = Colors.Danger;
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HaywireTelegraph)
            AOEs.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, -0.2f))); // actual dmg AOE happens ~0.2s before cast ends
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (AOEs.Count != 0 && spell.Action.ID == (uint)AID.WildSpeed)
            AOEs.RemoveAt(0);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Fetters && actor == Module.PrimaryActor)
            AOEs.Clear();
    }
}

class MagitekPulse(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagitekPulse, 6f);
class MagitekFireII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagitekFireII, 5f);
class MagitekFireIII(BossModule module) : Components.RaidwideCast(module, (uint)AID.MagitekFireIII);

class D051MagnaRoaderStates : StateMachineBuilder
{
    public D051MagnaRoaderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WildSpeedHaywire>()
            .ActivateOnEnter<MagitekFireII>()
            .ActivateOnEnter<MagitekFireIII>()
            .ActivateOnEnter<MagitekPulsePlayer>()
            .ActivateOnEnter<MagitekPulse>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 242, NameID = 6263)]
public class D051MagnaRoader(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Circle(new(-212.99f, 186.99f), 19.55f)], [new Rectangle(new(-213, 167), 20, 1.25f), new Rectangle(new(-213, 208), 20, 2.1f)]);
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.TwelfthLegionOptio));
        Arena.Actors(Enemies((uint)OID.MarkXLIIIMiniCannon), Colors.Object);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (FindComponent<WildSpeedHaywire>()?.AOEs.Count != 0)
        {
            var turrets = Enemies((uint)OID.MarkXLIIIMiniCannon);
            var countT = turrets.Count;
            for (var i = 0; i < countT; ++i)
            {
                var turret = turrets[i];
                if (turret.IsTargetable)
                    return;
            }
            var count = hints.PotentialTargets.Count;
            for (var i = 0; i < count; ++i)
            {
                var e = hints.PotentialTargets[i];
                e.Priority = AIHints.Enemy.PriorityUndesirable; // targeting anything will leave the cannon
            }
        }
        else
        {
            var count = hints.PotentialTargets.Count;
            for (var i = 0; i < count; ++i)
            {
                var e = hints.PotentialTargets[i];
                e.Priority = e.Actor.OID switch
                {
                    (uint)OID.TwelfthLegionOptio => 1,
                    _ => 0
                };
            }
        }
    }
}
