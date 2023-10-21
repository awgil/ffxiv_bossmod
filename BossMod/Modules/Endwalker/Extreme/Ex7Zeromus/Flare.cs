using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Extreme.Ex7Zeromus
{
    class FlareTowers : Components.CastTowers
    {
        public FlareTowers() : base(ActionID.MakeSpell(AID.FlareAOE), 5, 4, 4) { }
    }

    class FlareScald : Components.GenericAOEs
    {
        private List<AOEInstance> _aoes = new();

        private static AOEShapeCircle _shape = new(5);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.FlareAOE:
                    _aoes.Add(new(_shape, caster.Position, default, module.WorldState.CurrentTime.AddSeconds(2.1f)));
                    break;
                case AID.FlareScald:
                case AID.FlareKill:
                    ++NumCasts;
                    break;
            }
        }
    }

    class ProminenceSpine : Components.SelfTargetedAOEs
    {
        public ProminenceSpine() : base(ActionID.MakeSpell(AID.ProminenceSpine), new AOEShapeRect(60, 5)) { }
    }

    class SparklingBrandingFlare : Components.CastStackSpread
    {
        public SparklingBrandingFlare() : base(ActionID.MakeSpell(AID.BrandingFlareAOE), ActionID.MakeSpell(AID.SparkingFlareAOE), 4, 4) { }
    }

    // TODO: generalize to chasing aoe
    class Nox : Components.GenericAOEs
    {
        class Chaser
        {
            public Actor Player;
            public WPos? Pos;
            public int NumCasts;
            public DateTime Activation;

            public Chaser(Actor player)
            {
                Player = player;
            }

            public WPos PredictedPosition()
            {
                if (Pos == null)
                    return default;
                if (NumCasts == 0)
                    return Pos.Value;
                var toPlayer = Player.Position - Pos.Value;
                var dist = toPlayer.Length();
                if (dist < _moveDist)
                    return Player.Position;
                return Pos.Value + toPlayer * _moveDist / dist;
            }
        }

        private List<Chaser> _chasers = new();

        public bool Active => _chasers.Count > 0;

        private static AOEShapeCircle _shape = new(10);
        private static int _maxCasts = 5;
        private static float _moveDist = 5.5f;

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _chasers.Select(c => new AOEInstance(_shape, c.PredictedPosition(), default, c.Activation));

        public override void Update(BossModule module)
        {
            _chasers.RemoveAll(c => (c.Player.IsDestroyed || c.Player.IsDead) && (c.Pos == null || c.NumCasts > 0));
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var c in _chasers)
            {
                if (c.Pos != null)
                    arena.AddLine(c.Pos.Value, c.Player.Position, ArenaColor.Danger);
                //else
                //    _shape.Outline(arena, c.Player.Position);
            }
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.Nox)
                _chasers.Add(new(actor));
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.NoxAOEFirst && _chasers.Where(c => c.Pos == null).MinBy(c => (c.Player.Position - caster.Position).LengthSq()) is var chaser && chaser != null)
            {
                chaser.Pos = caster.Position;
                chaser.Activation = spell.FinishAt;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.NoxAOEFirst)
            {
                Advance(module, caster.Position);
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.NoxAOERest)
            {
                Advance(module, caster.Position);
            }
        }

        private void Advance(BossModule module, WPos pos)
        {
            ++NumCasts;
            var chaser = _chasers.MinBy(c => c.Pos != null ? (c.PredictedPosition() - pos).LengthSq() : float.MaxValue);
            if (chaser == null)
                return;

            if (++chaser.NumCasts < _maxCasts)
            {
                chaser.Pos = pos;
                chaser.Activation = module.WorldState.CurrentTime.AddSeconds(1.6f);
            }
            else
            {
                _chasers.Remove(chaser);
            }
        }
    }
}
