using BossMod.QuestBattle.Shadowbringers.SideQuests;

namespace BossMod.Shadowbringers.Quest.SleepNowInSapphire.P1GuidanceSystem;

public enum OID : uint
{
    Boss = 0x2DFF,
    Helper = 0x233C,
}

public enum AID : uint
{
    AerialBombardment = 21492, // 233C->location, 2.5s cast, range 12 circle
}

class AerialBombardment(BossModule module) : Components.StandardAOEs(module, AID.AerialBombardment, 12);

class GWarrior(BossModule module) : QuestBattle.RotationModule<SapphireWeapon>(module);

class GuidanceSystemStates : StateMachineBuilder
{
    public GuidanceSystemStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GWarrior>()
            .ActivateOnEnter<AerialBombardment>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69431, NameID = 9461)]
public class GuidanceSystem(WorldState ws, Actor primary) : BossModule(ws, primary, new(-15, 610), new ArenaBoundsSquare(60, 1))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor.FindStatus(Roleplay.SID.PyreticBooster) == null)
            hints.ActionsToExecute.Push(ActionID.MakeSpell(Roleplay.AID.PyreticBooster), actor, ActionQueue.Priority.Medium);
    }
}
