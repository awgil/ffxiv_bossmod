namespace BossMod.Endwalker.Alliance.A34Eulogia;

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeDonut transitionSmallerBounds = new(30f, 35f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x1B)
        {
            if (state == 0x00080004u)
            {
                Arena.Bounds = new ArenaBoundsCircle(35f);
            }
            else if (state == 0x00100001u)
            {
                Arena.Bounds = new ArenaBoundsCircle(30f);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.Hieroglyphika)
        {
            var center = Arena.Center;
            AddAOE(new AOEShapeCustom(center, [new Square(center, 30f)], [new Square(center, 24f)]), center);
        }
        else if (id == (uint)AID.Whorl)
        {
            AddAOE(transitionSmallerBounds, Arena.Center);
        }
        void AddAOE(AOEShape shape, WPos center)
        {
            _aoe = [new(shape, center, default, Module.CastFinishAt(spell), shapeDistance: transitionSmallerBounds.Distance(center, default))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.Hieroglyphika)
        {
            Arena.Bounds = new ArenaBoundsSquare(24f);
            _aoe = [];
        }
        else if (id == (uint)AID.Whorl)
        {
            Arena.Bounds = new ArenaBoundsCircle(30f);
            _aoe = [];
        }
    }
}

sealed class Sunbeam(BossModule module) : Components.BaitAwayCast(module, (uint)AID.SunbeamAOE, 6f);
sealed class DestructiveBolt(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.DestructiveBoltAOE, 6f, 8);

sealed class HandOfTheDestroyer(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.HandOfTheDestroyerWrathAOE, (uint)AID.HandOfTheDestroyerJudgmentAOE], new AOEShapeRect(90f, 20f));

sealed class SoaringMinuet(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SoaringMinuet, new AOEShapeCone(40f, 135f.Degrees()));
sealed class EudaimonEorzea(BossModule module) : Components.CastCounter(module, (uint)AID.EudaimonEorzeaAOE);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962u, NameID = 11301u, SortOrder = 7, PlanLevel = 90)]
public sealed class A34Eulogia(WorldState ws, Actor primary) : BossModule(ws, primary, new(945f, -945f), new ArenaBoundsCircle(35f));
