namespace BossMod.Shadowbringers.Ultimate.TEA;

// baited flarethrowers
class P3Inception2 : Components.GenericBaitAway
{
    private int _numAetheroplasmsDone;
    private BitMask _taken;

    private static readonly AOEShapeCone _shape = new(100, 45.Degrees()); // TODO: verify angle

    public P3Inception2(BossModule module) : base(module)
    {
        // assume first two are baited by tanks
        ForbiddenPlayers = Raid.WithSlot(true).WhereActor(a => a.Role != Role.Tank).Mask();
    }

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_numAetheroplasmsDone >= 4) // do not show anything until all aetheroplasms (part 1 of the mechanic) are done
        {
            var source = ((TEA)Module).BruteJustice();
            var target = source != null ? Raid.WithoutSlot().Closest(source.Position) : null;
            if (source != null && target != null)
                CurrentBaits.Add(new(source, target, _shape));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Aetheroplasm:
                ++_numAetheroplasmsDone;
                break;
            case AID.FlarethrowerP3:
                ++NumCasts;
                foreach (var t in spell.Targets)
                {
                    var slot = Raid.FindSlot(t.ID);
                    _taken.Set(slot);
                    ForbiddenPlayers.Set(slot);
                }
                if (ForbiddenPlayers.Raw == 0xff)
                {
                    // assume after both tanks have taken, rest is taken by raid
                    ForbiddenPlayers = _taken;
                }
                break;
        }
    }
}
