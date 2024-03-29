namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

// component for intermission adds (crystals & echoes)
class IntermissionAdds : BossComponent
{
    private HashSet<ulong> _activeCrystals = new();

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var echo in module.Enemies(OID.Echo))
            arena.Actor(echo, ArenaColor.Enemy);

        // note that there are two crystals per position, one targetable and one not - untargetable one can be tethered to second echo
        foreach (var crystal in module.Enemies(OID.CrystalOfLight))
        {
            if (crystal.IsTargetable && !crystal.IsDead)
            {
                bool isActive = _activeCrystals.Contains(crystal.InstanceID);
                arena.Actor(crystal, isActive ? ArenaColor.Danger : ArenaColor.PlayerGeneric);
            }

            var tether = module.WorldState.Actors.Find(crystal.Tether.Target);
            if (tether != null)
                arena.AddLine(crystal.Position, tether.Position, ArenaColor.Danger);
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.IncreaseConviction)
            _activeCrystals.Add(caster.InstanceID);
    }
}
