using System.Linq;

namespace BossMod.Shadowbringers.Ultimate.TEA
{
    class P3TemporalStasis : Components.GenericBaitAway
    {
        public enum Mechanic { None, AvoidDamage, StayClose, StayFar }

        public bool Frozen { get; private set; }
        private Mechanic[] _playerMechanics = new Mechanic[PartyState.MaxPartySize];

        private static AOEShapeCone _shapeBJ = new(100, 45.Degrees()); // TODO: verify angle
        private static AOEShapeCone _shapeCC = new(30, 45.Degrees()); // TODO: verify angle

        public P3TemporalStasis() : base(ActionID.MakeSpell(AID.FlarethrowerP3)) { }

        public override void Update(BossModule module)
        {
            CurrentBaits.Clear();
            if (BJ(module) is var bj && bj != null)
                CurrentBaits.AddRange(module.Raid.WithoutSlot().SortedByRange(bj.Position).Take(2).Select(t => new Bait(bj, t, _shapeBJ)));
            if (CC(module) is var cc && cc != null)
                CurrentBaits.AddRange(module.Raid.WithoutSlot().SortedByRange(cc.Position).Take(3).Select(t => new Bait(cc, t, _shapeCC)));
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            base.AddHints(module, slot, actor, hints, movementHints);

            switch (_playerMechanics[slot])
            {
                case Mechanic.StayClose:
                    if (FindPartner(module, slot) is var partner1 && partner1 != null && (partner1.Position - actor.Position).LengthSq() > 5 * 5)
                        hints.Add("Stay closer to partner!");
                    break;
                case Mechanic.StayFar:
                    if (FindPartner(module, slot) is var partner2 && partner2 != null && (partner2.Position - actor.Position).LengthSq() < 30 * 30)
                        hints.Add("Stay farther from partner!");
                    break;
            }

            if (movementHints != null)
                movementHints.Add(actor.Position, SafeSpot(module, slot, actor), ArenaColor.Safe);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            base.DrawArenaForeground(module, pcSlot, pc, arena);

            switch (_playerMechanics[pcSlot])
            {
                case Mechanic.StayClose:
                    if (FindPartner(module, pcSlot) is var partner1 && partner1 != null)
                        arena.AddLine(pc.Position, partner1.Position, (partner1.Position - pc.Position).LengthSq() > 5 * 5 ? ArenaColor.Danger : ArenaColor.Safe);
                    break;
                case Mechanic.StayFar:
                    if (FindPartner(module, pcSlot) is var partner2 && partner2 != null)
                        arena.AddLine(pc.Position, partner2.Position, (partner2.Position - pc.Position).LengthSq() < 30 * 30 ? ArenaColor.Danger : ArenaColor.Safe);
                    break;
            }

            arena.Actor(BJ(module), ArenaColor.Enemy, true);
            arena.Actor(CC(module), ArenaColor.Enemy, true);
            arena.AddCircle(SafeSpot(module, pcSlot, pc), 1, ArenaColor.Safe);
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.AggravatedAssault:
                    AssignMechanic(module, actor, Mechanic.AvoidDamage);
                    ForbiddenPlayers.Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
                case SID.HouseArrest:
                    AssignMechanic(module, actor, Mechanic.StayClose);
                    break;
                case SID.RestrainingOrder:
                    AssignMechanic(module, actor, Mechanic.StayFar);
                    break;
                case SID.TemporalDisplacement:
                    Frozen = true;
                    break;
            }
        }

        private Actor? FindPartner(BossModule module, int slot)
        {
            var partnerSlot = -1;
            for (int i = 0; i < _playerMechanics.Length; ++i)
                if (i != slot && _playerMechanics[i] == _playerMechanics[slot])
                    partnerSlot = i;
            return module.Raid[partnerSlot];
        }

        private WPos SafeSpot(BossModule module, int slot, Actor actor)
        {
            // using LPDU assignments:
            // - 'near' baiting N (th) / S (dd) of the eastern actor (BJ/CC doesn't matter)
            // - 'no debuff' baiting N (th) / S (dd) of the western actor (BJ/CC doesn't matter)
            // - 'far' E (th) / W (dd), whoever is closer to CC baits third aoe outside
            // - 'avoid' staying E/W, closer to BJ
            // BJ/CC are located at center +/- (6, 0)
            var bjLeft = BJ(module)?.Position.X < module.Bounds.Center.X;
            return module.Bounds.Center + _playerMechanics[slot] switch
            {
                Mechanic.AvoidDamage => new WDir(bjLeft ? -20 : +20, 0),
                Mechanic.StayClose => new WDir(6, actor.Class.IsSupport() ? -2 : +2),
                Mechanic.StayFar => new WDir(actor.Class.IsSupport() ? (bjLeft ? 15 : 20) : (bjLeft ? -20 : -15), 0),
                _ => new WDir(-6, actor.Class.IsSupport() ? -2 : +2)
            };
        }

        private void AssignMechanic(BossModule module, Actor actor, Mechanic mechanic)
        {
            var slot = module.Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
                _playerMechanics[slot] = mechanic;
        }

        private Actor? BJ(BossModule module) => ((TEA)module).BruteJustice();
        private Actor? CC(BossModule module) => ((TEA)module).CruiseChaser();
    }
}
