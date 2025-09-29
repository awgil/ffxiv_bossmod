namespace BossMod.Dawntrail.Ultimate.FRU;

class P2AbsoluteZero(BossModule module) : Components.CastCounter(module, AID.AbsoluteZeroAOE);

class P2SwellingFrost(BossModule module) : Components.Knockback(module, AID.SwellingFrost, true)
{
    private readonly DateTime _activation = module.WorldState.FutureTime(3.2f);

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        yield return new(Module.Center, 10, _activation);
    }
}

class P2SinboundBlizzard(BossModule module) : Components.StandardAOEs(module, AID.SinboundBlizzardAOE, new AOEShapeCone(50, 10.Degrees()));

class P2HiemalStorm(BossModule module) : Components.StandardAOEs(module, AID.HiemalStormAOE, 7)
{
    private bool _slowDodges;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // storms are cast every 3s, ray voidzones appear every 2s; to place voidzones more tightly, we pretend radius is smaller during first half of cast
        // there's no point doing it before first voidzone appears, however
        var deadline = _slowDodges ? WorldState.FutureTime(1.5f) : DateTime.MaxValue;
        foreach (var c in Casters)
        {
            var activation = Module.CastFinishAt(c.CastInfo);
            hints.AddForbiddenZone(ShapeContains.Circle(c.CastInfo!.LocXZ, activation > deadline ? 4 : 7), activation);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.HiemalRay)
            _slowDodges = true;
    }
}

class P2HiemalRay(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 4, AID.HiemalRay, module => module.Enemies(OID.HiemalRayVoidzone).Where(z => z.EventState != 7), 0.7f);

// TODO: show hint if ice veil is clipped
class P2Intermission(BossModule module) : Components.GenericBaitAway(module)
{
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private readonly P2SinboundBlizzard? _cones = module.FindComponent<P2SinboundBlizzard>();
    private readonly IReadOnlyList<Actor> _crystalsOfLight = module.Enemies(OID.CrystalOfLight);
    private readonly IReadOnlyList<Actor> _crystalsOfDarkness = module.Enemies(OID.CrystalOfDarkness);
    private readonly IReadOnlyList<Actor> _iceVeil = module.Enemies(OID.IceVeil);
    private bool _iceVeilInvincible = true;
    private bool _gaiaHammer;

    public bool CrystalsActive => CrystalsOfLight.Any();

    public override void Update()
    {
        IgnoreOtherBaits = true;
        CurrentBaits.Clear();
        if (_cones == null)
            return;
        foreach (var c in _crystalsOfDarkness)
        {
            var baiter = Raid.WithoutSlot().Closest(c.Position);
            if (baiter != null)
                CurrentBaits.Add(new(c, baiter, _cones.Shape));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // enemy priorities
        var clockSpot = _config.P2IntermissionClockSpots[assignment];
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.CrystalOfLight => CrystalPriority(e.Actor, clockSpot),
                OID.CrystalOfDarkness => AIHints.Enemy.PriorityPointless,
                OID.IceVeil => _iceVeilInvincible
                    ? AIHints.Enemy.PriorityInvincible
                    : e.Actor.PendingHPRatio < (_gaiaHammer ? 0 : 0.5f)
                        ? AIHints.Enemy.PriorityPointless
                        : 1,
                _ => 0
            };
        }

        // don't stand inside light crystals, to avoid bad puddle baits
        foreach (var c in CrystalsOfLight)
            hints.AddForbiddenZone(ShapeContains.Circle(c.Position, 4), WorldState.FutureTime(30));

        // mechanic resolution
        if (clockSpot < 0)
        {
            // no assignment, oh well...
        }
        else if ((clockSpot & 1) == 0)
        {
            // cardinals - bait puddles accurately
            var assignedDir = (180 - 45 * clockSpot).Degrees();
            var assignedPosition = Module.Center + 15 * assignedDir.ToDirection(); // crystal is at R=15
            var assignedCrystal = CrystalsOfLight.FirstOrDefault(c => c.Position.AlmostEqual(assignedPosition, 2));
            if (assignedCrystal != null)
            {
                hints.AddForbiddenZone(ShapeContains.InvertedCircle(assignedPosition, 5), WorldState.FutureTime(60));
                hints.AddForbiddenZone(ShapeContains.Circle(Module.Center, 17), DateTime.MaxValue); // prefer to stay near border, unless everything else is covered with aoes
            }
            else
            {
                // go to the ice veil
                // TODO: consider helping other melees with their crystals? a bit dangerous, can misbait
                // TODO: consider helping nearby ranged to bait their cones?
                hints.AddForbiddenZone(ShapeContains.InvertedCone(Module.Center, 7, assignedDir, 10.Degrees()), DateTime.MaxValue);
            }
        }
        else
        {
            // intercardinals - bait cones
            if (_cones?.Casters.Count == 0)
            {
                var assignedPosition = Module.Center + 9 * (180 - 45 * clockSpot).Degrees().ToDirection(); // crystal is at R=8
                var assignedCrystal = CrystalsOfDarkness.FirstOrDefault(c => c.Position.AlmostEqual(assignedPosition, 2));
                if (assignedCrystal != null)
                    hints.AddForbiddenZone(ShapeContains.PrecisePosition(assignedPosition, new WDir(0, 1), Module.Bounds.MapResolution, actor.Position, 0.1f));
            }
            // else: just dodge cones etc...
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(CrystalsOfLight, ArenaColor.Enemy);
        Arena.Actors(CrystalsOfDarkness, ArenaColor.Object);
        Arena.Actor(IceVeil, _iceVeilInvincible ? ArenaColor.Object : ArenaColor.Enemy);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Invincibility)
            _iceVeilInvincible = false;
    }

    private IEnumerable<Actor> ActiveActors(IReadOnlyList<Actor> raw) => raw.Where(a => a.IsTargetable && !a.IsDead);
    private IEnumerable<Actor> CrystalsOfLight => ActiveActors(_crystalsOfLight);
    private IEnumerable<Actor> CrystalsOfDarkness => ActiveActors(_crystalsOfDarkness);
    private Actor? IceVeil => ActiveActors(_iceVeil).FirstOrDefault();

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Icecrusher)
            _gaiaHammer = true;
    }

    private int CrystalPriority(Actor crystal, int clockSpot)
    {
        var offset = crystal.Position - Module.Center;
        var priority = clockSpot switch
        {
            0 => offset.Z < -10,
            2 => offset.X > +10,
            4 => offset.Z > +10,
            6 => offset.X < -10,
            _ => false
        };
        return priority ? 2 : 1;
    }
}
