using BossMod.Endwalker.Ultimate.DSW1;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    class P5DeathOfTheHeavensHeavyImpact : HeavyImpact
    {
        public P5DeathOfTheHeavensHeavyImpact() : base(10.5f) { }
    }

    class P5DeathOfTheHeavensGaze : DragonsGaze
    {
        public P5DeathOfTheHeavensGaze() : base(OID.BossP5) { }
    }

    // TODO: make more meaningful somehow
    class P5DeathOfTheHeavensDooms : BossComponent
    {
        public BitMask Dooms;

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (Dooms[slot])
                hints.Add("Doom", false);
        }

        // note: we could also use status, but it appears slightly later
        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.Deathstorm)
                foreach (var t in spell.Targets)
                    Dooms.Set(module.Raid.FindSlot(t.ID));
        }
    }

    class P5DeathOfTheHeavensLightningStorm : Components.UniformStackSpread
    {
        public P5DeathOfTheHeavensLightningStorm() : base(0, 5) { }

        public override void Init(BossModule module)
        {
            AddSpreads(module.Raid.WithoutSlot(true));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.LightningStormAOE)
                Spreads.Clear();
        }
    }

    class P5DeathOfTheHeavensHeavensflame : Components.Knockback
    {
        public bool KnockbackDone { get; private set; }
        private WPos[] _playerAdjustedPositions = new WPos[PartyState.MaxPartySize];
        private int[] _playerIcons = new int[PartyState.MaxPartySize]; // 0 = unassigned, 1 = circle/red, 2 = triangle/green, 3 = cross/blue, 4 = square/purple
        private BitMask _brokenTethers;
        private BitMask _dooms;
        private List<WPos> _cleanses = new();
        private WDir _relSouth; // TODO: this is quite hacky, works for LPDU...

        private static float _knockbackDistance = 16;
        private static float _aoeRadius = 10;
        private static float _tetherBreakDistance = 32; // TODO: verify...

        public P5DeathOfTheHeavensHeavensflame() : base(ActionID.MakeSpell(AID.HeavensflameAOE)) { }

        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
        {
            yield return new(module.Bounds.Center, _knockbackDistance);
        }

        public override void Update(BossModule module)
        {
            foreach (var (slot, player) in module.Raid.WithSlot())
                _playerAdjustedPositions[slot] = !KnockbackDone ? AwayFromSource(player.Position, module.Bounds.Center, _knockbackDistance) : player.Position;
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_playerIcons[slot] == 0)
                return;

            if (!KnockbackDone && IsImmune(slot, module.WorldState.CurrentTime))
                hints.Add("Cancel knockback immunity!");

            var actorAdjPos = _playerAdjustedPositions[slot];
            if (!module.Bounds.Contains(actorAdjPos))
                hints.Add("About to be knocked into wall!");

            if (module.Raid.WithSlot().Exclude(actor).WhereSlot(s => _playerAdjustedPositions[s].InCircle(actorAdjPos, _aoeRadius)).Any())
                hints.Add("Spread!");

            int partner = FindTetheredPartner(slot);
            if (partner >= 0 && _playerAdjustedPositions[partner].InCircle(actorAdjPos, _tetherBreakDistance))
                hints.Add("Aim to break tether!");
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return _playerIcons[pcSlot] == 0 ? PlayerPriority.Irrelevant :
                !_brokenTethers[pcSlot] && _playerIcons[pcSlot] == _playerIcons[playerSlot] ? PlayerPriority.Interesting
                : PlayerPriority.Normal;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_playerIcons[pcSlot] == 0)
                return;

            foreach (var hint in PositionHints(module, pcSlot))
                module.Arena.AddCircle(hint, 1, ArenaColor.Safe);

            int partner = FindTetheredPartner(pcSlot);
            if (partner >= 0)
                arena.AddLine(pc.Position, module.Raid[partner]!.Position, ArenaColor.Safe);

            DrawKnockback(pc, _playerAdjustedPositions[pcSlot], arena);

            foreach (var (slot, _) in module.Raid.WithSlot().Exclude(pc))
                arena.AddCircle(_playerAdjustedPositions[slot], _aoeRadius, ArenaColor.Danger);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.WingsOfSalvationAOE)
            {
                _cleanses.Add(spell.LocXZ);
                _relSouth += spell.LocXZ - module.Bounds.Center;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            if ((AID)spell.Action.ID == AID.FaithUnmoving)
                KnockbackDone = true;
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.Doom)
                _dooms.Set(module.Raid.FindSlot(actor.InstanceID));
        }

        public override void OnUntethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            _brokenTethers.Set(module.Raid.FindSlot(source.InstanceID));
            _brokenTethers.Set(module.Raid.FindSlot(tether.Target));
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            int icon = (IconID)iconID switch
            {
                IconID.HeavensflameCircle => 1,
                IconID.HeavensflameTriangle => 2,
                IconID.HeavensflameCross => 3,
                IconID.HeavensflameSquare => 4,
                _ => 0
            };
            if (icon != 0)
            {
                var slot = module.Raid.FindSlot(actor.InstanceID);
                if (slot >= 0)
                    _playerIcons[slot] = icon;
            }
        }

        private int FindTetheredPartner(int slot)
        {
            if (_brokenTethers[slot])
                return -1;
            if (_playerIcons[slot] == 0)
                return -1;
            for (int i = 0; i < _playerIcons.Length; ++i)
                if (i != slot && _playerIcons[i] == _playerIcons[slot])
                    return i;
            return -1;
        }

        // note: assumes LPDU strat (circles on E/W cleanses, triangles on SE/NW, crosses on N/S, squares on SW/NE)
        // TODO: handle bad cleanse placements somehow? or even deaths?
        private IEnumerable<WPos> PositionHints(BossModule module, int slot)
        {
            var icon = _playerIcons[slot];
            if (icon == 0)
                yield break;

            var angle = Angle.FromDirection(_relSouth) + 135.Degrees() - icon * 45.Degrees();
            var offset = _tetherBreakDistance * 0.5f * angle.ToDirection();
            switch (icon)
            {
                case 1: // circle - show two cleanses closest to E and W
                    yield return ClosestCleanse(module.Bounds.Center + offset);
                    yield return ClosestCleanse(module.Bounds.Center - offset);
                    break;
                case 2: // triangle/square - doom to closest cleanse to SE/SW, otherwise opposite
                case 4:
                    var cleanseSpot = ClosestCleanse(module.Bounds.Center + offset);
                    yield return _dooms[slot] ? cleanseSpot : module.Bounds.Center - (cleanseSpot - module.Bounds.Center);
                    break;
                case 3: // cross - show two spots to N and S
                    yield return module.Bounds.Center + offset;
                    yield return module.Bounds.Center - offset;
                    break;
            }
        }

        private WPos ClosestCleanse(WPos p) => _cleanses.MinBy(c => (c - p).LengthSq());
    }

    class P5DeathOfTheHeavensMeteorCircle : Components.Adds
    {
        public P5DeathOfTheHeavensMeteorCircle() : base((uint)OID.MeteorCircle) { }
    }
}
