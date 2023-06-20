namespace BossMod.Endwalker.Savage.P12S1Athena
{
    // TODO: generalize (line stack/spread)
    class EngravementOfSouls2Lines : BossComponent
    {
        public int NumCasts { get; private set; }
        private Actor? _lightRay;
        private Actor? _darkRay;
        private BitMask _lightCamp;
        private BitMask _darkCamp;

        private static AOEShapeRect _shape = new(100, 3);

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (InAOE(module, _lightRay, actor) != _darkCamp[slot])
                hints.Add(_darkCamp[slot] ? "Go to dark camp" : "GTFO from dark camp");
            if (InAOE(module, _darkRay, actor) != _lightCamp[slot])
                hints.Add(_lightCamp[slot] ? "Go to light camp" : "GTFO from light camp");
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return player == _lightRay || player == _darkRay ? PlayerPriority.Interesting : PlayerPriority.Normal;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            DrawOutline(module, _lightRay, _darkCamp[pcSlot]);
            DrawOutline(module, _darkRay, _lightCamp[pcSlot]);
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.UmbralTilt:
                case SID.UmbralbrightSoul:
                    _lightCamp.Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
                case SID.AstralTilt:
                case SID.AstralbrightSoul:
                    _darkCamp.Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
                case SID.UmbralstrongSoul:
                    _lightRay = actor;
                    _darkCamp.Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
                case SID.AstralstrongSoul:
                    _darkRay = actor;
                    _lightCamp.Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.UmbralImpact or AID.AstralImpact)
                ++NumCasts;
        }

        private bool InAOE(BossModule module, Actor? target, Actor actor) => target != null && _shape.Check(actor.Position, module.PrimaryActor.Position, Angle.FromDirection(target.Position - module.PrimaryActor.Position));

        private void DrawOutline(BossModule module, Actor? target, bool safe)
        {
            if (target != null)
                _shape.Outline(module.Arena, module.PrimaryActor.Position, Angle.FromDirection(target.Position - module.PrimaryActor.Position), safe ? ArenaColor.Safe : ArenaColor.Danger);
        }
    }

    class EngrameventOfSouls2Spread : Components.GenericStackSpread
    {
        public int NumCasts { get; private set; }

        public EngrameventOfSouls2Spread() : base(true, false) { }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            var radius = (SID)status.ID switch
            {
                SID.UmbralbrightSoul or SID.AstralbrightSoul => 3,
                SID.HeavensflameSoul => 6,
                _ => 0
            };
            if (radius != 0)
                Spreads.Add(new(actor, radius)); // TODO: activation
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            var radius = (AID)spell.Action.ID switch
            {
                AID.UmbralGlow or AID.AstralGlow => 3,
                AID.TheosHoly => 6,
                _ => 0
            };
            if (radius != 0)
            {
                Spreads.RemoveAll(s => s.Radius == radius);
                ++NumCasts;
            }
        }
    }
}
