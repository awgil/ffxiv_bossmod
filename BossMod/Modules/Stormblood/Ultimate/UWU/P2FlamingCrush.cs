namespace BossMod.Stormblood.Ultimate.UWU;

class FlamingCrush : Components.UniformStackSpread
{
    protected BitMask Avoid;

    public FlamingCrush() : base(4, 0, 6) { }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.FlamingCrush)
        {
            AddStack(actor, default, Avoid);
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
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
    public override void Init(BossModule module)
    {
        if (module.FindComponent<P2SearingWind>() is var searingWind && searingWind != null)
            foreach (var sw in searingWind.Spreads)
                Avoid.Set(module.Raid.FindSlot(sw.Target.InstanceID));
    }
}

// during P4 (annihilation), everyone should stack (except maybe ranged/caster that will handle mesohigh)
class P4FlamingCrush : FlamingCrush { }

// during P5 (suppression), everyone except mesohigh handler (typically tank) should stack
class P5FlamingCrush : FlamingCrush
{
    public override void Init(BossModule module)
    {
        Avoid = module.Raid.WithSlot(true).WhereActor(p => p.FindStatus(SID.ThermalLow) != null && p.Role != Role.Healer).Mask();
    }
}
