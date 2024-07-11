namespace BossMod.Dawntrail.Dungeon.D02WorqorZormor.D023Gurfurlur;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x32, 523 type
    Boss = 0x415F, // R7.000, x1
    AuraSphere = 0x4162, // R1.000, x0 (spawn during fight)
    BitingWind = 0x4160, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    HeavingHaymaker = 36375, // Helper->self, 5.3s cast, range 60 circle
    LithicImpact = 36302, // Helper->self, 6.8s cast, range 4 width 4 rect
    GreatFlood = 36307, // Helper->self, 7.0s cast, range 80 width 60 rect
    Allfire1 = 36303, // Helper->self, 7.0s cast, range 10 width 10 rect
    Allfire2 = 36304, // Helper->self, 8.5s cast, range 10 width 10 rect
    Allfire3 = 36305, // Helper->self, 10.0s cast, range 10 width 10 rect
    VolcanicDrop = 36306, // Helper->player, 5.0s cast, range 6 circle
    Sledgehammer = 36313, // Boss->self, 5.0s cast, range 60 width 8 rect
    SledgehammerEnd = 39260, // Boss->self, no cast, range 60 width 8 rect
    EnduringGlory = 36320, // Boss->self, 6.0s cast, range 60 circle
    Windswrath1 = 36310, // Helper->self, 7.0s cast, range 40 circle
    Windswrath2 = 39074, // Helper->self, 15.0s cast, range 40 circle
}

class HeavingHaymaker(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.HeavingHaymaker));
class LithicImpact(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LithicImpact), new AOEShapeRect(2, 2, 2));
class GreatFlood(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.GreatFlood), distance: 25, kind: Kind.DirForward)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var caster = Casters.FirstOrDefault();
        if (caster?.CastInfo == null)
            return;

        var knockbackTime = caster.CastInfo.NPCFinishAt;

        if (IsImmune(slot, knockbackTime))
            return;

        hints.AddForbiddenZone(new AOEShapeRect(20, 20, 5), Arena.Center, caster.Rotation, knockbackTime);
    }
}

class Allfire(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance>[] _aoes = [[], [], []];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.FirstOrDefault(x => x.Count != 0) ?? [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var castStage = GetStage(spell.Action.ID);
        if (castStage >= 0)
            _aoes[castStage].Add(new AOEInstance(new AOEShapeRect(5, 5, 5), caster.Position, default, spell.NPCFinishAt));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var castStage = GetStage(spell.Action.ID);
        if (castStage >= 0)
            _aoes[castStage].Clear();
    }

    private int GetStage(uint id) => GetStage((AID)id);

    private int GetStage(AID id) => id switch
    {
        AID.Allfire1 => 0,
        AID.Allfire2 => 1,
        AID.Allfire3 => 2,
        _ => -1
    };
}

class VolcanicDrop(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.VolcanicDrop), 6);

class Sledgehammer(BossModule module) : Components.GenericWildCharge(module, 4, ActionID.MakeSpell(AID.Sledgehammer), 60)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Sledgehammer)
        {
            Source = caster;
            foreach (var (i, p) in Raid.WithSlot(true))
                PlayerRoles[i] = p.InstanceID == spell.TargetID ? PlayerRole.Target : PlayerRole.Share;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SledgehammerEnd)
            Source = null;
    }
}

class AuraSpheres : Components.PersistentInvertibleVoidzone
{
    private bool IsActive => Sources(Module).Any();

    public AuraSpheres(BossModule module) : base(module, 2.5f, m => m.Enemies(OID.AuraSphere).Where(x => !x.IsDead))
    {
        InvertResolveAt = DateTime.MaxValue;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!IsActive)
            return;

        if (!Sources(Module).Any(x => Shape.Check(actor.Position, x)))
            hints.Add("Touch the balls!");
    }
}

class EnduringGlory(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.EnduringGlory));
abstract class Windswrath(BossModule module, ActionID aid) : Components.KnockbackFromCastTarget(module, aid, 15)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var caster = Casters.FirstOrDefault();
        if (caster?.CastInfo == null)
            return;

        var knockbackTime = caster.CastInfo.NPCFinishAt;

        if (IsImmune(slot, knockbackTime))
            return;

        hints.AddForbiddenZone(new AOEShapeDonut(5, 60), caster.Position, activation: knockbackTime);
    }
}
class Windswrath1(BossModule module) : Windswrath(module, ActionID.MakeSpell(AID.Windswrath1));
class Windswrath2(BossModule module) : Windswrath(module, ActionID.MakeSpell(AID.Windswrath2));
class BitingWind(BossModule module) : Components.PersistentVoidzone(module, 4, m => m.Enemies(OID.BitingWind));

class D023GurfurlurStates : StateMachineBuilder
{
    public D023GurfurlurStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HeavingHaymaker>()
            .ActivateOnEnter<LithicImpact>()
            .ActivateOnEnter<GreatFlood>()
            .ActivateOnEnter<Allfire>()
            .ActivateOnEnter<VolcanicDrop>()
            .ActivateOnEnter<Sledgehammer>()
            .ActivateOnEnter<AuraSpheres>()
            .ActivateOnEnter<EnduringGlory>()
            .ActivateOnEnter<Windswrath1>()
            .ActivateOnEnter<Windswrath2>()
            .ActivateOnEnter<BitingWind>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "xan", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 824, NameID = 12705)]
public class D023Gurfurlur(WorldState ws, Actor primary) : BossModule(ws, primary, new(-54, -195), new ArenaBoundsSquare(20));
