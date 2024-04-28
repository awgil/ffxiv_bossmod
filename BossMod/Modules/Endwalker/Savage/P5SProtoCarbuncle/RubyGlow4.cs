namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

// this includes venom pools and raging claw/searing ray aoes
class RubyGlow4(BossModule module) : RubyGlowRecolor(module, 5)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (CurRecolorState != RecolorState.BeforeStones && MagicStones.Any())
            yield return new(ShapeHalf, Module.Center, Angle.FromDirection(QuadrantDir(AOEQuadrant)));
        foreach (var p in ActivePoisonAOEs())
            yield return p;

        if (Module.PrimaryActor.CastInfo?.IsSpell(AID.RagingClaw) ?? false)
            yield return new(ShapeHalf, Module.PrimaryActor.Position, Module.PrimaryActor.CastInfo.Rotation, Module.PrimaryActor.CastInfo.NPCFinishAt);
        if (Module.PrimaryActor.CastInfo?.IsSpell(AID.SearingRay) ?? false)
            yield return new(ShapeHalf, Module.Center, Module.PrimaryActor.CastInfo.Rotation + 180.Degrees(), Module.PrimaryActor.CastInfo.NPCFinishAt);
    }
}
