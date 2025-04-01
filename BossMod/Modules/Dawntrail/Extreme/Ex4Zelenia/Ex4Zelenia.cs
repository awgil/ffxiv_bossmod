namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

class ThornedCatharsis : Components.RaidwideCast
{
    public ThornedCatharsis(BossModule module) : base(module, ActionID.MakeSpell(AID._Weaponskill_ThornedCatharsis))
    {
        KeepOnPhaseChange = true;
    }
}

class TileTracker : BossComponent
{
    public BitMask Tiles;

    public TileTracker(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
    }

    public override void OnEventMapEffect(uint index, ushort state)
    {
        if (index is >= 4 and <= 19)
        {
            Service.Log($"{index - 4}: {state:X4}");

            var ix = (int)index - 4;
            switch (state)
            {
                case 0x100:
                    Tiles.Set(ix);
                    break;
                case 0x20:
                    Tiles.Clear(ix);
                    break;
            }
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"Tiles: {string.Join(", ", Tiles.SetBits())}");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var tile in Tiles.SetBits())
        {
            var order = tile % 8;

            Arena.ZoneCone(Module.Center, tile < 8 ? 0 : 8, tile < 8 ? 8 : 16, 157.5f.Degrees() - 45.Degrees() * order, 22.5f.Degrees(), ArenaColor.AOE);
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1031, NameID = 13861)]
public class Zelenia(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(16));

