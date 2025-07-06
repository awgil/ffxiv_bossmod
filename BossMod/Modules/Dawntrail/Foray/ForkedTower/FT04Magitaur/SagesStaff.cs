using Lumina.Extensions;

namespace BossMod.Dawntrail.Foray.ForkedTower.FT04Magitaur;

class SagesStaff(BossModule module) : Components.MultiLineStack(module, 2, 40, (AID)0, AID.ManaExpulsion, 11.3f)
{
    private readonly ForkedTowerConfig _config = Service.Config.Get<ForkedTowerConfig>();
    private readonly List<(Actor Actor, int Group3)> _sources = [];
    private DateTime _activation;

    public int NumStaffs => _sources.Count;
    public bool Enabled;

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.SagesStaff && id == 0x11D4)
        {
            var alliance = 0;

            if (actor.Position.InRect(Arena.Center, 0.Degrees(), 32, 32, 2))
                alliance = 2;
            else if (actor.Position.InRect(Arena.Center, 120.Degrees(), 32, 32, 2))
                alliance = 3;
            else if (actor.Position.InRect(Arena.Center, -120.Degrees(), 32, 32, 2))
                alliance = 1;

            _sources.Add((actor, alliance));
            _activation = WorldState.FutureTime(ActivationDelay);
        }
    }

    public override void Update()
    {
        Stacks.Clear();

        if (_activation != default && Enabled)
            foreach (var s in _sources)
            {
                var group3 = _config.PlayerAlliance.Group3();
                var forbidden = group3 >= 0 && group3 != s.Group3;

                Stacks.Add(new(s.Actor.Position, WorldState.Actors.Where(a => a.Type == ActorType.Player && !a.IsDead).Closest(s.Actor.Position)!, _activation, new BitMask(forbidden ? 0xfful : 0)));
            }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if (spell.Action == WatchedAction)
            _sources.RemoveAll(s => s.Actor == caster);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        Arena.Actors(_sources.Select(s => s.Actor), ArenaColor.Object, true);

        var group3 = _config.PlayerAlliance.Group3();
        if (group3 > 0 && _sources.FirstOrNull(s => s.Group3 == group3) is { } staff)
            Arena.AddCircle(staff.Actor.Position, 1.5f, Enabled ? ArenaColor.Safe : ArenaColor.Danger);
    }
}
