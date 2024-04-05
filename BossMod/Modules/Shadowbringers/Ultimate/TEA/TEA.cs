namespace BossMod.Shadowbringers.Ultimate.TEA;

class P1FluidSwing : Components.Cleave
{
    public P1FluidSwing() : base(ActionID.MakeSpell(AID.FluidSwing), new AOEShapeCone(11.5f, 45.Degrees())) { }
}

class P1FluidStrike : Components.Cleave
{
    public P1FluidStrike() : base(ActionID.MakeSpell(AID.FluidSwing), new AOEShapeCone(11.6f, 45.Degrees()), (uint)OID.LiquidHand) { }
}

class P1Sluice : Components.LocationTargetedAOEs
{
    public P1Sluice() : base(ActionID.MakeSpell(AID.Sluice), 5) { }
}

class P1Splash : Components.CastCounter
{
    public P1Splash() : base(ActionID.MakeSpell(AID.Splash)) { }
}

class P1Drainage : Components.TankbusterTether
{
    public P1Drainage() : base(ActionID.MakeSpell(AID.DrainageP1), (uint)TetherID.Drainage, 6) { }
}

class P2JKick : Components.CastCounter
{
    public P2JKick() : base(ActionID.MakeSpell(AID.JKick)) { }
}

class P2EyeOfTheChakram : Components.SelfTargetedAOEs
{
    public P2EyeOfTheChakram() : base(ActionID.MakeSpell(AID.EyeOfTheChakram), new AOEShapeRect(73, 3, 3)) { }
}

class P2HawkBlasterOpticalSight : Components.LocationTargetedAOEs
{
    public P2HawkBlasterOpticalSight() : base(ActionID.MakeSpell(AID.HawkBlasterP2), 10) { }
}

class P2Photon : Components.CastCounter
{
    public P2Photon() : base(ActionID.MakeSpell(AID.PhotonAOE)) { }
}

class P2SpinCrusher : Components.SelfTargetedAOEs
{
    public P2SpinCrusher() : base(ActionID.MakeSpell(AID.SpinCrusher), new AOEShapeCone(10, 45.Degrees())) { }
}

class P2Drainage : Components.PersistentVoidzone
{
    public P2Drainage() : base(8, m => m.Enemies(OID.LiquidRage)) { } // TODO: verify distance
}

class P2PropellerWind : Components.CastLineOfSightAOE
{
    public P2PropellerWind() : base(ActionID.MakeSpell(AID.PropellerWind), 50, false) { }
    public override IEnumerable<Actor> BlockerActors(BossModule module) => module.Enemies(OID.GelidGaol);
}

class P2DoubleRocketPunch : Components.CastSharedTankbuster
{
    public P2DoubleRocketPunch() : base(ActionID.MakeSpell(AID.DoubleRocketPunch), 3) { }
}

class P3ChasteningHeat : Components.BaitAwayCast
{
    public P3ChasteningHeat() : base(ActionID.MakeSpell(AID.ChasteningHeat), new AOEShapeCircle(5), true) { }
}

class P3DivineSpear : Components.Cleave
{
    public P3DivineSpear() : base(ActionID.MakeSpell(AID.DivineSpear), new AOEShapeCone(24.2f, 45.Degrees()), (uint)OID.AlexanderPrime) { } // TODO: verify angle
}

class P3DivineJudgmentRaidwide : Components.CastCounter
{
    public P3DivineJudgmentRaidwide() : base(ActionID.MakeSpell(AID.DivineJudgmentRaidwide)) { }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.BossP1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 694)]
public class TEA : BossModule
{
    private IReadOnlyList<Actor> _liquidHand;
    public Actor? BossP1() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
    public Actor? LiquidHand() => _liquidHand.FirstOrDefault();

    private Actor? _bruteJustice;
    private Actor? _cruiseChaser;
    public Actor? BruteJustice() => _bruteJustice;
    public Actor? CruiseChaser() => _cruiseChaser;

    private Actor? _alexPrime;
    private IReadOnlyList<Actor> _trueHeart;
    public Actor? AlexPrime() => _alexPrime;
    public Actor? TrueHeart() => _trueHeart.FirstOrDefault();

    private Actor? _perfectAlex;
    public Actor? PerfectAlex() => _perfectAlex;

    public TEA(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 22))
    {
        _liquidHand = Enemies(OID.LiquidHand);
        _trueHeart = Enemies(OID.TrueHeart);
    }

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _bruteJustice ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies(OID.BruteJustice).FirstOrDefault() : null;
        _cruiseChaser ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies(OID.CruiseChaser).FirstOrDefault() : null;
        _alexPrime ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies(OID.AlexanderPrime).FirstOrDefault() : null;
        _perfectAlex ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies(OID.PerfectAlexander).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        switch (StateMachine.ActivePhaseIndex)
        {
            case -1:
            case 0:
                Arena.Actor(BossP1(), ArenaColor.Enemy);
                Arena.Actor(LiquidHand(), ArenaColor.Enemy);
                break;
            case 1:
                Arena.Actor(_bruteJustice, ArenaColor.Enemy, true);
                Arena.Actor(_cruiseChaser, ArenaColor.Enemy, true);
                break;
            case 2:
                Arena.Actor(_alexPrime, ArenaColor.Enemy);
                Arena.Actor(TrueHeart(), ArenaColor.Enemy);
                Arena.Actor(_bruteJustice, ArenaColor.Enemy);
                Arena.Actor(_cruiseChaser, ArenaColor.Enemy);
                break;
            case 3:
                Arena.Actor(_perfectAlex, ArenaColor.Enemy);
                break;
        }
    }
}
