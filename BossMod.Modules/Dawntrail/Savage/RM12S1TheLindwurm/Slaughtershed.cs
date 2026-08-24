namespace BossMod.Dawntrail.Savage.RM12S1TheLindwurm;

class Slaughtershed(BossModule module) : Components.RaidwideCastDelay(module, (AID)0, AID.SlaughtershedRaidwide, 2.4f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.SlaughtershedBoss2 or AID.SlaughtershedBoss1)
            Activation = Module.CastFinishAt(spell, Delay);
    }
}

class SlaughtershedStackSpread(BossModule module) : Components.IconStackSpread(module, (uint)IconID.SlaughtershedStack, (uint)IconID.SlaughtershedSpread, AID.CurtainCallFourthWallFusion, AID.CurtainCallDramaticLysis, 6, 6, 5.1f);

class SerpentineScourge(BossModule module) : Components.GenericAOEs(module, AID.SerpentineScourgeRect)
{
    public bool Draw;

    readonly List<(WPos, DateTime)> _sources = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Draw ? _sources.Take(1).Select(s => new AOEInstance(new AOEShapeRect(30, 10), s.Item1, default, s.Item2)) : [];

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.SerpentineScourgeWestEast:
                _sources.Add((new(90, 85), WorldState.FutureTime(13.6f)));
                _sources.Add((new(110, 85), WorldState.FutureTime(18.2f)));
                break;
            case AID.SerpentineScourgeEastWest:
                _sources.Add((new(110, 85), WorldState.FutureTime(13.6f)));
                _sources.Add((new(90, 85), WorldState.FutureTime(18.2f)));
                break;
            case AID.SerpentineScourgeRect:
                NumCasts++;
                if (_sources.Count > 0)
                    _sources.RemoveAt(0);
                break;
        }
    }
}

// 372.81 -> 386.2, 390.8
class RaptorKnuckles(BossModule module) : Components.Knockback(module, AID.RaptorKnucklesKB)
{
    public bool Draw;

    readonly List<(WPos, DateTime)> _sources = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Draw ? _sources.Take(1).Select(s => new Source(s.Item1, 30, s.Item2)) : [];

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.RaptorKnucklesWestEast:
                _sources.Add((new(82, 89), WorldState.FutureTime(13.4f)));
                _sources.Add((new(118, 89), WorldState.FutureTime(18)));
                break;
            case AID.RaptorKnucklesEastWest:
                _sources.Add((new(118, 89), WorldState.FutureTime(13.4f)));
                _sources.Add((new(82, 89), WorldState.FutureTime(18)));
                break;
            case AID.RaptorKnucklesKB:
                NumCasts++;
                if (_sources.Count > 0)
                    _sources.RemoveAt(0);
                break;
        }
    }
}

class ArmCounter(BossModule module) : Components.CastCounterMulti(module, [AID.SerpentineScourgeRect, AID.RaptorKnucklesKB]);
