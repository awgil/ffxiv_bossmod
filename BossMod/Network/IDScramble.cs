using FFXIVClientStructs.FFXIV.Client.System.Framework;

namespace BossMod.Network;

// some of the packets have a simplistic 'scramble' transformation applied to some of the fields:
// - action-id of ActorCast packet
// - action-id of ActionEffectN packets
// - param1 (icon-id) of ActorControl packet with TargetIcon category
// as of patch 7.2, the scramble delta is opcode-specific (kind of); there's an array of three ints stored on NetworkModulePacketReceiverCallback, so keys[opcode mod 3] is used as the starting value, then the game session and zone random values are subtracted from it
public static unsafe class IDScramble
{
    public const uint Delta = 0;

    public static NetworkState.IDScrambleFields Get()
    {
        var proxy = Framework.Instance()->NetworkModuleProxy->ReceiverCallback;
        return new NetworkState.IDScrambleFields(proxy->GameSessionRandom, proxy->LastPacketRandom, proxy->Key0, proxy->Key1, proxy->Key2);
    }
}
