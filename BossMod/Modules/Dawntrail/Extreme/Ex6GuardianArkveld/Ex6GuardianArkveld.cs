namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

class Roar1(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_Roar);
class Roar2(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_Roar1);
class Roar3(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_Roar2);

class ChainbladeBlow(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_ChainbladeBlow1, AID._Weaponskill_ChainbladeBlow2, AID._Weaponskill_ChainbladeBlow7, AID._Weaponskill_ChainbladeBlow8, AID._Weaponskill_ChainbladeBlow13, AID._Weaponskill_ChainbladeBlow14, AID._Weaponskill_ChainbladeBlow19, AID._Weaponskill_ChainbladeBlow20], new AOEShapeRect(40, 2));

class ChainbladeRadiance(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_WyvernsRadiance, AID._Weaponskill_WyvernsRadiance7, AID._Weaponskill_WyvernsRadiance19, AID._Weaponskill_WyvernsRadiance23], new AOEShapeRect(80, 14));

class ChainbladeRepeat(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AID[] TailsCast = [AID._Weaponskill_ChainbladeBlow1, AID._Weaponskill_ChainbladeBlow2, AID._Weaponskill_ChainbladeBlow7, AID._Weaponskill_ChainbladeBlow8, AID._Weaponskill_ChainbladeBlow13, AID._Weaponskill_ChainbladeBlow14, AID._Weaponskill_ChainbladeBlow19, AID._Weaponskill_ChainbladeBlow20];
    private static readonly AID[] BossCast = [AID._Weaponskill_WyvernsRadiance, AID._Weaponskill_WyvernsRadiance7, AID._Weaponskill_WyvernsRadiance19, AID._Weaponskill_WyvernsRadiance23];

    private static readonly AID[] TailsFast = [AID._Weaponskill_ChainbladeBlow4, AID._Weaponskill_ChainbladeBlow5, AID._Weaponskill_ChainbladeBlow10, AID._Weaponskill_ChainbladeBlow11, AID._Weaponskill_ChainbladeBlow16, AID._Weaponskill_ChainbladeBlow17, AID._Weaponskill_ChainbladeBlow22, AID._Weaponskill_ChainbladeBlow23];
    private static readonly AID[] BossFast = [AID._Weaponskill_WyvernsRadiance1, AID._Weaponskill_WyvernsRadiance8, AID._Weaponskill_WyvernsRadiance21, AID._Weaponskill_WyvernsRadiance24];

    private static readonly AOEShape TailShape = new AOEShapeRect(40, 2);
    private static readonly AOEShape CleaveShape = new AOEShapeRect(80, 14);

    public bool Draw;

    private readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Draw ? _predicted : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = (AID)spell.Action.ID;
        var shape = TailsCast.Contains(id)
            ? TailShape
            : BossCast.Contains(id)
                ? CleaveShape
                : null;

        if (shape == null)
            return;

        var n = Module.PrimaryActor.Rotation.ToDirection().OrthoR();
        var d0 = spell.Rotation.ToDirection();
        var angle = d0 - d0.Dot(n) * n * 2;

        var d1 = spell.LocXZ - Module.PrimaryActor.Position;
        var src = d1 - d1.Dot(n) * n * 2;

        _predicted.Add(new(shape, Module.PrimaryActor.Position + src, angle.ToAngle(), Module.CastFinishAt(spell, 4)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = (AID)spell.Action.ID;

        if (BossCast.Contains(id))
            Draw = true;

        if (TailsFast.Contains(id))
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);

        if (BossFast.Contains(id))
        {
            NumCasts++;
            Draw = false;
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
        }
    }
}

class WhiteFlash(BossModule module) : Components.StackWithCastTargets(module, AID._Weaponskill_WhiteFlash, 6, maxStackSize: 4);
class Dragonspark(BossModule module) : Components.StackWithCastTargets(module, AID._Weaponskill_Dragonspark, 6, maxStackSize: 4);

class WhiteFlashDragonspark(BossModule module) : Components.CastCounterMulti(module, [AID._Weaponskill_WhiteFlash, AID._Weaponskill_Dragonspark]);

