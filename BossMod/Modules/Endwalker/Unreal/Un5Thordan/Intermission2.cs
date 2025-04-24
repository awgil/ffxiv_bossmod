namespace BossMod.Endwalker.Unreal.Un5Thordan;

class SwordShieldOfTheHeavens(BossModule module) : BossComponent(module)
{
    public enum Buff { None, Shield, Sword }

    private readonly List<(Actor actor, Buff buff)> _adds = [];

    public bool Active => _adds.Any(a => AddActive(a.actor));

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.Adelphel or OID.Janlenoux)
            _adds.Add((actor, Buff.None));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_adds.Any(a => a.buff == Buff.Sword && a.actor.CastInfo?.TargetID == actor.InstanceID && a.actor.CastInfo.IsSpell(AID.HolyBladedance)))
            hints.Add("Mitigate NOW!");
        if (_adds.Any(a => a.buff == Buff.Shield && a.actor.TargetID != actor.InstanceID && a.actor.InstanceID == actor.TargetID))
            hints.Add("Swap target!");
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_adds.Count(a => !AddActive(a.actor)) == 2 && _adds[0].actor.Position.InCircle(_adds[1].actor.Position, 10)) // TODO: verify range
            hints.Add("Separate adds!");

        var focus = _adds.Find(a => a.buff == Buff.Sword);
        if (focus.actor != null)
            hints.Add($"Focus on {focus.actor.Name}!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var a in _adds)
            Arena.Actor(a.actor, ArenaColor.Enemy);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var buff = ClassifyStatus(status.ID);
        if (buff != Buff.None)
        {
            var index = _adds.FindIndex(a => a.actor == actor);
            if (index >= 0)
                _adds[index] = (actor, buff);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        var buff = ClassifyStatus(status.ID);
        if (buff != Buff.None)
        {
            var index = _adds.FindIndex(a => a.actor == actor);
            if (index >= 0 && _adds[index].buff == buff)
                _adds[index] = (actor, Buff.None);
        }
    }

    private Buff ClassifyStatus(uint sid) => (SID)sid switch
    {
        SID.ShieldOfTheHeavens => Buff.Shield,
        SID.SwordOfTheHeavens => Buff.Sword,
        _ => Buff.None
    };

    private bool AddActive(Actor add) => !add.IsDestroyed && add.IsTargetable;
}

class HoliestOfHoly(BossModule module) : Components.RaidwideCast(module, AID.HoliestOfHoly);

class SkywardLeap(BossModule module) : Components.GenericBaitAway(module, AID.SkywardLeap, centerAtTarget: true)
{
    private static readonly AOEShapeCircle _shape = new(20); // not sure about the spread radius, 15 seems to be enough but damage goes up to 20

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.SkywardLeap)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, _shape));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            if (CurrentBaits.Count > 0)
                CurrentBaits.RemoveAt(0);
        }
    }
}
