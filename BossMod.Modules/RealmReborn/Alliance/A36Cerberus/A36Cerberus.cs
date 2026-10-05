namespace BossMod.RealmReborn.Alliance.A36Cerberus;

public enum OID : uint
{
    Boss = 0xDF6, // R10.800, x1
    Helper = 0x1B2, // R0.500
    HelperAlt = 0x8EE, // R0.500
    HoundTarget = 0x19A, // R0.500
    StomachWall = 0xDFA, // R1.500
    GastricJuice = 0xDF9, // R1.000
    Wolfsbane = 0xDF8, // R0.800
    Unknown = 0xDF7, // R1.800, belly add
    Electron = 0xDFB, // R1.000
    SlabberVoidzone = 0x1E968C, // R0.500, EventObj (EventState 7 = inactive)
}

public enum AID : uint
{
    AutoAttack = 3509, // Boss->player, no cast, single-target
    PredatorClaws = 3245, // Boss->self, no cast, range 9+R cone
    TailBlow = 3246, // Boss->self, 2.0s cast, range 9+R 90-degree cone
    Innerspace = 3248, // Boss->player, no cast, single-target
    Mini = 3249, // GastricJuice->self, 3.0s cast, range 8+R circle
    Slabber = 3241, // Boss->location, 2.9s cast, range 8 circle
    Voidzone = 3376, // HelperAlt->self, no cast, range 8 circle
    Engorge = 3243, // Boss->location, no cast, range 8 circle
    Seedvolley = 344, // Wolfsbane->player, no cast, single-target
    DeathRay = 1913, // Unknown->player, no cast, single-target
    SulphurousBreath = 3250, // Boss->self, 2.0s cast, range 25+R width 6 rect
    SulphurousBreathHelper = 3251, // Helper->self, 3.0s cast, range 40+R width 6 rect
    Devour = 3242, // Boss->location, no cast, range 8 circle
    SourSough = 3510, // Wolfsbane->self, no cast, range 6+R cone
    Spew = 3244, // HelperAlt->self, no cast, range 60+R circle
    LightningBolt = 3252, // Boss->location, no cast, range 8 circle
    LightningBoltCharge = 3253, // Electron->Electron, 2.0s cast, width 4 rect charge
    HoundOutOfHell = 3247, // Boss->HoundTarget, 3.5s cast, width 14 rect charge
    Ululation = 3254, // Boss->self, 5.0s cast, range 80+R circle
    Reawakening = 3507, // Boss->self, 50.0s cast, single-target
    AutoAttackAdd = 872, // Wolfsbane/Unknown->player, no cast, single-target
    HexEye = 1914, // Unknown->self, 2.5s cast, range 3+R circle
}

public enum SID : uint
{
    Minimum = 438, // GastricJuice->player, extra=0xA
}

static class Belly
{
    public const float HeightThreshold = -100f;

    public static bool InBelly(Actor actor) => actor.PosRot.Y < HeightThreshold;
    public static bool IsMini(Actor actor) => actor.FindStatus(SID.Minimum) != null;

    public static bool WantsBelly(PartyState party, Actor actor)
        => actor.Role is Role.Melee or Role.Ranged || party.Alliance == AllianceLetter.B; // A = all get in the belly, B = belly is where you should go, C = could you please get in the belly
}

class BellyArena(BossModule module) : BossComponent(module)
{
    public override void Update()
    {
        var pc = Raid.Player();
        if (pc == null)
            return;
        if (Belly.InBelly(pc))
        {
            Arena.Center = A36Cerberus.BellyCenter;
            Arena.Bounds = A36Cerberus.BellyBounds;
        }
        else
        {
            Arena.Center = A36Cerberus.OutsideCenter;
            Arena.Bounds = A36Cerberus.OutsideBounds;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Belly.InBelly(actor))
            return;

        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.StomachWall or OID.Unknown => 2,
                _ => AIHints.Enemy.PriorityForbidden
            };
        }
    }
}

class TailBlow(BossModule module) : Components.StandardAOEs(module, AID.TailBlow, new AOEShapeCone(19.8f, 45.Degrees()))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Belly.InBelly(actor) ? [] : base.ActiveAOEs(slot, actor);
}

class SulphurousBreath(BossModule module) : Components.StandardAOEs(module, AID.SulphurousBreath, new AOEShapeRect(35.8f, 3))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Belly.InBelly(actor) ? [] : base.ActiveAOEs(slot, actor);
}

class SulphurousBreathHelper(BossModule module) : Components.StandardAOEs(module, AID.SulphurousBreathHelper, new AOEShapeRect(40.5f, 3))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Belly.InBelly(actor) ? [] : base.ActiveAOEs(slot, actor);
}

