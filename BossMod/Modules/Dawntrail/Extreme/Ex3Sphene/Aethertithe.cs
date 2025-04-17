namespace BossMod.Dawntrail.Extreme.Ex3Sphene;

class Aethertithe(BossModule module) : Components.GenericAOEs(module)
{
    public AOEInstance? AOE;

    private static readonly AOEShapeCone _shape = new(100, 35.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(AOE);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index != 0)
            return;
        Angle? dir = state switch
        {
            0x04000100 => -55.Degrees(),
            0x08000100 => 0.Degrees(),
            0x10000100 => 55.Degrees(),
            _ => null
        };
        if (dir != null)
        {
            AOE = new(_shape, Module.PrimaryActor.Position, dir.Value, WorldState.FutureTime(5.1f));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AethertitheAOER or AID.AethertitheAOEC or AID.AethertitheAOEL)
        {
            AOE = null;
            ++NumCasts;
        }
    }
}

class Retribute : Components.GenericWildCharge
{
    public Retribute(BossModule module) : base(module, 4, AID.RetributeAOE, 60)
    {
        Source = module.PrimaryActor;
        foreach (var (i, p) in module.Raid.WithSlot(true))
            PlayerRoles[i] = p.Role == Role.Healer ? PlayerRole.Target : PlayerRole.Share;
    }
}
