namespace BossMod.Heavensward.Dungeon.D13SohrKhai.D133Hraesvelgr;

public enum OID : uint
{
    Boss = 0x3D17,
    Helper = 0x233C,
}

public enum AID : uint
{
    _AutoAttack_ = 32142, // Boss->player, no cast, single-target
    _Weaponskill_Wyrmclaw = 32141, // Boss->player, 5.0s cast, single-target
    _Weaponskill_HallowedWings = 32136, // Boss->self, 6.0s cast, range 50 width 22 rect, regular aoe (with offset)
    _Weaponskill_HallowedWings1 = 32137, // Boss->self, 6.0s cast, range 50 width 22 rect, regular aoe (with offset)
    _Weaponskill_HolyStorm = 32127, // Boss->self, 5.0s cast, range 40 circle, raidwide
    _Weaponskill_HallowedDive = 32131, // Boss->self, 6.0s cast, range 40 width 20 rect, regular aoe
    _Weaponskill_HolyOrb = 32129, // Helper->self, 5.0s cast, range 6 circle, exaflares
    _Weaponskill_HolyOrb1 = 32130, // Helper->self, no cast, range 6 circle
    _Weaponskill_AkhMorn = 32132, // Boss->players, 5.0s cast, range 6 circle, stack 1
    _Weaponskill_AkhMorn1 = 32133, // Boss->players, no cast, range 6 circle, stack 2
    _Weaponskill_HolyBreath = 32138, // Boss->self, 5.0+1.0s cast, single-target
    _Weaponskill_HolyBreath1 = 32139, // Helper->player, 6.0s cast, range 6 circle, spread
    _Weaponskill_DiamondStorm = 32128, // Boss->self, 5.0s cast, range 40 circle, raidwide + ice
    _Weaponskill_FrigidDive = 32134, // Boss->self, 6.0s cast, range 40 width 20 rect, regular aoe
    _Weaponskill_FrostedOrb = 32135, // Helper->self, 5.0s cast, range 6 circle, regular aoe
}

class HolyStorm(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_HolyStorm));
class DiamondStorm(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_DiamondStorm));
class FrigidDive(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_FrigidDive), new AOEShapeRect(40, 10));
class HallowedDive(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_HallowedDive), new AOEShapeRect(40, 10));
class Wyrmclaw(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID._Weaponskill_Wyrmclaw));
class FrostedOrb(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_FrostedOrb), new AOEShapeCircle(6));
class HolyBreath(BossModule module) : Components.SpreadFromIcon(module, 311, ActionID.MakeSpell(AID._Weaponskill_HolyBreath1), 6, 6);
class HallowedWings(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select(c => new AOEInstance(new AOEShapeRect(50, 11), c.CastInfo!.LocXZ, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo)));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_HallowedWings or AID._Weaponskill_HallowedWings1)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_HallowedWings or AID._Weaponskill_HallowedWings1)
            Casters.Remove(caster);
    }
}
class AkhMorn(BossModule module) : Components.UniformStackSpread(module, 6, 0)
{
    private int Counter;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_AkhMorn)
            AddStack(WorldState.Actors.Find(spell.TargetID)!, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_AkhMorn or AID._Weaponskill_AkhMorn1)
            if (++Counter >= 6)
                Stacks.Clear();
    }
}
class HolyOrb(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(6))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_HolyOrb)
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
        if ((AID)spell.Action.ID is AID._Weaponskill_HolyOrb or AID._Weaponskill_HolyOrb1)
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

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 171, NameID = 4954)]
public class Hraesvelgr(WorldState ws, Actor primary) : BossModule(ws, primary, new(400, -400), new ArenaBoundsCircle(20));

