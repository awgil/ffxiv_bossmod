namespace BossMod.Stormblood.Ultimate.UCOB;

class P1Plummet(BossModule module) : Components.Cleave(module, AID.Plummet, new AOEShapeCone(12, 60.Degrees()), (uint)OID.Twintania)
{
    public bool Soak;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (origin, target, angle) in OriginsAndTargets())
        {
            var originE = hints.FindEnemy(origin);
            originE?.CanMove = false;

            if (actor != target)
            {
                var shape = Shape.GetSdf(origin.Position, angle);
                if (Soak && IsSoaker(assignment))
                    shape = shape.Inverted();
                hints.AddForbiddenZone(shape, NextExpected);

                // non-tanks preposition away from where tank might face boss
                if (originE?.DesiredRotation is { } rot)
                {
                    var predicted = Shape.GetSdf(origin.Position, rot);
                    if (Soak && IsSoaker(assignment))
                        predicted = predicted.Inverted();
                    hints.AddForbiddenZone(predicted, DateTime.MaxValue);
                }
            }
        }
    }

    // r1 is on hell duty, melees won't take enough damage
    static bool IsSoaker(PartyRolesConfig.Assignment ass) => ass is PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.H2 or PartyRolesConfig.Assignment.R2;
}
class P2BahamutsClaw(BossModule module) : Components.CastCounter(module, AID.BahamutsClaw);
class P3FlareBreath(BossModule module) : Components.Cleave(module, AID.FlareBreath, new AOEShapeCone(29.2f, 45.Degrees()), (uint)OID.BahamutPrime); // TODO: verify angle
class P5MornAfah(BossModule module) : Components.StackWithCastTargets(module, AID.MornAfah, 4, 8); // TODO: verify radius

[ModuleInfo(PrimaryActorOID = (uint)OID.Twintania, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 280, PlanLevel = 70)]
public class UCOB(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(21))
{
    public static readonly ArenaBoundsSquare PathfindHugBorderBounds = new(21);

    private Actor? _nael;
    private Actor? _bahamutPrime;

    public Actor? Twintania() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
    public Actor? Nael() => _nael;
    public Actor? BahamutPrime() => _bahamutPrime;

    public override bool ShouldPrioritizeAllEnemies => true;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _nael ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies(OID.NaelDeusDarnus).FirstOrDefault() : null;
        _bahamutPrime ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies(OID.BahamutPrime).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(Twintania(), ArenaColor.Enemy);
        Arena.Actor(Nael(), ArenaColor.Enemy);
        Arena.Actor(BahamutPrime(), ArenaColor.Enemy);
    }
}
