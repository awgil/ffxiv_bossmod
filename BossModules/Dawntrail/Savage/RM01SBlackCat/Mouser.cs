namespace BossMod.Dawntrail.Savage.RM01SBlackCat;

// note: grid cell indices are same as ENVC indices: 0 for NW, then increasing along x, then increasing along z (so NE is 3, SW is 12, SE is 15)
// normally, boss does 8 sets of 3 jumps then 2 sets of 2 jumps, destroying 12 cells and damaging remaining 4
// on enrage, boss does 8 sets of 4 jumps, destroying all cells
class Mouser(BossModule module) : Components.GenericAOEs(module)
{
    public BitMask DamagedCells;
    public BitMask DestroyedCells;
    private readonly List<AOEInstance> _jumps = [];

    public static readonly AOEShapeRect CellShape = new(5, 5, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var i in DestroyedCells.SetBits())
            yield return new(CellShape, CellCenter(i));
        var deadline = _jumps.Count == 0 ? default : _jumps.Count + NumCasts < 32 ? _jumps[0].Activation.AddSeconds(2.5) : _jumps[^1].Activation.AddSeconds(-0.5f); // jumps happen every second, we show three next sets
        foreach (var j in _jumps.TakeWhile(j => j.Activation < deadline).Where(j => !DestroyedCells[CellIndex(j.Origin)]))
            yield return j;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        for (int x = -10; x <= 10; x += 10)
            Arena.AddLine(Module.Center + new WDir(x, -20), Module.Center + new WDir(x, 20), ArenaColor.Border);
        for (int z = -10; z <= 10; z += 10)
            Arena.AddLine(Module.Center + new WDir(-20, z), Module.Center + new WDir(20, z), ArenaColor.Border);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.MouserPrepareJump1 or AID.MouserPrepareJump2)
            _jumps.Add(new(CellShape, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 10.5f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.MouserTileDamage)
        {
            ++NumCasts;
            var index = _jumps.FindIndex(j => j.Origin.AlmostEqual(caster.Position, 1));
            if (index >= 0)
                _jumps.RemoveAt(index);
            else
                ReportError($"Unexpected jump at {caster.Position}");
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index > 15)
            return;

        switch (state)
        {
            case 0x00020001: // damage tile (first jump)
                DamagedCells.Set(index);
                DestroyedCells.Clear(index);
                break;
            case 0x00200010: // destroy tile (second jump)
                DamagedCells.Clear(index);
                DestroyedCells.Set(index);
                break;
            case 0x01000004: // repair destroyed tile (after initial jumps)
            case 0x00800004: // repair damaged tile (mechanic end)
            case 0x00080004: // start short repair (will finish before kb)
                DamagedCells.Clear(index);
                DestroyedCells.Clear(index);
                break;
            case 0x00400004: // start long repair (won't finish before kb)
                break;
        }
    }

    public BitMask IntactCells => new BitMask(0xffff) ^ DamagedCells ^ DestroyedCells;

    public WPos CellCenter(int index)
    {
        var x = -15 + 10 * (index & 3);
        var z = -15 + 10 * (index >> 2);
        return Module.Center + new WDir(x, z);
    }

    public int CellIndex(WPos pos)
    {
        var off = pos - Module.Center;
        return (CoordinateIndex(off.Z) << 2) | CoordinateIndex(off.X);
    }

    private int CoordinateIndex(float coord) => coord switch
    {
        < -10 => 0,
        < 0 => 1,
        < 10 => 2,
        _ => 3
    };
}

class ElevateAndEviscerate(BossModule module) : Components.CastCounter(module, AID.ElevateAndEviscerateShockwave)
{
    private readonly Mouser? _mouser = module.FindComponent<Mouser>();
    private Actor? _nextTarget; // target selection icon appears in random order compared to cast start
    public Actor? CurrentTarget; // for current mechanic
    private DateTime _currentDeadline; // for current target - expected time when stun starts, which is deadline for positioning
    private int _currentKnockbackDistance;

