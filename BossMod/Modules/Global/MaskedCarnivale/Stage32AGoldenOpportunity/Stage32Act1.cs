namespace BossMod.Global.MaskedCarnivale.Stage32.Act1;

public enum OID : uint
{
    Boss = 0x3FA5, //R=2.5
    GlitteringSlime = 0x3FAB, // R=2.0
    BallOfFire = 0x3FA6, // R=1.5
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 34444, // Boss->player, no cast, single-target
    GoldorFireIII = 34447, // Boss->self, 4,2s cast, single-target
    GoldorFireIII2 = 34448, // Helper->location, 5,0s cast, range 8 circle
    GoldorFireIII3 = 34449, // Helper->location, 2,5s cast, range 8 circle
    GoldorBlast = 34450, // Boss->self, 3,5s cast, range 60 width 10 rect
    SlimySummon = 34462, // Boss->self, 4,0s cast, single-target, summons glittering slime
    Rupture = 34463, // GlitteringSlime->self, 8,0s cast, range 100 circle, enrage, slime must be killed fast with The Ram's Voice + Ultravibration
    GoldorQuakeVisual = 34456, // Boss->self, 3,0s cast, single-target
    GoldorQuake1 = 34457, // Helper->self, 4,0s cast, range 10 circle
    GoldorQuake2 = 34458, // Helper->self, 5,5s cast, range 10-20 donut
    GoldorQuake3 = 34459, // Helper->self, 7,0s cast, range 20-30 donut
    GoldorFire = 34445, // Boss->self, 4,0s cast, single-target
    GoldorAeroIII = 34460, // Boss->self, 4,0s cast, range 50 circle, raidwide, knockback 10 away from source
    Burn = 34446, // BallOfFire->self, 12,0s cast, range 10 circle
    GoldorGravity = 34451, // Boss->self, 5,0s cast, single-target
    GoldorGravity2 = 34452, // Helper->player, no cast, single-target, heavy damage + heavy
    GoldorThunderIIIVisual = 34453, // Boss->self, 4,0s cast, single-target
    GoldorThunderIII1 = 34454, // Helper->player, no cast, range 5 circle, applies cleansable electrocution
    GoldorThunderIII2 = 34455, // Helper->location, 2,5s cast, range 6 circle
    GoldorBlizzardIIIVisual = 34589, // Boss->self, 6,0s cast, single-target, interruptible, freezes player
    GoldorBlizzardIII = 34590, // Helper->player, no cast, range 6 circle
}

public enum SID : uint
{
    Heavy = 240, // Helper->player, extra=0x63
    Electrocution = 3779, // Helper->player, extra=0x0
}

class SlimySummon(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.SlimySummon), "Prepare to kill add ASAP");
class GoldorFireIII(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.GoldorFireIII2), 8);
class GoldorFireIII2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.GoldorFireIII3), 8);
class GoldorBlast(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GoldorBlast), new AOEShapeRect(60, 5));
class Rupture(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.Rupture), "Kill slime ASAP! (The Ram's Voice + Ultravibration)", true);

class GoldorQuake(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10), new AOEShapeDonut(10, 20), new AOEShapeDonut(20, 30)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.GoldorQuake1)
            AddSequence(Module.PrimaryActor.Position, spell.NPCFinishAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Sequences.Count > 0)
        {
            var order = (AID)spell.Action.ID switch
            {
                AID.GoldorQuake1 => 0,
                AID.GoldorQuake2 => 1,
                AID.GoldorQuake3 => 2,
                _ => -1
            };
            AdvanceSequence(order, Module.PrimaryActor.Position, WorldState.FutureTime(1.5f));
        }
    }
}

class GoldorAeroIII(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.GoldorAeroIII), 10)
{
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<Burn>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || !Module.Bounds.Contains(pos);
}

class GoldorAeroIIIRaidwide(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.GoldorAeroIII));
class Burn(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Burn), new AOEShapeCircle(10));
class GoldorGravity(BossModule module) : Components.RaidwideCastDelay(module, ActionID.MakeSpell(AID.GoldorGravity), ActionID.MakeSpell(AID.GoldorGravity2), 0.8f, "Dmg + Heavy debuff");
class GoldorThunderIII(BossModule module) : Components.RaidwideCastDelay(module, ActionID.MakeSpell(AID.GoldorThunderIIIVisual), ActionID.MakeSpell(AID.GoldorThunderIII1), 0.8f, "Prepare to cleanse Electrocution");
class GoldorThunderIII2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.GoldorThunderIII2), 6);
class GoldorBlizzardIII(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.GoldorBlizzardIIIVisual));

class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var electrocution = actor.FindStatus(SID.Electrocution);
        if (electrocution != null)
            hints.Add($"Cleanse Electrocution!");
        var heavy = actor.FindStatus(SID.Heavy);
        if (heavy != null)
            hints.Add("Use Loom to dodge AOEs!");
        var fireballs = Module.Enemies(OID.BallOfFire).Where(x => !x.IsDead).FirstOrDefault();
        if (fireballs != null)
            hints.Add("Destroy at least one fireball to create a safe spot!");
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"For this fight The Ram's Voice, Ultravibration, Diamondback,\nExuviation, Flying Sardine, Loom, a physical dmg ability and a healing\nability (preferably Pom Cure with healer mimicry) are mandatory.");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        hints.Add("Requirements for achievement: Don't destroy the crystal in act 2,\nuse no sprint, use all 6 magic elements, take no optional damage.", false);
    }
}

class Stage32Act1States : StateMachineBuilder
{
    public Stage32Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SlimySummon>()
            .ActivateOnEnter<GoldorFireIII>()
            .ActivateOnEnter<GoldorFireIII2>()
            .ActivateOnEnter<GoldorBlast>()
            .ActivateOnEnter<Rupture>()
            .ActivateOnEnter<GoldorQuake>()
            .ActivateOnEnter<GoldorAeroIII>()
            .ActivateOnEnter<GoldorAeroIIIRaidwide>()
            .ActivateOnEnter<Burn>()
            .ActivateOnEnter<GoldorGravity>()
            .ActivateOnEnter<GoldorThunderIII>()
            .ActivateOnEnter<GoldorThunderIII2>()
            .ActivateOnEnter<GoldorBlizzardIII>()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 948, NameID = 12471, SortOrder = 1)]
public class Stage32Act1 : BossModule
{
    public Stage32Act1(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 16))
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.BallOfFire))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.GlitteringSlime))
            Arena.Actor(s, ArenaColor.Object);
    }
}
