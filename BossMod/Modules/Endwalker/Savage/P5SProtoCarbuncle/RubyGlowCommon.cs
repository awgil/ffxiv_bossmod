using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle
{
    // common features for ruby glow mechanics
    // quadrants:
    // 0 1
    // 2 3
    // for diagonal, quadrant for coordinate is whatever cell is fully contained (so e.g. for DiagNW quadrant is either 1 or 2)
    abstract class RubyGlowCommon : Components.GenericAOEs
    {
        public enum ArenaState { Normal, Cells, DiagNW, DiagNE } // DiagNW == NW to SE, etc

        public ArenaState State { get; private set; }
        private List<Actor> _magicStones = new();
        private List<Actor> _poisonStones = new();
        public IEnumerable<Actor> MagicStones => _magicStones.Where(s => !s.IsDestroyed);
        public IEnumerable<Actor> PoisonStones => _poisonStones.Where(s => !s.IsDestroyed);

        public static AOEShape ShapeQuadrant = new AOEShapeRect(7.5f, 7.5f, 7.5f);
        public static AOEShape ShapeHalf = new AOEShapeRect(45, 45);
        public static AOEShape ShapePoison = new AOEShapeCircle(13);

        public RubyGlowCommon(ActionID watchedAction = new()) : base(watchedAction) { }

        public int QuadrantForPosition(BossModule module, WPos pos)
        {
            var offset = pos - module.Bounds.Center;
            return State switch
            {
                ArenaState.Cells => (offset.X < 0 ? 0 : 1) | (offset.Z < 0 ? 0 : 2),
                ArenaState.DiagNW => offset.X > offset.Z ? 1 : 2,
                ArenaState.DiagNE => offset.X > -offset.Z ? 3 : 0,
                _ => 0
            };
        }

        public WDir QuadrantDir(int q) => new WDir((q & 1) != 0 ? +1 : -1, (q & 2) != 0 ? +1 : -1); // both coords are +-1
        public WPos QuadrantCenter(BossModule module, int q) => module.Bounds.Center + module.Bounds.HalfSize * 0.5f * QuadrantDir(q);

        public Waymark WaymarkForQuadrant(BossModule module, int q)
        {
            var c = QuadrantCenter(module, q);
            Waymark w = Waymark.Count;
            float wd = float.MaxValue;
            for (int i = 0; i < (int)Waymark.Count; i++)
            {
                var pos = module.WorldState.Waymarks[(Waymark)i];
                var dist = pos != null ? (new WPos(pos.Value.XZ()) - c).LengthSq() : float.MaxValue;
                if (dist < wd)
                {
                    w = (Waymark)i;
                    wd = dist;
                }
            }
            return w;
        }

        public IEnumerable<AOEInstance> ActivePoisonAOEs(BossModule module)
        {
            // TODO: correct explosion time
            return PoisonStones.Select(o => new AOEInstance(ShapePoison, o.Position));
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            switch (State)
            {
                case ArenaState.Cells:
                    arena.AddLine(module.Bounds.Center - new WDir(module.Bounds.HalfSize, 0), module.Bounds.Center + new WDir(module.Bounds.HalfSize, 0), ArenaColor.Border);
                    arena.AddLine(module.Bounds.Center - new WDir(0, module.Bounds.HalfSize), module.Bounds.Center + new WDir(0, module.Bounds.HalfSize), ArenaColor.Border);
                    break;
                case ArenaState.DiagNW:
                    arena.AddLine(module.Bounds.Center - new WDir(module.Bounds.HalfSize, module.Bounds.HalfSize), module.Bounds.Center + new WDir(module.Bounds.HalfSize, module.Bounds.HalfSize), ArenaColor.Border);
                    break;
                case ArenaState.DiagNE:
                    arena.AddLine(module.Bounds.Center - new WDir(module.Bounds.HalfSize, -module.Bounds.HalfSize), module.Bounds.Center + new WDir(module.Bounds.HalfSize, -module.Bounds.HalfSize), ArenaColor.Border);
                    break;
            }
        }

        public override void OnActorEAnim(BossModule module, Actor actor, uint state)
        {
            if ((OID)actor.OID != OID.TopazStoneAny)
                return;

            switch (state)
            {
                case 0x00010002: // dormant -> magic
                    _magicStones.Add(actor);
                    break;
                case 0x00100080: // dormant -> poison
                    _poisonStones.Add(actor);
                    break;
                case 0x00100020: // magic -> poison
                    _magicStones.Remove(actor);
                    _poisonStones.Add(actor);
                    break;
                case 0x00040400: // magic -> dormant
                    _magicStones.Remove(actor);
                    break;
                case 0x00040200: // poison -> dormant
                    _poisonStones.Remove(actor);
                    break;
            }
        }

        public override void OnEventEnvControl(BossModule module, uint directorID, byte index, uint state)
        {
            if (directorID != 0x800375A5)
                return;

            var astate = index switch
            {
                0 => ArenaState.DiagNE,
                1 => ArenaState.DiagNW,
                2 => ArenaState.Cells,
                _ => ArenaState.Normal
            };
            if (astate == ArenaState.Normal)
                return;

            switch (state)
            {
                case 0x00020001:
                    if (State != ArenaState.Normal)
                        module.ReportError(this, $"Active state {State} while state {astate} is activated");
                    State = astate;
                    break;
                case 0x00080004:
                    if (State != astate)
                        module.ReportError(this, $"Active state {State} while state {astate} is deactivated");
                    State = ArenaState.Normal;
                    break;
                // 0x00100020 - happens ~1s after activation
            }
        }
    }

    // common features for ruby glow 4 & 6 (ones that feature recoloring)
    // this includes venom pools and raging claw/searing ray aoes
    abstract class RubyGlowRecolor : RubyGlowCommon
    {
        public enum RecolorState { BeforeStones, BeforeRecolor, Done }

        public RecolorState CurRecolorState { get; private set; }
        public int AOEQuadrant { get; private set; }

        private const float _recolorRadius = 5;

        // note: we show circles around healers until cast happens
        public RubyGlowRecolor() : base(ActionID.MakeSpell(AID.VenomPoolRecolorAOE)) { }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            base.AddHints(module, slot, actor, hints, movementHints);
            if (VenomPoolActive && module.Raid.WithoutSlot().Where(a => a.Role == Role.Healer).InRadius(actor.Position, _recolorRadius).Count() != 1)
                hints.Add("Stack with healer!");
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return VenomPoolActive && player.Role == Role.Healer ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            base.DrawArenaForeground(module, pcSlot, pc, arena);

            if (CurRecolorState == RecolorState.BeforeRecolor)
                foreach (var o in MagicStones)
                    if (QuadrantForPosition(module, o.Position) != AOEQuadrant)
                        arena.Actor(o, ArenaColor.Vulnerable, true);

            if (VenomPoolActive)
                foreach (var a in module.Raid.WithoutSlot().Where(a => a.Role == Role.Healer))
                    arena.AddCircle(a.Position, _recolorRadius, ArenaColor.Safe);
        }

        public override void OnActorEAnim(BossModule module, Actor actor, uint state)
        {
            base.OnActorEAnim(module, actor, state);
            switch (CurRecolorState)
            {
                case RecolorState.BeforeStones:
                    if (MagicStones.Count() == 5)
                    {
                        int[] counts = new int[4];
                        foreach (var o in MagicStones)
                            ++counts[QuadrantForPosition(module, o.Position)];
                        AOEQuadrant = Array.IndexOf(counts, 3);
                        CurRecolorState = RecolorState.BeforeRecolor;
                    }
                    break;
                case RecolorState.BeforeRecolor:
                    if (PoisonStones.Count() > 0)
                    {
                        CurRecolorState = RecolorState.Done;
                    }
                    break;
            }
        }

        private bool VenomPoolActive => NumCasts == 0 && CurRecolorState == RecolorState.BeforeRecolor;
    }
}
