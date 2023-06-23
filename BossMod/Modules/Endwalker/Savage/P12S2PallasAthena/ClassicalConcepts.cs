using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Savage.P12S2PallasAthena
{
    // note: rows at Z=100, 92, 84; columns at X=88, 96, 104, 112
    // note: assumes standard assignments (BPOG columns, alpha to tri, beta to square)
    class ClassicalConcepts : BossComponent
    {
        enum Debuff { None, Alpha, Beta }

        struct PlayerState
        {
            public int Column;
            public Debuff Debuff;
            public int PartnerSlot;
        }

        public int NumPlayerTethers { get; private set; }
        public int NumShapeTethers { get; private set; }
        private List<Actor> _hexa = new();
        private List<Actor> _tri = new();
        private List<Actor> _sq = new();
        private (WPos hexa, WPos tri, WPos sq)[] _resolvedShapes = new(WPos, WPos, WPos)[4];
        private PlayerState[] _states = new PlayerState[PartyState.MaxPartySize];
        private bool _invert;
        private bool _showShapes = true;
        private bool _showTethers = true;

        public ClassicalConcepts(bool invert)
        {
            _invert = invert;
        }

        public override void Init(BossModule module)
        {
            Array.Fill(_states, new() { Column = -1, PartnerSlot = -1 });
            _hexa = module.Enemies(OID.ConceptOfWater);
            _tri = module.Enemies(OID.ConceptOfFire);
            _sq = module.Enemies(OID.ConceptOfEarth);
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (PlayerShapes(slot) is var shapes && shapes.hexa != default && shapes.linked != default)
            {
                var soff = shapes.linked - shapes.hexa;
                var poff = actor.Position - shapes.hexa;
                var dir = soff.Normalized();
                var dot = poff.Dot(dir);
                if (dot < 0 || dot * dot > soff.LengthSq() || Math.Abs(dir.OrthoL().Dot(poff)) > 1)
                    hints.Add("Stand between assigned shapes!");
            }
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return _states[pcSlot].PartnerSlot == playerSlot ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (PlayerShapes(pcSlot) is var shapes && shapes.hexa != default && shapes.linked != default)
            {
                arena.Actor(shapes.hexa, default, ArenaColor.Object);
                arena.Actor(shapes.linked, default, ArenaColor.Object);
                var safespot = shapes.hexa + (shapes.linked - shapes.hexa) / 3;
                arena.AddCircle(safespot, 1, ArenaColor.Safe);
                if (_invert)
                    arena.AddCircle(InvertedPos(safespot), 1, ArenaColor.Danger);
            }
            if (_showTethers && module.Raid[_states[pcSlot].PartnerSlot] is var partner && partner != null)
            {
                arena.AddLine(pc.Position, partner.Position, ArenaColor.Safe);
            }
        }

        public override void OnActorCreated(BossModule module, Actor actor)
        {
            if ((OID)actor.OID is OID.ConceptOfFire or OID.ConceptOfWater or OID.ConceptOfEarth && _hexa.Count + _tri.Count + _sq.Count == 12)
            {
                for (int col = 0; col < _resolvedShapes.Length; ++col)
                {
                    var hexa = _hexa.Find(h => Utils.AlmostEqual(h.PosRot.X, 88 + col * 8, 1));
                    if (hexa == null)
                    {
                        module.ReportError(this, $"Failed to find hexagon at column {col}");
                        continue;
                    }

                    var tri = LinkedShape(_tri, hexa);
                    var sq = LinkedShape(_sq, hexa);
                    _resolvedShapes[col] = (hexa.Position, tri?.Position ?? default, sq?.Position ?? default);
                    if (tri == null || sq == null)
                        module.ReportError(this, $"Failed to find neighbour for column {col}");
                }
            }
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            var debuff = (SID)status.ID switch
            {
                SID.AlphaTarget => Debuff.Alpha,
                SID.BetaTarget => Debuff.Beta,
                _ => Debuff.None
            };
            if (debuff != Debuff.None && module.Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
                _states[slot].Debuff = debuff;
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            var column = (IconID)iconID switch
            {
                IconID.ClassicalConceptsCross => 0, // B
                IconID.ClassicalConceptsSquare => 1, // P
                IconID.ClassicalConceptsCircle => 2, // O
                IconID.ClassicalConceptsTriangle => 3, // G
                _ => -1
            };
            if (column >= 0 && module.Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            {
                var partner = Array.FindIndex(_states, s => s.Column == column);
                _states[slot].Column = column;
                if (partner >= 0)
                {
                    _states[slot].PartnerSlot = partner;
                    _states[partner].PartnerSlot = slot;
                }
            }
        }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            switch ((TetherID)tether.ID)
            {
                case TetherID.ClassicalConceptsPlayers:
                    ++NumPlayerTethers;
                    // note: tethers could be between players of different columns, if some people are dead
                    break;
                case TetherID.ClassicalConceptsShapes:
                    _showShapes = false; // stop showing shapes, now that they are baited
                    ++NumShapeTethers;
                    break;
            }
        }

        public override void OnUntethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            switch ((TetherID)tether.ID)
            {
                case TetherID.ClassicalConceptsPlayers:
                    --NumPlayerTethers;
                    _showTethers = false; // stop showing tethers, now that they are resolved
                    break;
                case TetherID.ClassicalConceptsShapes:
                    --NumShapeTethers;
                    break;
            }
        }

        private static bool ShapesAreNeighbours(Actor l, Actor r)
        {
            var d = (l.Position - r.Position).Abs();
            return Utils.AlmostEqual(d.X + d.Z, 8, 1);
        }
        private IEnumerable<Actor> Neighbours(IEnumerable<Actor> list, Actor shape) => list.Where(s => ShapesAreNeighbours(s, shape));
        private int NumNeighbouringHexagons(Actor shape) => _hexa.Count(h => ShapesAreNeighbours(h, shape));
        private Actor? LinkedShape(List<Actor> shapes, Actor hexa) => Neighbours(shapes, hexa).MinBy(NumNeighbouringHexagons);

        private WPos InvertedPos(WPos p) => new(200 - p.X, 184 - p.Z);

        private (WPos hexa, WPos linked) PlayerShapes(int slot)
        {
            if (!_showShapes)
                return (default, default);
            var state = _states[slot];
            if (state.Column < 0 || state.Debuff == Debuff.None)
                return (default, default);
            var shapes = _resolvedShapes[state.Column];
            var linked = state.Debuff == Debuff.Alpha ? shapes.tri : shapes.sq;
            return _invert ? (InvertedPos(shapes.hexa), InvertedPos(linked)) : (shapes.hexa, linked);
        }
    }

    class ClassicalConcepts1 : ClassicalConcepts
    {
        public ClassicalConcepts1() : base(false) { }
    }

    class ClassicalConcepts2 : ClassicalConcepts
    {
        public ClassicalConcepts2() : base(true) { }
    }

    class Implode : Components.SelfTargetedAOEs
    {
        public Implode() : base(ActionID.MakeSpell(AID.Implode), new AOEShapeCircle(4)) { }
    }

    class PalladianRayBait : Components.GenericBaitAway
    {
        private Actor[] _dummies = { new(0, 0, -1, "L dummy", ActorType.None, Class.None, new(92, 0, 92, 0)), new(0, 0, -1, "R dummy", ActorType.None, Class.None, new(108, 0, 92, 0)) };

        private static AOEShapeCone _shape = new(100, 15.Degrees());

        public PalladianRayBait() : base(ActionID.MakeSpell(AID.PalladianRayAOEFirst)) { }

        public override void Update(BossModule module)
        {
            CurrentBaits.Clear();
            foreach (var d in _dummies)
                foreach (var p in module.Raid.WithoutSlot().SortedByRange(d.Position).Take(4))
                    CurrentBaits.Add(new(d, p, _shape));
        }
    }

    class PalladianRayAOE : Components.GenericAOEs
    {
        private List<AOEInstance> _aoes = new();
        public int NumConcurrentAOEs => _aoes.Count;

        private static AOEShapeCone _shape = new(100, 15.Degrees());

        public PalladianRayAOE() : base(ActionID.MakeSpell(AID.PalladianRayAOERest)) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            if ((AID)spell.Action.ID == AID.PalladianRayAOEFirst)
                _aoes.Add(new(_shape, caster.Position, caster.Rotation));
        }
    }
}
