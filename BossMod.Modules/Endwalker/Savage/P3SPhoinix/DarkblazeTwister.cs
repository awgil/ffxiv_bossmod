namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to darkblaze twister mechanics
class DarkblazeTwister(BossModule module) : BossComponent(module)
{
    private const float _knockbackRange = 17;
    private const float _aoeInnerRadius = 5;
    private const float _aoeMiddleRadius = 7;
    private const float _aoeOuterRadius = 20;

    public IEnumerable<Actor> BurningTwisters() => Module.Enemies(OID.DarkblazeTwister).Where(twister => twister.CastInfo?.IsSpell(AID.BurningTwister) ?? false);
    public Actor? DarkTwister() => Module.Enemies(OID.DarkblazeTwister).FirstOrDefault(twister => twister.CastInfo?.IsSpell(AID.DarkTwister) ?? false);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var adjPos = Components.Knockback.AwayFromSource(actor.Position, DarkTwister(), _knockbackRange);
        if (actor.Position != adjPos && !Module.InBounds(adjPos))
        {
            hints.Add("About to be knocked back into wall!");
        }

        foreach (var twister in BurningTwisters())
        {
            if (adjPos.InCircle(twister.Position, _aoeInnerRadius) || adjPos.InDonut(twister.Position, _aoeMiddleRadius, _aoeOuterRadius))
            {
                hints.Add("GTFO from aoe!");
                break;
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var twister in BurningTwisters())
        {
            Arena.ZoneCircle(twister.Position, _aoeInnerRadius, ArenaColor.AOE);
            Arena.ZoneDonut(twister.Position, _aoeMiddleRadius, _aoeOuterRadius, ArenaColor.AOE);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var darkTwister = DarkTwister();
        if (darkTwister == null)
            return;

        var adjPos = Components.Knockback.AwayFromSource(pc.Position, darkTwister, _knockbackRange);
        if (adjPos != pc.Position)
        {
            Arena.AddLine(pc.Position, adjPos, ArenaColor.Danger);
            Arena.Actor(adjPos, pc.Rotation, ArenaColor.Danger);
        }

        var safeOffset = _knockbackRange + (_aoeInnerRadius + _aoeMiddleRadius) / 2;
        var safeRadius = (_aoeMiddleRadius - _aoeInnerRadius) / 2;
        foreach (var burningTwister in BurningTwisters())
        {
            var dir = burningTwister.Position - darkTwister.Position;
            var len = dir.Length();
            dir /= len;
            Arena.AddCircle(darkTwister.Position + dir * (len - safeOffset), safeRadius, ArenaColor.Safe);
        }
    }
}
