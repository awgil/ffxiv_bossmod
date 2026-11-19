#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Stormblood.Alliance.A33ThunderGod;

public enum OID : uint
{
    Boss = 0x25D7, // R19.880, x1
    Helper = 0x233C, // R0.500, x39, mixed types
    _Gen_EphemeralKnight = 0x25D8, // R7.100, x3
    _Gen_ = 0x261E, // R14.000, x3
}

public enum AID : uint
{
    _Weaponskill_Attack = 14151, // Boss->self, no cast, single-target
    _Weaponskill_Attack1 = 14152, // Helper->player, no cast, single-target
    _Weaponskill_CleansingStrike = 14161, // Boss->self, 3.0s cast, range 40 circle
    _Weaponskill_TGHolySword = 14153, // Boss->self, 6.0s cast, single-target
    _Weaponskill_HallowedBolt = 14155, // Helper->self, no cast, range 15 circle
    _Weaponskill_TGHolySword1 = 14160, // Boss->self, 6.0s cast, single-target
    _Weaponskill_JudgmentBlade = 15060, // Helper->self, no cast, range ?-35 donut
    _Weaponskill_Shadowblade = 14173, // Boss->self, 8.0s cast, single-target
    _Weaponskill_Shadowblade1 = 14174, // Helper->players, no cast, range 5 circle
    _Weaponskill_Shadowblade2 = 14175, // Helper->self, no cast, range 1 circle
    _Weaponskill_CrushHelm = 14163, // Helper->player, no cast, single-target
    _Weaponskill_CrushHelm1 = 14162, // Boss->self, 6.0s cast, single-target
    _Weaponskill_CrushHelm2 = 14164, // Helper->player, no cast, single-target
    _Weaponskill_Shadowblade3 = 14176, // Helper->self, no cast, range 70 circle
    _Weaponskill_TGHolySword2 = 14154, // Boss->self, 6.0s cast, single-target
    _Weaponskill_TGHolySword3 = 14159, // Boss->self, 6.0s cast, single-target
    _Weaponskill_NorthswainsStrike = 15059, // Helper->self, no cast, range 26 circle
    _Weaponskill_Duskblade = 14177, // Boss->self, 10.0s cast, range 70 circle
    _Weaponskill_CrushWeapon = 14165, // Boss->self, 4.0s cast, single-target
    _Weaponskill_CrushWeapon1 = 14166, // Helper->self, 4.0s cast, range 6 circle
    _Weaponskill_CrushWeapon2 = 14167, // Helper->self, no cast, range 6 circle
    _Weaponskill_Colosseum = 14178, // Boss->self, no cast, range 40 circle
    _AutoAttack_Attack = 870, // 25D8->player, no cast, single-target
    _Weaponskill_HallowedBolt1 = 14181, // 25D8->self, 5.0s cast, single-target
    _Weaponskill_HallowedBolt2 = 14182, // Helper->self, 5.0s cast, range 15 circle
    _Weaponskill_HallowedBolt3 = 14183, // Helper->self, 5.0s cast, range 15-30 donut
    _Weaponskill_HallowedBolt4 = 14184, // Helper->players, no cast, range 6 circle
    _Weaponskill_ = 14059, // Helper->self, no cast, single-target
    _Weaponskill_1 = 14372, // Helper->self, no cast, single-target
    _Weaponskill_BalanceAsunder = 14186, // Boss->self, no cast, single-target
    _Weaponskill_BalanceAsunder1 = 14187, // Helper->self, no cast, range 40 circle
    _Weaponskill_TGHolySword4 = 14156, // Boss->self, 8.0s cast, single-target
    _Weaponskill_DivineRuination = 14158, // Helper->self, no cast, range 16 circle
    _Weaponskill_CrushArmor = 14168, // Boss->self, 5.0s cast, single-target
    _Weaponskill_CrushArmor1 = 14169, // Helper->player, no cast, single-target
}

public enum SID : uint
{
    _Gen_Doom = 1769, // Boss->player, extra=0x0
    _Gen_VulnerabilityUp = 202, // Helper->player, extra=0x1
    _Gen_PhysicalVulnerabilityUp = 200, // Helper->player, extra=0x1/0x2/0x3/0x4
    _Gen_Bleeding = 643, // Helper->player, extra=0x2
    _Gen_Weakness = 43, // none->player, extra=0x0
    _Gen_Transcendent = 418, // none->player, extra=0x0
    _Gen_Electrocution = 271, // Helper->player, extra=0x0
    _Gen_MagicVulnerabilityUp = 1138, // Helper->player, extra=0x0
    _Gen_PhysicalVulnerabilityUp1 = 695, // Helper->player, extra=0x0
}

public enum IconID : uint
{
    _Gen_Icon_m0537teg_a0h = 170, // player->self
    _Gen_Icon_tracking_lockon01i = 197, // player->self
    _Gen_Icon_lockon5_t0h = 23, // player->self
    _Gen_Icon_com_share0c = 62, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_teke01h = 84, // Helper->player
}

class HB2(BossModule module) : Components.DebugCasts(module, [AID._Weaponskill_HallowedBolt], new AOEShapeCircle(15));

