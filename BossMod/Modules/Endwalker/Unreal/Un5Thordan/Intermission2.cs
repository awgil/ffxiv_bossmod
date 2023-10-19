using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Unreal.Un5Thordan;

class SwordShieldOfTheHeavens : BossComponent
{
    public enum Buff { None, Shield, Sword }

    private List<(Actor actor, Buff buff)> _adds = new();

    public bool Active => _adds.Any(a => AddActive(a.actor));

    public override void OnActorCreated(BossModule module, Actor actor)
    {
        if ((OID)actor.OID is OID.Adelphel or OID.Janlenoux)
            _adds.Add((actor, Buff.None));
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_adds.Any(a => a.buff == Buff.Sword && a.actor.CastInfo?.TargetID == actor.InstanceID && a.actor.CastInfo.IsSpell(AID.HolyBladedance)))
            hints.Add("Mitigate NOW!");
        if (_adds.Any(a => a.buff == Buff.Shield && a.actor.TargetID != actor.InstanceID && a.actor.InstanceID == actor.TargetID))
            hints.Add("Swap target!");
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (_adds.Count(a => !AddActive(a.actor)) == 2 && _adds[0].actor.Position.InCircle(_adds[1].actor.Position, 10)) // TODO: verify range
            hints.Add("Separate adds!");

        var focus = _adds.Find(a => a.buff == Buff.Sword);
        if (focus.actor != null)
            hints.Add($"Focus on {focus.actor.Name}!");
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var a in _adds)
            arena.Actor(a.actor, ArenaColor.Enemy);
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        var buff = ClassifyStatus(status.ID);
        if (buff != Buff.None)
        {
            var index = _adds.FindIndex(a => a.actor == actor);
            if (index >= 0)
                _adds[index] = (actor, buff);
        }
    }

    public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
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

class HoliestOfHoly : Components.RaidwideCast
{
    public HoliestOfHoly() : base(ActionID.MakeSpell(AID.HoliestOfHoly)) { }
}

class SkywardLeap : Components.GenericBaitAway
{
    private static AOEShapeCircle _shape = new(20); // not sure about the spread radius, 15 seems to be enough but damage goes up to 20

    public SkywardLeap() : base(ActionID.MakeSpell(AID.SkywardLeap), centerAtTarget: true) { }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.SkywardLeap)
            CurrentBaits.Add(new(module.PrimaryActor, actor, _shape));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            if (CurrentBaits.Count > 0)
                CurrentBaits.RemoveAt(0);
        }
    }
}
