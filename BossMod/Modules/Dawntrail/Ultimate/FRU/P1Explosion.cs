namespace BossMod.Dawntrail.Ultimate.FRU;

class P1ExplosionBurntStrikeFire(BossModule module) : Components.StandardAOEs(module, AID.ExplosionBurntStrikeFire, new AOEShapeRect(80, 5));
class P1ExplosionBurntStrikeLightning(BossModule module) : Components.StandardAOEs(module, AID.ExplosionBurntStrikeLightning, new AOEShapeRect(80, 5));
class P1ExplosionBurnout(BossModule module) : Components.StandardAOEs(module, AID.ExplosionBurnout, new AOEShapeRect(80, 10));

// TODO: non-fixed conga?
class P1Explosion(BossModule module) : Components.GenericTowers(module)
{
    public WDir TowerDir;
    public DateTime Activation;
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private bool _isWideLine;
    private bool _lineDone;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var role = _config.P1ExplosionsAssignment[assignment];
        if (role < 0 || TowerDir == default)
            return;

        if (role < 2)
        {
            // tanks: stay opposite towers on N/S side (unless cheesing tankbusters)
            // tweak for WAR: if PR is up, assume player will want to maintain full uptime on wide line by using it right before resolve - we want to stay far to increase travel time
            // if doing tankbuster cheese, after line resolves, stay on maxmelee far from towers to give more space for melees
            var horizOffset = !_lineDone
                ? (_isWideLine && actor.Class == Class.WAR && actor.FindStatus(WAR.SID.PrimalRend) != null ? 17 : 0)
                : (_config.P1ExplosionsTankbusterCheese ? 7 : 0);
            hints.AddForbiddenZone(ShapeContains.HalfPlane(Module.Center - horizOffset * TowerDir, -TowerDir), Activation);

            if (!_config.P1ExplosionsTankbusterCheese)
            {
                var vertDir = new WDir(0, role == 0 ? -1 : +1);
                hints.AddForbiddenZone(ShapeContains.HalfPlane(Module.Center + 5 * vertDir, vertDir), Activation);
            }
        }
        else
        {
            // others: soak assigned tower, or at least stay in lane with it (if knockback is imminent, or always for ranged)
            var index = Towers.FindIndex(t => !t.ForbiddenSoakers[slot]);
            if (index >= 0)
            {
                var needSoak = _lineDone || _isWideLine && actor.Role is Role.Healer or Role.Ranged;
                ref var t = ref Towers.Ref(index);
                if (needSoak)
                    hints.AddForbiddenZone(ShapeContains.InvertedCircle(t.Position, t.Radius), t.Activation);
                else
                    hints.AddForbiddenZone(ShapeContains.InvertedRect(new(Module.Center.X, t.Position.Z), TowerDir, 20, 0, t.Radius), t.Activation);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Explosion11:
            case AID.Explosion12:
                AddTower(caster.Position, 1, Module.CastFinishAt(spell));
                break;
            case AID.Explosion21:
            case AID.Explosion22:
                AddTower(caster.Position, 2, Module.CastFinishAt(spell));
                break;
            case AID.Explosion31:
            case AID.Explosion32:
                AddTower(caster.Position, 3, Module.CastFinishAt(spell));
                break;
            case AID.Explosion41:
            case AID.Explosion42:
                AddTower(caster.Position, 4, Module.CastFinishAt(spell));
                break;
            case AID.ExplosionBurnout:
                _isWideLine = true;
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Explosion11:
            case AID.Explosion12:
            case AID.Explosion21:
            case AID.Explosion22:
            case AID.Explosion31:
            case AID.Explosion32:
            case AID.Explosion41:
            case AID.Explosion42:
                ++NumCasts;
                Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
                break;
            case AID.ExplosionBurnout:
            case AID.ExplosionBlastburn:
                _lineDone = true;
                break;
        }
    }

    private void AddTower(WPos pos, int numSoakers, DateTime activation)
    {
        Activation = activation;
        Towers.Add(new(pos, 4, numSoakers, numSoakers, default, activation));
        if (Towers.Count != 3)
            return;

        // init assignments
        if (Towers.Sum(t => t.MinSoakers) != 6)
        {
            ReportError($"Unexpected tower state");
            return;
        }
        Towers.SortBy(t => t.Position.Z);
        TowerDir.X = Towers.Sum(t => t.Position.X - Module.Center.X) > 0 ? 1 : -1;

        Span<int> slotByGroup = [-1, -1, -1, -1, -1, -1, -1, -1];
        foreach (var (slot, group) in _config.P1ExplosionsAssignment.Resolve(Raid))
            slotByGroup[group] = slot;
        if (slotByGroup.Contains(-1))
            return;
        var nextFlex = 5;
        for (int i = 0; i < 3; ++i)
        {
            ref var tower = ref Towers.Ref(i);
            tower.ForbiddenSoakers.Raw = 0xFF;
            tower.ForbiddenSoakers.Clear(slotByGroup[i + 2]); // fixed assignment
            if (tower.MinSoakers == 1)
                continue; // this tower doesn't need anyone else

            if (_config.P1ExplosionsPriorityFill)
            {
                // priority fill strategy - grab assigned flex soaker
                tower.ForbiddenSoakers.Clear(slotByGroup[i + 5]);
                // if the tower requires >2 soakers, also assign each flex soaker that has natural 1-man tower (this works, because only patterns are 2-2-2, 1-2-3 and 1-1-4)
                if (tower.MinSoakers > 2)
                    for (int j = 0; j < 3; ++j)
                        if (Towers[j].MinSoakers == 1)
                            tower.ForbiddenSoakers.Clear(slotByGroup[j + 5]);
            }
            else
            {
                // conga fill strategy - grab next N flex soakers in priority order
                for (int j = 1; j < tower.MinSoakers; ++j)
                    tower.ForbiddenSoakers.Clear(slotByGroup[nextFlex++]);
            }
        }
    }
}
