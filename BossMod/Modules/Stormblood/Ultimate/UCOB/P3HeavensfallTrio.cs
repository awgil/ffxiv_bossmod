using System;
using System.Linq;

namespace BossMod.Stormblood.Ultimate.UCOB
{
    class P3HeavensfallTrio : BossComponent
    {
        private Actor? _nael;
        private Actor? _twin;
        private Actor? _baha;
        private WPos[] _safeSpots = new WPos[PartyState.MaxPartySize];

        public bool Active => _nael != null;

        private static Angle[] _offsetsNaelCenter = [10.Degrees(), 80.Degrees(), 100.Degrees(), 170.Degrees()];
        private static Angle[] _offsetsNaelSide = [60.Degrees(), 80.Degrees(), 100.Degrees(), 120.Degrees()];

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            arena.Actor(_nael, ArenaColor.Object, true);
            var safespot = _safeSpots[pcSlot];
            if (safespot != default)
                arena.AddCircle(safespot, 1, ArenaColor.Safe);
        }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if ((OID)actor.OID == OID.NaelDeusDarnus && id == 0x1E43)
            {
                _nael = actor;
                InitIfReady(module);
            }
            else if ((OID)actor.OID == OID.Twintania && id == 0x1E44)
            {
                _twin = actor;
                InitIfReady(module);
            }
            else if ((OID)actor.OID == OID.BahamutPrime && id == 0x1E43)
            {
                _baha = actor;
                InitIfReady(module);
            }
        }

        private void InitIfReady(BossModule module)
        {
            if (_nael == null || _twin == null || _baha == null)
                return;

            var dirToNael = Angle.FromDirection(_nael.Position - module.Bounds.Center);
            var dirToTwin = Angle.FromDirection(_twin.Position - module.Bounds.Center);
            var dirToBaha = Angle.FromDirection(_baha.Position - module.Bounds.Center);

            var twinRel = (dirToTwin - dirToNael).Normalized();
            var bahaRel = (dirToBaha - dirToNael).Normalized();
            var (offsetSymmetry, offsets) = twinRel.Rad * bahaRel.Rad < 0 // twintania & bahamut are on different sides => nael is in center
                ? (0.Degrees(), _offsetsNaelCenter)
                : ((twinRel + bahaRel) * 0.5f, _offsetsNaelSide);
            var dirSymmetry = dirToNael + offsetSymmetry;
            foreach (var p in Service.Config.Get<UCOBConfig>().P3QuickmarchTrioAssignments.Resolve(module.Raid))
            {
                bool left = p.group < 4;
                int order = p.group & 3;
                var offset = offsets[order];
                var dir = dirSymmetry + (left ? offset : -offset);
                _safeSpots[p.slot] = module.Bounds.Center + 20 * dir.ToDirection();
            }
        }
    }

    class P3HeavensfallTowers : Components.CastTowers
    {
        public P3HeavensfallTowers() : base(ActionID.MakeSpell(AID.MegaflareTower), 3) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            base.OnCastStarted(module, caster, spell);

            if (spell.Action == WatchedAction && Towers.Count == 8)
            {
                var nael = module.Enemies(OID.NaelDeusDarnus).FirstOrDefault();
                if (nael != null)
                {
                    var dirToNael = Angle.FromDirection(nael.Position - module.Bounds.Center);
                    var orders = Towers.Select(t => TowerSortKey(Angle.FromDirection(t.Position - module.Bounds.Center), dirToNael)).ToList();
                    MemoryExtensions.Sort(orders.AsSpan(), Towers.AsSpan());
                    foreach (var p in Service.Config.Get<UCOBConfig>().P3HeavensfallTrioTowers.Resolve(module.Raid))
                    {
                        Towers.Ref(p.group).ForbiddenSoakers = new(~(1ul << p.slot));
                    }
                }
            }
        }

        // order towers from nael's position CW
        private float TowerSortKey(Angle tower, Angle reference)
        {
            var cwDist = (reference - tower).Normalized().Deg;
            if (cwDist < -22.5f)
                cwDist += 360;
            return cwDist;
        }
    }
}
