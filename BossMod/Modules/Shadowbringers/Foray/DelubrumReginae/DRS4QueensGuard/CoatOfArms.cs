namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS4QueensGuard;

class CoatOfArms : Components.DirectionalParry
{
    public CoatOfArms() : base((uint)OID.AetherialWard) { }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        var sides = (AID)spell.Action.ID switch
        {
            AID.CoatOfArmsFB => Side.Front | Side.Back,
            AID.CoatOfArmsLR => Side.Left | Side.Right,
            _ => Side.None
        };
        if (sides != Side.None)
            PredictParrySide(caster.InstanceID, sides);
    }
}
