namespace BossMod.Global.MaskedCarnivale.Stage32.Act2;

public enum OID : uint
{
    Boss = 0x3FA7, //R=2.5
    GildedCrystal = 0x3FAC, //R=3.0
    GildedGolem = 0x3FA9, //R=2.1
    GildedMarionette = 0x3FA8, //R=1.2
    GildedCyclops = 0x3FAA, //R=3.2
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 34444, // Boss->player, no cast, single-target

    ShiningSummon = 34461, // Boss->self, 4.0s cast, single-target
    Teleport = 34129, // Helper->location, no cast, single-target
    GoldorAeroIII = 34460, // Boss->self, 4.0s cast, range 50 circle, raidwide, knockback 10 away from source
    GoldenCross = 34465, // GildedGolem->self, 7.0s cast, range 100 width 7 cross
    GoldorQuakeVisual = 34456, // Boss->self, 3.0s cast, single-target
    GoldorQuake1 = 34457, // Helper->self, 4.0s cast, range 10 circle
    GoldorQuake2 = 34458, // Helper->self, 5.5s cast, range 10-20 donut
    GoldorQuake3 = 34459, // Helper->self, 7.0s cast, range 20-30 donut
    GoldenBeam = 34464, // GildedMarionette->self, 7.0s cast, range 40 120-degree cone
    GoldorThunderIIIVisual = 34453, // Boss->self, 4.0s cast, single-target
    GoldorThunderIII1 = 34454, // Helper->player, no cast, range 5 circle, applies cleansable electrocution
    GoldorThunderIII2 = 34455, // Helper->location, 2.5s cast, range 6 circle
    TwentyFourCaratInhale = 34466, // GildedCyclops->self, 2.0s cast, range 50 circle, pull 30 between centers
    TwentyFourCaratSwing = 34467, // GildedCyclops->self, 4.0s cast, range 12 circle
    GoldFever = 34469, // Boss->self, 8.0s cast, single-target, Goldor draws power from crystal
    GoldorBlast = 34450, // Boss->self, 3.5s cast, range 60 width 10 rect
    GoldorGravity = 34451, // Boss->self, 5.0s cast, single-target
    GoldorGravity2 = 34452, // Helper->player, no cast, single-target, heavy damage + heavy
    GoldorRush = 34468, // Boss->self, 4.0s cast, range 50 circle, knockback 10 away from source, raidwide
    GoldorFireIII = 34449, // Helper->location, 2.5s cast, range 8 circle
    GoldorBlizzardIIIVisual = 34589, // Boss->self, 6.0s cast, single-target, interruptible, freezes player
    GoldorBlizzardIII = 34590 // Helper->player, no cast, range 6 circle
}

public enum SID : uint
{
    Heavy = 240, // Helper->player, extra=0x63
    Electrocution = 3779, // Helper->player, extra=0x0
    MagicDamageUp = 3707, // none->Boss, extra=0x0
    MagicResistance = 3621 // none->Boss, extra=0x0
}

sealed class GoldorFireIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GoldorFireIII, 8f);
sealed class GoldorBlast(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GoldorBlast, new AOEShapeRect(60f, 5f));
sealed class GoldenCross(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GoldenCross, new AOEShapeCross(100f, 3.5f));

sealed class GoldenBeam(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeCone cone = new(40f, 60f.Degrees());
    private AOEInstance[] _aoe = [];
    private readonly List<ConeV> cones = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GoldenBeam)
        {
            if (Module.Enemies((uint)OID.GildedMarionette).Count == 1)
            {
                var pos = spell.LocXZ;
                var rot = spell.Rotation;
                _aoe = [new(cone, pos, rot, Module.CastFinishAt(spell), shapeDistance: cone.Distance(pos, rot))];
            }
            else
            {
                cones.Add(new(spell.LocXZ, cone.Radius, spell.Rotation, cone.HalfAngle, 2));
                if (cones.Count == 4)
                {
                    var coneskip1 = new List<ConeV>(3);
                    for (var i = 1; i < 4; ++i)
                    {
                        coneskip1.Add(cones[i]);
                    }
                    ConeV[] conesA = [.. coneskip1];
                    ConeV[] cone0 = [cones[0]];
                    var center = Arena.Center;
                    var poly1 = PolygonClipper.GetCombinedPolygon(center, cone0, shapes2: conesA, operandType: OperandType.Xor);
                    var poly2 = PolygonClipper.GetCombinedPolygon(center, cone0, shapes2: conesA, operandType: OperandType.Intersection);
                    var clipper = new PolygonClipper();
                    var combinedShapes = clipper.Union(new PolygonClipper.Operand(poly2), new PolygonClipper.Operand(poly1));
                    var shape = new AOEShapeCustom(center, [], skipPolygonInit: true);
                    shape.ReplacePolygon(combinedShapes, center);
                    _aoe = [new(shape, center, default, Module.CastFinishAt(spell), shapeDistance: shape.Distance(center, default))];
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GoldenBeam)
        {
            _aoe = [];
            cones.Clear();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_aoe.Length != 0 && Module.Enemies((uint)OID.GildedMarionette).Count > 1)
        {
            hints.Add("Use Diamondback outside of marked area!");
        }
    }
}

