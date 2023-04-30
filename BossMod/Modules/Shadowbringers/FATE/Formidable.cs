using System;
using System.Collections.Generic;

namespace BossMod.Shadowbringers.FATE.Formidable
{
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
    };

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
        DwarvenDischargeDonut = 17404, // DwarvenChargeDonut->self, 3.5s cast, range 10-60 donut
        DwarvenDischargeCircle = 17405, // DwarvenChargeCircle->self, 3.0s cast, range 8 circle
        SteamDome = 17394, // Boss->self, 3.0s cast, range 30 circle knockback 15
        DynamicSensoryJammer = 17407, // Boss->self, 3.0s cast, range 70 circle
    };

    public enum IconID : uint
    {
        DrillShot = 62, // player
    };

    class Spincrush : Components.SelfTargetedAOEs
    {
        public Spincrush() : base(ActionID.MakeSpell(AID.Spincrush), new AOEShapeCone(15, 60.Degrees())) { }
    }

    class FireShot : Components.LocationTargetedAOEs
    {
        public FireShot() : base(ActionID.MakeSpell(AID.FireShot), 7) { }
    }

    class FiresOfMtGulg : Components.GenericAOEs
    {
        private Actor? _caster;
        private DateTime _activation;
        private static AOEShapeDonut _shape = new(10, 50);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_caster != null)
                yield return new(_shape, _caster.Position, default, _activation);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.FiresOfMtGulg)
            {
                _caster = caster;
                _activation = spell.FinishAt;
                NumCasts = 0;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.FiresOfMtGulg or AID.FiresOfMtGulgRepeat)
            {
                _activation = module.WorldState.CurrentTime.AddSeconds(3.1f);
                if (++NumCasts >= 7)
                    _caster = null;
            }
        }
    }

    // note: raidwide cast is followed by 7 aoes every ~2.7s
    class BarrageFire : Components.RaidwideCast
    {
        public BarrageFire() : base(ActionID.MakeSpell(AID.BarrageFire), "Raidwide + 7 repeats after") { }
    }

    // note: it could have been a simple StackWithCastTargets, however sometimes there is no cast - i assume it happens because actor spawns right before starting a cast, and sometimes due to timings cast-start is missed by the game
    // because of that, we just use icons & cast events
    // i've also seen player getting rez, immediately getting stack later than others, but then caster gets destroyed without finishing the cast
    class DrillShot : Components.StackWithCastTargets
    {
        public DrillShot() : base(ActionID.MakeSpell(AID.DrillShot), 6) { }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.DrillShot)
                AddStack(actor, module.WorldState.CurrentTime.AddSeconds(5.0f));
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell) { }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.DrillShot)
                Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
        }
    }

    class ExplosionMissile : BossComponent
    {
        private List<Actor> _activeMissiles = new();

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var m in _activeMissiles)
            {
                arena.Actor(m, ArenaColor.Object, true);
                arena.AddCircle(m.Position, 6, ArenaColor.Danger);
            }
        }

        public override void OnActorCreated(BossModule module, Actor actor)
        {
            if ((OID)actor.OID == OID.DwarvenDynamite)
                _activeMissiles.Add(actor);
        }

        public override void OnActorDestroyed(BossModule module, Actor actor)
        {
            if ((OID)actor.OID == OID.DwarvenDynamite)
                _activeMissiles.Remove(actor);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.ExplosionMissile)
                _activeMissiles.Remove(caster);
        }
    }

    class ExplosionGrenade : Components.SelfTargetedAOEs
    {
        public ExplosionGrenade() : base(ActionID.MakeSpell(AID.ExplosionGrenade), new AOEShapeCircle(12)) { }
    }

    class DwarvenDischarge : Components.GenericAOEs
    {
        private AOEShape _shape;
        private OID _oid;
        private AID _aid;
        private float _delay;
        private List<(Actor caster, DateTime activation)> _casters = new();
        //private AOEShapeDonut _shape = new(10, 60);

        public DwarvenDischarge(AOEShape shape, OID oid, AID aid, float delay)
        {
            _shape = shape;
            _oid = oid;
            _aid = aid;
            _delay = delay; 
        }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var c in _casters)
                yield return new(_shape, c.caster.Position, default, c.caster.CastInfo?.FinishAt ?? c.activation);
        }

        public override void OnActorCreated(BossModule module, Actor actor)
        {
            if ((OID)actor.OID == _oid)
                _casters.Add((actor, module.WorldState.CurrentTime.AddSeconds(_delay)));
        }

        public override void OnActorDestroyed(BossModule module, Actor actor)
        {
            if ((OID)actor.OID == _oid)
                _casters.RemoveAll(c => c.caster == actor);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == _aid)
                _casters.RemoveAll(c => c.caster == caster);
        }
    }

    class DwarvenDischargeDonut : DwarvenDischarge
    {
        public DwarvenDischargeDonut() : base(new AOEShapeDonut(10, 60), OID.DwarvenChargeDonut, AID.DwarvenDischargeDonut, 9.3f) { }
    }

    class DwarvenDischargeCircle : DwarvenDischarge
    {
        public DwarvenDischargeCircle() : base(new AOEShapeCircle(8), OID.DwarvenChargeCircle, AID.DwarvenDischargeCircle, 8.1f) { }
    }

    class AutomatonEscort : Components.Adds
    {
        public AutomatonEscort() : base((uint)OID.AutomatonEscort) { }
    }

    class SteamDome : Components.KnockbackFromCastTarget
    {
        public SteamDome() : base(ActionID.MakeSpell(AID.SteamDome), 15) { }
    }

    // TODO: stay/move component
    class DynamicSensoryJammer : Components.CastHint
    {
        public DynamicSensoryJammer() : base(ActionID.MakeSpell(AID.DynamicSensoryJammer), "Stay still!") { }
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

    public class Formidable : SimpleBossModule
    {
        public Formidable(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
