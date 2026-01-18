namespace BossMod.Dawntrail.Savage.RM11STheTyrant;

// not sure about falloff radius. 20 is roughly 1 unit away from touching the center of the arena
class MammothMeteor(BossModule module) : Components.StandardAOEs(module, AID.MammothMeteor, 20);
class AtomicImpact(BossModule module) : Components.SpreadFromIcon(module, 30, null, 5, 6)
{
    private int _numCasts;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.AtomicImpact)
        {
            _numCasts++;
            if (_numCasts >= 12)
                Spreads.Clear();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (ActiveSpreads.Any(s => s.Target.InstanceID == actor.InstanceID))
            hints.AddForbiddenZone(ShapeContains.Circle(Arena.Center, 10), DateTime.MaxValue);
    }
}
class AtomicImpactPuddle(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, AID.AtomicImpact, m => m.Enemies(OID.AtomicImpact).Where(e => e.EventState != 7), 1);

class StampedeMajesticMeteor(BossModule module) : Components.StandardAOEs(module, AID.MajesticMeteorEcliptic, 6);

class ImpactKiss(BossModule module) : Components.GenericTowers(module)
{
    private readonly RM11STheTyrantConfig _config = Service.Config.Get<RM11STheTyrantConfig>();
    private BitMask _prey;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.CosmicKissEcliptic:
                Towers.Add(new(spell.LocXZ, 4, 1, 1, Raid.WithSlot().WhereActor(a => a.Role != Role.Tank).Mask() | _prey, Module.CastFinishAt(spell)));
                Assign();
                break;
            case AID.WeightyImpact:
                Towers.Add(new(spell.LocXZ, 4, 2, 2, Raid.WithSlot().WhereActor(a => a.Role == Role.Tank).Mask() | _prey, Module.CastFinishAt(spell)));
                Assign();
                break;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.AtomicImpactPrey)
            _prey.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.CosmicKissEcliptic:
            case AID.WeightyImpact:
                Towers.RemoveAll(t => t.Position.AlmostEqual(spell.TargetXZ, 1));
                NumCasts++;
                break;
        }
    }

    void Assign()
    {
        if (Towers.Count < 4)
            return;

        var order = _config.StampedeTowersAssignment.Resolve(Raid).OrderBy(o => o.group).ToList();
        if (order.Count == 0)
            return; // invalid assignments

        Towers.SortByReverse(t => (t.Position - Arena.Center).ToAngle().Deg);

        var _usedPlayers = _prey;

        for (var i = 0; i < Towers.Count; i++)
        {
            ref var t = ref Towers.Ref(i);
            var needed = Towers[i].MaxSoakers;
            var fill = new BitMask();
            var suborder = needed == 1 ? order.Take(2) : order.Skip(2);
            foreach (var (slot, group) in suborder)
            {
                if (_usedPlayers[slot])
                    continue;
                fill.Set(slot);
                if (fill.NumSetBits() >= needed)
                {
                    t.ForbiddenSoakers = ~fill;
                    _usedPlayers |= fill;
                    break;
                }
            }
        }
    }
}

class StampedeMajesticMeteowrath(BossModule module) : Components.GenericBaitAway(module, AID.MajesticMeteowrathEcliptic)
{
    private readonly Actor?[] _tetheredTo = new Actor?[8];

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((OID)source.OID == OID.TheTyrant && Raid.TryFindSlot(tether.Target, out var slot))
        {
            if (_tetheredTo[slot] == null)
                CurrentBaits.Add(new(source, Raid[slot]!, new AOEShapeRect(60, 5), WorldState.FutureTime(7.1f)));
            _tetheredTo[slot] = source;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            CurrentBaits.Clear();
            Array.Fill(_tetheredTo, null);
        }
    }
}

class NWayFireball(BossModule module) : Components.UntelegraphedBait(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TwoWayFireballBoss:
                CurrentBaits.Add(new(caster.Position, default, new AOEShapeRect(60, 3), Module.CastFinishAt(spell, 1), 2, 4, isProximity: true, type: AIHints.PredictedDamageType.Shared));
                break;
            case AID.FourWayFireballBoss:
                CurrentBaits.Add(new(caster.Position, default, new AOEShapeRect(60, 3), Module.CastFinishAt(spell, 1), 4, 2, isProximity: true, type: AIHints.PredictedDamageType.Shared));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TwoWayFireball or AID.FourWayFireball)
        {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }
}
