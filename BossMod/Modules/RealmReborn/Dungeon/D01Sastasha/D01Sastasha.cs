namespace BossMod.RealmReborn.Dungeon.D01Sastasha;

public enum OID : uint
{
    BloodyMemoBlue = 0x1E8554,
    BloodyMemoRed = 0x1E8A8C,
    BloodyMemoGreen = 0x1E8A8D,
    CoralFormationBlue = 0x1E8555,
    CoralFormationRed = 0x1E8556,
    CoralFormationGreen = 0x1E8557,
    InconspicuousSwitch = 0x1E8558,
}

[ZoneModuleInfo(BossModuleInfo.Maturity.Verified, 4)]
public class D01Sastasha(WorldState ws) : ZoneModule(ws)
{
    public enum Switch { Unknown, Blue, Red, Green, Resolved }

    private Switch _switchColor;

    public override void Update()
    {
        if (_switchColor == Switch.Unknown)
        {
            _switchColor = (OID)(World.Actors.FirstOrDefault(a => a.IsTargetable && (OID)a.OID is OID.BloodyMemoBlue or OID.BloodyMemoRed or OID.BloodyMemoGreen)?.OID ?? 0) switch
            {
                OID.BloodyMemoBlue => Switch.Blue,
                OID.BloodyMemoRed => Switch.Red,
                OID.BloodyMemoGreen => Switch.Green,
                _ => Switch.Unknown
            };
        }
        else if (_switchColor != Switch.Resolved && World.Actors.Any(a => a.IsTargetable && (OID)a.OID == OID.InconspicuousSwitch))
        {
            _switchColor = Switch.Resolved;
        }
    }

    public override bool WantToBeDrawn() => _switchColor != Switch.Resolved;

    public override List<string> CalculateGlobalHints() => [$"Correct switch: {_switchColor}"];
}
