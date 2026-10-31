namespace BossMod.Stormblood.Ultimate.UWU;

[ConfigDisplay(Order = 0x210, Parent = typeof(StormbloodConfig))]
public class UWUConfig() : ConfigNode()
{
    [PropertyDisplay("P1: Thermal Low cleanse order")]
    [GroupDetails(["1", "2", "3"])]
    public GroupAssignmentFriction P1Friction = new() { Assignments = [-1, 2, -1, -1, 0, 1, -1, -1] };

    [PropertyDisplay("P1: Show hints for fast awaken")]
    public bool P1FastAwaken = false;

    [PropertyDisplay("Titan gaols priorities (close < far)")]
    [GroupDetails(["0", "1", "2", "3", "4", "5", "6", "7"])]
    public GroupAssignmentUnique P3GaolPriorities = GroupAssignmentUnique.Default();
}

public class GroupAssignmentFriction : GroupAssignment
{
    public override bool Validate()
    {
        var ass = new int[8];

        for (var i = 0; i < 8; ++i)
            if (Assignments[i] >= 0)
                ass[Assignments[i]]++;

        return ass[0] == 1 && ass[1] == 1 && ass[2] == 1;
    }
}
