namespace BossMod.Global.MaskedCarnivale.Stage32.Act2;

public enum OID : uint
{
    Boss = 0x3FA7, //R=2.5
    GildedCrystal = 0x3FAC, //R=3.0
    GildedGolem = 0x3FA9, //R=2.1
    GildedMarionette = 0x3FA8, //R=1.2
    GildedCyclops = 0x3FAA, //R=3.2
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 34444, // Boss->player, no cast, single-target
    ShiningSummon = 34461, // Boss->self, 4,0s cast, single-target
    Teleport = 34129, // Helper->location, no cast, single-target
    GoldorAeroIII = 34460, // Boss->self, 4,0s cast, range 50 circle, raidwide, knockback 10 away from source
    GoldenCross = 34465, // GildedGolem->self, 7,0s cast, range 100 width 7 cross
    GoldorQuakeVisual = 34456, // Boss->self, 3,0s cast, single-target
    GoldorQuake1 = 34457, // Helper->self, 4,0s cast, range 10 circle
    GoldorQuake2 = 34458, // Helper->self, 5,5s cast, range 10-20 donut
    GoldorQuake3 = 34459, // Helper->self, 7,0s cast, range 20-30 donut
    GoldenBeam = 34464, // GildedMarionette->self, 7,0s cast, range 40 120-degree cone
    GoldorThunderIIIVisual = 34453, // Boss->self, 4,0s cast, single-target
    GoldorThunderIII1 = 34454, // Helper->player, no cast, range 5 circle, applies cleansable electrocution
    GoldorThunderIII2 = 34455, // Helper->location, 2,5s cast, range 6 circle
    TwentyFourCaratInhale = 34466, // GildedCyclops->self, 2,0s cast, range 50 circle, pull 30 between centers
    TwentyFourCaratSwing = 34467, // GildedCyclops->self, 4,0s cast, range 12 circle
    GoldFever = 34469, // Boss->self, 8,0s cast, single-target, Goldor draws power from crystal
    GoldorBlast = 34450, // Boss->self, 3,5s cast, range 60 width 10 rect
    GoldorGravity = 34451, // Boss->self, 5,0s cast, single-target
    GoldorGravity2 = 34452, // Helper->player, no cast, single-target, heavy damage + heavy
    GoldorRush = 34468, // Boss->self, 4,0s cast, range 50 circle, knockback 10 away from source, raidwide
    GoldorFireIII = 34449, // Helper->location, 2,5s cast, range 8 circle
    GoldorBlizzardIIIVisual = 34589, // Boss->self, 6,0s cast, single-target, interruptible, freezes player
    GoldorBlizzardIII = 34590, // Helper->player, no cast, range 6 circle
}

public enum SID : uint
{
    Heavy = 240, // Helper->player, extra=0x63
    Electrocution = 3779, // Helper->player, extra=0x0
    MagicDamageUp = 3707, // none->Boss, extra=0x0
    MagicResistance = 3621, // none->Boss, extra=0x0
}

class GoldorFireIII(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.GoldorFireIII), 8);
class GoldorBlast(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GoldorBlast), new AOEShapeRect(60, 5));
class GoldenCross(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GoldenCross), new AOEShapeCross(100, 3.5f));
class GoldenBeam(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GoldenBeam), new AOEShapeCone(40, 60.Degrees()));
class TwentyFourCaratSwing(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TwentyFourCaratSwing), new AOEShapeCircle(12));

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
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<GoldenCross>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || !Module.Bounds.Contains(pos);
}

class GoldorAeroIIIRaidwide(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.GoldorAeroIII));
class GoldorRush(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.GoldorRush), 10);
class GoldorRushRaidwide(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.GoldorRush));
class TwentyFourCaratInhale(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.TwentyFourCaratInhale), 30, kind: Kind.TowardsOrigin);
class GoldorGravity(BossModule module) : Components.RaidwideCastDelay(module, ActionID.MakeSpell(AID.GoldorGravity), ActionID.MakeSpell(AID.GoldorGravity2), 0.8f, "Dmg + Heavy debuff");
class GoldorThunderIII(BossModule module) : Components.RaidwideCastDelay(module, ActionID.MakeSpell(AID.GoldorThunderIIIVisual), ActionID.MakeSpell(AID.GoldorThunderIII1), 0.8f, "Prepare to cleanse Electrocution");
class GoldorThunderIII2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.GoldorThunderIII2), 6);
class GoldorBlizzardIII(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.GoldorBlizzardIIIVisual));

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        var magicabsorb = Module.PrimaryActor.FindStatus(SID.MagicResistance);
        if (magicabsorb != null)
            hints.Add($"{Module.PrimaryActor.Name} is immune to magic damage! (Destroy crystal to remove buff)");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var electrocution = actor.FindStatus(SID.Electrocution);
        if (electrocution != null)
            hints.Add($"Cleanse Electrocution!");
        var heavy = actor.FindStatus(SID.Heavy);
        if (heavy != null)
            hints.Add("Use Loom to dodge AOEs!");
        var marionettes = Module.Enemies(OID.GildedMarionette).Count > 1;
        if (marionettes)
            hints.Add("Use Diamondback behind + between 2 marionettes!");
    }
}

class Stage31Act2States : StateMachineBuilder
{
    public Stage31Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GoldenCross>()
            .ActivateOnEnter<GoldorFireIII>()
            .ActivateOnEnter<GoldenBeam>()
            .ActivateOnEnter<GoldorBlast>()
            .ActivateOnEnter<TwentyFourCaratInhale>()
            .ActivateOnEnter<GoldorQuake>()
            .ActivateOnEnter<GoldorAeroIII>()
            .ActivateOnEnter<GoldorAeroIIIRaidwide>()
            .ActivateOnEnter<GoldorRush>()
            .ActivateOnEnter<GoldorRushRaidwide>()
            .ActivateOnEnter<TwentyFourCaratSwing>()
            .ActivateOnEnter<GoldorGravity>()
            .ActivateOnEnter<GoldorThunderIII>()
            .ActivateOnEnter<GoldorThunderIII2>()
            .ActivateOnEnter<GoldorBlizzardIII>()
            .ActivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 948, NameID = 12471, SortOrder = 2)]
public class Stage31Act2(WorldState ws, Actor primary) : BossModule(ws, primary, new ArenaBoundsCircle(new(100, 100), 16));
