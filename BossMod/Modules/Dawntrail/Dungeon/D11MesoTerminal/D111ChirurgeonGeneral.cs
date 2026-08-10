namespace BossMod.Dawntrail.Dungeon.D11MesoTerminal.D111ChirurgeonGeneral;

public enum OID : uint
{
    ChirurgeonGeneral = 0x488F, // R2.72
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 44246, // ChirurgeonGeneral->player, no cast, single-target

    MedicineField = 43798, // ChirurgeonGeneral->self, 5.0s cast, range 60 circle
    NoMansLand = 43804, // ChirurgeonGeneral->self, no cast, single-target
    PungentAerosol = 43807, // Helper->location, 5.5s cast, range 60 circle, knockback 24, away from source
    SterileSphereSmall = 43806, // Helper->self, 5.5s cast, range 8 circle
    SterileSphereBig = 43805, // Helper->self, 5.5s cast, range 15 circle
    BiochemicalFront = 43802, // ChirurgeonGeneral->self, 5.0s cast, range 40 width 65 rect
    SensoryDeprivation = 43797, // ChirurgeonGeneral->self, 3.0s cast, range 60 circle
    ConcentratedDose = 43799 // ChirurgeonGeneral->player, 5.0s cast, single-target
}

public enum SID : uint
{
    Poison = 2104 // ChirurgeonGeneral->player, extra=0x0
}

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.MedicineField && Arena.Bounds.Radius > 20f)
        {
            var center = Arena.Center;
            var shape = new AOEShapeCustom(center, [new Square(center, 21.5f)], [new Square(center, 20f)]);
            _aoe = [new(shape, center, default, Module.CastFinishAt(spell, 0.8d), shapeDistance: shape.Distance(center, default))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x04 && state == 0x00020001u)
        {
            Arena.Bounds = new ArenaBoundsSquare(20f);
            _aoe = [];
        }
    }
}

sealed class MedicineFieldPungentAerosol(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.MedicineField, (uint)AID.PungentAerosol]);
sealed class ConcentratedDose(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ConcentratedDose);
sealed class Poison(BossModule module) : Components.CleansableDebuff(module, (uint)SID.Poison, "Poison", "poisoned");
sealed class BiochemicalFront(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BiochemicalFront, new AOEShapeRect(40f, 32.5f));

sealed class SterileSphere(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [with(4)];
    private readonly AOEShapeCircle circleSmall = new(8f), circleBig = new(15f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnMapEffect(byte index, uint state)
    {
        if (state != 0x00020001u)
        {
            return;
        }
        var shape = index switch
        {
            0x0B or 0x0C or 0x0D or 0x0E => circleBig,
            0x0F or 0x10 or 0x11 or 0x12 => circleSmall,
            _ => null
        };
        if (shape == null)
        {
            return;
        }
        WPos pos = index switch
        {
            0x0B or 0x0F => new(260f, 2f),
            0x0C or 0x10 => new(280f, 2f),
            0x0D or 0x11 => new(260f, 22f),
            _ => new(280f, 22f), // 0x0E or 0x12
        };
        AOEs.Add(new(shape, pos.Quantized(), risky: false));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SterileSphereSmall)
        {
            var aoes = CollectionsMarshal.AsSpan(AOEs);
            var act = Module.CastFinishAt(spell);
            for (var i = 0; i < 4; ++i)
            {
                ref var aoe = ref aoes[i];
                aoe.Activation = act;
                aoe.Risky = true;
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SterileSphereSmall)
        {
            AOEs.Clear();
        }
    }
}

sealed class PungentAerosol(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.PungentAerosol, 24f)
{
    private readonly SterileSphere _aoe = module.FindComponent<SterileSphere>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
        {
            return;
        }
        ref readonly var c = ref Casters.Ref(0);
        var act = c.Activation;
        if (!IsImmune(slot, act))
        {
            // square intentionally slightly smaller to prevent sus knockback
            hints.AddForbiddenZone(new SDKnockbackInAABBSquareAwayFromOrigin(Arena.Center, c.Origin, 24f, 19f), act);
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.ActiveAOEs(slot, actor);
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

sealed class D111ChirurgeonGeneralStates : StateMachineBuilder
{
    public D111ChirurgeonGeneralStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<MedicineFieldPungentAerosol>()
            .ActivateOnEnter<ConcentratedDose>()
            .ActivateOnEnter<Poison>()
            .ActivateOnEnter<BiochemicalFront>()
            .ActivateOnEnter<SterileSphere>()
            .ActivateOnEnter<PungentAerosol>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.ChirurgeonGeneral, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1028u, NameID = 13970u, Category = BossModuleInfo.Category.Dungeon, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 1)]
public sealed class D111ChirurgeonGeneral(WorldState ws, Actor primary) : BossModule(ws, primary, new(270f, 12f), new ArenaBoundsSquare(21.5f));
