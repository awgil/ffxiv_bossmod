namespace BossMod.Dawntrail.Foray.ForkedTower.FT03MarbleDragon;

class ImitationStar(BossModule module) : Components.RaidwideCastDelay(module, AID._Weaponskill_ImitationStar, AID._Weaponskill_ImitationStar1, 1.8f);

class DraconiformMotion(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_DraconiformMotion1, AID._Weaponskill_DraconiformMotion2], new AOEShapeCone(60, 45.Degrees()));
class DraconiformHint(BossModule module) : BossComponent(module)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.AddCone(Module.PrimaryActor.Position, 60, Module.PrimaryActor.AngleTo(pc), 45.Degrees(), ArenaColor.Danger);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        hints.Add("Stack with party!", false);
    }
}

class FT03MarbleDragonStates : StateMachineBuilder
{
    public FT03MarbleDragonStates(BossModule module) : base(module)
    {
        DeathPhase(0, Phase1);
    }

    private void Phase1(uint id)
    {
        CastStart(id, AID._Weaponskill_ImitationStar, 6.1f)
            .ActivateOnEnter<ImitationStar>();
        ComponentCondition<ImitationStar>(id + 1, 6.9f, s => s.NumCasts > 0, "Raidwide");

        DraconiformMotion(id + 0x100, 8.6f);
        Blizzard1(id + 0x10000, 5);

        Timeout(id + 0xFF0000, 9999, "???");
    }

    private State DraconiformMotion(uint id, float delay)
    {
        CastStart(id, AID._Weaponskill_DraconiformMotion, delay)
            .ActivateOnEnter<DraconiformHint>()
            .ActivateOnEnter<DraconiformMotion>()
            .DeactivateOnExit<DraconiformHint>();
        return ComponentCondition<DraconiformMotion>(id + 0x10, 4.8f, d => d.NumCasts > 0, "Baited AOE")
            .DeactivateOnExit<DraconiformMotion>();
    }

    private void Blizzard1(uint id, float delay)
    {
        ComponentCondition<ImitationRain>(id, delay, i => i.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<ImitationRain>()
            .ActivateOnEnter<ImitationBlizzard>()
            .DeactivateOnExit<ImitationRain>();

        Cast(id + 0x10, AID._Weaponskill_ImitationIcicle, 3.6f, 3);

        DraconiformMotion(id + 0x100, 4.4f)
            .ExecOnEnter<ImitationBlizzard>(b => b.Enabled = true);

        ComponentCondition<ImitationBlizzard>(id + 0x200, 4.3f, b => b.NumCasts > 0, "Blizzard start");
        ComponentCondition<ImitationBlizzard>(id + 0x210, 3.1f, b => b.NumCasts >= 8, "Blizzard end");
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13838)]
public class FT03MarbleDragon(WorldState ws, Actor primary) : BossModule(ws, primary, new(-337, 157), new ArenaBoundsCircle(35))
{
    public override bool DrawAllPlayers => true;
}

