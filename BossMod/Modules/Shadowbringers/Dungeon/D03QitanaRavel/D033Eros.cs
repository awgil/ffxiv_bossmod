namespace BossMod.Shadowbringers.Dungeon.D03QitanaRavel.D033Eros;

public enum OID : uint
{
    Boss = 0x27B1, //R=7.02
    Helper = 0x233C, //R=0.5
    PoisonVoidzone = 0x1E972C,
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Rend = 15513, // Boss->player, 4.0s cast, single-target, tankbuster
    HoundOutOfHeaven = 15514, // Boss->self, 5.0s cast, single-target
    HoundOutOfHeavenTetherStretchSuccess = 17079, // Boss->player, no cast, single-target, tether break success
    HoundOutOfHeavenTetherStretchFail = 17080, // Boss->player, no cast, single-target, tether break fail
    Glossolalia = 15515, // Boss->self, 3.0s cast, range 50 circle, raidwide
    ViperPoison = 15516, // Boss->self, 6.0s cast, single-target
    ViperPoisonPatterns = 15518, // Helper->location, 6.0s cast, range 6 circle
    ViperPoisonBaitAway = 15517, // Helper->player, 6.0s cast, range 6 circle
    Jump = 15519, // Boss->location, no cast, single-target, visual?
    Inhale = 17168, // Boss->self, 4.0s cast, range 50 circle, attract 50 between centers
    HeavingBreath = 15520, // Boss->self, 3.5s cast, range 50 circle, knockback 35 forward
    HeavingBreath2 = 16923, // Helper->self, 3.5s cast, range 42 width 30 rect, visual?
    ConfessionOfFaith = 15524, // Boss->self, 5.0s cast, single-target
    ConfessionOfFaith2 = 15521, // Boss->self, 5.0s cast, single-target
    ConfessionOfFaithLeft = 15526, // Helper->self, 5.5s cast, range 60 41-degree cone
    ConfessionOfFaithRight = 15527, // Helper->self, 5.5s cast, range 60 41-degree cone
    ConfessionOfFaithStack = 15525, // Helper->players, 5.8s cast, range 6 circle, stack
    ConfessionOfFaithCenter = 15522, // Helper->self, 5.5s cast, range 60 40-degree cone
    ConfessionOfFaithSpread = 15523, // Helper->player, 5.8s cast, range 5 circle, spread
}

public enum IconID : uint
{
    tankbuster = 198, // player
    stack = 62, // player
    poisonbait = 171, // player
    spread = 96, // player
}

public enum TetherID : uint
{
    HoundOutOfHeavenTetherGood = 1, // Boss->player
    HoundOutOfHeavenTetherStretch = 57, // Boss->player
}

class HoundOutOfHeavenGood(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(0, 0.Degrees()), (uint)TetherID.HoundOutOfHeavenTetherGood) // TODO: consider generalizing stretched tethers?
{
    private ulong target;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        base.OnTethered(source, tether);
        if (tether.ID == (uint)TetherID.HoundOutOfHeavenTetherGood)
            target = tether.Target;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (DrawTethers && target == pc.InstanceID && CurrentBaits.Count > 0)
        {
            foreach (var b in ActiveBaits)
            {
                if (Arena.Config.ShowOutlinesAndShadows)
                    Arena.AddLine(b.Source.Position, b.Target.Position, 0xFF000000, 2);
                Arena.AddLine(b.Source.Position, b.Target.Position, ArenaColor.Safe);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (target == actor.InstanceID && CurrentBaits.Count > 0)
            hints.Add("Tether is stretched!", false);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (target == actor.InstanceID && CurrentBaits.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.Circle(Module.PrimaryActor.Position, 15));
    }
}

class HoundOutOfHeavenBad(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(0, 0.Degrees()), (uint)TetherID.HoundOutOfHeavenTetherStretch)
{
    private ulong target;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        base.OnTethered(source, tether);
        if (tether.ID == (uint)TetherID.HoundOutOfHeavenTetherStretch)
            target = tether.Target;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (DrawTethers && target == pc.InstanceID && CurrentBaits.Count > 0)
        {
            foreach (var b in ActiveBaits)
            {
                if (Arena.Config.ShowOutlinesAndShadows)
                    Arena.AddLine(b.Source.Position, b.Target.Position, 0xFF000000, 2);
                Arena.AddLine(b.Source.Position, b.Target.Position, ArenaColor.Danger);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (target == actor.InstanceID && CurrentBaits.Count > 0)
            hints.Add("Stretch tether further!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (target == actor.InstanceID && CurrentBaits.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.Circle(Module.PrimaryActor.Position, 15));
    }
}

class ViperPoisonPatterns(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, ActionID.MakeSpell(AID.ViperPoisonPatterns), m => m.Enemies(OID.PoisonVoidzone).Where(z => z.EventState != 7), 0);
class ConfessionOfFaithLeft(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ConfessionOfFaithLeft), new AOEShapeCone(60, 46.Degrees(), 20.Degrees())); // TODO: verify; there should not be an offset in reality here...
class ConfessionOfFaithRight(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ConfessionOfFaithRight), new AOEShapeCone(60, 46.Degrees(), -20.Degrees())); // TODO: verify; there should not be an offset in reality here...
class ConfessionOfFaithStack(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.ConfessionOfFaithStack), 6);
class ConfessionOfFaithCenter(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ConfessionOfFaithCenter), new AOEShapeCone(60, 40.Degrees()));
class ConfessionOfFaithSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.ConfessionOfFaithSpread), 5);

class ViperPoisonBait(BossModule module) : Components.GenericBaitAway(module)
{
    private bool targeted;
    private Actor? target;

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.poisonbait)
        {
            CurrentBaits.Add(new(actor, actor, new AOEShapeCircle(6)));
            targeted = true;
            target = actor;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ViperPoisonBaitAway)
        {
            CurrentBaits.Clear();
            targeted = false;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (target == actor && targeted)
            hints.Add("Bait voidzone away!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (target == actor && targeted)
            hints.AddForbiddenZone(ShapeDistance.Rect(new(17, -518), new(17, -558), 13));
    }
}

class Inhale(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Inhale), 50, kind: Kind.TowardsOrigin)
{
    //TODO: consider testing if path is unsafe in addition to destination
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => Module.FindComponent<ViperPoisonPatterns>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false;
}

class HeavingBreath(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.HeavingBreath), 35, kind: Kind.DirForward, stopAtWall: true)
{
    //TODO: consider testing if path is unsafe in addition to destination
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => Module.FindComponent<ViperPoisonPatterns>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false;
}

class Glossolalia(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Glossolalia));
class Rend(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.Rend));

class D033ErosStates : StateMachineBuilder
{
    public D033ErosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ViperPoisonBait>()
            .ActivateOnEnter<ViperPoisonPatterns>()
            .ActivateOnEnter<Rend>()
            .ActivateOnEnter<HoundOutOfHeavenGood>()
            .ActivateOnEnter<HoundOutOfHeavenBad>()
            .ActivateOnEnter<Glossolalia>()
            .ActivateOnEnter<ConfessionOfFaithLeft>()
            .ActivateOnEnter<ConfessionOfFaithRight>()
            .ActivateOnEnter<ConfessionOfFaithSpread>()
            .ActivateOnEnter<ConfessionOfFaithCenter>()
            .ActivateOnEnter<ConfessionOfFaithStack>()
            .ActivateOnEnter<HeavingBreath>()
            .ActivateOnEnter<Inhale>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 651, NameID = 8233)]
public class D033Eros(WorldState ws, Actor primary) : BossModule(ws, primary, new(17, -538), new ArenaBoundsRect(15, 20));