class HoundOutOfHell(BossModule module) : Components.ChargeAOEs(module, AID.HoundOutOfHell, 7)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Belly.InBelly(actor) ? [] : base.ActiveAOEs(slot, actor);
}

class LightningBoltCharge(BossModule module) : Components.ChargeAOEs(module, AID.LightningBoltCharge, 2)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Belly.InBelly(actor) ? [] : base.ActiveAOEs(slot, actor);
}

class HexEye(BossModule module) : Components.StandardAOEs(module, AID.HexEye, new AOEShapeCircle(4.8f));
class Ululation(BossModule module) : Components.RaidwideCast(module, AID.Ululation, "Stay near allies!")
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Belly.InBelly(actor))
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class Wolfsbane(BossModule module) : Components.Adds(module, (uint)OID.Wolfsbane, 1)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Belly.InBelly(actor))
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class GastricJuiceAdd(BossModule module) : Components.Adds(module, (uint)OID.GastricJuice)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Belly.InBelly(actor))
            return;
        foreach (var a in ActiveActors)
            hints.SetPriority(a, AIHints.Enemy.PriorityForbidden);
    }
}

class Electrons(BossModule module) : Components.Adds(module, (uint)OID.Electron);
class StomachAdds(BossModule module) : Components.AddsMulti(module, [OID.StomachWall, OID.Unknown], 2);

class Mini : Components.StandardAOEs
{
    public Mini(BossModule module) : base(module, AID.Mini, new AOEShapeCircle(9))
    {
        InvertedText = "Get hit by Mini!";
        WarningText = "GTFO from Mini!";
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Belly.InBelly(actor))
            yield break;
        var inverted = Belly.WantsBelly(Raid, actor) && !Belly.IsMini(actor);
        foreach (var aoe in base.ActiveAOEs(slot, actor))
            yield return aoe with { Inverted = inverted };
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Belly.InBelly(actor))
            return;
        var aoes = ActiveAOEs(slot, actor).Where(a => a.Risky).ToList();
        if (aoes.Count == 0)
            return;
        if (aoes[0].Inverted)
            hints.AddForbiddenZone(p => aoes.Max(a => a.Distance(p)), aoes.Min(a => a.Activation));
        else
            foreach (var c in aoes)
                hints.AddForbiddenZone(c.Distance, c.Activation);
    }
}

class Slabber : Components.VoidzoneAtCastTarget
{
    public Slabber(BossModule module) : base(module, 8, AID.Slabber, OID.SlabberVoidzone, 0)
    {
        WarningText = "GTFO from Slabber!";
        InvertedText = "Go to Slabber!";
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Belly.InBelly(actor))
            yield break;
        var inverted = Belly.IsMini(actor);
        foreach (var aoe in base.ActiveAOEs(slot, actor))
            yield return aoe with { Inverted = inverted };
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Belly.InBelly(actor))
            return;
        var aoes = ActiveAOEs(slot, actor).Where(a => a.Risky).ToList();
        if (aoes.Count == 0)
            return;
        if (aoes[0].Inverted)
            hints.AddForbiddenZone(p => aoes.Max(a => a.Distance(p)), aoes.Min(a => a.Activation));
        else
            foreach (var c in aoes)
                hints.AddForbiddenZone(c.Distance, c.Activation);
    }
}

class A36CerberusStates : StateMachineBuilder
{
    public A36CerberusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TailBlow>()
            .ActivateOnEnter<Slabber>()
            .ActivateOnEnter<Mini>()
            .ActivateOnEnter<SulphurousBreath>()
            .ActivateOnEnter<SulphurousBreathHelper>()
            .ActivateOnEnter<HoundOutOfHell>()
            .ActivateOnEnter<LightningBoltCharge>()
            .ActivateOnEnter<HexEye>()
            .ActivateOnEnter<Ululation>()
            .ActivateOnEnter<Wolfsbane>()
            .ActivateOnEnter<GastricJuiceAdd>()
            .ActivateOnEnter<Electrons>()
            .ActivateOnEnter<StomachAdds>()
            .ActivateOnEnter<BellyArena>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 111, NameID = 3234)]
public class A36Cerberus(ModuleInit init) : BossModule(init, OutsideCenter, OutsideBounds)
{
    public static readonly WPos OutsideCenter = new(0, -198);
    public static readonly ArenaBoundsRect OutsideBounds = new(20, 40);
    public static readonly WPos BellyCenter = new(1, -200);
    // this isn't accurate but it includes all the stomach walls and is *most* of the arena. cba shaping the actual shape
    public static readonly ArenaBoundsCircle BellyBounds = new(13);
}
