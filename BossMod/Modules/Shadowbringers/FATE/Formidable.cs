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
    DynamicSensoryJammer = 17407 // Boss->self, 3.0s cast, range 70 circle
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

class Spincrush(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spincrush, new AOEShapeCone(15f, 60f.Degrees()));
class FireShot(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FireShot, 7f);

class FiresOfMtGulg(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeDonut _shape = new(10f, 50f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FiresOfMtGulg)
        {
            _aoe = [new(_shape, spell.LocXZ, default, Module.CastFinishAt(spell))];
            NumCasts = 0;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.FiresOfMtGulg or (uint)AID.FiresOfMtGulgRepeat)
        {
            if (_aoe.Length != 0)
            {
                ref var aoe = ref _aoe[0];
                aoe.Activation = WorldState.FutureTime(3.1d);
            }
            if (++NumCasts >= 7)
            {
                _aoe = [];
            }
        }
    }
}

// note: raidwide cast is followed by 7 aoes every ~2.7s
class BarrageFire(BossModule module) : Components.RaidwideCast(module, (uint)AID.BarrageFire, "Raidwide + 7 repeats after");

// note: it could have been a simple StackWithCastTargets, however sometimes there is no cast - i assume it happens because actor spawns right before starting a cast, and sometimes due to timings cast-start is missed by the game
// because of that, we just use icons & cast events
// i've also seen player getting rez, immediately getting stack later than others, but then caster gets destroyed without finishing the cast
class DrillShot(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.DrillShot, 6)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.DrillShot)
            AddStack(actor, WorldState.FutureTime(5d));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) { }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.DrillShot)
        {
            var count = Stacks.Count;
            var id = spell.MainTargetID;
            var stacks = CollectionsMarshal.AsSpan(Stacks);
            for (var i = 0; i < count; ++i)
            {
                ref var stack = ref stacks[i];
                if (stack.Target.InstanceID == id)
                {
                    Stacks.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

class ExplosionMissile(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _activeMissiles = [];

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var m in _activeMissiles)
        {
            Arena.Actor(m, Colors.Object, true);
            Arena.ZoneCircleOutline(m.Position, 6f);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.DwarvenDynamite)
            _activeMissiles.Add(actor);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.DwarvenDynamite)
            _activeMissiles.Remove(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ExplosionMissile)
            _activeMissiles.Remove(caster);
    }
}

class ExplosionGrenade(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ExplosionGrenade, 12f);

class DwarvenDischarge(BossModule module, AOEShape shape, uint oid, uint aid, double delay) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == oid)
            _aoes.Add(new(shape, actor.Position.Quantized(), default, WorldState.FutureTime(delay), actorID: actor.InstanceID));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == oid)
            RemoveAOE(actor.InstanceID);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == aid)
            RemoveAOE(caster.InstanceID);
    }

    private void RemoveAOE(ulong instanceID)
    {
        var count = _aoes.Count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            if (aoe.ActorID == instanceID)
            {
                _aoes.RemoveAt(i);
                return;
            }
        }
    }
}

class DwarvenDischargeDonut(BossModule module) : DwarvenDischarge(module, new AOEShapeDonut(9f, 60f), (uint)OID.DwarvenChargeDonut, (uint)AID.DwarvenDischargeDonut, 9.3d);
class DwarvenDischargeCircle(BossModule module) : DwarvenDischarge(module, new AOEShapeCircle(8f), (uint)OID.DwarvenChargeCircle, (uint)AID.DwarvenDischargeCircle, 8.1d);

class AutomatonEscort(BossModule module) : Components.Adds(module, (uint)OID.AutomatonEscort);
class SteamDome(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.SteamDome, 15f);

class DynamicSensoryJammer(BossModule module) : Components.StayMove(module, 3f)
{
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.ExtremeCaution && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = new(Requirement.Stay, status.ExpireAt);
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.ExtremeCaution && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = default;
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

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Fate, GroupID = 1464, NameID = 8822)]
public class Formidable(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
