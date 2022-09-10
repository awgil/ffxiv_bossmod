using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.Savage.P7SAgdistis
{
    // common parts of various forbidden fruit / harvest mechanics
    // platform id's: 0 = W, 1 = S, 2 = E
    // TODO: show knockback for bird tethers, something for bull/minotaur tethers...
    class ForbiddenFruitCommon : Components.GenericAOEs
    {
        public int NumAssignedTethers { get; private set; }
        public bool MinotaursBaited { get; private set; }
        protected Actor?[] TetherSources = new Actor?[8];
        protected BitMask[] SafePlatforms = new BitMask[8];
        private List<(Actor, AOEShape, DateTime)> _predictedAOEs = new();
        private List<(Actor, AOEShape)> _activeAOEs = new();

        protected static BitMask ValidPlatformsMask = new(7);
        private static AOEShapeCircle _shapeBullUntethered = new(10);
        private static AOEShapeRect _shapeBirdUntethered = new(60, 4);
        private static AOEShapeCone _shapeMinotaurUntethered = new(60, 45.Degrees());

        public bool CastsActive => _activeAOEs.Count > 0;

        public ForbiddenFruitCommon(ActionID watchedAction) : base(watchedAction) { }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var (source, shape, time) in _predictedAOEs)
                yield return (shape, source.Position, source.Rotation, time);
            foreach (var (source, shape) in _activeAOEs)
                yield return (shape, source.Position, source.CastInfo!.Rotation, source.CastInfo.FinishAt);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var tetherSource = TetherSources[pcSlot];
            if (tetherSource != null)
                arena.AddLine(tetherSource.Position, pc.Position, TetherColor(tetherSource));

            foreach (var platform in SafePlatforms[pcSlot].SetBits())
                arena.AddCircle(module.Bounds.Center + PlatformDirection(platform).ToDirection() * Border.SmallPlatformOffset, Border.SmallPlatformRadius, ArenaColor.Safe);
        }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            TryAssignTether(module, source, tether);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.StaticMoon:
                    _predictedAOEs.Clear();
                    _activeAOEs.Add((caster, _shapeBullUntethered));
                    break;
                case AID.StymphalianStrike:
                    _predictedAOEs.Clear();
                    _activeAOEs.Add((caster, _shapeBirdUntethered));
                    break;
                case AID.BullishSwipeAOE:
                    MinotaursBaited = true;
                    _activeAOEs.Add((caster, _shapeMinotaurUntethered));
                    break;
            }
        }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if (id == 0x11D1)
            {
                var castStart = PredictUntetheredCastStart(module, actor);
                if (castStart != null)
                {
                    AOEShape? shape = (OID)actor.OID switch
                    {
                        OID.ForbiddenFruitBull => _shapeBullUntethered,
                        OID.ForbiddenFruitBird => _shapeBirdUntethered,
                        _ => null
                    };
                    if (shape != null)
                    {
                        _predictedAOEs.Add((actor, shape, castStart.Value.AddSeconds(3)));
                    }
                }
            }
        }

        // subclass can override and return non-null if specified fruit will become of untethered variety
        protected virtual DateTime? PredictUntetheredCastStart(BossModule module, Actor fruit) => null;

        // this is called by default OnTethered, but subclasses might want to call it themselves and use returned info (target slot if tether was assigned)
        protected int TryAssignTether(BossModule module, Actor source, ActorTetherInfo tether)
        {
            if ((TetherID)tether.ID is TetherID.Bull or TetherID.MinotaurClose or TetherID.MinotaurFar or TetherID.Bird)
            {
                int slot = module.Raid.FindSlot(tether.Target);
                if (slot >= 0)
                {
                    TetherSources[slot] = source;
                    ++NumAssignedTethers;
                    return slot;
                }
            }
            return -1;
        }

        protected uint TetherColor(Actor source) => (OID)source.OID switch
        {
            OID.ImmatureMinotaur => 0xffff00ff,
            OID.BullTetherSource => 0xffffff00,
            OID.ImmatureStymphalide => 0xff00ffff,
            _ => 0
        };

        protected int PlatformIDFromOffset(WDir offset) => offset.Z > 0 ? 1 : offset.X > 0 ? 2 : 0;
        protected Angle PlatformDirection(int id) => (id - 1) * 120.Degrees();
    }
}
