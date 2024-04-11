namespace BossMod.Stormblood.Ultimate.UWU;

class FlamingCrush(BossModule module) : Components.UniformStackSpread(module, 4, 0, 6)
{
    protected BitMask Avoid;

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.FlamingCrush)
        {
            AddStack(actor, default, Avoid);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.FlamingCrush)
        {
            Stacks.Clear();
        }
    }
}

// during P2, everyone except searing wind targets (typically two healers) should stack
class P2FlamingCrush : FlamingCrush
{
    public P2FlamingCrush(BossModule module) : base(module)
    {
        if (module.FindComponent<P2SearingWind>() is var searingWind && searingWind != null)
            foreach (var sw in searingWind.Spreads)
                Avoid.Set(Raid.FindSlot(sw.Target.InstanceID));
    }
}

// during P4 (annihilation), everyone should stack (except maybe ranged/caster that will handle mesohigh)
class P4FlamingCrush(BossModule module) : FlamingCrush(module) { }

// during P5 (suppression), everyone except mesohigh handler (typically tank) should stack
class P5FlamingCrush : FlamingCrush
{
    public P5FlamingCrush(BossModule module) : base(module)
    {
        Avoid = Raid.WithSlot(true).WhereActor(p => p.FindStatus(SID.ThermalLow) != null && p.Role != Role.Healer).Mask();
    }
}
