namespace BossMod.Endwalker.Savage.P4S1Hesperos;

// state related to shift mechanics
class Shift(BossModule module) : BossComponent(module)
{
    private readonly AOEShapeCone _swordAOE = new(50, 60.Degrees());
    private Actor? _swordCaster;
    private Actor? _cloakCaster;

    private const float _knockbackRange = 30;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_swordAOE.Check(actor.Position, _swordCaster))
        {
            hints.Add("GTFO from sword!");
        }
        else if (_cloakCaster != null && !Module.InBounds(Components.Knockback.AwayFromSource(actor.Position, _cloakCaster, _knockbackRange)))
        {
            hints.Add("About to be knocked into wall!");
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        _swordAOE.Draw(Arena, _swordCaster);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_cloakCaster != null)
        {
            Arena.AddCircle(_cloakCaster.Position, 5, ArenaColor.Safe);

            var adjPos = Components.Knockback.AwayFromSource(pc.Position, _cloakCaster, _knockbackRange);
            if (adjPos != pc.Position)
            {
                Arena.AddLine(pc.Position, adjPos, ArenaColor.Danger);
                Arena.Actor(adjPos, pc.Rotation, ArenaColor.Danger);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ShiftingStrikeCloak:
                _cloakCaster = caster;
                break;
            case AID.ShiftingStrikeSword:
                _swordCaster = caster;
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ShiftingStrikeCloak:
                _cloakCaster = null;
                break;
            case AID.ShiftingStrikeSword:
                _swordCaster = null;
                break;
        }
    }
}
