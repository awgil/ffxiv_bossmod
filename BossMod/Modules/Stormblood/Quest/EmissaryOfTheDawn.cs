using BossMod.QuestBattle.Stormblood.MSQ;

namespace BossMod.Stormblood.Quest.EmissaryOfTheDawn;

public enum OID : uint
{
    Boss = 0x234B,
    Helper = 0x233C,
}

class AlphiAI(BossModule module) : QuestBattle.RotationModule<AutoAlphi>(module);

class LB(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (WorldState.Actors.Any(x => x.OID == 0x2340 && x.FindStatus(1497) != null))
            hints.ActionsToExecute.Push(ActionID.MakeSpell(Roleplay.AID.Starstorm), null, ActionQueue.Priority.VeryHigh, targetPos: new Vector3(Arena.Center.X, 0, Arena.Center.Z));
    }
}

class HostileSkyArmorStates : StateMachineBuilder
{
    public HostileSkyArmorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AlphiAI>()
            .ActivateOnEnter<LB>()
            .Raw.Update = () => module.WorldState.CurrentCFCID != 582;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68612, NameID = 7257)]
public class HostileSkyArmor(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}

