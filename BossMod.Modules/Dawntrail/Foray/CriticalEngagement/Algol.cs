namespace BossMod.Dawntrail.Foray.CriticalEngagement.Algol;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x32, Helper type
    Boss = 0x4C4B, // R7.500, x1
    Algol = 0x4D87, // R6.000, x5
    CrescentTomato = 0x4C4C, // R0.900, x4
    CrescentOnion = 0x4C4D, // R0.900, x4
}

public enum AID : uint
{
    AutoAttack = 50644, // Boss->player, no cast, single-target
    DeathWall = 48118, // 4D87->self, no cast, range 24-30 donut
    ShrillPeal1 = 50426, // Boss->self, 3.0s cast, ???, raidwide
    ShrillPeal2 = 50427, // Helper->self, 4.0s cast, ???, raidwide, hits the other CE participants
    CursedScreech1 = 48100, // Boss->self, 5.0s cast, ???, raidwide
    CursedScreech2 = 48971, // Helper->self, 6.0s cast, ???, raidwide, hits the other CE participants
    InhaleCast1 = 48101, // Boss->self, 2.0+1.0s cast, single-target
    InhaleBoss1 = 48102, // Boss->self, no cast, single-target
    InhaleAOE = 48104, // 4D87->self, 3.5s cast, range 60 30-degree cone
    InhaleAdds = 48103, // Helper->4C4C/4C4D, 0.7s cast, single-target
    DevourSmall = 50469, // Helper->self, 6.8s cast, range 8 120-degree cone
    Regurgitomato = 48106, // Boss->location, no cast, single-target
    Regurgitonion = 48107, // Boss->location, no cast, single-target
    RottenTomatoFirst = 48109, // Helper->self, 4.0s cast, range 50 width 6 rect
    RottenOnionFirst = 48110, // Helper->self, 4.0s cast, range 60 30-degree cone
    RottenTomatoRest = 48111, // Helper->self, 2.0s cast, range 50 width 6 rect
    RottenOnionRest = 48112, // Helper->self, 2.0s cast, range 60 30-degree cone
    SpinningInhaleStart = 48113, // Boss->self, 5.0s cast, range 30 30-degree cone
    SpinningInhaleRepeat = 48249, // Helper->self, no cast, range 7 ?-degree cone
    SpinningInhaleUnk1 = 48114, // 4D87->self, no cast, range ?-30 donut
    SpinningInhaleUnk2 = 50942, // 4D87->self, no cast, range ?-30 donut
    Jump = 48115, // Boss->self, no cast, single-target
    DevourAdds = 48105, // Boss->self, no cast, range 12 ?-degree cone, only hits adds, not players
    DevourBig1 = 50422, // Helper->self, 3.0s cast, range 12 120-degree cone
    DevourBig2 = 50467, // Helper->self, 3.0s cast, range 12 120-degree cone
    DigestedJuiceBoss = 48116, // Boss->self, 4.0s cast, range 40 width 50 rect
    DigestedJuiceBossRepeat = 50423, // Boss->self, no cast, single-target
    DigestedJuice = 50424, // Helper->self, 4.0s cast, range 40 width 50 rect
    MaladyVisual = 48117, // Boss->self, no cast, range 12 circle
    Malady = 50425, // Helper->self, 3.0s cast, range 11 circle
}

public enum SID : uint
{
    Stun = 5411, // Algol/Helper->player, extra=0xEC7
}

class CursedScreech(BossModule module) : Components.RaidwideCast(module, AID.CursedScreech1);
class ShrillPeal(BossModule module) : Components.RaidwideCast(module, AID.ShrillPeal2);

class Inhale(BossModule module) : Components.StandardAOEs(module, AID.InhaleAOE, new AOEShapeCone(60, 15.Degrees()));

class DevourSmall(BossModule module) : Components.StandardAOEs(module, AID.DevourSmall, new AOEShapeCone(8, 60.Degrees()));

class RottenOnion : Components.GenericRotatingAOE
{
    public RottenOnion(BossModule module) : base(module)
    {
        FutureRisky = false;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RottenOnionFirst)
        {
            Sequences.Add(new(new AOEShapeCone(60, 15.Degrees()), spell.LocXZ, spell.Rotation, -30.Degrees(), Module.CastFinishAt(spell), 2, 4));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.RottenOnionFirst or AID.RottenOnionRest)
            AdvanceSequence(caster.Position, spell.Rotation, WorldState.CurrentTime);
    }
}

class RottenTomato : Components.Exaflare
{
    public RottenTomato(BossModule module) : base(module, new AOEShapeRect(50, 3))
    {
        FutureRisky = false;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RottenTomatoFirst)
        {
            var face = caster.Rotation.ToDirection();
            var advance = face.OrthoL() * 6 * (face.Dot(caster.DirectionTo(Arena.Center)) > 0 ? 1 : -1);
            Lines.Add(new()
            {
                Next = spell.LocXZ,
                Advance = advance,
                Rotation = spell.Rotation,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 2,
                ExplosionsLeft = 4,
                MaxShownExplosions = 2
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.RottenTomatoFirst or AID.RottenTomatoRest)
        {
            // TODO: is this really not part of CastEvent?
            var actualSource = caster.Position - caster.Rotation.ToDirection() * 25;

            var ix = Lines.FindIndex(l => l.Next.AlmostEqual(actualSource, 0.5f));
            if (ix >= 0)
                AdvanceLine(Lines[ix], actualSource);
        }
    }
}

class SpinningInhale(BossModule module) : Components.GenericRotatingAOE(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SpinningInhaleStart)
            Sequences.Add(new(new AOEShapeCone(30, 15.Degrees()), Arena.Center, spell.Rotation, -15.Degrees(), Module.CastFinishAt(spell), 0.3f, 25, 18));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SpinningInhaleRepeat)
            AdvanceSequence(Arena.Center, spell.Rotation, WorldState.CurrentTime);
    }
}

class DevourBig(BossModule module) : Components.GroupedAOEs(module, [AID.DevourBig1, AID.DevourBig2], new AOEShapeCone(12, 60.Degrees()));
class DigestedJuice(BossModule module) : Components.GroupedAOEs(module, [AID.DigestedJuiceBoss, AID.DigestedJuice], new AOEShapeRect(40, 25));
class Malady(BossModule module) : Components.StandardAOEs(module, AID.Malady, 11);

class Stun(BossModule module) : BossComponent(module)
{
    BitMask _stunned;
    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.Stun && Raid.TryFindSlot(actor, out var slot))
            _stunned.Set(slot);
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.Stun && Raid.TryFindSlot(actor, out var slot))
            _stunned.Clear(slot);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.ShouldCleanse |= _stunned;
    }
}

class AlgolStates : StateMachineBuilder
{
    public AlgolStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ShrillPeal>()
            .ActivateOnEnter<CursedScreech>()
            .ActivateOnEnter<Inhale>()
            .ActivateOnEnter<RottenOnion>()
            .ActivateOnEnter<RottenTomato>()
            .ActivateOnEnter<SpinningInhale>()
            .ActivateOnEnter<DevourSmall>()
            .ActivateOnEnter<DevourBig>()
            .ActivateOnEnter<DigestedJuice>()
            .ActivateOnEnter<Malady>()
            .ActivateOnEnter<Stun>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14790)]
public class Algol(WorldState ws, Actor primary) : CEModule(ws, primary, new(765, 0), new ArenaBoundsCircle(24));
