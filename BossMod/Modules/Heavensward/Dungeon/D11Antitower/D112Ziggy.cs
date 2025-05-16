namespace BossMod.Heavensward.Dungeon.D11Antitower.D112Ziggy;

public enum OID : uint
{
    Boss = 0x3D82, // R2.700
    Helper = 0x233C,
    Stardust = 0x3D83, // R2.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    GyratingGlare = 31835, // Boss->self, 5.0s cast, range 40 circle
    ShinySummoning = 31831, // Boss->self, no cast, single-target
    MysticLight = 31838, // Stardust->self, 6.0s cast, range 12 circle
    JitteringGlare = 31832, // Boss->self, 3.0s cast, range 40 30-degree cone
    JitteringJounceCast = 31833, // Boss->self, 6.0s cast, single-target
    JitteringJounceCharge = 31840, // Boss->player/Stardust, no cast, width 6 rect charge
    DeepFracture = 31839, // Stardust->self, 4.0s cast, range 11 circle
    JitteringJab = 31837, // Boss->player, 5.0s cast, single-target
}

public enum IconID : uint
{
    JitteringJounce = 2, // player
}

public enum TetherID : uint
{
    JitteringJounce = 2, // Boss->player/Stardust
}

class JitteringGlare(BossModule module) : Components.StandardAOEs(module, AID.JitteringGlare, new AOEShapeCone(40, 15.Degrees()));
class JitteringJab(BossModule module) : Components.SingleTargetCast(module, AID.JitteringJab);

class JitteringJounceAOE(BossModule module) : Components.GenericLineOfSightAOE(module, AID.JitteringJounceCharge, 100, false)
{
    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.JitteringJounce)
        {
            if (Raid.TryFindSlot(tether.Target, out var slot))
            {
                IgnoredPlayers = ~BitMask.Build(slot);
                Modify(Module.PrimaryActor.Position, Module.Enemies(OID.Stardust).Where(e => !e.IsDead).Select(s => (s.Position, 1f)), WorldState.FutureTime(6));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.JitteringJounceCharge)
            Modify(null, []);
    }
}

class JitteringJounceTether(BossModule module) : BossComponent(module)
{
    private Actor? Target;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.JitteringJounce)
            Target = WorldState.Actors.Find(tether.Target);
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.JitteringJounce)
            Target = null;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Target == null)
            return;

        var src = Module.PrimaryActor.Position;
        var dst = Target.Position;
        var shape = new AOEShapeRect((dst - src).Length(), 3);
        var angle = Angle.FromDirection(dst - src);
        if (Target == pc)
            shape.Outline(Module.Arena, src, angle, ArenaColor.Danger);
        else
            shape.Draw(Module.Arena, src, angle, ArenaColor.AOE);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Target == null || Target == actor)
            return;

        hints.AddForbiddenZone(ShapeContains.Rect(Module.PrimaryActor.Position, Target.Position, 3), Module.CastFinishAt(Module.PrimaryActor.CastInfo, 0.5f));
    }
}

class Stardust(BossModule module) : BossComponent(module)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc) => Arena.Actors(Module.Enemies(OID.Stardust).Where(x => !x.IsDead), ArenaColor.Object, true);
}
class DeepFracture(BossModule module) : Components.StandardAOEs(module, AID.DeepFracture, new AOEShapeCircle(11));
class GyratingGlare(BossModule module) : Components.RaidwideCast(module, AID.GyratingGlare);
class MysticLight(BossModule module) : Components.StandardAOEs(module, AID.MysticLight, new AOEShapeCircle(12));

class ZiggyStates : StateMachineBuilder
{
    public ZiggyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Stardust>()
            .ActivateOnEnter<GyratingGlare>()
            .ActivateOnEnter<MysticLight>()
            .ActivateOnEnter<DeepFracture>()
            .ActivateOnEnter<JitteringJounceAOE>()
            .ActivateOnEnter<JitteringJounceTether>()
            .ActivateOnEnter<JitteringJab>()
            .ActivateOnEnter<JitteringGlare>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 141, NameID = 4808, Contributors = "xan")]
public class Ziggy(WorldState ws, Actor primary) : BossModule(ws, primary, new(185.78f, 137.5f), new ArenaBoundsCircle(20));