class TGHallowedBolt(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_HallowedBolt)
{
    private readonly List<AOEInstance> _predicted = [];
    public static readonly AOEShape Shape = new AOEShapeCircle(15);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_TGHolySword:
                _predicted.Add(new(Shape, spell.LocXZ + new WDir(12.5f, 21.7f).Rotate(spell.Rotation), default, Module.CastFinishAt(spell, 1)));
                _predicted.Add(new(Shape, spell.LocXZ + new WDir(-25, 0).Rotate(spell.Rotation), default, Module.CastFinishAt(spell, 1.6f)));
                _predicted.Add(new(Shape, spell.LocXZ + new WDir(12.5f, -21.7f).Rotate(spell.Rotation), default, Module.CastFinishAt(spell, 2.1f)));
                break;
            case AID._Weaponskill_TGHolySword2:
                _predicted.Add(new(Shape, spell.LocXZ + new WDir(-12.5f, 21.7f).Rotate(spell.Rotation), default, Module.CastFinishAt(spell, 1)));
                _predicted.Add(new(Shape, spell.LocXZ + new WDir(25, 0).Rotate(spell.Rotation), default, Module.CastFinishAt(spell, 1.6f)));
                _predicted.Add(new(Shape, spell.LocXZ + new WDir(-12.5f, -21.7f).Rotate(spell.Rotation), default, Module.CastFinishAt(spell, 2.1f)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
            NumCasts++;
        }
    }
}
class TGJudgmentBlade(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_JudgmentBlade)
{
    private readonly List<AOEInstance> _predicted = [];
    public static readonly AOEShape Shape = new AOEShapeDonut(20, 35);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_TGHolySword1:
                _predicted.Add(new(Shape, spell.LocXZ, default, Module.CastFinishAt(spell, 0.3f)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
            NumCasts++;
        }
    }
}
class TGNorthswainsStrike(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_NorthswainsStrike)
{
    private readonly List<AOEInstance> _predicted = [];
    public static readonly AOEShape Shape = new AOEShapeCircle(26);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_TGHolySword3:
                _predicted.Add(new(Shape, spell.LocXZ, default, Module.CastFinishAt(spell, 0.3f)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
            NumCasts++;
        }
    }
}

class A33ThunderGodStates : StateMachineBuilder
{
    public A33ThunderGodStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HB2>()
            .ActivateOnEnter<TGHallowedBolt>()
            .ActivateOnEnter<TGJudgmentBlade>()
            .ActivateOnEnter<TGNorthswainsStrike>();
    }
}

// growing ball radius = 12
/*
[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7899)]
public class A33ThunderGod(ModuleInit init) : BossModule(init, new(-600, -600), DefaultBounds)
{
    public static readonly ArenaBoundsCustom DefaultBounds = MakeBounds();

    static ArenaBoundsCustom MakeBounds()
    {
        var platform = CurveApprox.Circle(9.6f, 0.05f);
        var starting = 90;
        var clipper = new PolygonClipper();
        var platforms = new RelSimplifiedComplexPolygon();
        for (var i = 0; i < 6; i++)
            platforms.Parts.Add(new([.. platform.Select(p => p + (starting + 60 * i).Degrees().ToDirection() * 25)]));

        // please avert your eyes
        List<WPos> bridgeAbs = [
            new(-600, -579.7298f),
            new(-598.51733f, -579.9248f), new(-597.07306f, -581.2062f),
            new(-596, -573.1124f), new(-597.7342f, -573.97614f),
            new(-600, -573.67786f),
            new(-602.2658f, -573.97614f), new(-604, -573.1124f),
            new(-602.92694f, -581.2062f), new(-601.48267f, -579.9248f)
        ];

        var b23 = bridgeAbs[2] - bridgeAbs[1];
        bridgeAbs[2] = bridgeAbs[1] + b23 * 2;
        var b43 = bridgeAbs[3] - bridgeAbs[4];
        bridgeAbs[3] = bridgeAbs[4] + b43 * 2;
        var b67 = bridgeAbs[7] - bridgeAbs[6];
        bridgeAbs[7] = bridgeAbs[6] + b67 * 2;
        var b98 = bridgeAbs[8] - bridgeAbs[9];
        bridgeAbs[8] = bridgeAbs[9] + b98 * 2;

        bridgeAbs.Reverse();

        var bridgeS = ShrinkPoly.Shrink(bridgeAbs.Select(s => s - new WPos(-600, -600)), 0.4f);

        for (var i = 0; i < 6; i++)
            platforms = clipper.Union(new(platforms), new(bridgeS.Select(p => p.Rotate((60 * i).Degrees()))));

        return new(34.5f, platforms);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        float? angle = Raid.Members[slot].Alliance switch
        {
            AllianceLetter.A => -120,
            AllianceLetter.B => 0,
            AllianceLetter.C => 120,
            _ => null
        };
        if (angle.HasValue)
            hints.GoalZones.Add(AIHints.GoalSingleTarget(Arena.Center + angle.Value.Degrees().ToDirection() * 22, 22, 0.5f));

        base.CalculateModuleAIHints(slot, actor, assignment, hints);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.ActorInsideBounds(PrimaryActor.Position, PrimaryActor.Rotation, ArenaColor.Enemy);
    }
}
*/
