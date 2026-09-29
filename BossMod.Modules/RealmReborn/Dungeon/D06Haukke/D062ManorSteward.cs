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

    IceSpikes = 859, // Boss->player, 3.0s cast, single target
    Blizzard = 967 // Boss->player, 1.0s cast, single target
}

class SoulDrain(BossModule module) : Components.StandardAOEs(module, AID.SoulDrain, 9);
class IceSpikes(BossModule module) : Components.SingleTargetCast(module, AID.IceSpikes); // TODO: confirm — may be self-buff, not tankbuster
class Blizzard(BossModule module) : Components.SingleTargetCast(module, AID.Blizzard);
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

[ModuleInfo(Contributors = "Kagekazu", Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 6, NameID = 424)]
public class D062ManorSteward(WorldState ws, Actor primary) : BossModule(ws, primary, primary.Position, new ArenaBoundsCircle(20));
