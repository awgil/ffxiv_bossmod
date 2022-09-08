using System.Linq;

namespace BossMod.Endwalker.Savage.P7SAgdistis
{
    // TODO: currently we don't expose aggro to components, so we just assume tanks are doing their job...
    class DispersedCondensedAero : BossComponent
    {
        public bool Done { get; private set; }
        private bool _condensed;

        private const float _radiusDispersed = 8;
        private const float _radiusCondensed = 6;

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_condensed)
            {
                if (module.PrimaryActor.TargetID == actor.InstanceID)
                {
                    hints.Add("Stack with other tank or press invuln!", module.Raid.WithoutSlot().InRadiusExcluding(actor, _radiusCondensed).Any(a => a.Role != Role.Tank));
                }
                else
                {
                    var tank = module.WorldState.Actors.Find(module.PrimaryActor.TargetID);
                    if (tank != null && actor.Position.InCircle(tank.Position, _radiusCondensed))
                    {
                        hints.Add("GTFO from tank!");
                    }
                }
            }
            else
            {
                if (actor.Role == Role.Tank)
                {
                    hints.Add("GTFO from raid!", module.Raid.WithoutSlot().InRadiusExcluding(actor, _radiusDispersed).Any());
                }
                else if (module.Raid.WithoutSlot().Where(a => a.Role == Role.Tank).InRadius(actor.Position, _radiusDispersed).Any())
                {
                    hints.Add("GTFO from tanks!");
                }
            }
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return (_condensed ? module.PrimaryActor.TargetID == player.InstanceID : player.Role == Role.Tank) ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_condensed)
            {
                var tank = module.WorldState.Actors.Find(module.PrimaryActor.TargetID);
                if (tank != null)
                    arena.AddCircle(tank.Position, _radiusCondensed, ArenaColor.Danger);
            }
            else
            {
                foreach (var tank in module.Raid.WithoutSlot().Where(a => a.Role == Role.Tank))
                    arena.AddCircle(tank.Position, _radiusDispersed, ArenaColor.Danger);
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.CondensedAero)
                _condensed = true;
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.CondensedAeroAOE or AID.DispersedAeroAOE)
                Done = true;
        }
    }
}
