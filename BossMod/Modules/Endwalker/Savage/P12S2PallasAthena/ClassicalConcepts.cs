namespace BossMod.Endwalker.Savage.P12S2PallasAthena;

// note: rows at Z=100, 92, 84; columns at X=88, 96, 104, 112
// note: assumes standard assignments (BPOG columns, alpha to tri, beta to square)
class ClassicalConcepts(BossModule module, bool invert) : BossComponent(module)
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
    private readonly IReadOnlyList<Actor> _hexa = module.Enemies(OID.ConceptOfWater);
    private readonly IReadOnlyList<Actor> _tri = module.Enemies(OID.ConceptOfFire);
    private readonly IReadOnlyList<Actor> _sq = module.Enemies(OID.ConceptOfEarth);
    private readonly (WPos hexa, WPos tri, WPos sq)[] _resolvedShapes = new (WPos, WPos, WPos)[4];
    private readonly PlayerState[] _states = Utils.MakeArray(PartyState.MaxPartySize, new PlayerState() { Column = -1, PartnerSlot = -1 });
    private readonly bool _invert = invert;
    private bool _showShapes = true;
    private bool _showTethers = true;

    public override void AddHints(int slot, Actor actor, TextHints hints)
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

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return _states[pcSlot].PartnerSlot == playerSlot ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (PlayerShapes(pcSlot) is var shapes && shapes.hexa != default && shapes.linked != default)
        {
            Arena.Actor(shapes.hexa, default, ArenaColor.Object);
            Arena.Actor(shapes.linked, default, ArenaColor.Object);
            var safespot = shapes.hexa + (shapes.linked - shapes.hexa) / 3;
            Arena.AddCircle(safespot, 1, ArenaColor.Safe);
            if (_invert)
                Arena.AddCircle(InvertedPos(safespot), 1, ArenaColor.Danger);
        }
        if (_showTethers && Raid[_states[pcSlot].PartnerSlot] is var partner && partner != null)
        {
            Arena.AddLine(pc.Position, partner.Position, ArenaColor.Safe);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.ConceptOfFire or OID.ConceptOfWater or OID.ConceptOfEarth && _hexa.Count + _tri.Count + _sq.Count == 12)
        {
            for (int col = 0; col < _resolvedShapes.Length; ++col)
            {
                var hexa = _hexa.FirstOrDefault(h => Utils.AlmostEqual(h.PosRot.X, 88 + col * 8, 1));
                if (hexa == null)
                {
                    ReportError($"Failed to find hexagon at column {col}");
                    continue;
                }

                var tri = LinkedShape(_tri, hexa);
                var sq = LinkedShape(_sq, hexa);
                _resolvedShapes[col] = (hexa.Position, tri?.Position ?? default, sq?.Position ?? default);
                if (tri == null || sq == null)
                    ReportError($"Failed to find neighbour for column {col}");
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var debuff = (SID)status.ID switch
        {
            SID.AlphaTarget => Debuff.Alpha,
            SID.BetaTarget => Debuff.Beta,
            _ => Debuff.None
        };
        if (debuff != Debuff.None && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            _states[slot].Debuff = debuff;
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        var column = (IconID)iconID switch
        {
            IconID.ClassicalConceptsCross => 0, // B
            IconID.ClassicalConceptsSquare => 1, // P
            IconID.ClassicalConceptsCircle => 2, // O
            IconID.ClassicalConceptsTriangle => 3, // G
            _ => -1
        };
        if (column >= 0 && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
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

    public override void OnTethered(Actor source, ActorTetherInfo tether)
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

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
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
    private Actor? LinkedShape(IReadOnlyList<Actor> shapes, Actor hexa) => Neighbours(shapes, hexa).MinBy(NumNeighbouringHexagons);

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

class ClassicalConcepts1(BossModule module) : ClassicalConcepts(module, false);
class ClassicalConcepts2(BossModule module) : ClassicalConcepts(module, true);

class Implode(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Implode), new AOEShapeCircle(4));

class PalladianRayBait(BossModule module) : Components.GenericBaitAway(module, ActionID.MakeSpell(AID.PalladianRayAOEFirst))
{
    private readonly Actor[] _dummies = [new(0, 0, -1, "L dummy", 0, ActorType.None, Class.None, 0, new(92, 0, 92, 0)), new(0, 0, -1, "R dummy", 0, ActorType.None, Class.None, 0, new(108, 0, 92, 0))];

    private static readonly AOEShapeCone _shape = new(100, 15.Degrees());

    public override void Update()
    {
        CurrentBaits.Clear();
        foreach (var d in _dummies)
            foreach (var p in Raid.WithoutSlot().SortedByRange(d.Position).Take(4))
                CurrentBaits.Add(new(d, p, _shape));
    }
}

class PalladianRayAOE(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.PalladianRayAOERest))
{
    private readonly List<AOEInstance> _aoes = [];
    public int NumConcurrentAOEs => _aoes.Count;

    private static readonly AOEShapeCone _shape = new(100, 15.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.PalladianRayAOEFirst)
            _aoes.Add(new(_shape, caster.Position, caster.Rotation));
    }
}
