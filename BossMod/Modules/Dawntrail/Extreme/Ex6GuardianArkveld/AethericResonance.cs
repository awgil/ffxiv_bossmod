namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

abstract class ResonanceTower : Components.CastTowers
{
    private DateTime _lastBaitIn;

    protected ResonanceTower(BossModule module, AID aid, float radius, bool tankbuster) : base(module, aid, radius, maxSoakers: 2, damageType: tankbuster ? AIHints.PredictedDamageType.Tankbuster : AIHints.PredictedDamageType.Raidwide)
    {
        EnableHints = false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (_lastBaitIn != default)
            foreach (var t in Towers)
                // prevent baits from dropping in the middle 1.5y of each tower - small tower is only 2y and large tower tanks should have plenty of room
                hints.AddForbiddenZone(ShapeContains.Circle(t.Position, 7.5f), _lastBaitIn);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (_lastBaitIn != default && Towers.Any(t => actor.Position.InCircle(t.Position, 7.5f)))
            hints.Add("Bait away from towers!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (Towers.Count > 0 && !EnableHints)
            Arena.AddCircle(pc.Position, 6, ArenaColor.Danger);
    }

    public void EnableBaitHints(float deadline)
    {
        _lastBaitIn = WorldState.FutureTime(deadline);
    }

    public void DisableBaitHints()
    {
        _lastBaitIn = default;
        EnableHints = true;
    }
}

class GuardianResonanceTowerSmall(BossModule module) : ResonanceTower(module, AID.GuardianResonanceTowerSmall, 2, false);

class GuardianResonanceTowerLarge(BossModule module) : ResonanceTower(module, AID.GuardianResonanceTowerLarge, 4, true)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction)
            foreach (ref var t in Towers.AsSpan())
                t.ForbiddenSoakers |= Raid.WithSlot().WhereActor(a => a.Role != Role.Tank).Mask();
    }
}
