using System;
using System.Collections.Generic;
using System.Linq;
using BossMod.Components;

namespace BossMod.Endwalker.Alliance.A34OschonBig

{
    class PitonPullAOE : Components.LocationTargetedAOEs
    {
        public PitonPullAOE() : base(ActionID.MakeSpell(AID.PitonPullAOE), 22) { }
    }
    class WeaponskillAOE : Components.LocationTargetedAOEs
    {
        public WeaponskillAOE() : base(ActionID.MakeSpell(AID.WeaponskillAOE), 6) { }
    }
    class AltitudeAOE : Components.LocationTargetedAOEs
    {
        public AltitudeAOE() : base(ActionID.MakeSpell(AID.AltitudeAOE), 6) { }
    }
    class GreatWhirlwindAOE : Components.LocationTargetedAOEs
    {
        public GreatWhirlwindAOE() : base(ActionID.MakeSpell(AID.GreatWhirlwindAOE), 23) { }
    }
    class DownhillSmallAOE : Components.LocationTargetedAOEs
    {
        public DownhillSmallAOE() : base(ActionID.MakeSpell(AID.DownhillSmallAOE), 6) { }
    }
    class DownhillBigAOE : Components.LocationTargetedAOEs
    {
        public DownhillBigAOE() : base(ActionID.MakeSpell(AID.DownhillBigAOE), 8) { }
    }

    class ArrowTrailAOE : Components.SelfTargetedAOEs
    {
        public ArrowTrailAOE() : base(ActionID.MakeSpell(AID.ArrowTrailAOE), new AOEShapeRect(10f, 5f)) { }
    }

    class ArrowTrailRectAOE : Components.SelfTargetedAOEs
    {
        public ArrowTrailRectAOE() : base(ActionID.MakeSpell(AID.ArrowTrailRectAOE), new AOEShapeRect(40f, 5f)) { }
    }
  
    class WanderingVolley : Components.Knockback
    {
        private List<Source> _sources = new();
        private static AOEShapeCone _shape = new(30, 90.Degrees());

        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor) => _sources;

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.WanderingVolley or AID.WanderingVolley2)
            {
                _sources.Clear();
                // charge always happens through center, so create two sources with origin at center looking orthogonally
                _sources.Add(new(module.Bounds.Center, 12, spell.NPCFinishAt, _shape, spell.Rotation + 90.Degrees(), Kind.DirForward));
                _sources.Add(new(module.Bounds.Center, 12, spell.NPCFinishAt, _shape, spell.Rotation - 90.Degrees(), Kind.DirForward));
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.WanderingVolley or AID.WanderingVolley2)
            {
                _sources.Clear();
                ++NumCasts;
            }
        }
    }
    class WanderingVolleyAOE : Components.GenericAOEs
    {
        private List<AOEInstance> _aoes = new();

        private static AOEShapeRect _shape = new(40, 5, 40);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.WanderingVolley or AID.WanderingVolley2)
                _aoes.Add(new(_shape, caster.Position, spell.Rotation, spell.NPCFinishAt));
        }
        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.WanderingVolley or AID.WanderingVolley2)
            {
                _aoes.Clear();
                ++NumCasts;
            }
        }
    }
}
