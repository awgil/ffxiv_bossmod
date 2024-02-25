using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Criterion.C03AAI.C031Ketuduke
{
    class FlukeGale : Components.Knockback
    {
        public enum Debuff { None, BubbleWeave, FoamyFetters }
        public enum Resolve { None, Stack, Spread }

        public List<Source> Gales = new();
        private SpringCrystalsRect? _crystals;
        private Debuff[] _debuffs = new Debuff[PartyState.MaxPartySize];
        private Resolve _resolution;

        private static AOEShapeRect _shape = new(20, 10);
        private static AOEShapeRect _safeZone = new(5, 5, 5);

        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor) => _debuffs[slot] != Debuff.FoamyFetters ? Gales : Enumerable.Empty<Source>();

        public override void Init(BossModule module)
        {
            _crystals = module.FindComponent<SpringCrystalsRect>();
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            base.AddHints(module, slot, actor, hints, movementHints);
            if (_debuffs[slot] != Debuff.None)
                hints.Add($"Debuff: {(_debuffs[slot] == Debuff.BubbleWeave ? "bubble" : "bind")}", false);
            if (_resolution != Resolve.None && _debuffs[slot] != Debuff.None && Gales.Count == 4 && _crystals != null)
            {
                var finalPos = CalculateMovements(module, slot, actor).LastOrDefault((actor.Position, actor.Position)).Item2;
                if (!SafeZones(slot).Any(c => _safeZone.Check(finalPos, c, default)))
                    hints.Add("Aim towards safe zone!");
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var c in SafeZones(pcSlot))
                _safeZone.Draw(arena, c, default, ArenaColor.SafeFromAOE);
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            var debuff = (SID)status.ID switch
            {
                SID.BubbleWeave => Debuff.BubbleWeave,
                SID.FoamyFetters => Debuff.FoamyFetters,
                _ => Debuff.None
            };
            if (debuff != Debuff.None && module.Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
                _debuffs[slot] = debuff;
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.FlukeGaleAOE1:
                case AID.FlukeGaleAOE2:
                    Gales.Add(new(caster.Position, 20, spell.NPCFinishAt, _shape, spell.Rotation, Kind.DirForward));
                    Gales.SortBy(s => s.Activation);
                    break;
                case AID.Hydrofall:
                    _resolution = Resolve.Stack;
                    break;
                case AID.Hydrobullet:
                    _resolution = Resolve.Spread;
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.FlukeGaleAOE1 or AID.FlukeGaleAOE2)
            {
                ++NumCasts;
                Gales.RemoveAll(s => s.Origin.AlmostEqual(caster.Position, 1));
            }
        }

        private IEnumerable<WPos> SafeZones(int slot)
        {
            if (_resolution == Resolve.None || _debuffs[slot] == Debuff.None || Gales.Count < 4 || _crystals == null)
                yield break;
            // bind will stay, bubble will always end in '1', so bind has to end in '1' or '2' depending on stack/spread
            var wantedOrder = _debuffs[slot] == Debuff.FoamyFetters && _resolution == Resolve.Spread ? 2 : 0;
            foreach (var c in _crystals.SafeZoneCenters)
            {
                var order = Gales.FindIndex(s => s.Shape!.Check(c, s.Origin, s.Direction));
                if (order == wantedOrder || order == wantedOrder + 1)
                    yield return c;
            }
        }
    }
}
