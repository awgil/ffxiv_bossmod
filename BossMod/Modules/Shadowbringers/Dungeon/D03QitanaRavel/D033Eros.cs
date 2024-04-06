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
    Rend = 15513, // Boss->player, 4,0s cast, single-target, tankbuster
    HoundOutOfHeaven = 15514, // Boss->self, 5,0s cast, single-target
    HoundOutOfHeavenTetherStretchSuccess = 17079, // Boss->player, no cast, single-target, tether break success
    HoundOutOfHeavenTetherStretchFail = 17080, // Boss->player, no cast, single-target, tether break fail
    Glossolalia = 15515, // Boss->self, 3,0s cast, range 50 circle, raidwide
    ViperPoison = 15516, // Boss->self, 6,0s cast, single-target
    ViperPoisonPatterns = 15518, // Helper->location, 6,0s cast, range 6 circle
    ViperPoisonBaitAway = 15517, // Helper->player, 6,0s cast, range 6 circle
    Jump = 15519, // Boss->location, no cast, single-target, visual?
    Inhale = 17168, // Boss->self, 4,0s cast, range 50 circle, attract 50 between centers
    HeavingBreath = 15520, // Boss->self, 3,5s cast, range 50 circle, knockback 35 forward
    HeavingBreath2 = 16923, // Helper->self, 3,5s cast, range 42 width 30 rect, visual?
    ConfessionOfFaith = 15524, // Boss->self, 5,0s cast, single-target
    ConfessionOfFaith2 = 15521, // Boss->self, 5,0s cast, single-target
    ConfessionOfFaithLeft = 15526, // Helper->self, 5,5s cast, range 60 41-degree cone
    ConfessionOfFaithRight = 15527, // Helper->self, 5,5s cast, range 60 41-degree cone
    ConfessionOfFaithStack = 15525, // Helper->players, 5,8s cast, range 6 circle, stack
    ConfessionOfFaithCenter = 15522, // Helper->self, 5,5s cast, range 60 40-degree cone
    ConfessionOfFaithSpread = 15523, // Helper->player, 5,8s cast, range 5 circle, spread
};

public enum IconID : uint
{
    tankbuster = 198, // player
    stack = 62, // player
    poisonbait = 171, // player
    spread = 96, // player
};

public enum TetherID : uint
{
    HoundOutOfHeavenTetherGood = 1, // Boss->player
    HoundOutOfHeavenTetherStretch = 57, // Boss->player
};

class HoundOutOfHeavenGood : Components.BaitAwayTethers  //TODO: consider generalizing stretched tethers?
{
    private ulong target;
    public HoundOutOfHeavenGood() : base(new AOEShapeCone(0, 0.Degrees()), (uint)TetherID.HoundOutOfHeavenTetherGood) { }
    public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        base.OnTethered(module, source, tether);
        if (tether.ID == (uint)TetherID.HoundOutOfHeavenTetherGood)
            target = tether.Target;
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (DrawTethers && target == pc.InstanceID && CurrentBaits.Count > 0)
        {
            foreach (var b in ActiveBaits)
            {
                if (arena.Config.ShowOutlinesAndShadows)
                    arena.AddLine(b.Source.Position, b.Target.Position, 0xFF000000, 2);
                arena.AddLine(b.Source.Position, b.Target.Position, ArenaColor.Safe);
            }
        }
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (target == actor.InstanceID && CurrentBaits.Count > 0)
            hints.Add("Tether is stretched!", false);
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(module, slot, actor, assignment, hints);
        if (target == actor.InstanceID && CurrentBaits.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.Circle(module.PrimaryActor.Position, 15));
    }
}

