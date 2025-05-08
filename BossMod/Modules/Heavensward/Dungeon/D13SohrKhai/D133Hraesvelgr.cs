namespace BossMod.Heavensward.Dungeon.D13SohrKhai.D133Hraesvelgr;

public enum OID : uint
{
    Boss = 0x3D17,
    Helper = 0x233C,
}

public enum AID : uint
{
    Wyrmclaw = 32141, // Boss->player, 5.0s cast, single-target
    HallowedWingsLeft = 32136, // Boss->self, 6.0s cast, range 50 width 22 rect, regular aoe (with offset)
    HallowedWingsRight = 32137, // Boss->self, 6.0s cast, range 50 width 22 rect, regular aoe (with offset)
    HolyStorm = 32127, // Boss->self, 5.0s cast, range 40 circle, raidwide
    HallowedDive = 32131, // Boss->self, 6.0s cast, range 40 width 20 rect, regular aoe
    HolyOrb = 32129, // Helper->self, 5.0s cast, range 6 circle, exaflares
    HolyOrbRepeat = 32130, // Helper->self, no cast, range 6 circle
    AkhMorn = 32132, // Boss->players, 5.0s cast, range 6 circle, stack 1
    AkhMornRepeat = 32133, // Boss->players, no cast, range 6 circle, stack 2
    HolyBreathVisual = 32138, // Boss->self, 5.0+1.0s cast, single-target
    HolyBreath = 32139, // Helper->player, 6.0s cast, range 6 circle, spread
    DiamondStorm = 32128, // Boss->self, 5.0s cast, range 40 circle, raidwide + ice
    FrigidDive = 32134, // Boss->self, 6.0s cast, range 40 width 20 rect, regular aoe
    FrostedOrb = 32135, // Helper->self, 5.0s cast, range 6 circle, regular aoe
}

class HolyStorm(BossModule module) : Components.RaidwideCast(module, AID.HolyStorm);
class DiamondStorm(BossModule module) : Components.RaidwideCast(module, AID.DiamondStorm);
class FrigidDive(BossModule module) : Components.StandardAOEs(module, AID.FrigidDive, new AOEShapeRect(40, 10));
class HallowedDive(BossModule module) : Components.StandardAOEs(module, AID.HallowedDive, new AOEShapeRect(40, 10));
class Wyrmclaw(BossModule module) : Components.SingleTargetCast(module, AID.Wyrmclaw);
class FrostedOrb(BossModule module) : Components.StandardAOEs(module, AID.FrostedOrb, new AOEShapeCircle(6));
class HolyBreath(BossModule module) : Components.SpreadFromIcon(module, 311, AID.HolyBreath, 6, 6);
class HallowedWings(BossModule module) : Components.GroupedAOEs(module, [AID.HallowedWingsLeft, AID.HallowedWingsRight], new AOEShapeRect(50, 11));
class AkhMorn(BossModule module) : Components.UniformStackSpread(module, 6, 0)
{
    private int Counter;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AkhMorn)
            AddStack(WorldState.Actors.Find(spell.TargetID)!, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AkhMorn or AID.AkhMornRepeat)
            if (++Counter >= 6)
                Stacks.Clear();
    }
}
class HolyOrb(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(6))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HolyOrb)
            Lines.Add(new Line()
            {
                Next = caster.Position,
                Advance = caster.Rotation.ToDirection() * 6.8f,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 0.95f,
                ExplosionsLeft = 5,
                MaxShownExplosions = 3
            });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.HolyOrb or AID.HolyOrbRepeat)
        {
            int index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1));
            if (index == -1)
            {
                ReportError($"Failed to find entry for {caster.InstanceID:X}");
                return;
            }

            AdvanceLine(Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}

class HraesvelgrStates : StateMachineBuilder
{
    public HraesvelgrStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Wyrmclaw>()
            .ActivateOnEnter<HallowedWings>()
            .ActivateOnEnter<FrigidDive>()
            .ActivateOnEnter<HallowedDive>()
            .ActivateOnEnter<HolyStorm>()
            .ActivateOnEnter<DiamondStorm>()
            .ActivateOnEnter<AkhMorn>()
            .ActivateOnEnter<FrostedOrb>()
            .ActivateOnEnter<HolyBreath>()
            .ActivateOnEnter<HolyOrb>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 171, NameID = 4954, Contributors = "xan")]
public class Hraesvelgr(WorldState ws, Actor primary) : BossModule(ws, primary, new(400, -400), new ArenaBoundsCircle(20));
