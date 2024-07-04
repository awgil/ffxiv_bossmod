namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class Border(BossModule module) : BossComponent(module)
{
    public bool LBridgeActive { get; private set; }
    public bool RBridgeActive { get; private set; }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state is 0x00020001 or 0x00080004)
        {
            switch (index)
            {
                case 2: RBridgeActive = state == 0x00020001; break;
                case 3: LBridgeActive = state == 0x00020001; break;
            }
        }
        if (!LBridgeActive && !RBridgeActive)
            Module.Arena.Bounds = P10SPandaemonium.arena;
        if (!LBridgeActive && RBridgeActive)
            Module.Arena.Bounds = P10SPandaemonium.arenaR;
        if (LBridgeActive && !RBridgeActive)
            Module.Arena.Bounds = P10SPandaemonium.arenaL;
        if (LBridgeActive && RBridgeActive)
            Module.Arena.Bounds = P10SPandaemonium.arenaLR;
    }
}
