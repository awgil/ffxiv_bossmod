namespace BossMod.Stormblood.Trial.T02Lakshmi;

public enum OID : uint
{
    Boss = 0x1E20, // R3.500, x1
    Helper = 0x18D6, // R0.500, x16, 523 type : Looks like a helper instance pops up where voidzones are cast also.
    Lakshmi2 = 0x1D27, // R1.000, x10
    Lakshmi3 = 0x1E23, // R0.000, x1
    DreamingKshatriya = 0x1E22, // R1.000, x2
    Actor1e24 = 0x1E24, // R5.000, x1 (spawn during fight)
    Vril = 0x1E21, // R1.000, x12 (spawn during fight)
    VoidZone = 0x1EA76C // R0.5 voidzone aoes : Spawn during fight.
}

public enum AID : uint
{
    AutoAttack = 8535, // Boss->player, no cast, single-target
    AetherDrain = 9357, // Vril->player, no cast, single-target
    AlluringArm = 9352, // Boss->self, 7.0s cast, single-target
    AlluringEmbrace1 = 9358, // Lakshmi3->self, no cast, range 0 circle
    AlluringEmbrace2 = 9366, // Helper->self, no cast, range 100 circle

    BlissfulArrow1 = 9353, // Helper->player, no cast, single-target
    BlissfulArrow2 = 9354, // Helper->player, no cast, single-target

    BlissfulSpear1 = 9355, // Helper->self, no cast, range 40 width 8 cross
    BlissfulSpear2 = 9356, // Helper->self, no cast, range 40 width 8 cross
    BlissfulSpear3 = 9364, // Helper->player, no cast, range 7 circle
    BlissfulSpear4 = 9365, // Helper->player, no cast, range 7 circle

    Chanchala = 9348, // Boss->self, 3.0s cast, single-target
    DivineDenial = 9349, // Boss->self, 8.0s cast, range 40 circle
    HandOfGrace = 9350, // Boss->self, 7.0s cast, single-target
    HandOfBeauty = 9351, // Boss->self, 7.0s cast, single-target
    Jagadishwari = 9026, // Boss->self, no cast, single-target
    // Regular raid wide.
    Stotram1 = 9347, // Boss->self, 3.0s cast, range 40 circle
    // Use Vril shield
    Stotram2 = 9374, // Boss->self, 3.0s cast, range 40 circle

    ThePallOfLight1 = 9360, // Boss->player, 5.0s cast, range 7 circle
    ThePallOfLightStack = 9361, // Boss->players, 5.0s cast, range 7 circle

    ThePullOfLightTB1 = 9362, // Boss->player, 5.0s cast, single-target
    ThePullOfLightTB2 = 9363, // Boss->player, 5.0s cast, single-target

    ThePathOfLightCleave = 9359, // Boss->self, no cast, range 40+R ?-degree cone
    ThePathOfLightProtean = 9377, // Boss->self, no cast, range 40+R ?-degree cone

    Unknown1 = 9305, // Lakshmi3->self, no cast, single-target
    Unknown2 = 9306, // Helper->self, no cast, single-target

    // Dreamer abilities
    TailSlap = 9612, // Dreamer->self, no cast, range 6+R ?-degree cone
    InnerDemons = 9613, //Dreamer->self, 4.0 cast range 6+R circle, Effect range 6

    //Duty Action
    //VrilShield = 9345 // Duty action for vril shielding => registered in ClassShared for aihint usage.
}

public enum SID : uint
{
    TargetRight = 1374, // none->player, extra=0x0 : Indicates a cross shaped bait away is coming.
    TargetLeft = 1375, // none->player, extra=0x0 : Indicates a circle shaped bait away is coming.
    Bleeding = 320, // none->player, extra=0x0 : This happens when you stand in puddles.
    Seduced = 1389, // none->player, extra=0x17EC : You lose control of character and dance while boss does raidwide.
    Chanchala = 1410, // Boss->Helper/Boss, extra=0x0
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    Vril = 1290 // player->player : When you push the duty action Vril you get the Vril status.
}

public enum IconID : uint
{
    ProteanCleave = 14, // player : 45 degree cleave
    Stackmarker = 62, // player
    SpreadCross = 107, // player  : This baitaway is the cross shape
    SpreadCircle = 109, // player : This baitaway is the circle shape
    Tankbuster = 218, // player : 2
}

