namespace BossMod.RealmReborn.Raid.T04Gauntlet;

public enum OID : uint
{
    ClockworkBugP1 = 0x8EA, // R1.200, x6
    ClockworkBug = 0x7E0, // R1.200, spawn during fight
    ClockworkSoldier = 0x7E1, // R1.400, spawn during fight
    ClockworkKnight = 0x7E2, // R1.400, spawn during fight
    SpinnerRook = 0x7E3, // R0.500, spawn during fight
    ClockworkDreadnaught = 0x7E4, // R3.000, spawn during fight
    DriveCylinder = 0x1B2, // R0.500, x1
    TerminalEnd = 0x1E86FA,
    TerminalStart = 0x1E86F9,
};

public enum AID : uint
{
    AutoAttack = 872, // ClockworkBugP1/ClockworkBug/ClockworkSoldier/ClockworkKnight/SpinnerRook/ClockworkDreadnaught->player, no cast, single-target
    Leech = 1230, // ClockworkBugP1/ClockworkBug->player, no cast, single-target
    HeadspinSoldier = 1231, // ClockworkSoldier->self, 0.5s cast, range 4+R circle cleave
    HeadspinKnight = 1233, // ClockworkKnight->self, 0.5s cast, range 4+R circle cleave
    Electromagnetism = 673, // ClockworkKnight->player, no cast, single-target attract
    BotRetrieval = 1239, // ClockworkDreadnaught->self, no cast, single-target, kills bug and buffs dreadnaught
    Rotoswipe = 1238, // ClockworkDreadnaught->self, no cast, range 8+R ?-degree cone cleave
    GravityThrust = 1236, // SpinnerRook->player, 1.5s cast, single-target damage (avoidable by moving to the back)
    Pox = 1237, // SpinnerRook->player, 3.0s cast, single-target debuff (avoidable by moving to the back)
    EmergencyOverride = 1258, // DriveCylinder->self, no cast, soft enrage raidwide
};

class Rotoswipe : Components.Cleave
{
    public Rotoswipe() : base(ActionID.MakeSpell(AID.Rotoswipe), new AOEShapeCone(11, 60.Degrees()), (uint)OID.ClockworkDreadnaught) { } // TODO: verify angle
}

class GravityThrustPox : Components.GenericAOEs
{
    private static readonly AOEShape _shape = new AOEShapeRect(50, 50);

    public GravityThrustPox() : base(new(), "Move behind rook!") { }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        foreach (var c in ((T04Gauntlet)module).Rooks.Where(a => a.CastInfo?.TargetID == actor.InstanceID))
            yield return new(_shape, c.Position, c.CastInfo!.Rotation, c.CastInfo.NPCFinishAt);
    }
}

class EmergencyOverride : Components.CastCounter
{
    public EmergencyOverride() : base(ActionID.MakeSpell(AID.EmergencyOverride)) { }
}

class T04GauntletStates : StateMachineBuilder
{
    private T04Gauntlet _module;

    public T04GauntletStates(T04Gauntlet module) : base(module)
    {
        _module = module;
        SimplePhase(0, SinglePhase, "Gauntlet")
            .Raw.Update = () => module.Enemies(OID.TerminalStart).Any(e => e.IsTargetable) || module.Enemies(OID.TerminalEnd).Any(e => e.IsTargetable) || module.Enemies(OID.TerminalStart) != null || module.Enemies(OID.TerminalEnd) != null;
    }

    private void SinglePhase(uint id)
    {
        Condition(id + 0x00000, 61.1f, () => _module.Soldiers.Any(e => e.IsTargetable) || _module.Knights.Any(e => e.IsTargetable), "2x Soldiers + 2x Knights");
        Condition(id + 0x10000, 60.0f, () => _module.Dreadnaughts.Any(e => e.IsTargetable), "Dreadnaught + 4x Bugs");
        Condition(id + 0x20000, 60.0f, () => _module.Rooks.Any(e => e.IsTargetable), "2x Rooks + 4x Bugs")
            .ActivateOnEnter<Rotoswipe>();
        Condition(id + 0x30000, 60.0f, () => _module.Dreadnaughts.Any(e => e.IsTargetable && !e.IsDead) && _module.Soldiers.Any(e => e.IsTargetable && !e.IsDead) && _module.Knights.Any(e => e.IsTargetable && !e.IsDead), "Dreadnaught + Soldier + Knight")
            .ActivateOnEnter<GravityThrustPox>();
        Condition(id + 0x40000, 60.0f, () => _module.Dreadnaughts.Any(e => e.IsTargetable && !e.IsDead) && _module.Soldiers.Any(e => e.IsTargetable && !e.IsDead) && _module.Knights.Any(e => e.IsTargetable && !e.IsDead) && _module.Bugs.Any(e => e.IsTargetable && !e.IsDead), "Dreadnaught + Soldier + Knight + Rook + 2x Bugs");
        ComponentCondition<EmergencyOverride>(id + 0x50000, 118.9f, comp => comp.NumCasts > 0, "Soft enrage")
            .ActivateOnEnter<EmergencyOverride>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.TerminalStart, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 96)]
public class T04Gauntlet : BossModule
{
    public IReadOnlyList<Actor> P1Bugs;
    public IReadOnlyList<Actor> Bugs;
    public IReadOnlyList<Actor> Soldiers;
    public IReadOnlyList<Actor> Knights;
    public IReadOnlyList<Actor> Rooks;
    public IReadOnlyList<Actor> Dreadnaughts;

    public T04Gauntlet(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(0, 0), 25))
    {
        P1Bugs = Enemies(OID.ClockworkBugP1);
        Bugs = Enemies(OID.ClockworkBug);
        Soldiers = Enemies(OID.ClockworkSoldier);
        Knights = Enemies(OID.ClockworkKnight);
        Rooks = Enemies(OID.SpinnerRook);
        Dreadnaughts = Enemies(OID.ClockworkDreadnaught);
    }

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);

        // note: we don't bother checking for physical/magical defense on knights/soldiers and just have everyone aoe them down; magic reflect is very small
        // note: we try to kill dreadnaught first, since it's the only dangerous thing here
        // note: we try to offtank all bugs and not have dreadnaught eat them
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID == OID.ClockworkDreadnaught ? 2 : 1;
            e.AttackStrength = (OID)e.Actor.OID == OID.ClockworkDreadnaught ? 0.2f : 0.05f;
            e.ShouldBeTanked = assignment switch
            {
                PartyRolesConfig.Assignment.MT => (OID)e.Actor.OID != OID.ClockworkBug,
                PartyRolesConfig.Assignment.OT => (OID)e.Actor.OID == OID.ClockworkBug,
                _ => false
            };
        }
    }

    protected override bool CheckPull() => !Enemies(OID.TerminalStart).Any(a => a.IsTargetable) && Enemies(OID.TerminalStart) != null;

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        foreach (var e in P1Bugs)
            Arena.Actor(e, ArenaColor.Enemy);
        foreach (var e in Bugs)
            Arena.Actor(e, ArenaColor.Enemy);
        foreach (var e in Soldiers)
            Arena.Actor(e, ArenaColor.Enemy);
        foreach (var e in Knights)
            Arena.Actor(e, ArenaColor.Enemy);
        foreach (var e in Rooks)
            Arena.Actor(e, ArenaColor.Enemy);
        foreach (var e in Dreadnaughts)
            Arena.Actor(e, ArenaColor.Enemy);
    }
}