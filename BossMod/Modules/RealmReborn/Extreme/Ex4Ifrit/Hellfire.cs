using System;

namespace BossMod.RealmReborn.Extreme.Ex4Ifrit
{
    class Hellfire : BossComponent
    {
        public bool Invincible { get; private set; }
        public bool PlumesImminent { get; private set; }
        private int _numPlumes;
        private DateTime _expectedRaidwide;

        public Angle NextSafeSpot => _numPlumes switch
        {
            0 => 150.Degrees(),
            1 => 110.Degrees(),
            _ => 70.Degrees()
        };

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            if (Invincible)
                hints.PotentialTargets.RemoveAll(e => e.Actor == module.PrimaryActor);
            if (_expectedRaidwide != new DateTime())
                hints.PredictedDamage.Add((module.Raid.WithSlot().Mask(), _expectedRaidwide));
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (PlumesImminent)
                arena.AddCircle(module.Bounds.Center + 15 * NextSafeSpot.ToDirection(), 2, ArenaColor.Safe);
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if (actor == module.PrimaryActor && (SID)status.ID == SID.Invincibility)
            {
                Invincible = true;
                _expectedRaidwide = module.WorldState.CurrentTime.AddSeconds(5);
            }
        }

        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            if (actor == module.PrimaryActor && (SID)status.ID == SID.Invincibility)
            {
                Invincible = false;
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.Hellfire:
                    _expectedRaidwide = spell.FinishAt;
                    PlumesImminent = true;
                    break;
                case AID.RadiantPlume:
                    PlumesImminent = false;
                    ++_numPlumes;
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.Hellfire)
            {
                _expectedRaidwide = new();
            }
        }
    }
}
