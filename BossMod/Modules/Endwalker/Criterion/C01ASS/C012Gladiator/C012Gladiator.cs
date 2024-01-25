﻿namespace BossMod.Endwalker.Criterion.C01ASS.C012Gladiator
{
    class RushOfMightFront : Components.SelfTargetedAOEs
    {
        public RushOfMightFront(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeCone(60, 90.Degrees())) { }
    }
    class NRushOfMightFront : RushOfMightFront { public NRushOfMightFront() : base(AID.NRushOfMightFront) { } }
    class SRushOfMightFront : RushOfMightFront { public SRushOfMightFront() : base(AID.SRushOfMightFront) { } }

    class RushOfMightBack : Components.SelfTargetedAOEs
    {
        public RushOfMightBack(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeCone(60, 90.Degrees())) { }
    }
    class NRushOfMightBack : RushOfMightBack { public NRushOfMightBack() : base(AID.NRushOfMightBack) { } }
    class SRushOfMightBack : RushOfMightBack { public SRushOfMightBack() : base(AID.SRushOfMightBack) { } }

    public abstract class C012Gladiator : BossModule
    {
        public C012Gladiator(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(-35, -271), 20)) { }
    }

    [ModuleInfo(PrimaryActorOID = (uint)OID.NBoss, CFCID = 878, NameID = 11387)]
    public class C012NGladiator : C012Gladiator { public C012NGladiator(WorldState ws, Actor primary) : base(ws, primary) { } }

    [ModuleInfo(PrimaryActorOID = (uint)OID.SBoss, CFCID = 879, NameID = 11387)]
    public class C012SGladiator : C012Gladiator { public C012SGladiator(WorldState ws, Actor primary) : base(ws, primary) { } }
}
