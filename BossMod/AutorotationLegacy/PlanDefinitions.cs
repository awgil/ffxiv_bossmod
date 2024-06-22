//namespace BossMod;

//public static class PlanDefinitions
//{
//    public static readonly Dictionary<Class, ClassData> Classes = new()
//    {
//        [Class.PLD] = DefinePLD(),
//    };

//    private static ClassData DefinePLD()
//    {
//        var c = new ClassData(typeof(PLD.AID));
//        c.CooldownTracks.Add(new("Sentinel", ActionID.MakeSpell(PLD.AID.Sentinel), 38));
//        c.CooldownTracks.Add(new("Rampart", ActionID.MakeSpell(PLD.AID.Rampart), 8));
//        c.CooldownTracks.Add(new("HallowedGround", ActionID.MakeSpell(PLD.AID.HallowedGround), 30));
//        c.CooldownTracks.Add(new("Sheltron", ActionID.MakeSpell(PLD.AID.Sheltron), 35));
//        c.CooldownTracks.Add(new("ArmsLength", ActionID.MakeSpell(PLD.AID.ArmsLength), 32));
//        c.CooldownTracks.Add(new("Reprisal", ActionID.MakeSpell(PLD.AID.Reprisal), 22));
//        return c;
//    }
//}