// Medusa mobs and abililties.
sealed class EmanationTrash(BossModule module) : Components.Adds(module, (uint)OID.DreamingKshatriya);

// Half angle is an estimation.
sealed class TailSlap(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TailSlap, new AOEShapeCone(7f, 60f.Degrees()));

sealed class InnerDemonsGaze(BossModule module) : Components.CastGaze(module, (uint)AID.InnerDemons);

// Lakshmi abilities
/*
 * Display the Vril when it appears.  If player needs vril grab one.  If special action is
 * powered up it should ignore the vril in arena.
 */
sealed class VrilOrbs(BossModule module) : BossComponent(module)
{
    // store whether or not we have the vril status
    private BitMask _vrilStatus;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Vril && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            _vrilStatus[slot] = true;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            if (status.ID == (uint)SID.Vril)
                _vrilStatus[slot] = false;
        }
    }
    public static List<Actor> GetOrbs(BossModule module)
    {
        var orbs = module.Enemies((uint)OID.Vril);
        var count = orbs.Count;
        if (count == 0)
            return [];

        var filteredorbs = new List<Actor>(count);
        for (var i = 0; i < count; ++i)
        {
            var z = orbs[i];
            if (!z.IsDead)
                filteredorbs.Add(z);
        }
        return filteredorbs;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        var orbs = GetOrbs(Module);
        var count = orbs.Count;
        if (count != 0)
            hints.Add("One Vril orb if Vril action not charged.");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var vrilCharge = WorldState.Client.DutyActions[0].CurCharges > 0;

        var orbs = GetOrbs(Module);
        var count = orbs.Count;

        // If orbs exist and we have no charges and we don't have status vril
        // The idea is to stay charged without grabbing all the orbs.
        if (count != 0 && !vrilCharge && !_vrilStatus[slot])
        {
            var orbz = new ShapeDistance[count];
            for (var i = 0; i < count; ++i)
            {
                var o = orbs[i];
                orbz[i] = new SDInvertedRect(o.Position + 0.5f * o.Rotation.ToDirection(), new WDir(default, 1f), 0.5f, 0.5f, 0.5f);
            }
            hints.AddForbiddenZone(new SDIntersection(orbz), DateTime.MaxValue);
        }
    }

    // Draw the Vril as circles on radar.
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var orbs = GetOrbs(Module);
        var count = orbs.Count;
        for (var i = 0; i < count; ++i)
            Arena.ZoneCircle(orbs[i].Position, 0.75f, Colors.Safe);
    }
}

// Without shield you are knocked off platform and die.
// Would be nice if the indicator updates if you activate Vril duty action during cast,
// but it is much easier to just set it to the shorter version.
sealed class DivineDenial(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.DivineDenial, 6.5f)
{
    private bool _casting;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == (uint)AID.DivineDenial)
            _casting = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (spell.Action.ID == (uint)AID.DivineDenial)
            _casting = false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_casting)
        {
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Vril), actor, ActionQueue.Priority.High);
        }
    }
}

// Regular raidwide
sealed class Stotram1(BossModule module) : Components.RaidwideCast(module, (uint)AID.Stotram1);

// Pops Vril Duty Action for Jagadishwari, Stotram2.  Not sure if stotram requires it, but not much
// else to use shield for in second half.
sealed class StotramShielding(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.Stotram2, (uint)AID.Jagadishwari])
{
    private bool _casting;
    private bool _jagadishwari;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID is (uint)AID.Stotram2)
            _casting = true;
    }

    // Jagadishwari won't pop shield if we use _casting.
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Jagadishwari)
        {
            _jagadishwari = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (spell.Action.ID is (uint)AID.Stotram2)
            _casting = false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_casting || _jagadishwari)
        {
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Vril), actor, ActionQueue.Priority.High);
            _jagadishwari = false;
        }
    }
}

sealed class ThePallOfLightStack(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.ThePallOfLightStack, 7, 8, 8);

// These should probably be converted to bait away tankbusters
sealed class ThePullOfLightTB1(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ThePullOfLightTB1);
sealed class ThePullOfLightTB2(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ThePullOfLightTB2);

