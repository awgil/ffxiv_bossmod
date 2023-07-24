using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    class P6HallowedWings : Components.GenericAOEs
    {
        public WPos SafeSpotCenter { get; private set; }
        private List<AOEInstance> _aoes = new();

        private static AOEShapeRect _shape = new(80, 11); // note: hallowed wings are actually length 50, but that doesn't really matter

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            var offset = (AID)spell.Action.ID switch
            {
                AID.HallowedWingsLN or AID.HallowedWingsLF => _shape.HalfWidth,
                AID.HallowedWingsRN or AID.HallowedWingsRF => -_shape.HalfWidth,
                _ => 0
            };
            if (offset != 0)
            {
                _aoes.Add(new(_shape, caster.Position + offset * spell.Rotation.ToDirection().OrthoL(), spell.Rotation, spell.FinishAt.AddSeconds(0.8f)));
                if (module.Enemies(OID.NidhoggP6).FirstOrDefault() is var cauterizeCaster && cauterizeCaster != null)
                {
                    _aoes.Add(new(_shape, cauterizeCaster.Position, cauterizeCaster.Rotation, spell.FinishAt.AddSeconds(1.1f)));
                    SafeSpotCenter = new(2 * module.Bounds.Center.X - cauterizeCaster.Position.X, 2 * module.Bounds.Center.Z - _aoes[0].Origin.Z);
                }
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.HallowedWingsAOELeft or AID.HallowedWingsAOERight or AID.CauterizeN)
                ++NumCasts;
        }
    }

    class P6HallowedPlume : Components.GenericBaitAway
    {
        private P6HallowedWings? _wings;
        private Actor? _caster;
        private bool _far;

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
                foreach (var p in SafeSpots(actor))
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
            foreach (var p in SafeSpots(pc))
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

        private IEnumerable<WPos> SafeSpots(Actor actor)
        {
            if (_wings == null)
                yield break;

            bool shouldBait = actor.Role == Role.Tank;
            bool stayFar = shouldBait == _far;
            float xOffset = stayFar ? -9 : +9; // assume hraesvelgr is always at +22
            if (shouldBait)
            {
                yield return _wings.SafeSpotCenter + new WDir(xOffset, 9);
                yield return _wings.SafeSpotCenter + new WDir(xOffset, -9);
            }
            else
            {
                yield return _wings.SafeSpotCenter + new WDir(xOffset, 0);
            }
        }
    }
}
