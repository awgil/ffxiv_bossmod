namespace BossMod.Dawntrail.Foray.ForkedTower.FT03MarbleDragon;

class ImitationRain : Components.RaidwideInstant
{
    public ImitationRain(BossModule module) : base(module, AID.ImitationRainInstant2, 0)
    {
        Activation = WorldState.FutureTime(5);
    }
}

abstract class ImitationBlizzard(BossModule module, float activationGap, bool imminentOnly = false) : Components.GenericAOEs(module)
{
    public static readonly AOEShape Circle = new AOEShapeCircle(20);
    public static readonly AOEShape Cross = new AOEShapeCross(60, 8);

    public bool Risky = true;

    // sorted in activation order
    protected abstract IEnumerable<(Actor Actor, DateTime Activation, bool Safe)> Puddles();

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        DateTime firstActivation = default;

        foreach (var cur in Puddles())
        {
            if (firstActivation == default)
                firstActivation = cur.Activation.AddSeconds(0.5f);

            var shape = cur.Actor.OID == (uint)OID.IcePuddle ? Circle : Cross;

            if (cur.Activation < firstActivation)
                yield return new AOEInstance(shape, cur.Actor.Position, cur.Actor.Rotation, cur.Activation, imminentOnly ? ArenaColor.AOE : ArenaColor.Danger, Risky: Risky);
            else if (cur.Activation < firstActivation.AddSeconds(activationGap) && !imminentOnly)
                yield return new AOEInstance(shape, cur.Actor.Position, cur.Actor.Rotation, cur.Activation, Risky: Risky);
            else
                break;
        }
    }
}

// 4 puddles north, 4 puddles south, activated in "domino" order
class ImitationBlizzard1(BossModule module) : ImitationBlizzard(module, 1)
{
    private readonly List<Actor> _puddlesNorth = []; // sorted by X coord
    private readonly List<Actor> _puddlesSouth = []; // sorted by X coord

    protected readonly List<(Actor Actor, DateTime Activation, bool Safe)> _aoes = [];

    public bool Enabled;

    protected override IEnumerable<(Actor Actor, DateTime Activation, bool Safe)> Puddles() => Enabled ? _aoes : [];

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ImitationBlizzardCircle or AID.ImitationBlizzardCross)
        {
            NumCasts++;
            _aoes.RemoveAll(l => l.Actor.Position.AlmostEqual(caster.Position, 0.5f));
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.IcePuddle or OID.CrossPuddle)
        {
            var p = actor.Position.Z < Arena.Center.Z ? _puddlesNorth : _puddlesSouth;
            p.Add(actor);
            p.SortBy(a => a.Position.X);
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.IcePuddle or OID.CrossPuddle)
        {
            _puddlesNorth.Remove(actor);
            _puddlesSouth.Remove(actor);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ImitationIcicleAOE)
        {
            var pos = spell.LocXZ;
            var line = pos.Z < Arena.Center.Z ? _puddlesNorth : _puddlesSouth;
            switch (line.FindIndex(l => l.Position.InCircle(pos, 16)))
            {
                case 0:
                    AddLine(line, [[0], [1], [2], [3]], spell);
                    break;
                case 1:
                    AddLine(line, [[1], [0, 2], [3]], spell);
                    break;
                case 2:
                    AddLine(line, [[2], [1, 3], [0]], spell);
                    break;
                case 3:
                    AddLine(line, [[3], [2], [1], [0]], spell);
                    break;
                default:
                    ReportError($"no puddle found for cast location {pos}");
                    break;
            }
        }
    }

    protected virtual void AddLine(List<Actor> actorList, int[][] ixs, ActorCastInfo spell)
    {
        var start = Module.CastFinishAt(spell, 4.6f);
        foreach (var group in ixs)
        {
            foreach (var ix in group)
                _aoes.Add((actorList[ix], start, false));
            start = start.AddSeconds(1);
        }
        _aoes.SortBy(l => l.Activation);
    }
}
