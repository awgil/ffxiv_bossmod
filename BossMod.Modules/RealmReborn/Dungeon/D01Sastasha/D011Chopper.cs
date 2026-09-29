namespace BossMod.RealmReborn.Dungeon.D01Sastasha.D011Chopper;

public enum OID : uint
{
    Boss = 0x4B4, // TODO: confirm hitbox radius from live/replay; Chopper (NameID 1204)
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    ChargedWhisker = 351, // Boss->self, 3.0s cast, range 3 circle (paralysis)
}

class ChargedWhisker(BossModule module) : Components.StandardAOEs(module, AID.ChargedWhisker, 3); // TODO: verify radius vs hitbox

class D011ChopperStates : StateMachineBuilder
{
    public D011ChopperStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ChargedWhisker>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 4, NameID = 1204)] // TODO: clear after Charged Whisker radius verify
public class D011Chopper(WorldState ws, Actor primary) : BossModule(ws, primary, primary.Position, new ArenaBoundsCircle(20));
