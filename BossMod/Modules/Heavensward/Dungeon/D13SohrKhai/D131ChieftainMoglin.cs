namespace BossMod.Heavensward.Dungeon.D13SohrKhai.D131ChieftainMoglin;

public enum OID : uint
{
    Boss = 0x15FB, // R1.8
    PomguardPomchopper = 0x15FD, // R0.9
    PomguardPompincher = 0x1602, // R0.9
    CaptainMogsun = 0x15FC, // R0.9
    DemoniacalMogcane = 0x1603, // R5.0
    PomguardPomcrier = 0x1601, // R0.9
    PomguardPompiercer = 0x15FF, // R0.9
    PomguardPomfryer = 0x1600, // R0.9
    PomguardPomfluffer = 0x15FE, // R0.9
    Helper = 0x1B2
}

public enum AID : uint
{
    AutoAttack1 = 872, // Boss/PomguardPomfryer/PomguardPomcrier->player, no cast, single-target
    AutoAttack2 = 870, // PomguardPompincher/CaptainMogsun->player, no cast, single-target

    ThousandKuponzeCharge = 6019, // Boss->self, no cast, range 8+R 120-degree cone
    HundredKuponzeSwipe = 4429, // PomguardPomchopper->self, 3.0s cast, range 20+R 90-degree cone
    PoisonNeedle = 4432, // PomguardPompincher->player, no cast, single-target
    PomHoly = 6020, // Boss->location, 3.0s cast, range 50 circle, raidwide
    DemoniacalMogcane = 6017, // Boss->self, 2.0s cast, single-target
    PomBomVisual = 6018, // DemoniacalMogcane->self, no cast, single-target
    PomBom = 6212, // Helper->self, no cast, range 40+R width 4 rect
    MoogleEyeShot = 4427, // PomguardPompiercer->player, no cast, single-target

    MarchOfTheMoogles = 4431, // PomguardPomcrier->self, 5.0s cast, range 10 circle, apply damage buff to affected moogles
    PomPraiseVisual = 6015, // Boss->self, 13.0s cast, single-target
    PomPraise = 6016, // Helper->self, 13.0s cast, range 4 circle, raise moogle + apply HP buff to affected moogles
    SpinningMogshield = 4425, // CaptainMogsun->self, 3.0s cast, range 6+R circle
    PomCure = 4426, // PomguardPomfluffer->Boss, 3.0s cast, single-target
    PomFlare = 4428 // PomguardPomfryer->self, 6.0s cast, range 21+R circle
}

public enum SID : uint
{
    Invincibility = 325, // none->Boss, extra=0x0
    OffBalance = 1064 // none->PomguardPomchopper/PomguardPompincher/CaptainMogsun/PomguardPomfluffer, extra=0x0
}

class PomBom(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCross cross = new(40.5f, 2f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.DemoniacalMogcane)
        {
            _aoe = [new(cross, actor.Position.Quantized(), default, WorldState.FutureTime(6.3d))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.PomBom)
        {
            _aoe = [];
        }
    }
}

class ThousandKuponzeCharge(BossModule module) : Components.Cleave(module, (uint)AID.ThousandKuponzeCharge, new AOEShapeCone(9.8f, 60f.Degrees()))
{
    private bool active = true; // cleave happens near the start of the fight then again after every demoniacal mogcane cast

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DemoniacalMogcane)
            active = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ThousandKuponzeCharge)
            active = false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (active)
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (active)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (active)
            base.DrawArenaForeground(pcSlot, pc);
    }
}

