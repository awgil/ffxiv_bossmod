namespace BossMod.Dawntrail.Quantum.Q01TheFinalVerse;

// TODO: add component for Abyssal Sun raidwide (5.1s after director update)
class AbyssalSunTower : Components.GenericTowers
{
    private static readonly WPos[] _towerPositions = [
        new(-607.78f, -307.78f),
        new(-592.22f, -307.78f),
        new(-607.78f, -292.22f),
        new(-592.22f, -292.22f),
    ];

    private BitMask _forbiddenPlayers;

    public AbyssalSunTower(BossModule module) : base(module)
    {
        EnableHints = false;

        _forbiddenPlayers = Raid.WithSlot().WhereActor(a => a.FindStatus(SID.DarkVengeance) != null).Mask();

        Towers.AddRange(_towerPositions.Select(p => new Tower(p, 2, forbiddenSoakers: _forbiddenPlayers)));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        // TODO: this is unnecessary in quantum <40
        if (EnableHints && _forbiddenPlayers[slot])
            hints.Add("Get light debuff!");
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.LightVengeance && Raid.TryFindSlot(actor, out var slot))
        {
            _forbiddenPlayers.Clear(slot);
            UpdateMask();
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.LightVengeance && Raid.TryFindSlot(actor, out var slot))
        {
            _forbiddenPlayers.Set(slot);
            UpdateMask();
        }
    }

    private void UpdateMask()
    {
        for (var i = 0; i < Towers.Count; i++)
            Towers.Ref(i).ForbiddenSoakers = _forbiddenPlayers;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        var off = index - 0x1B;
        if (off is >= 0 and < 4)
        {
            if (state == 0x00080004)
                Towers.RemoveAll(t => t.Position == _towerPositions[off]);
        }
    }
}

class BallOfFire(BossModule module) : Components.StandardAOEs(module, AID.BallOfFirePuddle, 6);
class Eruption(BossModule module) : Components.StandardAOEs(module, AID.Eruption, 6);

class UnholyDarkness(BossModule module) : Components.RaidwideCast(module, AID.UnholyDarkness);
class UnholyDarknessEnrage(BossModule module) : Components.RaidwideCast(module, AID.UnholyDarknessEnrage);

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1063, NameID = 14037, PlanLevel = 100)]
public class Q01TheFinalVerse(WorldState ws, Actor primary) : BossModule(ws, primary, new(-600, -300), new ArenaBoundsRect(20, 15))
{
    private Actor? _eater;

    public Actor? Eater() => _eater;

    public override Actor? GetDefaultTarget(int slot)
    {
        if (Raid[slot] is { } player)
        {
            if (player.FindStatus(SID.DarkVengeance) != null)
                return Enemies(OID.DevouredEater).FirstOrDefault();

            if (player.FindStatus(SID.LightVengeance) != null)
                return PrimaryActor;
        }

        return null;
    }

    protected override void UpdateModule()
    {
        _eater ??= Enemies(OID.DevouredEater).FirstOrDefault();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actor(_eater, ArenaColor.Enemy);
    }
}
