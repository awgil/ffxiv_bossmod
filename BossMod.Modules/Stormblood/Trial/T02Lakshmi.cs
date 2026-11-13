namespace BossMod.Stormblood.Trial.T02Lakshmi;

public enum OID : uint
{
    Boss = 0x1E20, // R3.500
    Helper = 0x18D6, // R0.500
    DreamingKshatriya = 0x1E22, // R1.000
    Vril = 0x1E21, // R1.000
    VoidZone = 0x1EA76C, // R0.500
}

public enum AID : uint
{
    AutoAttack = 8535, // Boss->player, no cast, single-target
    Stotram = 9347, // Boss->self, 3.0s cast, range 40 circle
    Chanchala = 9348, // Boss->self, 3.0s cast, single-target
    DivineDenial = 9349, // Boss->self, 8.0s cast, range 40 circle
    HandOfGrace = 9350, // Boss->self, 7.0s cast, single-target
    HandOfBeauty = 9351, // Boss->self, 7.0s cast, single-target
    AlluringArm = 9352, // Boss->self, 7.0s cast, single-target
    BlissfulSpearCross1 = 9355, // Helper->self, no cast, range 40 width 8 cross
    BlissfulSpearCross2 = 9356, // Helper->self, no cast, range 40 width 8 cross
    BlissfulSpearCircle1 = 9364, // Helper->player, no cast, range 7 circle
    BlissfulSpearCircle2 = 9365, // Helper->player, no cast, range 7 circle
    ThePallOfLightStack = 9361, // Boss->players, 5.0s cast, range 7 circle
    ThePullOfLight = 9362, // Boss->player, 5.0s cast, single-target
    ThePullOfLightAlt = 9363, // Boss->player, 5.0s cast, single-target
    ThePathOfLight = 9359, // Boss->self, no cast, range 40+R cone
    ThePathOfLightProtean = 9377, // Boss->self, no cast, range 40+R cone
    StotramChanchala = 9374, // Boss->self, 3.0s cast, range 40 circle
    VrilDuty = 9345,
    AetherDrain = 9357, // Vril->player, no cast
    TailSlap = 9612, // DreamingKshatriya->self, no cast, range 6+R cone
    InnerDemons = 9613, // DreamingKshatriya->self, 4.0s cast, range 6+R circle
}

public enum IconID : uint
{
    ProteanCleave = 14, // player
    SpreadCross = 107, // player
    SpreadCircle = 109, // player
}

public enum SID : uint
{
    Vril = 1290,
}

class DreamingKshatriyaAdds(BossModule module) : Components.Adds(module, (uint)OID.DreamingKshatriya);
class TailSlap(BossModule module) : Components.StandardAOEs(module, AID.TailSlap, new AOEShapeCone(7, 60.Degrees()));
class InnerDemons(BossModule module) : Components.CastGaze(module, AID.InnerDemons);
class Stotram(BossModule module) : Components.RaidwideCast(module, AID.Stotram);
class StotramChanchala(BossModule module) : Components.RaidwideCast(module, AID.StotramChanchala);
class DivineDenial(BossModule module) : Components.KnockbackFromCastTarget(module, AID.DivineDenial, 6.5f);
class ThePallOfLight(BossModule module) : Components.StackWithCastTargets(module, AID.ThePallOfLightStack, 7, 8, 8);
class ThePullOfLightTB(BossModule module) : Components.SingleTargetCast(module, AID.ThePullOfLight);
class ThePullOfLightTBAlt(BossModule module) : Components.SingleTargetCast(module, AID.ThePullOfLightAlt);
class CircleZone(BossModule module) : Components.Voidzone(module, 10, OID.VoidZone);

class VrilOrbs(BossModule module) : BossComponent(module)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Orbs().Any() || actor != Raid.Player())
            return;
        if (Charges > 0 && actor.FindStatus(SID.Vril) == null)
            hints.Add("Use Vril!");
        else if (Charges == 0)
            hints.Add("Grab an orb to refill Vril");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor != Raid.Player() || !Orbs().Any())
            return;

        if (Charges > 0)
        {
            if (actor.FindStatus(SID.Vril) == null)
                hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.VrilDuty), actor, ActionQueue.Priority.VeryHigh);
            return;
        }

        var orbs = Orbs().Select(o => ShapeDistance.InvertedCircle(o.Position, 1.5f)).ToList();
        hints.AddForbiddenZone(ShapeDistance.Intersection(orbs), WorldState.FutureTime(8));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Charges == 0)
            foreach (var z in Orbs())
                Arena.AddCircle(z.Position, 1, ArenaColor.Safe);
    }

    private int Charges => WorldState.Client.DutyActions.FirstOrDefault(a => a.Action.ID == (uint)AID.VrilDuty).CurCharges;
    private IEnumerable<Actor> Orbs() => Module.Enemies(OID.Vril).Where(z => !z.IsDead);
}

