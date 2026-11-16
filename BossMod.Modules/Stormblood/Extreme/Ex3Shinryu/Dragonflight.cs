namespace BossMod.Stormblood.Extreme.Ex3Shinryu;

class Dragonflight : Components.CastHint
{
    private const float HealWindow = 13;
    public int NumWaves { get; private set; }
    private DateTime _lastSpawn;
    public bool WaveJustSpawned => _lastSpawn != default && (WorldState.CurrentTime - _lastSpawn).TotalSeconds < 5;

    public Dragonflight(BossModule module) : base(module, AID.Dragonflight, "Heal Reiryu to full!", true)
    {
        KeepOnPhaseChange = true;
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID != OID.Reiryu)
            return;
        if (WorldState.CurrentTime > _lastSpawn.AddSeconds(5))
            ++NumWaves;
        _lastSpawn = WorldState.CurrentTime;
    }

    private DateTime Deadline(Actor r) => r.CastInfo?.IsSpell(AID.Dragonflight) == true ? Module.CastFinishAt(r.CastInfo) : _lastSpawn.AddSeconds(HealWindow);

    public override void AddGlobalHints(GlobalHints hints)
    {
        var unhealed = Module.Enemies(OID.Reiryu).Where(r => !r.IsDead && r.IsTargetable && r.HPMP.CurHP < r.HPMP.MaxHP && Deadline(r) > WorldState.CurrentTime).ToList();
        if (unhealed.Count > 0)
            hints.Add($"Heal all Reiryu to full: {unhealed.Count} left, {(unhealed.Min(Deadline) - WorldState.CurrentTime).TotalSeconds:f0}s");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var r in Module.Enemies(OID.Reiryu))
            hints.SetPriority(r, AIHints.Enemy.PriorityForbidden);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) => Arena.Actors(Module.Enemies(OID.Reiryu), ArenaColor.Vulnerable);
}
