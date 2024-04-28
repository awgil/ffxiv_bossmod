namespace BossMod.Endwalker.Savage.P7SAgdistis;

// TODO: implement!
class ForbiddenFruit10(BossModule module) : ForbiddenFruitCommon(module, ActionID.MakeSpell(AID.BronzeBellows))
{
    private BitMask _minotaurPlaforms = ValidPlatformsMask;
    private BitMask _bullPlatforms;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);
        foreach (var (slot, target) in Raid.WithSlot(true))
        {
            var source = TetherSources[slot];
            if (source != null)
            {
                AOEShape shape = (OID)source.OID == OID.ImmatureMinotaur ? ShapeMinotaurTethered : ShapeBullBirdTethered;
                shape.Draw(Arena, source.Position, Angle.FromDirection(target.Position - source.Position));
            }
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        var slot = TryAssignTether(source, tether);
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

    protected override DateTime? PredictUntetheredCastStart(Actor fruit)
    {
        switch ((OID)fruit.OID)
        {
            case OID.ForbiddenFruitMinotaur:
                // minotaurs spawn on bridges, minotaur platform is adjacent => their opposite platform is never minotaur one
                _minotaurPlaforms.Clear(PlatformIDFromOffset(Module.Center - fruit.Position));
                break;
            case OID.ForbiddenFruitBull:
                _bullPlatforms.Set(PlatformIDFromOffset(fruit.Position - Module.Center));
                break;
        }
        return null;
    }
}
