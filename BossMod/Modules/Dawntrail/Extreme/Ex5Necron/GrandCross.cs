namespace BossMod.Dawntrail.Extreme.Ex5Necron;

class GrandCrossArena(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;
    public int NumChanges;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(new AOEShapeDonut(9, 60), Arena.Center, default, _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_)
            _activation = Module.CastFinishAt(spell);
    }

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID == 0x8000000D)
        {
            if (param1 == 2)
            {
                NumChanges++;
                _activation = default;
                Arena.Bounds = new ArenaBoundsCircle(9);
            }
            else if (param1 == 1)
            {
                NumChanges++;
                Arena.Bounds = new ArenaBoundsRect(18, 15);
            }
        }
    }
}

class GrandCrossRaidwide(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_GrandCross);
class GrandCrossPuddle(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_GrandCross1, 3);
class GrandCrossSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID._Weaponskill_GrandCross3, 3);
class GrandCrossLine(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_GrandCross2)
{
    private readonly List<(Angle, DateTime)> _lasers = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var l in _lasers)
            yield return new AOEInstance(new AOEShapeRect(20, 2, 20), Arena.Center, l.Item1, l.Item2);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        var angle = (Arena.Center - source.Position).ToAngle();

        if (tether.ID == 344)
        {
            _lasers.Add((angle + 207.Degrees(), WorldState.FutureTime(5)));
            _lasers.SortBy(l => l.Item2);
        }
        if (tether.ID == 343)
        {
            _lasers.Add((angle + 42.Degrees(), WorldState.FutureTime(7)));
            _lasers.SortBy(l => l.Item2);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction && _lasers.Count > 0)
            _lasers.RemoveAt(0);
    }
}
class GrandCrossLineCast(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_GrandCross2, new AOEShapeRect(100, 2));
class Shock(BossModule module) : Components.CastTowers(module, AID._Weaponskill_Shock, 3)
{
    private BitMask _forbidden;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.SmallSpread5s)
        {
            _forbidden.Set(Raid.FindSlot(actor.InstanceID));
            UpdateMask();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action == WatchedAction)
            UpdateMask();
    }

    private void UpdateMask()
    {
        foreach (ref var t in Towers.AsSpan())
            t.ForbiddenSoakers |= _forbidden;
    }
}

class GrandCrossProximity(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_GrandCross4, new AOEShapeRect(100, 4.5f));

class NeutronRing(BossModule module) : Components.RaidwideCastDelay(module, AID._Weaponskill_NeutronRing, AID._Weaponskill_NeutronRing1, 2.6f);
