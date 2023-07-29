using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    class P6HallowedWings : Components.GenericAOEs
    {
        public AOEInstance? AOE; // origin is always (122, 100 +- 11), direction -90

        private static AOEShapeRect _shape = new(50, 11);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (AOE != null)
                yield return AOE.Value;
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            var offset = (AID)spell.Action.ID switch
            {
                AID.HallowedWingsLN or AID.HallowedWingsLF => _shape.HalfWidth,
                AID.HallowedWingsRN or AID.HallowedWingsRF => -_shape.HalfWidth,
                _ => 0
            };
            if (offset == 0)
                return;
            var origin = caster.Position + offset * spell.Rotation.ToDirection().OrthoL();
            AOE = new(_shape, origin, spell.Rotation, spell.FinishAt.AddSeconds(0.8f));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.HallowedWingsAOELeft or AID.HallowedWingsAOERight or AID.CauterizeN)
                ++NumCasts;
        }
    }

    class P6CauterizeN : Components.GenericAOEs
    {
        public AOEInstance? AOE; // origin is always (100 +- 11, 100 +- 34), direction 0/180

        private static AOEShapeRect _shape = new(80, 11);

        public P6CauterizeN() : base(ActionID.MakeSpell(AID.CauterizeN)) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (AOE != null)
                yield return AOE.Value;
        }

        // note: we want to show hint much earlier than cast start - we assume component is created right as hallowed wings starts, meaning nidhogg is already in place
        public override void Init(BossModule module)
        {
            var caster = module.Enemies(OID.NidhoggP6).FirstOrDefault();
            if (caster != null)
                AOE = new(_shape, caster.Position, caster.Rotation, module.WorldState.CurrentTime.AddSeconds(8.6f));
        }
    }

    abstract class P6HallowedPlume : Components.GenericBaitAway
    {
        protected P6HallowedWings? _wings;
        protected bool _far;
        private Actor? _caster;

        private static AOEShapeCircle _shape = new(10);

        public P6HallowedPlume() : base(ActionID.MakeSpell(AID.HallowedPlume), centerAtTarget: true) { }

        public override void Init(BossModule module) => _wings = module.FindComponent<P6HallowedWings>();

        public override void Update(BossModule module)
        {
            CurrentBaits.Clear();
            if (_caster != null)
            {
                var players = module.Raid.WithoutSlot().SortedByRange(_caster.Position);
                var targets = _far ? players.TakeLast(2) : players.Take(2);
                foreach (var t in targets)
                    CurrentBaits.Add(new(_caster, t, _shape));
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            bool shouldBait = actor.Role == Role.Tank;
            bool isBaiting = ActiveBaitsOn(actor).Any();
            bool stayFar = shouldBait == _far;
            hints.Add(stayFar ? "Stay far!" : "Stay close!", shouldBait != isBaiting);

            if (shouldBait == isBaiting)
            {
                if (shouldBait)
                {
                    if (ActiveBaitsOn(actor).Any(b => PlayersClippedBy(module, b).Any()))
                        hints.Add("Bait away from raid!");
                }
                else
                {
                    if (ActiveBaitsNotOn(actor).Any(b => IsClippedBy(actor, b)))
                        hints.Add("GTFO from baited aoe!");
                }
            }

            if (movementHints != null)
                foreach (var p in SafeSpots(module, actor))
                    movementHints.Add(actor.Position, p, ArenaColor.Safe);
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (_caster != null)
                hints.Add($"Tankbuster {(_far ? "far" : "near")}");
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            base.DrawArenaForeground(module, pcSlot, pc, arena);
            foreach (var p in SafeSpots(module, pc))
                arena.AddCircle(p, 1, ArenaColor.Safe);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            bool? far = (AID)spell.Action.ID switch
            {
                AID.HallowedWingsLN or AID.HallowedWingsRN => false,
                AID.HallowedWingsLF or AID.HallowedWingsRF => true,
                _ => null
            };
            if (far != null)
            {
                _caster = caster;
                _far = far.Value;
            }
        }

        protected abstract IEnumerable<WPos> SafeSpots(BossModule module, Actor actor);
    }

    class P6HallowedPlume1 : P6HallowedPlume
    {
        private P6CauterizeN? _cauterize;

        public override void Init(BossModule module)
        {
            base.Init(module);
            _cauterize = module.FindComponent<P6CauterizeN>();
        }

        protected override IEnumerable<WPos> SafeSpots(BossModule module, Actor actor)
        {
            if (_wings?.AOE == null || _cauterize?.AOE == null)
                yield break;

            var safeSpotCenter = module.Bounds.Center;
            safeSpotCenter.Z -= _wings.AOE.Value.Origin.Z - module.Bounds.Center.Z;
            safeSpotCenter.X -= _cauterize.AOE.Value.Origin.X - module.Bounds.Center.X;

            bool shouldBait = actor.Role == Role.Tank;
            bool stayFar = shouldBait == _far;
            float xOffset = stayFar ? -9 : +9; // assume hraesvelgr is always at +22
            if (shouldBait)
            {
                // TODO: configurable tank assignments (e.g. MT always center/out/N/S)
                yield return safeSpotCenter + new WDir(xOffset, 9);
                yield return safeSpotCenter + new WDir(xOffset, -9);
            }
            else
            {
                yield return safeSpotCenter + new WDir(xOffset, 0);
            }
        }
    }

    class P6HallowedPlume2 : P6HallowedPlume
    {
        private P6HotWingTail? _wingTail;

        public override void Init(BossModule module)
        {
            base.Init(module);
            _wingTail = module.FindComponent<P6HotWingTail>();
        }

        protected override IEnumerable<WPos> SafeSpots(BossModule module, Actor actor)
        {
            if (_wings?.AOE == null || _wingTail == null)
                yield break;

            float zCoeff = _wingTail.NumAOEs switch
            {
                1 => 15.75f / 11,
                2 => 4.0f / 11,
                _ => 1
            };
            var safeSpotCenter = module.Bounds.Center;
            safeSpotCenter.Z -= zCoeff * (_wings.AOE.Value.Origin.Z - module.Bounds.Center.Z);

            bool shouldBait = actor.Role == Role.Tank;
            bool stayFar = shouldBait == _far;
            float xOffset = stayFar ? -20 : +20; // assume hraesvelgr is always at +22
            if (shouldBait)
            {
                // TODO: configurable tank assignments (e.g. MT always center/border/near/far)
                yield return safeSpotCenter;
                yield return safeSpotCenter + new WDir(xOffset, 0);
            }
            else
            {
                yield return safeSpotCenter + new WDir(xOffset, 0);
            }
        }
    }
}
