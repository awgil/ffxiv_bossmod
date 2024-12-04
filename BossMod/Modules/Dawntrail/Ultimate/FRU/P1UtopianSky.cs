namespace BossMod.Dawntrail.Ultimate.FRU;

class P1UtopianSkyBlastingZone(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.BlastingZoneAOE))
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeRect _shape = new(50, 8);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if ((OID)actor.OID == OID.FatebreakersImage && modelState == 4)
        {
            AOEs.Add(new(_shape, actor.Position, actor.Rotation, WorldState.FutureTime(9.1f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            AOEs.Clear();
        }
    }
}

class P1UtopianSkySpreadStack(BossModule module) : Components.UniformStackSpread(module, 6, 5, 4, 4, true)
{
    public enum Mechanic { None, Spread, Stack }

    private Mechanic _curMechanic;

    public void Show(DateTime activation)
    {
        switch (_curMechanic)
        {
            case Mechanic.Stack:
                // TODO: this can target either tanks or healers
                AddStacks(Module.Raid.WithoutSlot(true).Where(p => p.Role == Role.Healer), activation);
                break;
            case Mechanic.Spread:
                AddSpreads(Module.Raid.WithoutSlot(true), activation);
                break;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_curMechanic != Mechanic.None)
            hints.Add($"Next: {_curMechanic}");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        _curMechanic = (AID)spell.Action.ID switch
        {
            AID.UtopianSkyStack => Mechanic.Stack,
            AID.UtopianSkySpread => Mechanic.Spread,
            _ => _curMechanic
        };
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.SinboundFire:
                Stacks.Clear();
                break;
            case AID.SinboundThunder:
                Spreads.Clear();
                break;
        }
    }
}
