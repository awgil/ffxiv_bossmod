namespace BossMod.Endwalker.Savage.P7SAgdistis;

// TODO: implement!
class ForbiddenFruit10 : ForbiddenFruitCommon
{
    private BitMask _minotaurPlaforms = ValidPlatformsMask;
    private BitMask _bullPlatforms;

    public ForbiddenFruit10() : base(ActionID.MakeSpell(AID.BronzeBellows)) { }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        base.DrawArenaBackground(module, pcSlot, pc, arena);
        foreach (var (slot, target) in module.Raid.WithSlot(true))
        {
            var source = TetherSources[slot];
            if (source != null)
            {
                AOEShape shape = (OID)source.OID == OID.ImmatureMinotaur ? ShapeMinotaurTethered : ShapeBullBirdTethered;
                shape.Draw(arena, source.Position, Angle.FromDirection(target.Position - source.Position));
            }
        }
    }

    public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        var slot = TryAssignTether(module, source, tether);
        if (slot < 0)
            return;
        var safe = (TetherID)tether.ID switch
        {
            TetherID.Bull => _bullPlatforms,
            TetherID.Bird => ValidPlatformsMask & ~(_minotaurPlaforms | _bullPlatforms),
            _ => _minotaurPlaforms
        };
        SafePlatforms[slot] = safe;
    }

    protected override System.DateTime? PredictUntetheredCastStart(BossModule module, Actor fruit)
    {
        switch ((OID)fruit.OID)
        {
            case OID.ForbiddenFruitMinotaur:
                // minotaurs spawn on bridges, minotaur platform is adjacent => their opposite platform is never minotaur one
                _minotaurPlaforms.Clear(PlatformIDFromOffset(module.Bounds.Center - fruit.Position));
                break;
            case OID.ForbiddenFruitBull:
                _bullPlatforms.Set(PlatformIDFromOffset(fruit.Position - module.Bounds.Center));
                break;
        }
        return null;
    }
}