sealed class PathOfLightCleave(BossModule module) : Components.GenericBaitAway(module, (uint)AID.ThePathOfLightCleave, tankbuster: true)
{
    private Actor? _nextTarget;
    private DateTime _nextActivation;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        // Activation time is an estimation.  Actual cast event is instant.
        _nextActivation = WorldState.FutureTime(4.7f);
        // next target is the pc with the icon.
        _nextTarget = WorldState.Actors.Find(targetID);

        if ((IconID)iconID == IconID.ProteanCleave && _nextTarget != null)
        {
            // Half angle is an estimation at this time.
            CurrentBaits.Add(new(Module.PrimaryActor, _nextTarget, new AOEShapeCone(40f, 37.5f.Degrees()), _nextActivation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID
            is (uint)AID.ThePathOfLightProtean
            or (uint)AID.ThePathOfLightCleave)
        {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }
}

/*
 * This should cover the various baits from 'hand/arm of' spells that use the SpreadCross/SpreadCircle icons.
 *
 * TODO: would be good if the circle baits were given a safezone area on outside of arena to bait the voidzones to.
 */
sealed class BlissfulBaits(BossModule module) : Components.GenericBaitAway(module, (uint)AID.BlissfulSpear2)
{
    private Actor? _nextTarget;
    private DateTime _nextActivation;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        _nextActivation = WorldState.FutureTime(4.7d);
        // next target is the pc with the icon.
        _nextTarget = WorldState.Actors.Find(targetID);

        if (iconID == (uint)IconID.SpreadCross && _nextTarget != null)
        {
            // 180 degrees rotation to keep the cross from spinning with pc.
            CurrentBaits.Add(new(_nextTarget, _nextTarget, new AOEShapeCross(40f, 4f), _nextActivation, customRotation: 180f.Degrees()));
        }
        else if (iconID == (uint)IconID.SpreadCircle && _nextTarget != null)
        {
            CurrentBaits.Add(new(_nextTarget, _nextTarget, new AOEShapeCircle(7f), _nextActivation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.BlissfulSpear1 or (uint)AID.BlissfulSpear2 or (uint)AID.BlissfulSpear3 or (uint)AID.BlissfulSpear4)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }
}

// This puddle grows when players take damage.  We make the aoe indicator larger in order to try and outrun it.
// Could be worth figuring out the logic behind them growing, [if any player slot in voidcircle and status == bleeding : increase voidcircle radius] maybe.
// but for first pass we just want something to show so that pc runs out of voidzone.
sealed class CircleZone(BossModule module) : Components.Voidzone(module, 10, m => m.Enemies((uint)OID.VoidZone));

sealed class T02LakshmiStates : StateMachineBuilder
{
    public T02LakshmiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            // Dreaming Kshatriya
            .ActivateOnEnter<EmanationTrash>()
            .ActivateOnEnter<TailSlap>()
            .ActivateOnEnter<InnerDemonsGaze>()
            // Lakshmi
            .ActivateOnEnter<DivineDenial>()
            .ActivateOnEnter<Stotram1>()
            .ActivateOnEnter<StotramShielding>()
            .ActivateOnEnter<ThePallOfLightStack>()
            .ActivateOnEnter<ThePullOfLightTB1>()
            .ActivateOnEnter<ThePullOfLightTB2>()
            .ActivateOnEnter<PathOfLightCleave>()
            .ActivateOnEnter<BlissfulBaits>()
            .ActivateOnEnter<VrilOrbs>()
            .ActivateOnEnter<CircleZone>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "wen, Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 263, NameID = 6385)]

public sealed class T02Lakshmi(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsCircle(20f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        if (PrimaryActor.IsTargetable)
        {
            if (PrimaryActor.Position.InCircle(Arena.Center, 20f))
                Arena.ActorInsideBounds(Arena.Center - new WDir(0, 20), PrimaryActor.Rotation, Colors.Enemy);
            else
                Arena.ActorOutsideBounds(Arena.Center - new WDir(0, 20f), PrimaryActor.Rotation, Colors.Enemy);
        }

        Arena.Actors(Enemies((uint)OID.Boss));              // R3.500, x1
        Arena.Actors(Enemies((uint)OID.DreamingKshatriya)); // R1.000, x2
        Arena.Actors(Enemies((uint)OID.Vril), Colors.Object); // orbs that recharge the special action shield ability.
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
            hints.PotentialTargets[i].Priority = 0;
    }
}
