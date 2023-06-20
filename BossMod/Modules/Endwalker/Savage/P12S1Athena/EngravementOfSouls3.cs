namespace BossMod.Endwalker.Savage.P12S1Athena
{
    class EngravementOfSouls3Shock : Components.CastTowers
    {
        private BitMask _towers;
        private BitMask _plus;
        private BitMask _cross;

        public EngravementOfSouls3Shock() : base(ActionID.MakeSpell(AID.Shock), 3) { }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.UmbralbrightSoul:
                case SID.AstralbrightSoul:
                    _towers.Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
                case SID.QuarteredSoul:
                    _plus.Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
                case SID.XMarkedSoul:
                    _cross.Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            
            if (spell.Action == WatchedAction)
            {
                BitMask forbidden = spell.Location.Z switch
                {
                    < 90 => ~_plus, // TODO: technically cross and plus could switch places
                    > 110 => ~_cross,
                    _ => ~_towers // TODO: assign specific towers based on debuffs
                };
                Towers.Add(new(spell.LocXZ, Radius, forbiddenSoakers: forbidden));
            }
        }
    }

    class EngravementOfSouls3Spread : Components.UniformStackSpread
    {
        private EngravementOfSoulsTethers? _tethers;
        private EngravementOfSoulsTethers.TetherType _soakers;

        public EngravementOfSouls3Spread() : base(0, 3, alwaysShowSpreads: true, raidwideOnResolve: false) { }

        public override void Init(BossModule module)
        {
            _tethers = module.FindComponent<EngravementOfSoulsTethers>();
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            if (IsSpreadTarget(pc))
                return _tethers?.States[playerSlot].Tether == _soakers ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
            else
                return base.CalcPriority(module, pcSlot, pc, playerSlot, player, ref customColor);
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            var soakers = (SID)status.ID switch
            {
                SID.UmbralbrightSoul => EngravementOfSoulsTethers.TetherType.Dark,
                SID.AstralbrightSoul => EngravementOfSoulsTethers.TetherType.Light,
                _ => EngravementOfSoulsTethers.TetherType.None
            };
            if (soakers != EngravementOfSoulsTethers.TetherType.None)
            {
                _soakers = soakers;
                AddSpread(actor); // TODO: activation
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.UmbralGlow or AID.AstralGlow)
                Spreads.Clear();
        }
    }

    class TheosCross : Components.SelfTargetedAOEs
    {
        public TheosCross() : base(ActionID.MakeSpell(AID.TheosCross), new AOEShapeCross(40, 3)) { }
    }

    class TheosSaltire : Components.SelfTargetedAOEs
    {
        public TheosSaltire() : base(ActionID.MakeSpell(AID.TheosSaltire), new AOEShapeCross(40, 3)) { }
    }
}
