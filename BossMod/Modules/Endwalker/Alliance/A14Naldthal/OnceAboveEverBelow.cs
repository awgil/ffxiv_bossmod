namespace BossMod.Endwalker.Alliance.A14Naldthal;

class OnceAboveEverBelow(BossModule module) : Components.Exaflare(module, 6)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.EverfireFirst or AID.OnceBurnedFirst)
        {
            var advance = 6 * spell.Rotation.ToDirection();
            // lines are offset by 6/18/30; outer have 1 explosion only, mid have 4 or 5, inner 5
            var numExplosions = (caster.Position - Module.Center).LengthSq() > 500 ? 1 : 5;
            Lines.Add(new() { Next = caster.Position, Advance = advance, NextExplosion = spell.NPCFinishAt, TimeToMove = 1.5f, ExplosionsLeft = numExplosions, MaxShownExplosions = 5 });
            Lines.Add(new() { Next = caster.Position, Advance = -advance, NextExplosion = spell.NPCFinishAt, TimeToMove = 1.5f, ExplosionsLeft = numExplosions, MaxShownExplosions = 5 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.EverfireFirst:
            case AID.OnceBurnedFirst:
                var dir = caster.Rotation.ToDirection();
                Advance(caster.Position, dir);
                Advance(caster.Position, -dir);
                ++NumCasts;
                break;
            case AID.EverfireRest:
            case AID.OnceBurnedRest:
                Advance(caster.Position, caster.Rotation.ToDirection());
                ++NumCasts;
                break;
        }
    }

    private void Advance(WPos position, WDir dir)
    {
        int index = Lines.FindIndex(item => item.Next.AlmostEqual(position, 1) && item.Advance.Dot(dir) > 5);
        if (index == -1)
        {
            ReportError($"Failed to find entry for {position} / {dir}");
            return;
        }

        AdvanceLine(Lines[index], position);
        if (Lines[index].ExplosionsLeft == 0)
            Lines.RemoveAt(index);
    }
}
