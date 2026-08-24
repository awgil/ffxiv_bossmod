namespace BossMod.Dawntrail.Alliance.A14ShadowLord;

class UmbraSmash(BossModule module) : Components.Exaflare(module, new AOEShapeRect(5, 30))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // use only imminent aoes for hints
        foreach (var (c, t, r) in ImminentAOEs())
            hints.AddForbiddenZone(Shape, c, r, t);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.UmbraSmashAOE1 or AID.UmbraSmashAOE2 or AID.UmbraSmashAOE3 or AID.UmbraSmashAOE4 or AID.UmbraSmashAOEClone)
        {
            var dir = spell.Rotation.ToDirection();
            var origin = caster.Position + 30 * dir;
            Lines.Add(new() { Next = origin, Advance = 5 * dir.OrthoL(), Rotation = spell.Rotation + 90.Degrees(), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 2.3f, ExplosionsLeft = 6, MaxShownExplosions = 2 });
            Lines.Add(new() { Next = origin, Advance = 5 * dir.OrthoR(), Rotation = spell.Rotation - 90.Degrees(), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 2.3f, ExplosionsLeft = 6, MaxShownExplosions = 2 });
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.UmbraSmashAOE1:
            case AID.UmbraSmashAOE2:
            case AID.UmbraSmashAOE3:
            case AID.UmbraSmashAOE4:
            case AID.UmbraSmashAOEClone:
                ++NumCasts;
                var origin = caster.Position + 30 * spell.Rotation.ToDirection();
                foreach (var l in Lines.Where(l => l.Next.AlmostEqual(origin, 1)))
                {
                    l.Next = origin + l.Advance;
                    l.TimeToMove = 1;
                    l.NextExplosion = WorldState.FutureTime(l.TimeToMove);
                    --l.ExplosionsLeft;
                }
                break;
            case AID.UmbraWave:
                ++NumCasts;
                int index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1));
                if (index == -1)
                {
                    ReportError($"Failed to find entry for {caster.InstanceID:X}");
                    return;
                }

                AdvanceLine(Lines[index], caster.Position);
                if (Lines[index].ExplosionsLeft == 0)
                    Lines.RemoveAt(index);
                break;
        }
    }
}
