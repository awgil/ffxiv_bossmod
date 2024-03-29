namespace BossMod.Endwalker.Savage.P4S2Hesperos;

// state related to act 4 wreath of thorns
class WreathOfThorns4 : BossComponent
{
    public bool ReadyToBreak;
    private IconID[] _playerIcons = new IconID[8];
    private Actor?[] _playerTetherSource = new Actor?[8];
    private List<Actor>? _darkOrder; // contains sources
    private int _doneTowers = 0;
    private int _activeTethers = 0;

    private static readonly float _waterExplosionRange = 10;

    public override void Update(BossModule module)
    {
        if (_darkOrder == null && _activeTethers == 8)
        {
            // build order for dark explosion; TODO: this is quite hacky right now, and probably should be configurable
            // current logic assumes we break N or NW tether first, and then move clockwise
            _darkOrder = new();
            var c = module.Bounds.Center;
            AddAOETargetToOrder(_darkOrder, p => p.Z < c.Z && p.X <= c.X);
            AddAOETargetToOrder(_darkOrder, p => p.X > c.X && p.Z <= c.Z);
            AddAOETargetToOrder(_darkOrder, p => p.Z > c.Z && p.X >= c.X);
            AddAOETargetToOrder(_darkOrder, p => p.X < c.X && p.Z >= c.Z);
        }
        else if (_darkOrder?.Count == _activeTethers + 1 && _darkOrder[0].Tether.Target != 0)
        {
            // update order if unexpected tether was the first one to break
            if (_darkOrder[1].Tether.Target == 0)
            {
                var moved = _darkOrder.First();
                _darkOrder.RemoveAt(0);
                _darkOrder.Add(moved);
            }
            else if (_darkOrder[_darkOrder.Count - 1].Tether.Target == 0)
            {
                var moved = _darkOrder.Last();
                _darkOrder.RemoveAt(_darkOrder.Count - 1);
                _darkOrder.Insert(0, moved);
            }
        }
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (!ReadyToBreak)
            return;

        if (_doneTowers < 4)
        {
            if (_playerIcons[slot] == IconID.AkanthaiWater)
            {
                hints.Add("Break tether!");
                if (module.Raid.WithoutSlot().InRadiusExcluding(actor, _waterExplosionRange).Any())
                {
                    hints.Add("GTFO from others!");
                }
            }
            else if (_playerIcons[slot] == IconID.AkanthaiDark)
            {
                var soakedTower = _playerTetherSource.Zip(_playerIcons).Where(si => si.Item1 != null && si.Item2 == IconID.AkanthaiWater).Select(si => si.Item1!).InRadius(actor.Position, P4S2.WreathTowerRadius).FirstOrDefault();
                hints.Add("Soak the tower!", soakedTower == null);
            }
        }
        else
        {
            var nextAOE = NextAOE();
            if (nextAOE != null)
            {
                if (nextAOE.Tether.Target == actor.InstanceID)
                {
                    hints.Add("Break tether!");
                }
                if (actor.Position.InCircle(nextAOE.Position, P4S2.WreathAOERadius))
                {
                    hints.Add("GTFO from AOE!");
                }
            }
        }
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (_darkOrder != null && _activeTethers > 0)
        {
            var skip = 4 - Math.Min(_activeTethers, 4);
            hints.Add($"Dark order: {string.Join(" -> ", _darkOrder.Skip(skip).Select(src => module.WorldState.Actors.Find(src.Tether.Target)?.Name ?? "???"))}");
        }
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (_doneTowers < 4)
            return;
        var nextAOE = NextAOE();
        if (nextAOE != null)
            arena.ZoneCircle(nextAOE.Position, P4S2.WreathAOERadius, ArenaColor.AOE);
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        // draw other players
        foreach ((int slot, var player) in module.Raid.WithSlot().Exclude(pc))
        {
            var icon = _playerIcons[slot];
            bool nextBreaking = _doneTowers < 4 ? icon == IconID.AkanthaiWater : (icon == IconID.AkanthaiDark && NextAOE()?.Tether.Target == player.InstanceID);
            arena.Actor(player, nextBreaking ? ArenaColor.Danger : ArenaColor.PlayerGeneric);
        }

        // tether
        var pcTetherSource = _playerTetherSource[pcSlot];
        if (pcTetherSource == null)
            return; // pc is not tethered anymore, nothing to draw...

        var pcIcon = _playerIcons[pcSlot];
        arena.AddLine(pc.Position, pcTetherSource.Position, pcIcon == IconID.AkanthaiWater ? 0xffff8000 : 0xffff00ff);

        if (_doneTowers < 4)
        {
            if (pcIcon == IconID.AkanthaiWater)
            {
                // if player has blue => show AOE radius around him and single safe spot
                arena.AddCircle(pc.Position, _waterExplosionRange, ArenaColor.Danger);
                arena.AddCircle(DetermineWaterSafeSpot(module, pcTetherSource), 1, ArenaColor.Safe);
            }
            else
            {
                // if player has dark => show AOE radius around blue players and single tower to soak
                foreach ((var player, var icon) in module.Raid.Members.Zip(_playerIcons))
                {
                    if (icon == IconID.AkanthaiWater && player != null)
                    {
                        arena.AddCircle(player.Position, _waterExplosionRange, ArenaColor.Danger);
                    }
                }
                var tower = DetermineTowerToSoak(module, pcTetherSource);
                if (tower != null)
                {
                    arena.AddCircle(tower.Position, P4S2.WreathTowerRadius, ArenaColor.Safe);
                }
            }
        }
    }

