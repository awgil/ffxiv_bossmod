namespace BossMod.Endwalker.Savage.P9SKokytos;

class DualityOfDeath(BossModule module) : Components.GenericBaitAway(module, ActionID.MakeSpell(AID.DualityOfDeathFire), centerAtTarget: true)
{
    private ulong _firstFireTarget;

    private static readonly AOEShapeCircle _shape = new(6);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveBaitsOn(actor).Any())
        {
            if (Raid.WithoutSlot().InRadiusExcluding(actor, _shape.Radius).Any())
                hints.Add("GTFO from raid!");
            if (Module.PrimaryActor.TargetID == _firstFireTarget)
                hints.Add(actor.InstanceID != _firstFireTarget ? "Taunt!" : "Pass aggro!");
        }
        else if (ActiveBaits.Any(b => IsClippedBy(actor, b)))
        {
            hints.Add("GTFO from tanks!");
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.DualityOfDeath)
        {
            CurrentBaits.Add(new(Module.PrimaryActor, actor, _shape));
            _firstFireTarget = Module.PrimaryActor.TargetID;
        }
    }
}
