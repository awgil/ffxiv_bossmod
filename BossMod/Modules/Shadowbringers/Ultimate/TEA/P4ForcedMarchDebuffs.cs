namespace BossMod.Shadowbringers.Ultimate.TEA
{
    abstract class P4ForcedMarchDebuffs : BossComponent
    {
        public enum Debuff { None, LightBeacon, LightFollow, DarkBeacon, DarkFollow }

        public bool Done { get; protected set; }
        protected Debuff[] Debuffs = new Debuff[PartyState.MaxPartySize];
        protected Actor? LightBeacon;
        protected Actor? DarkBeacon;

        private static float _forcedMarchDistance = 20; // TODO: verify
        private static float _minLightDistance => 22; // TODO: verify
        private static float _maxDarkDistance => 5; // TODO: verify

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (Done || Debuffs[slot] == Debuff.None)
                return;

            switch (Debuffs[slot])
            {
                case Debuff.LightFollow:
                    if (LightBeacon != null && (actor.Position - LightBeacon.Position).LengthSq() < _minLightDistance * _minLightDistance)
                        hints.Add("Move away from light beacon!");
                    break;
                case Debuff.DarkFollow:
                    if (DarkBeacon != null)
                    {
                        if (!module.Bounds.Contains(Components.Knockback.AwayFromSource(actor.Position, DarkBeacon.Position, _forcedMarchDistance)))
                            hints.Add("Aim away from wall!");
                        if ((actor.Position - DarkBeacon.Position).LengthSq() > _maxDarkDistance * _maxDarkDistance)
                            hints.Add("Move closer to dark beacon!");
                    }
                    break;
            }

            if (movementHints != null)
                movementHints.Add(actor.Position, module.Bounds.Center + SafeSpotDirection(slot), ArenaColor.Safe);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (Done || Debuffs[pcSlot] == Debuff.None)
                return;

            switch (Debuffs[pcSlot])
            {
                case Debuff.LightFollow:
                    if (LightBeacon != null)
                    {
                        var pos = (pc.Position - LightBeacon.Position).LengthSq() <= _forcedMarchDistance * _forcedMarchDistance ? LightBeacon.Position : pc.Position + _forcedMarchDistance * (LightBeacon.Position - pc.Position).Normalized();
                        Components.Knockback.DrawKnockback(pc, pos, arena);
                    }
                    break;
                case Debuff.DarkFollow:
                    if (DarkBeacon != null)
                    {
                        var pos = Components.Knockback.AwayFromSource(pc.Position, DarkBeacon.Position, _forcedMarchDistance);
                        Components.Knockback.DrawKnockback(pc, pos, arena);
                    }
                    break;
            }

            arena.AddCircle(module.Bounds.Center + SafeSpotDirection(pcSlot), 1, ArenaColor.Safe);
        }

        protected abstract WDir SafeSpotDirection(int slot);
    }
}
