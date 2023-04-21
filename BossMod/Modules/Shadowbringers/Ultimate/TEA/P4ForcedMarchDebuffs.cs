namespace BossMod.Shadowbringers.Ultimate.TEA
{
    abstract class P4ForcedMarchDebuffs : BossComponent
    {
        public enum Debuff { None, LightBeacon, LightFollow, DarkBeacon, DarkFollow }

        public bool Done { get; private set; }
        protected Debuff[] Debuffs = new Debuff[PartyState.MaxPartySize];
        private Actor? _lightBeacon;
        private Actor? _darkBeacon;

        private static float _forcedMarchDistance = 20; // TODO: verify
        private static float _minLightDistance => _forcedMarchDistance + 2;
        private static float _maxDarkDistance => 28 - _forcedMarchDistance;

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (Done || Debuffs[slot] == Debuff.None)
                return;

            switch (Debuffs[slot])
            {
                case Debuff.LightFollow:
                    if (_lightBeacon != null && (actor.Position - _lightBeacon.Position).LengthSq() < _minLightDistance * _minLightDistance)
                        hints.Add("Move away from light beacon!");
                    break;
                case Debuff.DarkFollow:
                    if (_darkBeacon != null)
                    {
                        if (!module.Bounds.Contains(Components.Knockback.AwayFromSource(actor.Position, _darkBeacon.Position, _forcedMarchDistance)))
                            hints.Add("Aim away from wall!");
                        if ((actor.Position - _darkBeacon.Position).LengthSq() > _maxDarkDistance * _maxDarkDistance)
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
                    if (_lightBeacon != null)
                    {
                        var pos = (pc.Position - _lightBeacon.Position).LengthSq() <= _forcedMarchDistance * _forcedMarchDistance ? _lightBeacon.Position : pc.Position + _forcedMarchDistance * (_lightBeacon.Position - pc.Position).Normalized();
                        Components.Knockback.DrawKnockback(pc, pos, arena);
                    }
                    break;
                case Debuff.DarkFollow:
                    if (_darkBeacon != null)
                    {
                        var pos = Components.Knockback.AwayFromSource(pc.Position, _darkBeacon.Position, _forcedMarchDistance);
                        Components.Knockback.DrawKnockback(pc, pos, arena);
                    }
                    break;
            }

            arena.AddCircle(module.Bounds.Center + SafeSpotDirection(pcSlot), 1, ArenaColor.Safe);
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.FinalWordContactProhibition:
                    AssignDebuff(module, actor, Debuff.LightFollow);
                    break;
                case SID.FinalWordContactRegulation:
                    AssignDebuff(module, actor, Debuff.LightBeacon);
                    _lightBeacon = actor;
                    break;
                case SID.FinalWordEscapeProhibition:
                    AssignDebuff(module, actor, Debuff.DarkFollow);
                    break;
                case SID.FinalWordEscapeDetection:
                    AssignDebuff(module, actor, Debuff.DarkBeacon);
                    _darkBeacon = actor;
                    break;
                case SID.ContactProhibitionOrdained:
                case SID.ContactRegulationOrdained:
                case SID.EscapeProhibitionOrdained:
                case SID.EscapeDetectionOrdained:
                    Done = true;
                    break;
            }
        }

        private void AssignDebuff(BossModule module, Actor actor, Debuff debuff)
        {
            var slot = module.Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
                Debuffs[slot] = debuff;
        }

        protected abstract WDir SafeSpotDirection(int slot);
    }
}
