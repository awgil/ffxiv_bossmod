namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

// component for third lightwave (safe zone)
class Lightwave3 : LightwaveCommon
{
    public override void Update(BossModule module)
    {
        // try to find two helpers with Z=70 before first cast
        if (Waves.Count == 0)
        {
            foreach (var wave in module.Enemies(OID.Helper).Where(IsInitialLightwave))
            {
                Waves.Add(wave);
            }
        }
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (Waves.Count == 0)
            return;

        if (Waves.Any(w => WaveAOE.Check(actor.Position, w)))
            hints.Add("GTFO from wave!");
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (Waves.Count == 0)
            return;

        foreach (var wave in Waves)
            WaveAOE.Draw(arena, wave);
    }

    private bool IsInitialLightwave(Actor a)
    {
        var pos = a.Position;
        return Math.Abs(pos.X - 70) < 1 || Math.Abs(pos.X - 130) < 1 || Math.Abs(pos.Z - 70) < 1 || Math.Abs(pos.Z - 130) < 1;
    }
}
