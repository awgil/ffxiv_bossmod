namespace BossMod.Shadowbringers.FATE.Formidable;

public enum OID : uint
{
    Boss = 0x294D, // R9.000, x1
    //MilitiaSpear = 0x2AE2, // R0.500, spawn during fight
    FireShotHelper = 0x2BAD, // R0.500, spawn during fight
    PullHelper = 0x2A9D, // R0.500, spawn during fight
    DrillShotHelper = 0x2BAE, // R0.500, spawn during fight
    GiantGrenade = 0x2A9B, // R0.800, spawn during fight (expanding grenade)
    DwarvenDynamite = 0x2BD5, // R1.300, spawn during fight (missile)
    ExpandHelper = 0x2BD8, // R0.500, spawn during fight
    DwarvenChargeDonut = 0x2A9C, // R2.500, spawn during fight
    DwarvenChargeCircle = 0x2BDC, // R1.500, spawn during fight
    AutomatonEscort = 0x2A74, // R3.000, spawn during fight
    //_Gen_Actor1eadec = 0x1EADEC, // R0.500, EventObj type, spawn during fight
    //_Gen_Actor1eadee = 0x1EADEE, // R0.500, EventObj type, spawn during fight
    //_Gen_Actor1eaded = 0x1EADED, // R0.500, EventObj type, spawn during fight
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/AutomatonEscort->player, no cast, single-target
    Spincrush = 17408, // Boss->self, 3.0s cast, range 15 120-degree cone
    FireShot = 17397, // FireShotHelper->location, 5.0s cast, range 7 circle puddle
    FiresOfMtGulg = 17395, // Boss->self, 4.0s cast, range 10-20 donut
    FiresOfMtGulgPull = 17396, // PullHelper->self, no cast, range 10-50 donut, attract 30
    FiresOfMtGulgRepeat = 18002, // Boss->self, no cast, range 10-20 donut
    BarrageFire = 17393, // Boss->self, 5.0s cast, range 40 circle
    BarrageFireRepeat = 18001, // Boss->self, no cast, range 40 circle
    DrillShot = 17401, // DrillShotHelper->players, 5.0s cast, range 6 circle
    DwarvenDeluge = 17412, // Boss->self, 3.0s cast, single-target, visual
    ExplosionMissile = 18003, // DwarvenDynamite->self, no cast, range 6 circle
    ExpandGrenadeRadius = 18006, // ExpandHelper->self, no cast, range 60 circle (applies Altered States with extra 0x50 to grenades, increasing their aoe radius by 8)
    ExplosionGrenade = 17411, // GiantGrenade->self, 12.0s cast, range 4+8 circle (expanded due to altered states)
    Shock = 17402, // Boss->self, 5.0s cast, single-target, visual (donut/circles)
    DwarvenDischargeDonut = 17404, // DwarvenChargeDonut->self, 3.5s cast, range 9-60 donut
    DwarvenDischargeCircle = 17405, // DwarvenChargeCircle->self, 3.0s cast, range 8 circle
    SteamDome = 17394, // Boss->self, 3.0s cast, range 30 circle knockback 15
    DynamicSensoryJammer = 17407, // Boss->self, 3.0s cast, range 70 circle
}

public enum IconID : uint
{
    DrillShot = 62, // player
}
public enum SID : uint
{
    AlteredStates = 1387, // ExpandHelper-->GiantGrenade
    ExtremeCaution = 1269, // Boss->players
}

class Spincrush(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Spincrush), new AOEShapeCone(15, 60.Degrees()));
class FireShot(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.FireShot), 7);

class FiresOfMtGulg(BossModule module) : Components.GenericAOEs(module)
{
    private Actor? _caster;
    private DateTime _activation;
    private static readonly AOEShapeDonut _shape = new(10, 50);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_caster != null)
            yield return new(_shape, _caster.Position, default, _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FiresOfMtGulg)
        {
            _caster = caster;
            _activation = spell.NPCFinishAt;
            NumCasts = 0;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FiresOfMtGulg or AID.FiresOfMtGulgRepeat)
        {
            _activation = WorldState.FutureTime(3.1f);
            if (++NumCasts >= 7)
                _caster = null;
        }
    }
}

