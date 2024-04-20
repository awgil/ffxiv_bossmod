namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS6TrinityAvowed;

abstract class TemperatureAOE(BossModule module) : Components.GenericAOEs(module)
{
    private class PlayerState
    {
        public int BaseTemperature;
        public int Brand;

        public int Temperature => BaseTemperature + Brand;
    }

    private readonly Dictionary<ulong, PlayerState> _playerState = [];

    public int Temperature(Actor player) => _playerState.GetValueOrDefault(player.InstanceID)?.Temperature ?? 0;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.RunningHot1:
                _playerState.GetOrAdd(actor.InstanceID).BaseTemperature = +1;
                break;
            case SID.RunningHot2:
                _playerState.GetOrAdd(actor.InstanceID).BaseTemperature = +2;
                break;
            case SID.RunningCold1:
                _playerState.GetOrAdd(actor.InstanceID).BaseTemperature = -1;
                break;
            case SID.RunningCold2:
                _playerState.GetOrAdd(actor.InstanceID).BaseTemperature = -2;
                break;
            case SID.HotBrand1:
                _playerState.GetOrAdd(actor.InstanceID).Brand = +1;
                break;
            case SID.HotBrand2:
                _playerState.GetOrAdd(actor.InstanceID).Brand = +2;
                break;
            case SID.ColdBrand1:
                _playerState.GetOrAdd(actor.InstanceID).Brand = -1;
                break;
            case SID.ColdBrand2:
                _playerState.GetOrAdd(actor.InstanceID).Brand = -2;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.RunningHot1:
            case SID.RunningHot2:
            case SID.RunningCold1:
            case SID.RunningCold2:
                _playerState.GetOrAdd(actor.InstanceID).BaseTemperature = 0;
                break;
            case SID.HotBrand1:
            case SID.HotBrand2:
            case SID.ColdBrand1:
            case SID.ColdBrand2:
                _playerState.GetOrAdd(actor.InstanceID).Brand = 0;
                break;
        }
    }
}
