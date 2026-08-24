namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

class WhiteFlash(BossModule module) : Components.StackWithCastTargets(module, AID.WhiteFlash, 6, maxStackSize: 4);
class Dragonspark(BossModule module) : Components.StackWithCastTargets(module, AID.Dragonspark, 6, maxStackSize: 4);

class WhiteFlashDragonspark(BossModule module) : Components.CastCounterMulti(module, [AID.WhiteFlash, AID.Dragonspark]);

class BossSiegeflight(BossModule module) : Components.GroupedAOEs(module, [AID.BossFlight1, AID.BossFlight2, AID.BossFlight3, AID.BossFlight4], new AOEShapeRect(40, 2));
class HelperSiegeflight(BossModule module) : Components.GroupedAOEs(module, [AID.FlightCast1, AID.FlightCast2, AID.FlightCast3, AID.FlightCast4], new AOEShapeRect(40, 4));

class WyvernsRadianceSides(BossModule module) : Components.GroupedAOEs(module, [AID.FlightRadiance1, AID.FlightRadiance2, AID.FlightRadiance3, AID.FlightRadiance4], new AOEShapeRect(40, 9));

class GuardianResonance(BossModule module) : Components.GroupedAOEs(module, [AID.FlightResonance1, AID.FlightResonance2], new AOEShapeRect(40, 8));

class GuardianResonancePuddle(BossModule module) : Components.StandardAOEs(module, AID.GuardianResonancePuddle, 6)
{
    public int NumStarted { get; private set; }
    private DateTime _lastCastStarted;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction && _lastCastStarted.AddSeconds(2) < WorldState.CurrentTime)
        {
            _lastCastStarted = WorldState.CurrentTime;
            NumStarted++;
        }
    }
}