// note: raidwide cast is followed by 7 aoes every ~2.7s
class BarrageFire(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.BarrageFire), "Raidwide + 7 repeats after");

// note: it could have been a simple StackWithCastTargets, however sometimes there is no cast - i assume it happens because actor spawns right before starting a cast, and sometimes due to timings cast-start is missed by the game
// because of that, we just use icons & cast events
// i've also seen player getting rez, immediately getting stack later than others, but then caster gets destroyed without finishing the cast
class DrillShot(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.DrillShot), 6)
{
    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.DrillShot)
            AddStack(actor, WorldState.FutureTime(5.0f));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) { }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DrillShot)
            Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
    }
}

class ExplosionMissile(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _activeMissiles = [];

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var m in _activeMissiles)
        {
            Arena.Actor(m, ArenaColor.Object, true);
            Arena.AddCircle(m.Position, 6, ArenaColor.Danger);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.DwarvenDynamite)
            _activeMissiles.Add(actor);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.DwarvenDynamite)
            _activeMissiles.Remove(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ExplosionMissile)
            _activeMissiles.Remove(caster);
    }
}

class ExplosionGrenade(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ExplosionGrenade), new AOEShapeCircle(12));

class DwarvenDischarge(BossModule module, AOEShape shape, OID oid, AID aid, float delay) : Components.GenericAOEs(module)
{
    private readonly AOEShape _shape = shape;
    private readonly OID _oid = oid;
    private readonly AID _aid = aid;
    private readonly float _delay = delay;
    private readonly List<(Actor caster, DateTime activation)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (caster, activation) in _casters)
            yield return new(_shape, caster.Position, default, caster.CastInfo?.NPCFinishAt ?? activation);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == _oid)
            _casters.Add((actor, WorldState.FutureTime(_delay)));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == _oid)
            _casters.RemoveAll(c => c.caster == actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == _aid)
            _casters.RemoveAll(c => c.caster == caster);
    }
}
class DwarvenDischargeDonut(BossModule module) : DwarvenDischarge(module, new AOEShapeDonut(9, 60), OID.DwarvenChargeDonut, AID.DwarvenDischargeDonut, 9.3f);
class DwarvenDischargeCircle(BossModule module) : DwarvenDischarge(module, new AOEShapeCircle(8), OID.DwarvenChargeCircle, AID.DwarvenDischargeCircle, 8.1f);

class AutomatonEscort(BossModule module) : Components.Adds(module, (uint)OID.AutomatonEscort);
class SteamDome(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.SteamDome), 15);

class DynamicSensoryJammer(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.DynamicSensoryJammer), "")
{
    private BitMask _ec;
    public bool Ec { get; private set; }
    private bool casting;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DynamicSensoryJammer)
            casting = true;
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DynamicSensoryJammer)
            casting = false;
    }
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ExtremeCaution)
            _ec.Set(Raid.FindSlot(actor.InstanceID));
    }
    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ExtremeCaution)
            _ec.Clear(Raid.FindSlot(actor.InstanceID));
    }
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_ec[slot] != Ec)
            hints.Add("Extreme Caution on you! STOP everything or get launched into the air!");
    }
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (casting)
            hints.Add("Stop everything including auto attacks or get launched into the air");
    }
}

class FormidableStates : StateMachineBuilder
{
    public FormidableStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Spincrush>()
            .ActivateOnEnter<FireShot>()
            .ActivateOnEnter<FiresOfMtGulg>()
            .ActivateOnEnter<BarrageFire>()
            .ActivateOnEnter<DrillShot>()
            .ActivateOnEnter<ExplosionMissile>()
            .ActivateOnEnter<ExplosionGrenade>()
            .ActivateOnEnter<DwarvenDischargeDonut>()
            .ActivateOnEnter<DwarvenDischargeCircle>()
            .ActivateOnEnter<AutomatonEscort>()
            .ActivateOnEnter<SteamDome>()
            .ActivateOnEnter<DynamicSensoryJammer>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Fate, GroupID = 1464, NameID = 8822)]
public class Formidable(WorldState ws, Actor primary) : SimpleBossModule(ws, primary) { }
