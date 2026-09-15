namespace BossMod.Endwalker.Savage.P12S2PallasAthena;

// TODO: allow invulning instead
// TODO: not sure at what point target is snapshotted - assume first hit is on primary target when cast starts, second on current main target?..
class PalladianGrasp(BossModule module) : Components.CastCounter(module, default)
{
    private ulong _firstPrimaryTarget;

    private static readonly AOEShapeRect _shape = new(P12S2PallasAthena.DefaultBounds.HalfHeight, P12S2PallasAthena.DefaultBounds.HalfWidth / 2, P12S2PallasAthena.DefaultBounds.HalfHeight);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Module.PrimaryActor.TargetID == _firstPrimaryTarget && actor.Role == Role.Tank)
            hints.Add(actor.InstanceID != _firstPrimaryTarget ? "Taunt!" : "Pass aggro!");

        var target = Target();
        if (target != null)
        {
            if (actor.InstanceID == target.InstanceID)
            {
                if (Raid.WithoutSlot().Exclude(actor).InShape(_shape, Origin(target), default).Any())
                    hints.Add("Bait away from raid!");
            }
            else
            {
                if (_shape.Check(actor.Position, Origin(target), default))
                    hints.Add("GTFO from cleaved side!");
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => Target()?.InstanceID == player.InstanceID ? PlayerPriority.Danger : PlayerPriority.Irrelevant;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Target() is var target && target != default)
            _shape.Draw(Arena, Origin(target), default, pc.InstanceID == target.InstanceID ? ArenaColor.SafeFromAOE : ArenaColor.AOE);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.PalladianGrasp1)
            _firstPrimaryTarget = caster.TargetID;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.PalladianGraspL or AID.PalladianGraspR)
            ++NumCasts;
    }

    private Actor? Target() => WorldState.Actors.Find(NumCasts == 0 ? _firstPrimaryTarget : Module.PrimaryActor.TargetID);
    private WPos Origin(Actor target) => Module.Center + new WDir(target.Position.X < Module.Center.X ? -_shape.HalfWidth : +_shape.HalfWidth, 0);
}
