using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE14VigilForLost
{
    public enum OID : uint
    {
        Boss = 0x2DBD, // R8.000, x1
        Helper = 0x233C, // R0.500, x11
        MagitekBit = 0x2F58, // R0.660, spawn during fight
    };

    public enum AID : uint
    {
        AutoAttack = 6499, // Boss->player, no cast, single-target
        LightLeap = 20146, // Boss->location, 4.0s cast, range 10 circle aoe
        AreaBombardment = 20147, // Boss->self, 5.0s cast, single-target, visual (main mechanic start)
        ChemicalMissile = 20148, // Helper->self, 4.0s cast, range 12 circle aoe
        TailMissile = 20149, // Boss->self, 5.0s cast, single-target, visual (big aoe)
        TailMissileAOE = 20150, // Helper->self, 8.0s cast, range 30 circle aoe
        Shockwave = 20151, // Boss->self, 6.0s cast, range 16 circle aoe
        ExplosiveFlare = 20152, // Helper->self, 3.0s cast, range 10 circle aoe
        CripplingBlow = 20153, // Boss->player, 4.0s cast, single-target, tankbuster
        PlasmaField = 20154, // Boss->self, 4.0s cast, range 60 circle, raidwide
        Explosion = 21266, // Helper->self, 7.0s cast, range 6 circle tower
        MassiveExplosion = 21267, // Helper->self, no cast, range 60 circle, failed tower
        MagitekRay = 21268, // MagitekBit->self, 2.5s cast, range 50 width 4 rect
    };

    class LightLeap : Components.LocationTargetedAOEs
    {
        public LightLeap() : base(ActionID.MakeSpell(AID.LightLeap), 10) { }
    }

    class ChemicalMissile : Components.SelfTargetedAOEs
    {
        public ChemicalMissile() : base(ActionID.MakeSpell(AID.ChemicalMissile), new AOEShapeCircle(12)) { }
    }

    class TailMissile : Components.SelfTargetedAOEs
    {
        public TailMissile() : base(ActionID.MakeSpell(AID.TailMissileAOE), new AOEShapeCircle(30)) { }
    }

    class Shockwave : Components.SelfTargetedAOEs
    {
        public Shockwave() : base(ActionID.MakeSpell(AID.Shockwave), new AOEShapeCircle(16)) { }
    }

    class ExplosiveFlare : Components.SelfTargetedAOEs
    {
        public ExplosiveFlare() : base(ActionID.MakeSpell(AID.ExplosiveFlare), new AOEShapeCircle(10)) { }
    }

    class CripplingBlow : Components.SingleTargetCast
    {
        public CripplingBlow() : base(ActionID.MakeSpell(AID.CripplingBlow)) { }
    }

    class PlasmaField : Components.RaidwideCast
    {
        public PlasmaField() : base(ActionID.MakeSpell(AID.PlasmaField)) { }
    }

    // TODO: generalize towers
    class Towers : BossComponent
    {
        private List<Actor> _towers = new();
        private static float _radius = 6;

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_towers.Count > 0 && !_towers.Any(t => t.Position.InCircle(actor.Position, _radius)))
                hints.Add("Soak the tower!", true);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var t in _towers)
                arena.AddCircle(t.Position, _radius, ArenaColor.Safe, 2);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.Explosion)
                _towers.Add(caster);
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.Explosion)
                _towers.Remove(caster);
        }
    }

    class MagitekRay : Components.SelfTargetedAOEs
    {
        public MagitekRay() : base(ActionID.MakeSpell(AID.MagitekRay), new AOEShapeRect(50, 2)) { }
    }

    class CE14VigilForLostStates : StateMachineBuilder
    {
        public CE14VigilForLostStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<LightLeap>()
                .ActivateOnEnter<ChemicalMissile>()
                .ActivateOnEnter<TailMissile>()
                .ActivateOnEnter<Shockwave>()
                .ActivateOnEnter<ExplosiveFlare>()
                .ActivateOnEnter<CripplingBlow>()
                .ActivateOnEnter<PlasmaField>()
                .ActivateOnEnter<Towers>()
                .ActivateOnEnter<MagitekRay>();
        }
    }

    public class CE14VigilForLost : BossModule
    {
        public CE14VigilForLost(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(451, 830), 30)) { }
    }
}
