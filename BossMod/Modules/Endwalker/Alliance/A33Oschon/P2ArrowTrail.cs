namespace BossMod.Endwalker.Alliance.A33Oschon;

class P2ArrowTrail : Components.Exaflare
{
    public P2ArrowTrail(BossModule module) : base(module, new AOEShapeRect(5, 5))
    {
        ImminentColor = ArenaColor.AOE;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ArrowTrailHint)
            Lines.Add(new() { Next = caster.Position, Advance = 5 * caster.Rotation.ToDirection(), NextExplosion = spell.NPCFinishAt.AddSeconds(0.4f), TimeToMove = 0.5f, ExplosionsLeft = 8, MaxShownExplosions = 8 });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ArrowTrailAOE)
        {
            ++NumCasts;
            int index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1));
            if (index >= 0)
            {
                AdvanceLine(Lines[index], caster.Position);
                if (Lines[index].ExplosionsLeft == 0)
                    Lines.RemoveAt(index);
            }
        }
    }
}

class P2DownhillArrowTrailDownhill(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.ArrowTrailDownhill), 6);
