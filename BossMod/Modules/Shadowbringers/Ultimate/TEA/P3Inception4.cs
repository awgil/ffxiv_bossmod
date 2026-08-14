namespace BossMod.Shadowbringers.Ultimate.TEA;

[SkipLocalsInit]
sealed class P3Inception4Cleaves(BossModule module) : Components.GenericBaitAway(module, (uint)AID.AlphaSwordP3)
{
    private readonly AOEShapeCone _shape = new(30f, 45f.Degrees());
    private readonly TEA bossmod = (TEA)module;

    public override void Update()
    {
        CurrentBaits.Clear();
        var source = bossmod.CruiseChaser();
        if (source != null)
            CurrentBaits.AddRange(Raid.WithoutSlot(false, true, true).SortedByRange(source.Position).Take(3).Select(t => new Bait(source, t, _shape)));
    }
}

sealed class P3Inception4Hints(BossModule module) : BossComponent(module)
{
    private List<WPos>[]? _safespots;
    private readonly TEA bossmod = (TEA)module;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SacramentInception)
        {
            Init();
        }
    }

    private void Init()
    {
        _safespots = new List<WPos>[8];

        var jumpSrc = bossmod.BruteJustice();
        if (jumpSrc == null)
        {
            return;
        }

        var center = Arena.Center;
        var bjDir = Angle.FromDirection(jumpSrc.Position - center).ToDirection();

        var raid = Raid.WithSlot(true, true, true);
        var len = raid.Length;
        for (var i = 0; i < len; ++i)
        {
            var p = raid[i];
            var slot = p.Item1;
            var actor = p.Item2;
            _safespots[slot] = [with(2)];

            // phys vuln, player can't bait alpha sword
            if (actor.FindStatus((uint)SID.PhysicalVulnerabilityUp, DateTime.MaxValue) != null)
            {
                // wait on far side of CC
                _safespots[slot].Add(center - bjDir * 7f);

                // remind both tanks to bait super jump (TODO: add config option to define tank prio)
                if (actor.Role == Role.Tank)
                {
                    _safespots[slot].Add(center - bjDir * 18.5f);
                }
            }
            else if (actor.Role == Role.Healer)
            {
                // healers bait alpha north/south (TODO: add config option to define healer prio)
                _safespots[slot].Add(center + bjDir.OrthoR() * 2.5f);
                _safespots[slot].Add(center + bjDir.OrthoL() * 2.5f);
            }
            else
            {
                // remaining dps bait await from party
                _safespots[slot].Add(center + bjDir * 2.5f);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_safespots != null && _safespots[pcSlot] is List<WPos> safespots)
        {
            var count = safespots.Count;
            var color = Colors.Safe;
            for (var i = 0; i < count; ++i)
            {
                Arena.ZoneCircleOutline(safespots[i], 1f, color);
            }
        }
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (_safespots != null && _safespots[slot] is List<WPos> safespots)
        {
            var count = safespots.Count;
            var color = Colors.Safe;
            var pos = actor.Position;
            for (var i = 0; i < count; ++i)
            {
                movementHints.Add((pos, safespots[i], color));
            }
        }
    }
}
