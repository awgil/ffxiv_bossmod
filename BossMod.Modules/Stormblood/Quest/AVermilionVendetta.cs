namespace BossMod.Stormblood.Quest.AVermilionVendetta;

public enum OID : uint
{
    Boss = 0x1AE1, // R3.800
}

public enum AID : uint
{
    ScreamOfTheDead = 7981, // Boss->player/DeepRedXrhunTia, 4.7s cast, range 8 circle stack
    EntropicFlame = 9134, // Boss->self, 2.7s cast, range 50 width 8 rect
}

class ScreamOfTheDead(BossModule module) : Components.StackWithCastTargets(module, AID.ScreamOfTheDead, 8);
class EntropicFlame(BossModule module) : Components.StandardAOEs(module, AID.EntropicFlame, new AOEShapeRect(50, 4));

class FearGortaStates : StateMachineBuilder
{
    public FearGortaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ScreamOfTheDead>()
            .ActivateOnEnter<EntropicFlame>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68116, NameID = 6672)]
public class FearGorta(ModuleInit init) : BossModule(init, new(-312, 670), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly && x.IsTargetable), ArenaColor.Enemy);
}