class PomPraise(BossModule module) : BossComponent(module)
{
    private readonly List<WPos> positions = [];
    public List<Actor> RelevantMoogles = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.PomPraise)
            positions.Add(spell.LocXZ);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.PomPraiseVisual)
        {
            positions.Clear();
            RelevantMoogles.Clear();
        }
    }

    public override void Update()
    {
        var countP = positions.Count;
        if (countP != 0) // it is assumed that the moogle hitbox must not intersect the circle, as usual for NPCs
        {
            var smallMoogles = Module.Enemies(D131ChieftainMoglin.SmallMoogles);
            var count = smallMoogles.Count;
            var relevantMoogles = new List<Actor>(count);
            for (var i = 0; i < count; ++i)
            {
                var moogle = smallMoogles[i];
                if (moogle.FindStatus((uint)SID.OffBalance) != null)
                {
                    for (var j = 0; j < countP; ++j)
                    {
                        if (moogle.Position.InCircle(positions[j], 4.9f))
                        {
                            relevantMoogles.Add(moogle);
                            break;
                        }
                    }
                }
            }
            RelevantMoogles = relevantMoogles;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (RelevantMoogles.Count != 0)
        {
            var count = hints.PotentialTargets.Count;
            for (var i = 0; i < count; ++i)
            {
                var t = hints.PotentialTargets[i];
                if (RelevantMoogles.Contains(t.Actor))
                {
                    t.Priority = 3;
                }
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (RelevantMoogles.Count != 0)
            hints.Add("Push defeated moogles out of circles!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (RelevantMoogles.Count != 0)
        {
            var count = positions.Count;
            for (var i = 0; i < count; ++i)
                Arena.ZoneCircleOutline(positions[i], 4.9f, Colors.Vulnerable);
        }
    }
}

class HundredKuponzeSwipe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HundredKuponzeSwipe, new AOEShapeCone(20.9f, 45f.Degrees()));
class PomFlare(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PomFlare, 20.9f);
class PomHoly(BossModule module) : Components.RaidwideCast(module, (uint)AID.PomHoly);
class SpinningMogshield(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpinningMogshield, 6.9f);

class D131ChieftainMoglinStates : StateMachineBuilder
{
    public D131ChieftainMoglinStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PomBom>()
            .ActivateOnEnter<ThousandKuponzeCharge>()
            .ActivateOnEnter<HundredKuponzeSwipe>()
            .ActivateOnEnter<PomFlare>()
            .ActivateOnEnter<PomHoly>()
            .ActivateOnEnter<SpinningMogshield>()
            .ActivateOnEnter<PomPraise>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 171, NameID = 4943, SortOrder = 2)]
public class D131ChieftainMoglin(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(-400f, -158.04f), 19.5f * CosPI.Pi60th, 64)], [new Rectangle(new(-400f, -138f), 20f, 1.05f),
    new Rectangle(new(-400f, -178f), 20f, 0.8f)]);

    public static readonly uint[] SmallMoogles = [(uint)OID.CaptainMogsun, (uint)OID.PomguardPomfluffer, (uint)OID.PomguardPomfryer, (uint)OID.PomguardPompincher,
    (uint)OID.PomguardPomchopper, (uint)OID.PomguardPompiercer, (uint)OID.PomguardPomcrier];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        if (PrimaryActor.FindStatus((uint)SID.Invincibility) == null)
            Arena.Actor(PrimaryActor);
        var smallmoogles = Enemies(SmallMoogles);
        var count = smallmoogles.Count;
        List<Actor> balancedMoogles = [with(count)];
        List<Actor> offBalanceMoogles = [with(count)];
        for (var i = 0; i < count; ++i)
        {
            var moogle = smallmoogles[i];
            if (moogle.FindStatus((uint)SID.OffBalance) == null)
                balancedMoogles.Add(moogle);
            else
                offBalanceMoogles.Add(moogle);
        }
        Arena.Actors(balancedMoogles);
        Arena.Actors(offBalanceMoogles, Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (FindComponent<PomPraise>()?.RelevantMoogles.Count == 0)
        {
            var count = hints.PotentialTargets.Count;
            for (var i = 0; i < count; ++i)
            {
                var e = hints.PotentialTargets[i];
                if (e.Actor.FindStatus((uint)SID.Invincibility) != null)
                {
                    e.Priority = AIHints.Enemy.PriorityInvincible;
                    continue;
                }
                e.Priority = e.Actor.OID switch
                {
                    (uint)OID.CaptainMogsun => 2,
                    (uint)OID.PomguardPomfluffer or (uint)OID.PomguardPomfryer or (uint)OID.PomguardPomcrier or
                    (uint)OID.PomguardPompincher or (uint)OID.PomguardPompiercer or (uint)OID.PomguardPomchopper => 1,
                    _ => 0
                };
            }
        }
    }
}
