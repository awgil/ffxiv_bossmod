namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

class WyvernsWealCast(BossModule module) : Components.GroupedAOEs(module, [AID.WyvernsWealSlow, AID.WyvernsWealFast], new AOEShapeRect(60, 3));

class WyvernsWeal(BossModule module) : Components.GenericAOEs(module, null)
{
    public int NumBaits { get; private set; }

    public class Bait(Actor t)
    {
        public Actor Target = t;
        public DateTime NextCast;
        public DateTime FinalCast { get; init; }
        public Angle LastRotation;
        public int NumCasts;
        public Angle StartRotation;
    }

    private readonly List<Bait> _baits = [];
    private readonly List<(Actor, float)> _crystals = [];

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.LineTarget)
        {
            _baits.Add(new(actor)
            {
                NextCast = WorldState.FutureTime(10.1f),
                FinalCast = WorldState.FutureTime(16.1f)
            });

            if (NumBaits == 0)
            {
                _crystals.Clear();
                _crystals.AddRange(WorldState.Actors.Where(a => (OID)a.OID is OID.BigCrystal or OID.SmallCrystal).Select(a => (a, (OID)a.OID == OID.BigCrystal ? 12f : 6)));
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WyvernsWealSlow)
            NumBaits++;

        if ((AID)spell.Action.ID is AID.WyvernsWealSlow or AID.WyvernsWealFast && _baits.Count > 0)
        {
            _baits[0].LastRotation = spell.Rotation;
            _baits[0].NextCast = Module.CastFinishAt(spell, 0.7f);
            if ((AID)spell.Action.ID == AID.WyvernsWealSlow)
                _baits[0].StartRotation = spell.Rotation;
            if (++_baits[0].NumCasts >= 9)
                _baits.RemoveAt(0);
        }

        if ((AID)spell.Action.ID is AID.SmallCrystalExplosion or AID.BigCrystalExplosion)
            _crystals.RemoveAll(c => c.Item1.Position.AlmostEqual(caster.Position, 1));
    }

    public override void Update()
    {
        // laser repeat is occasionally only cast 7 times instead of 8 :/
        if (_baits.Count > 0 && _baits[0].FinalCast.AddSeconds(1) < WorldState.CurrentTime)
        {
            ReportError($"misbehaving bait on {_baits[0].Target} (not enough casts)");
            _baits.RemoveAt(0);
            NumCasts++; // for state machine
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WyvernsWealSlow or AID.WyvernsWealFast)
            NumCasts++;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        BitMask hitCrystals = default;

        foreach (var b in _baits.Take(1))
        {
            foreach (var (i, angle) in PredictedAngles(b))
            {
                var explodeTime = b.NextCast.AddSeconds(0.7f * i);
                if (b.Target != actor)
                    yield return new AOEInstance(new AOEShapeRect(60, 3), Module.PrimaryActor.Position, angle, explodeTime);

                var ic = 0;
                foreach (var c in _crystals)
                {
                    if (!hitCrystals[ic] && c.Item1.Position.InRect(Module.PrimaryActor.Position, angle, 60, 0, 3 + c.Item1.HitboxRadius))
                    {
                        hitCrystals.Set(ic);
                        yield return new AOEInstance(new AOEShapeCircle(c.Item2), c.Item1.Position, default, explodeTime.AddSeconds(1));
                    }
                    ic++;
                }
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        foreach (var bait in _baits)
        {
            if (bait.Target == pc)
            {
                // we are the target and the laser hasn't activated, draw a cone approximating the full range of the laser rotation (it seems to be 90 degrees or slightly wider)
                if (bait.LastRotation == default)
                {
                    var fakeSrc = Module.PrimaryActor.Position - new WDir(3, 3).Rotate(-45.Degrees());
                    var side = MathF.Sign(pc.Position.X - Arena.Center.X);
                    Arena.AddCone(fakeSrc, 60, Module.PrimaryActor.AngleTo(pc) - (45 * side).Degrees(), 45.Degrees(), ArenaColor.Danger);
                }
                else
                {
                    // just draw baits, TOP-style
                    foreach (var (_, angle) in PredictedAngles(bait))
                        Arena.AddRect(Module.PrimaryActor.Position, angle.ToDirection(), 60, 0, 3, ArenaColor.Danger);
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        // forbid the side of the arena we started on
        if (_baits.FirstOrDefault() is { } b && b.Target == actor && b.StartRotation != default)
            hints.AddForbiddenZone(ShapeContains.Rect(Module.PrimaryActor.Position, b.StartRotation, 60, 3, 60), b.FinalCast);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (BaitOn(actor) is { } b && b.LastRotation == default)
        {
            var ts = (b.NextCast - WorldState.CurrentTime).TotalSeconds - 2;
            if (ts > 0)
                hints.Add($"Laser starts in {ts:f1}s", false);

            var fakeSrc = Module.PrimaryActor.Position - new WDir(3, 3).Rotate(-45.Degrees());
            var side = MathF.Sign(actor.Position.X - Arena.Center.X);
            var predictCenter = Module.PrimaryActor.AngleTo(actor) - (45 * side).Degrees();
            // check if predicted cone hits any players that are not currently baiting
            // (anyone who is baiting has to run across the arena, so they will always be clipped by our bait)
            if (Raid.WithoutSlot().Where(r => BaitOn(r) == null).InShape(new AOEShapeCone(60, 45.Degrees()), fakeSrc, predictCenter).Any())
                hints.Add("Bait away from raid!");
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        var ix = _baits.FindIndex(b => b.Target == player);
        return ix == 0
            ? PlayerPriority.Danger
            : ix > 0
                ? PlayerPriority.Interesting
                : base.CalcPriority(pcSlot, pc, playerSlot, player, ref customColor);
    }

    // enumerate the remaining rotations of the laser given the starting angle
    // this completely breaks if a laser target dies early, but there's nothing we can do about that with no way to know who the new target is
    private IEnumerable<(int, Angle)> PredictedAngles(Bait b)
    {
        var actual = Module.PrimaryActor.AngleTo(b.Target);
        if (b.LastRotation == default)
        {
            yield return (0, actual);
            yield break;
        }

        var rotateDist = actual - b.LastRotation;
        for (var i = 1; i <= (9 - b.NumCasts); i++)
        {
            var moveDeg = Math.Clamp(rotateDist.Deg, -11.25f * i, 11.25f * i);
            var predicted = moveDeg.Degrees();
            yield return (i - 1, b.LastRotation + predicted);
            // break to avoid drawing multiple AOEs on top of each other
            if (moveDeg == rotateDist.Deg)
                yield break;
        }
    }

    private Bait? BaitOn(Actor p) => _baits.FirstOrDefault(b => b.Target == p);
}
