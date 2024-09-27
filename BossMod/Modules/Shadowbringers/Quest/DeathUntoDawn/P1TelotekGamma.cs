﻿using BossMod.QuestBattle.Shadowbringers.MSQ;

namespace BossMod.Shadowbringers.Quest.DeathUntoDawn.P1;

public enum AID : uint
{
    _Weaponskill_AntiPersonnelMissile = 24845, // 233C->player/321D, 5.0s cast, range 6 circle
    _Weaponskill_MRVMissile = 24843, // 233C->location, 8.0s cast, range 12 circle
}

enum OID : uint
{
    Boss = 0x3376
}

class AlisaieAI(BossModule module) : Components.RotationModule<AutoAlisaie>(module);
class AntiPersonnelMissile(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_AntiPersonnelMissile), 6);
class MRVMissile(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_MRVMissile), 12, maxCasts: 6);

public class TelotekGammaStates : StateMachineBuilder
{
    public TelotekGammaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AlisaieAI>()
            .ActivateOnEnter<AntiPersonnelMissile>()
            .ActivateOnEnter<MRVMissile>();
    }
}

// actually using npc graha's OID here since this encounter consists of a bunch of adds before the boss even spawns
[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 780)]
public class TelotekGamma(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -180), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}
