using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    // TODO: hints?..
    class P4Hatebound : BossComponent
    {
        public enum Color { None, Red, Blue }

        public bool ColorsAssigned { get; private set; }
        private List<(Actor orb, Color color, bool exploded)> _orbs = new(); // 'red' is actually 'yellow orb'
        private Color[] _playerColors = new Color[PartyState.MaxPartySize];

        public bool ColorReady(Color c) => _orbs.Any(o => o.color == c && OrbReady(o.orb));
        public bool YellowReady => ColorReady(Color.Red);
        public bool BlueReady => ColorReady(Color.Blue);

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_playerColors[slot] != Color.None)
            {
                hints.Add($"Color: {_playerColors[slot]}", false);
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var o in _orbs.Where(o => !o.exploded))
            {
                arena.Actor(o.orb, ArenaColor.Object, true);
                if (OrbReady(o.orb))
                    arena.AddCircle(o.orb.Position, 6, _playerColors[pcSlot] == Color.Red ? ArenaColor.Safe : ArenaColor.Danger);
            }
        }

        public override void OnActorCreated(BossModule module, Actor actor)
        {
            Color color = (OID)actor.OID switch
            {
                OID.AzurePrice => Color.Blue,
                OID.GildedPrice => Color.Red,
                _ => Color.None
            };
            if (color != Color.None)
                _orbs.Add((actor, color, false));
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            var color = (SID)status.ID switch
            {
                SID.Clawbound => Color.Red,
                SID.Fangbound => Color.Blue,
                _ => Color.None
            };
            if (color != Color.None && module.Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            {
                ColorsAssigned = true;
                _playerColors[slot] = color;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.FlareStar or AID.FlareNova or AID.FlareNovaFail && _orbs.FindIndex(o => o.orb == caster) is var index && index >= 0)
                _orbs.AsSpan()[index].exploded = true;
        }

        private bool OrbReady(Actor orb) => orb.HitboxRadius > 1.501f; // TODO: verify...
    }

    class P4MirageDive : Components.CastCounter
    {
        private List<int> _targets = new();
        private BitMask _forbidden;
        private BitMask _baiters;

        private static float _radius = 4;

        public P4MirageDive() : base(ActionID.MakeSpell(AID.MirageDiveAOE)) { }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_baiters[slot])
            {
                // note: not showing this hint, since typically pc will wait until someone swaps the color
                //if (_forbidden[slot])
                //    hints.Add("Pass the tether!");
                if (!_forbidden[slot] && module.Raid.WithoutSlot().InRadiusExcluding(actor, _radius).Any())
                    hints.Add("GTFO from raid!");
            }
            else if (module.Raid.WithSlot(true).IncludedInMask(_baiters).ExcludedFromMask(_forbidden).InRadius(actor.Position, _radius).Any())
            {
                hints.Add("GTFO from baiters!");
            }
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return _baiters[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Normal;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            bool pcCanSwap = !_forbidden[pcSlot] && !_baiters[pcSlot];
            foreach (var (slot, player) in module.Raid.WithSlot(true).IncludedInMask(_baiters))
            {
                bool canSwap = pcCanSwap && _forbidden[slot];
                arena.AddCircle(player.Position, _radius, canSwap ? ArenaColor.Safe : ArenaColor.Danger);
            }
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.Clawbound)
                _baiters.Set(module.Raid.FindSlot(actor.InstanceID));
        }

        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.Clawbound)
                _baiters.Clear(module.Raid.FindSlot(actor.InstanceID));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            if (spell.Action == WatchedAction)
            {
                _targets.Add(module.Raid.FindSlot(spell.MainTargetID));
                _forbidden.Reset();
                foreach (int i in _targets.TakeLast(4))
                    _forbidden.Set(i);
            }
        }
    }
}
