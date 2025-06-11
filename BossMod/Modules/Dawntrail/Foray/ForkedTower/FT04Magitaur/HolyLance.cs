namespace BossMod.Dawntrail.Foray.ForkedTower.FT04Magitaur;

class HolyLance(BossModule module) : Components.GenericAOEs(module, AID._Ability_2)
{
    private DateTime _activation;

    public bool Enabled;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Enabled)
            yield break;

        if (NumCasts % 4 == 0)
            yield return new(FT04Magitaur.NotPlatforms, Arena.Center, Activation: _activation);
        else
        {
            var platform = FT04Magitaur.Platforms[NumCasts < 4 ? 2 : NumCasts < 8 ? 1 : 0];
            yield return new(new AOEShapeRect(10, 10, 10), Arena.Center + platform.Item1, platform.Item2, Activation: _activation);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.LuminousLance && _activation == default)
            _activation = WorldState.FutureTime(13);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _activation = WorldState.FutureTime(2);
        }
    }
}

// TODO: add hints to not clip other parties with outside-floor stack
class HolyIV(BossModule module) : Components.StackWithIcon(module, (uint)IconID._Gen_Icon_com_share2i, AID._Ability_HolyIV, 6, 8);
