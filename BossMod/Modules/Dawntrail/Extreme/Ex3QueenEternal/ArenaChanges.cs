namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
    public override bool KeepOnPhaseChange => true;
    private bool firstEarthArena = true;

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID != 0x8000000D || param1 > 0x08u)
        {
            return;
        }
        switch (param1)
        {
            case 0x01u: // default arena
                SetDefaultArena();
                break;
            case 0x02u: // x arena (wind)
                var arenaWind = Trial.T03QueenEternal.T03QueenEternal.GetXArena();
                SetArena(arenaWind, arenaWind.Center);
                break;
            case 0x04u: // disjointed rect (Earth) arena
                if (firstEarthArena)
                {
                    firstEarthArena = false; // don't want to switch arena here because of gravity stuff
                }
                else
                {
                    var arenaEarth = Trial.T03QueenEternal.T03QueenEternal.GetSplitArena();
                    SetArena(arenaEarth, arenaEarth.Center);
                }
                break;
            case 0x08u: // ice arena
                var arenaIce = new ArenaBoundsCustom(Ex3QueenEternal.GetIceRects());
                SetArena(arenaIce, arenaIce.Center);
                break;
        }
    }

    private void SetDefaultArena() => SetArena(new ArenaBoundsSquare(20f), new(100f, 100f));

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x08)
        {
            if (state == 0x01000080u)
            {
                SetArena(new ArenaBoundsRect(20f, 10f), new(100, 110));
            }
            else if (state == 0x02000001u)
            {
                SetDefaultArena();
            }
        }
    }

    private void SetArena(ArenaBounds bounds, WPos center)
    {
        Arena.Bounds = bounds;
        Arena.Center = center;
    }
}