    private static readonly AOEShapeCross _shockwave = new(60, 5);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_mouser == null)
            return;

        if (CurrentTarget == actor)
        {
            hints.Add($"{(_currentKnockbackDistance > 0 ? "Knockback" : "Hit")} in {Math.Max(0, (_currentDeadline - Module.WorldState.CurrentTime).TotalSeconds):f1}s", false);

            var adjPos = actor.Position;
            if (_currentKnockbackDistance > 0)
                adjPos += _currentKnockbackDistance * actor.Rotation.ToDirection();
            if (!_mouser.IntactCells[_mouser.CellIndex(adjPos)])
                hints.Add("Aim to intact cell!");
        }
        else if (CurrentTarget != null)
        {
            if (_shockwave.Check(actor.Position, _mouser.CellCenter(_mouser.CellIndex(CurrentTarget.Position)), default))
                hints.Add("GTFO from shockwave!");
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => player == CurrentTarget ? PlayerPriority.Danger : PlayerPriority.Irrelevant;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_mouser == null)
            return;

        if (CurrentTarget == pc)
        {
            foreach (var i in _mouser.IntactCells.SetBits())
                Mouser.CellShape.Draw(Arena, _mouser.CellCenter(i), default, ArenaColor.SafeFromAOE);
        }
        else if (CurrentTarget != null)
        {
            _shockwave.Draw(Arena, _mouser.CellCenter(_mouser.CellIndex(CurrentTarget.Position)), default);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (CurrentTarget == pc && _currentKnockbackDistance > 0)
        {
            Components.Knockback.DrawKnockback(pc, pc.Position + _currentKnockbackDistance * pc.Rotation.ToDirection(), Arena);
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID != (uint)IconID.ElevateAndEviscerate)
            return;
        if (_nextTarget != null)
            ReportError($"Next target icon before previous was consumed");

        _nextTarget = actor;
        if (_currentDeadline != default)
            CurrentTarget = actor;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ElevateAndEviscerateKnockback or AID.ElevateAndEviscerateHit)
        {
            if (_currentDeadline != default)
            {
                ReportError("Cast started before previous jump is resolved");
                return;
            }

            CurrentTarget = _nextTarget;
            _currentDeadline = Module.CastFinishAt(spell, 1.8f);
            _currentKnockbackDistance = (AID)spell.Action.ID == AID.ElevateAndEviscerateKnockback ? 10 : 0;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            _nextTarget = CurrentTarget = null;
            _currentDeadline = default;
            _currentKnockbackDistance = 0;
        }
    }
}

class GrimalkinGaleShockwave(BossModule module) : Components.KnockbackFromCastTarget(module, AID.GrimalkinGaleShockwaveAOE, 21, true)
{
    private readonly Mouser? _mouser = module.FindComponent<Mouser>();

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !Module.InBounds(pos) || _mouser != null && _mouser.DestroyedCells[_mouser.CellIndex(pos)];
}

class GrimalkinGaleSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID.GrimalkinGaleSpreadAOE, 5);

class Overshadow(BossModule module) : Components.GenericWildCharge(module, 2.5f, AID.OvershadowAOE, 100)
{
    private readonly ElevateAndEviscerate? _jumps = module.FindComponent<ElevateAndEviscerate>();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Overshadow)
        {
            Source = caster;
            foreach (var (i, p) in Raid.WithSlot(true))
                PlayerRoles[i] = p.InstanceID == spell.TargetID ? PlayerRole.Target : _jumps?.CurrentTarget == p ? PlayerRole.Ignore : PlayerRole.Share;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            Source = null;
            ++NumCasts;
        }
    }
}

class SplinteringNails(BossModule module) : Components.CastCounter(module, AID.SplinteringNailsAOE)
{
    private readonly ElevateAndEviscerate? _jumps = module.FindComponent<ElevateAndEviscerate>();
    private Actor? _source;

    private static readonly AOEShapeCone _shape = new(100, 25.Degrees()); // TODO: verify angle

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_source != null && _jumps?.CurrentTarget != actor)
        {
            var pcRole = EffectiveRole(actor);
            var pcDir = Angle.FromDirection(actor.Position - _source.Position);
            if (Raid.WithoutSlot().Exclude(_jumps?.CurrentTarget).Any(a => EffectiveRole(a) != pcRole && _shape.Check(a.Position, _source.Position, pcDir)))
                hints.Add("Spread by roles!");
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => _source == null || _jumps?.CurrentTarget == pc || _jumps?.CurrentTarget == player ? PlayerPriority.Irrelevant : EffectiveRole(player) == EffectiveRole(pc) ? PlayerPriority.Normal : PlayerPriority.Interesting;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_source != null && _jumps?.CurrentTarget != pc)
        {
            var pcDir = Angle.FromDirection(pc.Position - _source.Position);
            _shape.Outline(Arena, _source.Position, pcDir);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SplinteringNails)
        {
            _source = caster;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            _source = null;
        }
    }

    private Role EffectiveRole(Actor a) => a.Role == Role.Ranged ? Role.Melee : a.Role;
}
