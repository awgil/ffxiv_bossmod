namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

// component for intermission adds (crystals & echoes)
class IntermissionAdds(BossModule module) : BossComponent(module)
{
    private readonly HashSet<ulong> _activeCrystals = [];

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var echo in Module.Enemies(OID.Echo))
            Arena.Actor(echo, ArenaColor.Enemy);

        // note that there are two crystals per position, one targetable and one not - untargetable one can be tethered to second echo
        foreach (var crystal in Module.Enemies(OID.CrystalOfLight))
        {
            if (crystal.IsTargetable && !crystal.IsDead)
            {
                bool isActive = _activeCrystals.Contains(crystal.InstanceID);
                Arena.Actor(crystal, isActive ? ArenaColor.Danger : ArenaColor.PlayerGeneric);
            }

            var tether = WorldState.Actors.Find(crystal.Tether.Target);
            if (tether != null)
                Arena.AddLine(crystal.Position, tether.Position, ArenaColor.Danger);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.IncreaseConviction)
            _activeCrystals.Add(caster.InstanceID);
    }
}
