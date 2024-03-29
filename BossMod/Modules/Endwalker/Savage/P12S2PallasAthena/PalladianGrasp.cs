namespace BossMod.Endwalker.Savage.P12S2PallasAthena;

// TODO: allow invulning instead
// TODO: not sure at what point target is snapshotted - assume first hit is on primary target when cast starts, second on current main target?..
class PalladianGrasp : Components.CastCounter
{
    private ulong _firstPrimaryTarget;

    private static readonly AOEShapeRect _shape = new(P12S2PallasAthena.DefaultBounds.HalfHeight, P12S2PallasAthena.DefaultBounds.HalfWidth / 2, P12S2PallasAthena.DefaultBounds.HalfHeight);

    public PalladianGrasp() : base(default) { }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (module.PrimaryActor.TargetID == _firstPrimaryTarget && actor.Role == Role.Tank)
            hints.Add(actor.InstanceID != _firstPrimaryTarget ? "Taunt!" : "Pass aggro!");

        var target = Target(module);
        if (target != null)
        {
            if (actor.InstanceID == target.InstanceID)
            {
                if (module.Raid.WithoutSlot().Exclude(actor).InShape(_shape, Origin(module, target), default).Any())
                    hints.Add("Bait away from raid!");
            }
            else
            {
                if (_shape.Check(actor.Position, Origin(module, target), default))
                    hints.Add("GTFO from cleaved side!");
            }
        }
    }

    public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return Target(module)?.InstanceID == player.InstanceID ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (Target(module) is var target && target != default)
            _shape.Draw(arena, Origin(module, target), default, pc.InstanceID == target.InstanceID ? ArenaColor.SafeFromAOE : ArenaColor.AOE);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.PalladianGrasp1)
            _firstPrimaryTarget = caster.TargetID;
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.PalladianGraspL or AID.PalladianGraspR)
            ++NumCasts;
    }

    private Actor? Target(BossModule module) => module.WorldState.Actors.Find(NumCasts == 0 ? _firstPrimaryTarget : module.PrimaryActor.TargetID);
    private WPos Origin(BossModule module, Actor target) => module.Bounds.Center + new WDir(target.Position.X < module.Bounds.Center.X ? -_shape.HalfWidth : +_shape.HalfWidth, 0);
}