    public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.Helper)
        {
            var slot = module.Raid.FindSlot(tether.Target);
            if (slot >= 0)
            {
                _playerTetherSource[slot] = source;
                ++_activeTethers;
            }
        }
    }

    public override void OnUntethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.Helper)
        {
            var slot = module.Raid.FindSlot(tether.Target);
            if (slot >= 0)
            {
                _playerTetherSource[slot] = null;
                --_activeTethers;
            }
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AkanthaiExplodeTower)
            ++_doneTowers;
    }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        var slot = module.Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
            _playerIcons[slot] = (IconID)iconID;
    }

    private void AddAOETargetToOrder(List<Actor> order, Predicate<WPos> sourcePred)
    {
        var source = _playerTetherSource.Zip(_playerIcons).Where(si => si.Item2 == IconID.AkanthaiDark && si.Item1 != null && sourcePred(si.Item1.Position)).FirstOrDefault().Item1;
        if (source != null)
            order.Add(source);
    }

    private WPos RotateCW(BossModule module, WPos pos, Angle angle, float radius)
    {
        var dir = Angle.FromDirection(pos - module.Bounds.Center) - angle;
        return module.Bounds.Center + radius * dir.ToDirection();
    }

    private WPos DetermineWaterSafeSpot(BossModule module, Actor source)
    {
        bool ccw = Service.Config.Get<P4S2Config>().Act4WaterBreakCCW;
        Angle dir = (ccw ? -3 : 3) * 45.Degrees();
        return RotateCW(module, source.Position, dir, 18);
    }

    private Actor? DetermineTowerToSoak(BossModule module, Actor source)
    {
        bool ccw = Service.Config.Get<P4S2Config>().Act4DarkSoakCCW;
        var pos = RotateCW(module, source.Position, (ccw ? -1 : 1) * 45.Degrees(), 18);
        return _playerTetherSource.Where(x => x != null && x.Position.InCircle(pos, 4)).FirstOrDefault();
    }

    private Actor? NextAOE()
    {
        int nextIndex = Math.Max(0, 4 - _activeTethers);
        return _darkOrder != null && nextIndex < _darkOrder.Count ? _darkOrder[nextIndex] : null;
    }
}
