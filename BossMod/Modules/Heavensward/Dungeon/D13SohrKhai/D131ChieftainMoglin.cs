namespace BossMod.Heavensward.Dungeon.D13SohrKhai.D131ChieftainMoglin;

public enum OID : uint
{
    Boss = 0x15FB, // R1.800, x1
    Helper = 0x1B2, // R0.500, x23
    PomguardPompincher = 0x1602, // R0.900, x0 (spawn during fight)
    PomguardPomchopper = 0x15FD, // R0.900, x0 (spawn during fight)
    CaptainMogsun = 0x15FC, // R0.900, x0 (spawn during fight)
    PomguardPomcrier = 0x1601, // R0.900, x1
    PomguardPomfluffer = 0x15FE, // R0.900, x1
    PomguardPomfryer = 0x1600, // R0.900, x1
    PomguardPompiercer = 0x15FF, // R0.900, x1
    DemoniacalMogcane = 0x1603, // R5.000, x0 (spawn during fight)
}

public enum AID : uint
{
    ThousandKuponzeCharge = 6019, // Boss->self, no cast, range 8+R ?-degree cone
    PoisonNeedle = 4432, // 1602->player, no cast, single-target
    PomHoly = 6020, // Boss->location, 3.0s cast, range 50 circle
    PomPraise = 6016, // 1B2->self, 13.0s cast, range 4 circle
    PomPraiseVisual = 6015, // Boss->self, 13.0s cast, single-target
    HundredKuponzeSwipe = 4429, // 15FD->self, 3.0s cast, range 20+R 90-degree cone
    DemoniacalMogcane = 6017, // Boss->self, 2.0s cast, single-target
    MoogleEyeShot = 4427, // 15FF->player, no cast, single-target
    PomBomVisual = 6018, // 1603->self, no cast, single-target
    PomBom = 6212, // ChieftainMoglin->self, no cast, range 40+R width 4 rect
    SpinningMogshield = 4425, // CaptainMogsun->self, 3.0s cast, range 6+R circle
    MarchOfTheMoogles = 4431, // PomguardPomcrier->self, 5.0s cast, range 10 circle, damage buff to allies
    PomCure = 4426, // PomguardPomfluffer->Boss, 3.0s cast, single-target
    PomFlare = 4428, // PomguardPomfryer->self, 6.0s cast, range 21+R circle
}

public enum SID : uint
{
    Invincibility = 325, // none->Boss, extra=0x0
    OffBalance = 1064, // none->CaptainMogsun/PomguardPomchopper/PomguardPompincher, extra=0x0
    Poison = 18, // PomguardPompincher->player, extra=0x0
}

class Mogshield(BossModule module) : Components.StandardAOEs(module, AID.SpinningMogshield, new AOEShapeCircle(6.9f));
class ThousandKuponzeCharge(BossModule module) : Components.GenericBaitAway(module)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DemoniacalMogcane)
            CurrentBaits.Add(new(caster, WorldState.Actors.Find(caster.TargetID)!, new AOEShapeCone(9.8f, 60.Degrees()), WorldState.FutureTime(4.2f)));

        if ((AID)spell.Action.ID == AID.ThousandKuponzeCharge)
            CurrentBaits.Clear();
    }

    public override void Update()
    {
        base.Update();

        // sanity check: clear baits if cleave is delayed enough (haven't seen this happen in any replays)
        CurrentBaits.RemoveAll(b => (WorldState.CurrentTime - b.Activation).TotalSeconds > 6);
    }
}
class PomFlare(BossModule module) : Components.StandardAOEs(module, AID.PomFlare, new AOEShapeCircle(21.9f));
class PomHoly(BossModule module) : Components.RaidwideCast(module, AID.PomHoly);
class Swipe(BossModule module) : Components.StandardAOEs(module, AID.HundredKuponzeSwipe, new AOEShapeCone(20.9f, 45.Degrees()));
// pombom 6.4f
class PomBom(BossModule module) : Components.GenericAOEs(module)
{
    private record struct CaneObj(Actor Cane, DateTime Activation);
    private CaneObj? Cane;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(Cane).Select(c => new AOEInstance(new AOEShapeCross(20.25f, 2), c.Cane.Position, c.Cane.Rotation, c.Activation));

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.DemoniacalMogcane)
        {
            Cane = new(actor, WorldState.FutureTime(6.4f));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.PomBom)
            Cane = null;
    }
}
class PomPraise(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> Casters = [];

    private IEnumerable<Actor> RaisedMoogles => Casters.Count == 0 ? [] : WorldState.Actors.Where(e => e.FindStatus(SID.OffBalance) != null && WillRaise(e));

    private bool WillRaise(Actor moogle) => Casters.Any(c => moogle.Position.InCircle(c.Position, 4));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.PomPraise)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.PomPraise)
            Casters.Remove(caster);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var e in WorldState.Actors.Exclude(Module.PrimaryActor).Where(x => !x.IsAlly && x.IsTargetable))
            Arena.Actor(e, e.FindStatus(SID.OffBalance) == null ? ArenaColor.Enemy : WillRaise(e) ? ArenaColor.Object : ArenaColor.PlayerGeneric);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var c in Casters)
            Arena.ZoneCircle(c.Position, 4, ArenaColor.AOE);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
            return;

        foreach (var r in hints.PotentialTargets)
            if (r.Actor.FindStatus(SID.OffBalance) != null && WillRaise(r.Actor))
                r.Priority = 5;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (RaisedMoogles.Any())
            hints.Add("Knock moogles out of raise!");
    }
}

class ChieftainMoglinStates : StateMachineBuilder
{
    public ChieftainMoglinStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PomHoly>()
            .ActivateOnEnter<Swipe>()
            .ActivateOnEnter<PomPraise>()
            .ActivateOnEnter<PomFlare>()
            .ActivateOnEnter<PomBom>()
            .ActivateOnEnter<ThousandKuponzeCharge>()
            .ActivateOnEnter<Mogshield>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 171, NameID = 4943, Contributors = "xan")]
public class ChieftainMoglin(WorldState ws, Actor primary) : BossModule(ws, primary, new(-400, -158), new ArenaBoundsCircle(20))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = h.Actor.FindStatus(SID.Invincibility) == null ? 1 : 0;
    }
}
