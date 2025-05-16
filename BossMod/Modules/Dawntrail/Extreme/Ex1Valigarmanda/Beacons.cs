namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

class Beacons(BossModule module) : BossComponent(module)
{
    public readonly List<Actor> Actors = [];
    public IEnumerable<Actor> ActiveActors => Actors.Where(a => a.IsTargetable && !a.IsDead);

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.ThunderousBeacon or OID.FlameKissedBeacon or OID.GlacialBeacon)
            Actors.Add(actor);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Actors, ArenaColor.Enemy);
    }
}

class CalamitousCry : Components.GenericWildCharge
{
    public CalamitousCry(BossModule module) : base(module, 3, AID.CalamitousCryAOE, 80)
    {
        Reset();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.CalamitousCryTargetFirst:
            case AID.CalamitousCryTargetRest:
                Source = Module.PrimaryActor;
                if (Raid.TryFindSlot(spell.MainTargetID, out var slot))
                    PlayerRoles[slot] = PlayerRole.Target;
                break;
            case AID.CalamitousCryAOE:
                ++NumCasts;
                Reset();
                break;
        }
    }

    private void Reset()
    {
        Source = null;
        foreach (var (i, p) in Module.Raid.WithSlot(true))
            PlayerRoles[i] = p.Role == Role.Tank ? PlayerRole.Share : PlayerRole.ShareNotFirst;
    }
}

class CalamitousEcho(BossModule module) : Components.StandardAOEs(module, AID.CalamitousEcho, new AOEShapeCone(40, 10.Degrees()));
