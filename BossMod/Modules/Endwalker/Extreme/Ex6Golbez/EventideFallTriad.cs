namespace BossMod.Endwalker.Extreme.Ex6Golbez;

// TODO: improve/generalize
class EventideFallTriad : BossComponent
{
    public enum Mechanic { None, Parties, Roles }

    private Mechanic _curMechanic;

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (_curMechanic != Mechanic.None)
            hints.Add($"Stack by: {_curMechanic}");
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        var mechanic = (AID)spell.Action.ID switch
        {
            AID.EventideFall => Mechanic.Parties,
            AID.EventideTriad => Mechanic.Roles,
            _ => Mechanic.None
        };
        if (mechanic != Mechanic.None)
            _curMechanic = mechanic;
    }
}
