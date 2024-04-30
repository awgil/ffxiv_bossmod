namespace BossMod.RealmReborn.Dungeon.D01Sastasha.D010Switch;

public enum OID : uint
{
    Blue = 0x1E8554,
    Red = 0x1E8A8C,
    Green = 0x1E8A8D,
}

class SwitchHint(BossModule module) : BossComponent(module)
{
    private string _hint = "";

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_hint.Length == 0)
        {
            var a = WorldState.Actors.FirstOrDefault(a => a.IsTargetable && (OID)a.OID is OID.Blue or OID.Red or OID.Green);
            if (a != null)
                _hint = ((OID)a.OID).ToString();
        }
        hints.Add($"Correct switch: {_hint}");
    }
}

class D010SwitchStates : StateMachineBuilder
{
    public D010SwitchStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

// the switch spawns chopper, so maybe we can use that as a nameID
[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.Blue, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 4, NameID = 1204)]
public class D010Switch : BossModule
{
    public D010Switch(WorldState ws, Actor primary) : base(ws, primary, primary.Position, new ArenaBoundsSquare(20))
    {
        ActivateComponent<SwitchHint>();
    }
}
