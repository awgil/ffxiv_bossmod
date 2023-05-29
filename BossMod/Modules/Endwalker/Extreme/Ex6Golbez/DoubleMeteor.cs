using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.Extreme.Ex6Golbez
{
    class DragonsDescent : Components.Knockback
    {
        private Actor? _source;
        private DateTime _activation;

        public DragonsDescent() : base(ActionID.MakeSpell(AID.DragonsDescent)) { }

        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
        {
            if (_source != null && _source != actor)
                yield return new(_source.Position, 13, _activation);
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.DragonsDescent)
            {
                _source = actor;
                _activation = module.WorldState.CurrentTime.AddSeconds(8.2f);
            }
        }
    }

    class DoubleMeteor : Components.UniformStackSpread
    {
        public DoubleMeteor() : base(0, 15, alwaysShowSpreads: true) { } // TODO: verify falloff

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.DoubleMeteor)
                AddSpread(actor, module.WorldState.CurrentTime.AddSeconds(11.1f));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.DoubleMeteorAOE1 or AID.DoubleMeteorAOE2)
                Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
        }
    }

    class Explosion : BossComponent
    {
        public bool Done { get; private set; }
        private BitMask _forbidden;
        private Actor? _towerTH;
        private Actor? _towerDD;

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var tower = _forbidden[slot] ? null : actor.Class.IsSupport() ? _towerTH : _towerDD;
            if (tower != null)
                hints.Add("Soak the tower!", !actor.Position.InCircle(tower.Position, 4));
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            DrawTower(arena, _towerTH, !_forbidden[pcSlot] && pc.Class.IsSupport());
            DrawTower(arena, _towerDD, !_forbidden[pcSlot] && pc.Class.IsDD());
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.ExplosionDouble:
                    _towerTH = caster;
                    break;
                case AID.ExplosionTriple:
                    _towerDD = caster;
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.ExplosionDouble:
                    _towerTH = null;
                    Done = true;
                    break;
                case AID.ExplosionTriple:
                    _towerDD = null;
                    Done = true;
                    break;
            }
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if ((IconID)iconID is IconID.DoubleMeteor or IconID.DragonsDescent)
                _forbidden.Set(module.Raid.FindSlot(actor.InstanceID));
        }

        private void DrawTower(MiniArena arena, Actor? tower, bool safe)
        {
            if (tower != null)
                arena.AddCircle(tower.Position, 4, safe ? ArenaColor.Safe : ArenaColor.Danger, 2);
        }
    }

    class Cauterize : Components.GenericBaitAway
    {
        public Cauterize() : base(ActionID.MakeSpell(AID.Cauterize)) { }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            if (tether.ID == (uint)TetherID.Cauterize && module.WorldState.Actors.Find(tether.Target) is var target && target != null)
            {
                CurrentBaits.Add(new(source, target, new AOEShapeRect(50, 6)));
            }
        }
    }
}
