namespace BossMod.Shadowbringers.Ultimate.TEA;

[SkipLocalsInit]
abstract class P4ForcedMarchDebuffs(BossModule module) : BossComponent(module)
{
    public enum Debuff { None, LightBeacon, LightFollow, DarkBeacon, DarkFollow }

    public bool Done;
    protected Debuff[] Debuffs = new Debuff[PartyState.MaxPartySize];
    protected Actor? LightBeacon;
    protected Actor? DarkBeacon;

    private const float _forcedMarchDistance = 20f; // TODO: verify
    private const float _minLightDistance = 22f; // TODO: verify
    private const float _maxDarkDistance = 5f; // TODO: verify

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Done || Debuffs[slot] == Debuff.None)
            return;

        switch (Debuffs[slot])
        {
            case Debuff.LightFollow:
                if (LightBeacon != null && (actor.Position - LightBeacon.Position).LengthSq() < _minLightDistance * _minLightDistance)
                    hints.Add("Move away from light beacon!");
                break;
            case Debuff.DarkFollow:
                if (DarkBeacon != null)
                {
                    if (!Arena.InBounds(Components.GenericKnockback.AwayFromSource(actor.Position, DarkBeacon.Position, _forcedMarchDistance)))
                        hints.Add("Aim away from wall!");
                    if ((actor.Position - DarkBeacon.Position).LengthSq() > _maxDarkDistance * _maxDarkDistance)
                        hints.Add("Move closer to dark beacon!");
                }
                break;
        }
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (Done || Debuffs[slot] == Debuff.None)
            return;
        movementHints.Add(actor.Position, Arena.Center + SafeSpotDirection(slot), Colors.Safe);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Done || Debuffs[pcSlot] == Debuff.None)
            return;

        switch (Debuffs[pcSlot])
        {
            case Debuff.LightFollow:
                if (LightBeacon != null)
                {
                    var pos = (pc.Position - LightBeacon.Position).LengthSq() <= _forcedMarchDistance * _forcedMarchDistance ? LightBeacon.Position : pc.Position + _forcedMarchDistance * (LightBeacon.Position - pc.Position).Normalized();
                    Components.GenericKnockback.DrawKnockback(pc, pos, Arena);
                }
                break;
            case Debuff.DarkFollow:
                if (DarkBeacon != null)
                {
                    var pos = Components.GenericKnockback.AwayFromSource(pc.Position, DarkBeacon.Position, _forcedMarchDistance);
                    Components.GenericKnockback.DrawKnockback(pc, pos, Arena);
                }
                break;
        }

        Arena.ZoneCircleOutline(Arena.Center + SafeSpotDirection(pcSlot), 1, Colors.Safe);
    }

    protected abstract WDir SafeSpotDirection(int slot);
}
