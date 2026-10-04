#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Global.Crucible.GolemPiece;

public enum OID : uint
{
    Boss = 0x4CCB, // R4.000, x1
    DeadGolemPiece = 0x4CCC, // R4.000, x1
    GolemPieceHeart = 0x4D5A, // R2.200, x0 (spawn during fight), Part type
    Helper = 0x233C, // R0.500, x28, Helper type
}

public enum AID : uint
{
    _Spell_Stone = 50930, // Boss/4CCC->player, no cast, single-target
    _Weaponskill_Plaincracker = 48754, // Boss->self, 6.0+1.0s cast, single-target
    _Weaponskill_Plaincracker1 = 48755, // Helper->self, 7.0s cast, range 20 circle
    _Weaponskill_RockWall = 48748, // Boss->self, 3.0+1.0s cast, single-target
    _Weaponskill_Shockwave = 48750, // Helper->self, 4.0s cast, range 5 width 5 rect
    _Weaponskill_Rockslide = 48751, // Helper->self, no cast, range 30 width 10 rect
    _Weaponskill_Plaincracker2 = 48760, // Boss->self, 6.0+1.0s cast, single-target
    _Ability_ = 48761, // Helper->4CCC/Boss, no cast, single-target
    _Ability_1 = 50687, // 4CCC/Boss->self, no cast, single-target
    _Weaponskill_Plaincracker3 = 48764, // 4CCC->self, no cast, single-target
    _Weaponskill_Plaincracker4 = 48765, // Helper->self, 1.0s cast, range 20 circle
    _Weaponskill_Stoneshower = 48756, // 4CCC->self, 3.0s cast, single-target
    _Weaponskill_Stoneshower1 = 48758, // Helper->self, 1.0s cast, range 3-11 donut
    _Weaponskill_Stoneshower2 = 48757, // Helper->self, 1.0s cast, range 8 circle
    _Weaponskill_Outcrop = 48767, // 4CCC->self, 4.2+0.8s cast, single-target
    _Weaponskill_Outcrop1 = 48768, // Helper->self, 5.0s cast, range 40 60-degree cone
    _Weaponskill_Obliterate = 50649, // 4CCC->self, 5.0s cast, range 60 circle
    _Weaponskill_EarthenRing = 48752, // 4CCC->self, 6.2+0.8s cast, single-target
    _Weaponskill_EarthenRing1 = 48753, // Helper->self, 7.0s cast, range 5-50 donut
    _Weaponskill_SelfDestruct = 48766, // 4D5A->self, 20.0s cast, range 100 circle
}

public enum TetherID : uint
{
    _Gen_Tether_chn_m0021_chest1_j2 = 431, // Boss->4CCC
}

class PlaincrackerSolo(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Plaincracker1, 20);
class EarthenRingSolo(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_EarthenRing1, new AOEShapeDonut(5, 50));
class Shockwave(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Shockwave, new AOEShapeRect(5, 2.5f));

class RockWall(BossModule module) : Components.GenericAOEs(module)
{
    BitMask _blocks;
    int _direction; // 1 = out, -1 = in

    static readonly WDir[] Blocks = [
        new(-7.5f, -12.5f),
        new(7.5f, -12.5f),
        new(-7.5f, -7.5f),
        new(7.5f, -7.5f),
        new(-7.5f, -2.5f),
        new(7.5f, -2.5f),
        new(-7.5f, 2.5f),
        new(7.5f, 2.5f),
        new(-7.5f, 7.5f),
        new(7.5f, 7.5f),
        new(-7.5f, 12.5f),
        new(7.5f, 12.5f),
    ];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        switch (_direction)
        {
            case -1:
                yield return new(new AOEShapeRect(15, 5, 15), Arena.Center);
                break;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);

        foreach (var bit in _blocks.SetBits())
            Arena.ZoneRect(Arena.Center + Blocks[bit], default(Angle), 2.5f, 2.5f, 2.5f, ArenaColor.AOE);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 0x11 and <= 0x1C)
        {
            var bit = index - 0x11;
            switch (state)
            {
                case 0x00020001:
                    _blocks.Set(bit);
                    _direction = -1;
                    break;
                case 0x00080004:
                    _blocks.Clear(bit);
                    if (!_blocks.Any())
                        _direction = 0;
                    break;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        foreach (var bit in _blocks.SetBits())
            hints.TemporaryObstacles.Add(ShapeDistance.Rect(Arena.Center + Blocks[bit], default(Angle), 3, 3, 3));
    }
}

class Outcrop(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Outcrop, new AOEShapeCone(40, 30.Degrees()));

// Rockslide triggers 9 seconds after first Shockwave cast event
class GolemPieceStates : StateMachineBuilder
{
    public GolemPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PlaincrackerSolo>()
            .ActivateOnEnter<EarthenRingSolo>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<RockWall>()
            .ActivateOnEnter<Outcrop>()
            .Raw.Update = () => ((GolemPiece)module).Heart is { IsDeadOrDestroyed: true };
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1091, NameID = 14617)]
public class GolemPiece(ModuleInit init) : BossModule(init, new(520, 0), new ArenaBoundsRect(20, 15))
{
    public Actor? OtherGolem;
    public Actor? Heart;

    protected override void UpdateModule()
    {
        OtherGolem ??= Enemies(OID.DeadGolemPiece).FirstOrDefault();
        Heart ??= Enemies(OID.GolemPieceHeart).FirstOrDefault();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);

        Arena.Actors(Enemies(OID.DeadGolemPiece), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.GolemPieceHeart), ArenaColor.Enemy);
    }
}
