
namespace BossMod.Dawntrail.Savage.RM01SBlackCat;

// note: grid cell indices are same as ENVC indices: 0 for SW, then increasing along x, then increasing along z (so SE is 3, NW is 12, NE is 15)
// normally, boss does 8 sets of 3 jumps then 2 sets of 2 jumps, destroying 12 cells and damaging remaining 4
// on enrage, ...
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
        var deadline = _jumps.Count > 0 ? _jumps[0].Activation.AddSeconds(2.5) : default; // jumps happen every second, we show three next sets
        foreach (var j in _jumps.TakeWhile(j => j.Activation < deadline))
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

    public override void OnEventEnvControl(byte index, uint state)
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

class ElevateAndEviscerate(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.ElevateAndEviscerateShockwave))
{
    private readonly Mouser? _mouser = module.FindComponent<Mouser>();
    private Actor? _nextTarget; // target selection icon appears before cast start
    private Actor? _currentTarget; // for current mechanic
    private DateTime _currentDeadline; // for current target - expected time when stun starts, which is deadline for positioning
    private int _currentKnockbackDistance;

    private static readonly AOEShapeCross _shockwave = new(60, 5);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_mouser == null)
            return;

        if (_currentTarget == actor)
        {
            hints.Add($"{(_currentKnockbackDistance > 0 ? "Knockback" : "Hit")} in {Math.Max(0, (_currentDeadline - Module.WorldState.CurrentTime).TotalSeconds):f1}s", false);

            var adjPos = actor.Position;
            if (_currentKnockbackDistance > 0)
                adjPos += _currentKnockbackDistance * actor.Rotation.ToDirection();
            if (!_mouser.IntactCells[_mouser.CellIndex(adjPos)])
                hints.Add("Aim to intact cell!");
        }
        else if (_currentTarget != null)
        {
            if (_shockwave.Check(actor.Position, _mouser.CellCenter(_mouser.CellIndex(_currentTarget.Position)), default))
                hints.Add("GTFO from shockwave!");
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => player == _currentTarget ? PlayerPriority.Danger : PlayerPriority.Irrelevant;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_mouser == null)
            return;

        if (_currentTarget == pc)
        {
            foreach (var i in _mouser.IntactCells.SetBits())
                Mouser.CellShape.Draw(Arena, _mouser.CellCenter(i), default, ArenaColor.SafeFromAOE);
        }
        else if (_currentTarget != null)
        {
            _shockwave.Draw(Arena, _mouser.CellCenter(_mouser.CellIndex(_currentTarget.Position)), default);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_currentTarget == pc && _currentKnockbackDistance > 0)
        {
            Components.Knockback.DrawKnockback(pc, pc.Position + _currentKnockbackDistance * pc.Rotation.ToDirection(), Arena);
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID != (uint)IconID.ElevateAndEviscerate)
            return;
        if (_nextTarget != null)
            ReportError($"Next target icon before previous was consumed");
        _nextTarget = actor;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ElevateAndEviscerateKnockback or AID.ElevateAndEviscerateHit)
        {
            if (_nextTarget == null)
            {
                ReportError("Cast started before target selection");
                return;
            }

            _currentTarget = _nextTarget;
            _currentDeadline = Module.CastFinishAt(spell, 1.8f);
            _currentKnockbackDistance = (AID)spell.Action.ID == AID.ElevateAndEviscerateKnockback ? 10 : 0;
            _nextTarget = null;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            _currentTarget = null;
            _currentDeadline = default;
            _currentKnockbackDistance = 0;
        }
    }
}

class GrimalkinGaleShockwave(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.GrimalkinGaleShockwaveAOE), 21, true)
{
    private readonly Mouser? _mouser = module.FindComponent<Mouser>();

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !Module.InBounds(pos) || _mouser != null && _mouser.DestroyedCells[_mouser.CellIndex(pos)];
}

class GrimalkinGaleSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.GrimalkinGaleSpreadAOE), 5);
