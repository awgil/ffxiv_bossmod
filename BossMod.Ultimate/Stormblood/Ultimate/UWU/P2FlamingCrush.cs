namespace BossMod.Stormblood.Ultimate.UWU;

class FlamingCrush(BossModule module) : Components.UniformStackSpread(module, 4, 0, 6)
{
    protected BitMask Avoid;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.FlamingCrush)
        {
            AddStack(actor, WorldState.FutureTime(5.1f), Avoid);
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
class P4FlamingCrush(BossModule module) : FlamingCrush(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        // dodge left/right together
        if (Stacks.Count > 0)
        {
            var z = assignment == PartyRolesConfig.Assignment.R1 ? 99 : 97;
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(new WPos(94, z), new WPos(106, z), 0.51f), Stacks[0].Activation);
        }
    }
}

// during P5 (suppression), everyone except mesohigh handler (typically tank) should stack
class P5FlamingCrush : FlamingCrush
{
    public P5FlamingCrush(BossModule module) : base(module)
    {
        Avoid = Raid.WithSlot(true).WhereActor(p => p.FindStatus(SID.ThermalLow) != null && p.Role != Role.Healer).Mask();
    }
}
