namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

// component for third lightwave (safe zone)
class Lightwave3(BossModule module) : LightwaveCommon(module)
{
    public override void Update()
    {
        // try to find two helpers with Z=70 before first cast
        if (Waves.Count == 0)
        {
            foreach (var wave in Module.Enemies(OID.Helper).Where(IsInitialLightwave))
            {
                Waves.Add(wave);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Waves.Count == 0)
            return;

        if (Waves.Any(w => WaveAOE.Check(actor.Position, w)))
            hints.Add("GTFO from wave!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Waves.Count == 0)
            return;

        foreach (var wave in Waves)
            WaveAOE.Draw(Arena, wave);
    }

    private bool IsInitialLightwave(Actor a)
    {
        var pos = a.Position;
        return Math.Abs(pos.X - 70) < 1 || Math.Abs(pos.X - 130) < 1 || Math.Abs(pos.Z - 70) < 1 || Math.Abs(pos.Z - 130) < 1;
    }
}
