namespace BossMod.Dawntrail.Ultimate.UMAD;

class P3BlizzardIII(BossModule module) : Components.StandardAOEs(module, AID.BlizzardIII, 6)
{
    public int NumBaits { get; private set; }
    DateTime _prevStart;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction)
        {
            if (WorldState.CurrentTime > _prevStart.AddSeconds(1))
                NumBaits++;
            _prevStart = WorldState.CurrentTime;
        }
    }
}

class P3StompAMole(BossModule module) : Components.GenericTowers(module, AID.StompAMole)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.StompAMoleVisual)
        {
            Towers.Add(new(caster.Position + spell.Rotation.ToDirection().OrthoR() * 10, 5, 2, 2, new(0xff), Module.CastFinishAt(spell, 1.6f)));
            Towers.Add(new(caster.Position + spell.Rotation.ToDirection().OrthoL() * 10, 5, 2, 2, new(0xff), Module.CastFinishAt(spell, 2.9f)));
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.KnockDownShare && NumCasts == 0)
        {
            var roleShare = Raid.WithSlot().WhereActor(a => a.Class.IsSupport() == actor.Class.IsSupport()).Mask();
            for (var i = 0; i < Towers.Count; i++)
                Towers.Ref(i).ForbiddenSoakers = roleShare;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {

            if (Towers.Count > 0)
                Towers.RemoveAt(0);

            if (NumCasts++ < 2)
                Towers.Add(new(caster.Position, 5, 2, 2, default, WorldState.FutureTime(5)));
        }
    }
}

class P3KnockDown(BossModule module) : Components.StackWithIcon(module, (uint)IconID.KnockDownShare, AID.KnockDown, 6, 5.7f, 4, 4);
