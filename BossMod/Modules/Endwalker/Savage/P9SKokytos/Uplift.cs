namespace BossMod.Endwalker.Savage.P9SKokytos;

class Uplift(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Uplift), new AOEShapeRect(6, 8))
{

    public override void OnEventEnvControl(byte index, uint state)
    {
        // state 00080004 => remove walls
        if (state == 0x00020001)
        {
            if (index == 2)
                Module.Arena.Bounds = P9SKokytos.arenaUplift0;
            if (index == 3)
                Module.Arena.Bounds = P9SKokytos.arenaUplift45;
        }
    }
}