sealed class TwentyFourCaratSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TwentyFourCaratSwing, 12f);

sealed class GoldorQuake(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 20f), new AOEShapeDonut(20f, 30f)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GoldorQuake1)
        {
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.GoldorQuake1 => 0,
                (uint)AID.GoldorQuake2 => 1,
                (uint)AID.GoldorQuake3 => 2,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(1.5d));
        }
    }
}

sealed class GoldorAeroIII(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.GoldorAeroIII, 10f)
{
    private readonly GoldenCross _aoe = module.FindComponent<GoldenCross>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = CollectionsMarshal.AsSpan(_aoe.Casters);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            if (aoes[i].Check(pos))
            {
                return true;
            }
        }
        return !Arena.InBounds(pos);
    }
}

sealed class GoldorAeroIIIGoldorRushRaidwide(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.GoldorAeroIII, (uint)AID.GoldorRush]);
sealed class GoldorRush(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.GoldorRush, 10f);
sealed class TwentyFourCaratInhale(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.TwentyFourCaratInhale, 30f, kind: Kind.TowardsOrigin);
sealed class GoldorGravity(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.GoldorGravity, (uint)AID.GoldorGravity2, 0.8d, "Dmg + Heavy debuff");
sealed class GoldorThunderIII(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.GoldorThunderIIIVisual, (uint)AID.GoldorThunderIII1, 0.8d, "Prepare to cleanse Electrocution");
sealed class GoldorThunderIII2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GoldorThunderIII2, 6f);
sealed class GoldorBlizzardIII(BossModule module) : Components.CastInterruptHint(module, (uint)AID.GoldorBlizzardIIIVisual);

sealed class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Module.PrimaryActor.FindStatus((uint)SID.MagicResistance) != null)
        {
            hints.Add($"{Module.PrimaryActor.Name} is immune to magic damage! (Destroy crystal to remove buff)");
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.FindStatus((uint)SID.Electrocution) != null)
        {
            hints.Add($"Cleanse Electrocution!");
        }
        if (actor.FindStatus((uint)SID.Heavy) != null)
        {
            hints.Add("Use Loom to dodge AOEs!");
        }
    }
}

sealed class Stage32Act2States : StateMachineBuilder
{
    public Stage32Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GoldenCross>()
            .ActivateOnEnter<GoldorFireIII>()
            .ActivateOnEnter<GoldenBeam>()
            .ActivateOnEnter<GoldorBlast>()
            .ActivateOnEnter<TwentyFourCaratInhale>()
            .ActivateOnEnter<GoldorQuake>()
            .ActivateOnEnter<GoldorAeroIII>()
            .ActivateOnEnter<GoldorAeroIIIGoldorRushRaidwide>()
            .ActivateOnEnter<GoldorRush>()
            .ActivateOnEnter<TwentyFourCaratSwing>()
            .ActivateOnEnter<GoldorGravity>()
            .ActivateOnEnter<GoldorThunderIII>()
            .ActivateOnEnter<GoldorThunderIII2>()
            .ActivateOnEnter<GoldorBlizzardIII>()
            .ActivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 948u, NameID = 12471u, SortOrder = 2)]
public sealed class Stage32Act2(WorldState ws, Actor primary) : BossModule(ws, primary, Layouts.ArenaCenter, Layouts.CircleSmall);
