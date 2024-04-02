namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to darkblaze twister mechanics
class DarkblazeTwister : BossComponent
{
    private static readonly float _knockbackRange = 17;
    private static readonly float _aoeInnerRadius = 5;
    private static readonly float _aoeMiddleRadius = 7;
    private static readonly float _aoeOuterRadius = 20;

    public IEnumerable<Actor> BurningTwisters(BossModule module) => module.Enemies(OID.DarkblazeTwister).Where(twister => twister.CastInfo?.IsSpell(AID.BurningTwister) ?? false);
    public Actor? DarkTwister(BossModule module) => module.Enemies(OID.DarkblazeTwister).FirstOrDefault(twister => twister.CastInfo?.IsSpell(AID.DarkTwister) ?? false);

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        var adjPos = Components.Knockback.AwayFromSource(actor.Position, DarkTwister(module), _knockbackRange);
        if (actor.Position != adjPos && !module.Bounds.Contains(adjPos))
        {
            hints.Add("About to be knocked back into wall!");
        }

        foreach (var twister in BurningTwisters(module))
        {
            if (adjPos.InCircle(twister.Position, _aoeInnerRadius) || adjPos.InDonut(twister.Position, _aoeMiddleRadius, _aoeOuterRadius))
            {
                hints.Add("GTFO from aoe!");
                break;
            }
        }
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var twister in BurningTwisters(module))
        {
            arena.ZoneCircle(twister.Position, _aoeInnerRadius, ArenaColor.AOE);
            arena.ZoneDonut(twister.Position, _aoeMiddleRadius, _aoeOuterRadius, ArenaColor.AOE);
        }
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        var darkTwister = DarkTwister(module);
        if (darkTwister == null)
            return;

        var adjPos = Components.Knockback.AwayFromSource(pc.Position, darkTwister, _knockbackRange);
        if (adjPos != pc.Position)
        {
            arena.AddLine(pc.Position, adjPos, ArenaColor.Danger);
            arena.Actor(adjPos, pc.Rotation, ArenaColor.Danger);
        }

        var safeOffset = _knockbackRange + (_aoeInnerRadius + _aoeMiddleRadius) / 2;
        var safeRadius = (_aoeMiddleRadius - _aoeInnerRadius) / 2;
        foreach (var burningTwister in BurningTwisters(module))
        {
            var dir = burningTwister.Position - darkTwister.Position;
            var len = dir.Length();
            dir /= len;
            arena.AddCircle(darkTwister.Position + dir * (len - safeOffset), safeRadius, ArenaColor.Safe);
        }
    }
}
