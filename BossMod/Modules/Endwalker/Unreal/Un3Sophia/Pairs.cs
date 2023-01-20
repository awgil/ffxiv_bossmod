using System;
using System.Linq;

namespace BossMod.Endwalker.Unreal.Un3Sophia
{
    // TODO: there doesn't seem to be any event if mechanic is resolved correctly?..
    class Pairs : BossComponent
    {
        private BitMask _players1;
        private BitMask _players2;
        private DateTime _activation;

        private static float _radius = 4; // TODO: verify

        public bool Active => (_players1 | _players2).Any();

        public override void Update(BossModule module)
        {
            if (module.WorldState.CurrentTime > _activation && Active)
            {
                _players1.Reset();
                _players2.Reset();
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            bool atRisk = _players1[slot] ? AtRisk(module, actor, _players1, _players2) : _players2[slot] ? AtRisk(module, actor, _players2, _players1) : false;
            if (atRisk)
                hints.Add("Stack with opposite color!");
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var p in module.Raid.WithSlot().IncludedInMask(_players1).Exclude(pc))
                arena.AddCircle(p.Item2.Position, _radius, _players1[pcSlot] ? ArenaColor.Danger : ArenaColor.Safe);
            foreach (var p in module.Raid.WithSlot().IncludedInMask(_players2).Exclude(pc))
                arena.AddCircle(p.Item2.Position, _radius, _players2[pcSlot] ? ArenaColor.Danger : ArenaColor.Safe);
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            switch ((IconID)iconID)
            {
                case IconID.Pairs1:
                    _players1.Set(module.Raid.FindSlot(actor.InstanceID));
                    _activation = module.WorldState.CurrentTime.AddSeconds(5); // TODO: verify
                    break;
                case IconID.Pairs2:
                    _players2.Set(module.Raid.FindSlot(actor.InstanceID));
                    _activation = module.WorldState.CurrentTime.AddSeconds(5); // TODO: verify
                    break;
            }
        }

        private bool AtRisk(BossModule module, Actor actor, BitMask same, BitMask opposite)
        {
            return module.Raid.WithSlot().IncludedInMask(opposite).InRadius(actor.Position, _radius).Any() || !module.Raid.WithSlot().IncludedInMask(same).InRadiusExcluding(actor, _radius).Any();
        }
    }
}