// TODO: verify Path of Light half-angle
class PathOfLight(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(40, 37.5f.Degrees()), (uint)IconID.ProteanCleave, AID.ThePathOfLight, 5.3f, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.ThePathOfLightProtean)
            CurrentBaits.Clear();
    }
}

class BlissfulBaits(BossModule module) : Components.GenericBaitAway(module)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (WorldState.Actors.Find(targetID) is not { } target)
            return;
        if (iconID == (uint)IconID.SpreadCross)
            CurrentBaits.Add(new(target, target, new AOEShapeCross(40, 4), WorldState.FutureTime(12.7f), IgnoreRotation: true));
        else if (iconID == (uint)IconID.SpreadCircle)
            CurrentBaits.Add(new(target, target, new AOEShapeCircle(7), WorldState.FutureTime(12.2f)));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Zone(actor) is var (dir, halfAngle, minRadius, activation) && activation != default)
            hints.AddForbiddenZone(ShapeDistance.Union([ShapeDistance.Circle(Arena.Center, minRadius), ShapeDistance.InvertedCone(Arena.Center, 30, dir, halfAngle)]), activation);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);
        if (Zone(pc) is var (dir, halfAngle, minRadius, activation) && activation != default)
            Arena.ZoneCone(Arena.Center, minRadius, 20, dir, halfAngle, ArenaColor.SafeFromAOE);
    }

    private (Angle dir, Angle halfAngle, float minRadius, DateTime activation) Zone(Actor actor)
    {
        if (ActiveBaitsOn(actor).FirstOrDefault() is not { Target: not null } mine)
            return default;
        var f = Module.PrimaryActor.Rotation.ToDirection();
        var fwd = MathF.Abs(f.X) >= MathF.Abs(f.Z) ? new WDir(MathF.Sign(f.X), 0) : new WDir(0, MathF.Sign(f.Z));
        var side = fwd.OrthoL();
        var circle = mine.Shape is AOEShapeCircle;
        var goLeft = ActiveBaits.FirstOrDefault(b => b.Target != actor && b.Shape.GetType() == mine.Shape.GetType()) is { Target: not null } other
            ? (actor.Position - other.Target.Position).Dot(side) >= 0
            : (actor.Position - Arena.Center).Dot(side) >= 0;
        var sideDir = goLeft ? side : -side;
        var dir = circle ? sideDir * 0.97f + fwd * 0.26f : sideDir + (goLeft ? fwd : -fwd);
        return (Angle.FromDirection(dir), (circle ? 30 : 25).Degrees(), circle ? 14 : 10, mine.Activation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BlissfulSpearCross1 or AID.BlissfulSpearCross2 or AID.BlissfulSpearCircle1 or AID.BlissfulSpearCircle2)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }
}

class T02LakshmiStates : StateMachineBuilder
{
    public T02LakshmiStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<DreamingKshatriyaAdds>()
            .ActivateOnEnter<TailSlap>()
            .ActivateOnEnter<InnerDemons>()
            .ActivateOnEnter<DivineDenial>()
            .ActivateOnEnter<Stotram>()
            .ActivateOnEnter<StotramChanchala>()
            .ActivateOnEnter<ThePallOfLight>()
            .ActivateOnEnter<ThePullOfLightTB>()
            .ActivateOnEnter<ThePullOfLightTBAlt>()
            .ActivateOnEnter<PathOfLight>()
            .ActivateOnEnter<BlissfulBaits>()
            .ActivateOnEnter<VrilOrbs>()
            .ActivateOnEnter<CircleZone>();
    }

    private void SinglePhase(uint id)
    {
        WaitForCast(id, AID.Stotram, 67, "Stotram");
        Targetable(id + 0x10, false, 5.8f, "Disappear");
        Targetable(id + 0x20, true, 42.5f, "Reappear");
        SimpleState(id + 0xFF0000, 10000, "???");
    }

    private void WaitForCast(uint id, AID aid, float expected, string name)
        => Condition(id, expected, () => Module.PrimaryActor.CastInfo?.IsSpell(aid) ?? false, name, 300);
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 263, NameID = 6385)]
public class T02Lakshmi(ModuleInit init) : BossModule(init, new(0, 0), new ArenaBoundsCircle(20));
