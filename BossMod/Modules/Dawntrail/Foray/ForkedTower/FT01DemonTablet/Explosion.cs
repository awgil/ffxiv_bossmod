namespace BossMod.Dawntrail.Foray.ForkedTower.FT01DemonTablet;

class Explosion : Components.CastTowers
{
    private readonly ForkedTowerConfig _config = Service.Config.Get<ForkedTowerConfig>();

    public Explosion(BossModule module) : base(module, AID.Explosion, 4, 4, maxSoakers: int.MaxValue)
    {
        EnableHints = false;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            var pos = DeterminePosition(caster, spell);
            var towerAssignment = AssignmentUtil.GetTowerAssignment(pos);

            var playerAssign = _config.PlayerAlliance;

            var allowed = playerAssign == ForkedTowerConfig.Alliance.None || playerAssign == towerAssignment;

            Towers.Add(new(pos, Radius, MinSoakers, MaxSoakers, activation: Module.CastFinishAt(spell), forbiddenSoakers: allowed ? default : new BitMask(0xff)));
        }
    }
}

// we can't draw 6 towers when they only have 3 unique positions, plus the player only gets one chance to levitate, so we ignore the levitating towers entirely and treat the ground set as the only ones
class GravityExplosion : Components.CastTowers
{
    private readonly ForkedTowerConfig _config = Service.Config.Get<ForkedTowerConfig>();

    public GravityExplosion(BossModule module) : base(module, AID.ExplosionGround, 4, minSoakers: 4, maxSoakers: int.MaxValue)
    {
        EnableHints = false;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            var pos = DeterminePosition(caster, spell);
            var towerAssignment = AssignmentUtil.GetTowerAssignment(pos);

            var playerAssign = _config.PlayerAlliance.Group3() switch
            {
                1 => ForkedTowerConfig.Alliance.A,
                2 => ForkedTowerConfig.Alliance.B,
                3 => ForkedTowerConfig.Alliance.C,
                _ => _config.PlayerAlliance
            };

            var allowed = playerAssign == ForkedTowerConfig.Alliance.None || playerAssign == towerAssignment;

            Towers.Add(new(pos, Radius, MinSoakers, MaxSoakers, activation: Module.CastFinishAt(spell), forbiddenSoakers: allowed ? default : new BitMask(0xff)));
        }
    }
}
class EraseGravity : Components.StandardAOEs
{
    private readonly ForkedTowerConfig _config = Service.Config.Get<ForkedTowerConfig>();

    public EraseGravity(BossModule module) : base(module, AID.EraseGravity, 4)
    {
        Risky = false;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var shouldLevitate = _config.PlayerAlliance.Group2() == 2;

        foreach (var aoe in base.ActiveAOEs(slot, actor))
            yield return aoe with { Color = shouldLevitate ? ArenaColor.SafeFromAOE : ArenaColor.AOE, Risky = Risky && !shouldLevitate };
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Risky)
            return;

        var shouldLevitate = _config.PlayerAlliance.Group2() == 2;

        if (Casters.Count > 0)
        {
            var inZone = Casters.Any(c => actor.Position.InCircle(c.CastInfo!.LocXZ, 4));
            if (shouldLevitate)
                hints.Add("Go to statue!", !inZone);
            else if (inZone)
                hints.Add("GTFO from aoe!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Risky)
            return;

        var shouldLevitate = _config.PlayerAlliance.Group2() == 2;

        var zones = Casters.Select(c => ShapeContains.Circle(c.CastInfo!.LocXZ, 4)).ToList();
        if (zones.Count > 0)
        {
            var union = ShapeContains.Union(zones);
            if (shouldLevitate)
                hints.AddForbiddenZone(p => !union(p), Module.CastFinishAt(Casters[0].CastInfo));
            else
                hints.AddForbiddenZone(p => union(p), Module.CastFinishAt(Casters[0].CastInfo));
        }
    }
}
