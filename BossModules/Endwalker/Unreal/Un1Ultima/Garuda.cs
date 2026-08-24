namespace BossMod.Endwalker.Unreal.Un1Ultima;

class Garuda(BossModule module) : BossComponent(module)
{
    private bool _vulcanBurstImminent;
    private Actor? _mistralSong;
    private Actor? _eots;
    private Actor? _geocrush;

    private static readonly AOEShapeCone _aoeMistralSong = new(20, 75.Degrees());
    private static readonly AOEShapeDonut _aoeEOTS = new(13, 25); // TODO: check inner range
    private static readonly AOEShapeCircle _aoeGeocrush = new(18); // TODO: check falloff

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoeMistralSong != null)
        {
            var adjPos = _vulcanBurstImminent ? Arena.ClampToBounds(Components.Knockback.AwayFromSource(actor.Position, _mistralSong, 30)) : actor.Position;
            if (_aoeMistralSong.Check(adjPos, _mistralSong))
                hints.Add("GTFO from aoe!");
        }

        if (_eots != null)
            hints.Add("Stand near inner edge", _aoeEOTS.Check(actor.Position, _eots));
        else if (_aoeGeocrush.Check(actor.Position, _geocrush))
            hints.Add("Go to edge!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        _aoeMistralSong.Draw(Arena, _mistralSong);
        _aoeEOTS.Draw(Arena, _eots);
        if (_eots == null)
            _aoeGeocrush.Draw(Arena, _geocrush);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var adjPos = _vulcanBurstImminent ? Arena.ClampToBounds(Components.Knockback.AwayFromSource(pc.Position, _mistralSong, 30)) : pc.Position;
        if (adjPos != pc.Position)
        {
            Arena.AddLine(pc.Position, adjPos, ArenaColor.Danger);
            Arena.Actor(adjPos, 0.Degrees(), ArenaColor.Danger);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.MistralSong:
                _mistralSong = caster;
                _vulcanBurstImminent = true;
                break;
            case AID.EyeOfTheStorm:
                _eots = caster;
                break;
            case AID.Geocrush:
                _geocrush = caster;
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.MistralSong:
                _mistralSong = null;
                _vulcanBurstImminent = false;
                break;
            case AID.EyeOfTheStorm:
                _eots = null;
                break;
            case AID.Geocrush:
                _geocrush = null;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.VulcanBurst)
            _vulcanBurstImminent = false;
    }
}