class HoundOutOfHeavenBad : Components.BaitAwayTethers
{
    private ulong target;
    public HoundOutOfHeavenBad() : base(new AOEShapeCone(0, 0.Degrees()), (uint)TetherID.HoundOutOfHeavenTetherStretch) { }
    public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        base.OnTethered(module, source, tether);
        if (tether.ID == (uint)TetherID.HoundOutOfHeavenTetherStretch)
            target = tether.Target;
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (DrawTethers && target == pc.InstanceID && CurrentBaits.Count > 0)
        {
            foreach (var b in ActiveBaits)
            {
                if (arena.Config.ShowOutlinesAndShadows)
                    arena.AddLine(b.Source.Position, b.Target.Position, 0xFF000000, 2);
                arena.AddLine(b.Source.Position, b.Target.Position, ArenaColor.Danger);
            }
        }
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (target == actor.InstanceID && CurrentBaits.Count > 0)
            hints.Add("Stretch tether further!");
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(module, slot, actor, assignment, hints);
        if (target == actor.InstanceID && CurrentBaits.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.Circle(module.PrimaryActor.Position, 15));
    }
}

class ViperPoisonPatterns : Components.PersistentVoidzoneAtCastTarget
{
    public ViperPoisonPatterns() : base(6, ActionID.MakeSpell(AID.ViperPoisonPatterns), m => m.Enemies(OID.PoisonVoidzone).Where(z => z.EventState != 7), 0) { }
}

class ConfessionOfFaithLeft : Components.SelfTargetedAOEs
{
    public ConfessionOfFaithLeft() : base(ActionID.MakeSpell(AID.ConfessionOfFaithLeft), new AOEShapeCone(60, 46.Degrees(), 20.Degrees())) { } // TODO: verify; there should not be an offset in reality here...
}

class ConfessionOfFaithRight : Components.SelfTargetedAOEs
{
    public ConfessionOfFaithRight() : base(ActionID.MakeSpell(AID.ConfessionOfFaithRight), new AOEShapeCone(60, 46.Degrees(), -20.Degrees())) { } // TODO: verify; there should not be an offset in reality here...
}
class ConfessionOfFaithStack : Components.StackWithCastTargets
{
    public ConfessionOfFaithStack() : base(ActionID.MakeSpell(AID.ConfessionOfFaithStack), 6) { }
}

class ConfessionOfFaithCenter : Components.SelfTargetedAOEs
{
    public ConfessionOfFaithCenter() : base(ActionID.MakeSpell(AID.ConfessionOfFaithCenter), new AOEShapeCone(60, 40.Degrees())) { }
}

class ConfessionOfFaithSpread : Components.SpreadFromCastTargets
{
    public ConfessionOfFaithSpread() : base(ActionID.MakeSpell(AID.ConfessionOfFaithSpread), 5) { }
}

class ViperPoisonBait : Components.GenericBaitAway
{
    private bool targeted;
    private Actor? target;

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.poisonbait)
        {
            CurrentBaits.Add(new(actor, actor, new AOEShapeCircle(6)));
            targeted = true;
            target = actor;
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ViperPoisonBaitAway)
        {
            CurrentBaits.Clear();
            targeted = false;
        }
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        base.AddHints(module, slot, actor, hints, movementHints);
        if (target == actor && targeted)
            hints.Add("Bait voidzone away!");
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(module, slot, actor, assignment, hints);
        if (target == actor && targeted)
            hints.AddForbiddenZone(ShapeDistance.Rect(new(17, -518), new(17, -558), 13));
    }
}

class Inhale : Components.KnockbackFromCastTarget
{
    public Inhale() : base(ActionID.MakeSpell(AID.Inhale), 50, kind: Kind.TowardsOrigin) { }

    //TODO: consider testing if path is unsafe in addition to destination
    public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos) => module.FindComponent<ViperPoisonPatterns>()?.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false;
}

class HeavingBreath : Components.KnockbackFromCastTarget
{
    public HeavingBreath() : base(ActionID.MakeSpell(AID.HeavingBreath), 35, kind: Kind.DirForward)
    {
        StopAtWall = true;
    }

    //TODO: consider testing if path is unsafe in addition to destination
    public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos) => module.FindComponent<ViperPoisonPatterns>()?.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false;
}

class Glossolalia : Components.RaidwideCast
{
    public Glossolalia() : base(ActionID.MakeSpell(AID.Glossolalia)) { }
}

class Rend : Components.SingleTargetDelayableCast
{
    public Rend() : base(ActionID.MakeSpell(AID.Rend)) { }
}

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
public class D033Eros : BossModule
{
    public D033Eros(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(17, -538), 15, 20)) { }
}
