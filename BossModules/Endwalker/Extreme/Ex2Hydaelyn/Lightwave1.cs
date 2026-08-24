namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

// component for first lightwave (2 waves, 4 crystals) mechanic
// first we wait until we find two helpers with Z=70 - these are our lightwaves
class Lightwave1(BossModule module) : LightwaveCommon(module)
{
    private WPos _safeCrystal;
    private WPos _firstHitCrystal;
    private WPos _secondHitCrystal;
    private WPos _thirdHitCrystal;

    public override void Update()
    {
        // try to find two helpers with Z=70 before first cast
        if (Waves.Count == 0)
        {
            foreach (var wave in Module.Enemies(OID.Helper).Where(a => a.Position.Z < 71))
            {
                Waves.Add(wave);
            }

            if (Waves.Count > 0)
            {
                bool leftWave = Waves.Any(w => w.Position.X < 90);
                _safeCrystal = new(leftWave ? 110 : 90, 92);
                _firstHitCrystal = new(100, 86);
                _secondHitCrystal = new(leftWave ? 90 : 110, 92);
                _thirdHitCrystal = new(100, 116);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Waves.Count == 0)
            return;

        if (Waves.Any(w => WaveAOE.Check(actor.Position, w)))
            hints.Add("GTFO from wave!");

        bool safe = NumCasts switch
        {
            0 => InSafeCone(_firstHitCrystal, _safeCrystal, actor.Position),
            1 => InSafeCone(_secondHitCrystal, _safeCrystal, actor.Position),
            2 => InSafeCone(_thirdHitCrystal, _safeCrystal, actor.Position) || InSafeCone(_thirdHitCrystal, _firstHitCrystal, actor.Position) || InSafeCone(_thirdHitCrystal, _secondHitCrystal, actor.Position),
            _ => true
        };
        if (!safe)
            hints.Add("Hide behind crystal!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Waves.Count == 0)
            return;

        foreach (var wave in Waves)
            WaveAOE.Draw(Arena, wave);

        switch (NumCasts)
        {
            case 0:
                DrawSafeCone(_firstHitCrystal, _safeCrystal);
                break;
            case 1:
                DrawSafeCone(_secondHitCrystal, _safeCrystal);
                break;
            case 2:
                DrawSafeCone(_thirdHitCrystal, _safeCrystal);
                DrawSafeCone(_thirdHitCrystal, _firstHitCrystal);
                DrawSafeCone(_thirdHitCrystal, _secondHitCrystal);
                break;
        }
    }
}
