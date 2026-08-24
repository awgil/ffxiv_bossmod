namespace BossMod.Shadowbringers.Ultimate.TEA;

class P3Inception4Cleaves(BossModule module) : Components.GenericBaitAway(module, AID.AlphaSwordP3)
{
    private static readonly AOEShapeCone _shape = new(30, 45.Degrees()); // TODO: verify angle

    public override void Update()
    {
        CurrentBaits.Clear();
        var source = ((TEA)Module).CruiseChaser();
        if (source != null)
            CurrentBaits.AddRange(Raid.WithoutSlot().SortedByRange(source.Position).Take(3).Select(t => new Bait(source, t, _shape)));
    }
}

class P3Inception4Hints(BossModule module) : BossComponent(module)
{
    private List<WPos>[]? _safespots;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SacramentInception)
            Init();
    }

    private void Init()
    {
        _safespots = new List<WPos>[8];

        var jumpSrc = Module.Enemies(OID.BruteJustice).FirstOrDefault();
        if (jumpSrc == null)
        {
            ReportError($"Brute Justice not found while initializing Inception hints, wtf?");
            for (var i = 0; i < _safespots.Length; i++)
                _safespots[i] = [];
            return;
        }

        var bjDir = Angle.FromDirection(jumpSrc.Position - Arena.Center).ToDirection();

        foreach (var (slot, actor) in Raid.WithSlot())
        {
            _safespots[slot] = [];

            // phys vuln, player can't bait alpha sword
            if (actor.FindStatus(SID.PhysicalVulnerabilityUp, DateTime.MaxValue) != null)
            {
                // wait on far side of CC
                _safespots[slot].Add(Arena.Center - bjDir * 7);

                // remind both tanks to bait super jump (TODO: add config option to define tank prio)
                if (actor.Role == Role.Tank)
                    _safespots[slot].Add(Arena.Center - bjDir * 18.5f);
            }
            else if (actor.Role == Role.Healer)
            {
                // healers bait alpha north/south (TODO: add config option to define healer prio)
                _safespots[slot].Add(Arena.Center + bjDir.OrthoR() * 2.5f);
                _safespots[slot].Add(Arena.Center + bjDir.OrthoL() * 2.5f);
            }
            else
            {
                // remaining dps bait await from party
                _safespots[slot].Add(Arena.Center + bjDir * 2.5f);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_safespots != null)
        {
            foreach (var spot in _safespots[pcSlot])
                Arena.AddCircle(spot, 1, ArenaColor.Safe);
        }
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (_safespots != null)
        {
            foreach (var spot in _safespots[slot])
                movementHints.Add((actor.Position, spot, ArenaColor.Safe));
        }
    }
}