class BossSiegeflight(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_GuardianSiegeflight, AID._Weaponskill_GuardianSiegeflight2, AID._Weaponskill_WyvernsSiegeflight, AID._Weaponskill_WyvernsSiegeflight2], new AOEShapeRect(40, 2));
class HelperSiegeflight(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_GuardianSiegeflight1, AID._Weaponskill_GuardianSiegeflight3, AID._Weaponskill_WyvernsSiegeflight1, AID._Weaponskill_WyvernsSiegeflight3], new AOEShapeRect(40, 4));

class GuardianResonance(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_GuardianResonance, AID._Weaponskill_GuardianResonance4], new AOEShapeRect(40, 8));

class WyvernsRadiancePuddle(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_WyvernsRadiance3, 6);
class WyvernsRadianceExaflare(BossModule module) : Components.Exaflare(module, new AOEShapeRect(8, 20))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_WyvernsRadiance2)
        {
            Lines.Add(new()
            {
                Next = spell.LocXZ,
                Advance = caster.Rotation.ToDirection() * 8,
                Rotation = caster.Rotation,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 2.5f,
                ExplosionsLeft = 5,
                MaxShownExplosions = 2
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_WyvernsRadiance2 or AID._Weaponskill_WyvernsRadiance4 && Lines.Count > 0)
        {
            AdvanceLine(Lines[0], caster.Position);
            Lines.RemoveAll(l => l.ExplosionsLeft <= 0);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (c, t, r) in ImminentAOEs())
            hints.AddForbiddenZone(Shape, c, r, t);
    }
}

class WyvernsRadianceSides(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_WyvernsRadiance5, AID._Weaponskill_WyvernsRadiance6, AID._Weaponskill_WyvernsRadiance25, AID._Weaponskill_WyvernsRadiance26], new AOEShapeRect(40, 9));

class WyvernsRadianceQuake(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(8), new AOEShapeDonut(8, 14), new AOEShapeDonut(14, 20), new AOEShapeDonut(20, 26)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_WyvernsRadiance9)
            AddSequence(caster.Position, Module.CastFinishAt(spell), default);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID._Weaponskill_WyvernsRadiance9 => 0,
            AID._Weaponskill_WyvernsRadiance10 => 1,
            AID._Weaponskill_WyvernsRadiance11 => 2,
            AID._Weaponskill_WyvernsRadiance12 => 3,
            _ => -1
        };
        if (order >= 0)
            AdvanceSequence(order, caster.Position, WorldState.FutureTime(1.95f));
    }
}

class Rush(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_Rush)
{
    private readonly List<(WPos From, WPos To, DateTime Activation)> _charges = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in _charges)
            yield return new(new AOEShapeRect((c.To - c.From).Length(), 6), c.From, (c.To - c.From).ToAngle(), c.Activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_3)
        {
            _charges.Add((caster.Position, spell.LocXZ, Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_Rush or AID._Weaponskill_Rush1)
            _charges.RemoveAll(c => c.To.AlmostEqual(spell.TargetXZ, 1));
    }
}

class WyvernsOuroblade(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_WyvernsOuroblade1, AID._Weaponskill_WyvernsOuroblade3, AID._Weaponskill_WyvernsOuroblade5, AID._Weaponskill_WyvernsOuroblade7], new AOEShapeCone(40, 90.Degrees()));

class WildEnergy(BossModule module) : Components.SpreadFromCastTargets(module, AID._Weaponskill_WildEnergy, 6);

class SteeltailThrust(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_SteeltailThrust1, AID._Weaponskill_SteeltailThrust3], new AOEShapeRect(60, 3));

class ChainbladeCharge(BossModule module) : Components.StackWithIcon(module, (uint)IconID._Gen_Icon_com_share2i, AID._Weaponskill_ChainbladeCharge2, 6, 8.3f, minStackSize: 5);

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1044, NameID = 14237, DevOnly = true)]
public class Ex6GuardianArkveld(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
