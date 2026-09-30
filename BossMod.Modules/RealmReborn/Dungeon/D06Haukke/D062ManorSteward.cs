namespace BossMod.RealmReborn.Dungeon.D06Haukke.D062ManorSteward;

public enum OID : uint
{
    Boss = 0x112,

    ManorJester = 0x111
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast
    HellSlash = 341, // Boss->player, no cast, single target
    SoulDrain = 860, // Boss->self, 4.0s cast, range 9 circle aoe

    IceSpikes = 859, // ManorJester->self, 3.0s cast, self-buff (interruptible)
    Blizzard = 967 // ManorJester->player, 1.0s cast, single target
}

class SoulDrain(BossModule module) : Components.StandardAOEs(module, AID.SoulDrain, 9);
class IceSpikes(BossModule module) : Components.CastHint(module, AID.IceSpikes, "Interrupt Ice Spikes"); // Manor Jester self-buff
class Blizzard(BossModule module) : Components.SingleTargetCast(module, AID.Blizzard); // Manor Jester
class ManorJester(BossModule module) : Components.Adds(module, (uint)OID.ManorJester, 1);

class D062ManorStewardStates : StateMachineBuilder
{
    public D062ManorStewardStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SoulDrain>()
            .ActivateOnEnter<IceSpikes>()
            .ActivateOnEnter<Blizzard>()
            .ActivateOnEnter<ManorJester>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 6, NameID = 427)]
public class D062ManorSteward(ModuleInit init) : BossModule(init, init.Primary.Position, new ArenaBoundsCircle(20));
