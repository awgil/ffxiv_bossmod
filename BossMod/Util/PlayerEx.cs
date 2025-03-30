using CSGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;

namespace BossMod.Util;
public static unsafe class PlayerEx
{
    public static CSGameObject* GameObject
    {
        get
        {
            var localPlayer = Service.ClientState.LocalPlayer;
            return localPlayer != null ? (CSGameObject*)localPlayer.Address : null;
        }
    }

    public static Vector3 Position
    {
        get
        {
            var localPlayer = Service.ClientState.LocalPlayer;
            return localPlayer != null ? localPlayer.Position : Vector3.Zero;
        }
    }
}
