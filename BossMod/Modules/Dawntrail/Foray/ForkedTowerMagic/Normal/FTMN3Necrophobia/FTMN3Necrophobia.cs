namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN3Necrophobia;

sealed class HailOfHellflares(BossModule module) : Components.RaidwideCast(module, (uint)AID.HailOfHellflares);
sealed class AncientFire(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.AncientFireIII, (uint)AID.AncientFireIII1, (uint)AID.SeveredFireIII], 18f); //necessary to predict Ancient Fire III1?
sealed class AncientBlizzard(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.AncientBlizzardIII, (uint)AID.AncientBlizzardIII1, (uint)AID.SeveredBlizzardIII], new AOEShapeCross(45f, 7.5f));
sealed class CorpseMangler(BossModule module) : Components.SingleTargetCast(module, (uint)AID.CorpseMangler, "");
sealed class AncientThunder(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.AncientThunderIII1, (uint)AID.AncientThunderIII3], new AOEShapeCone(60f, 22.5f.Degrees()));
sealed class DarkCurrent1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkCurrent1, new AOEShapeRect(60f, 5f));
sealed class DarkCurrent2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkCurrent2, new AOEShapeRect(10f, 30f)); // happens x2 on both sides, add predict since cast time so low
sealed class DarkCurrent(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeRect _rect = new(60f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count == 5 ? 3 : count > 3 ? 4 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes)[..max];
        var isFourAOEs = max == 4;
        var isThreeAOEs = max == 3;

        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];

            var shouldBeDanger = isFourAOEs && i < 2 || isThreeAOEs && i == 0;
            var shouldBeRisky = shouldBeDanger || max == 2 && i < 2;

            if (shouldBeDanger)
                aoe.Color = Colors.Danger;

            if (shouldBeRisky)
                aoe.Risky = true;
        }

        return aoes;
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DarkCurrent1)
        {
            //2.1s between casts
            var act = Module.CastFinishAt(spell);
            var position = spell.LocXZ;
            var rotation = spell.Rotation;
            var dir = rotation.ToDirection().OrthoL().Normalized();
            var distance = 10f;
            _aoes.Add(new(_rect, position, rotation, act, risky: true));

            for (var i = 1; i <= 2; i++)
            {
                _aoes.Add(new(_rect, position + i * distance * dir, rotation, act.AddSeconds(2.1d * i), risky: false));
                _aoes.Add(new(_rect, position + i * distance * dir * -1f, rotation, act.AddSeconds(2.1d * i), risky: false));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.DarkCurrent1:
                case (uint)AID.DarkCurrent2:
                    _aoes.RemoveAt(0);
                    break;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // stay near initial cast to move in after
        if (_aoes.Count == 5)
        {
            ref var aoe = ref _aoes.Ref(0);
            var shape = new SDInvertedRect(aoe.Origin, aoe.Rotation, 30f, 30f, 12f);
            hints.AddForbiddenZone(shape, aoe.Activation);
        }
        base.AddAIHints(slot, actor, assignment, hints);
    }
}
sealed class DeathlyRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeathlyRay, new AOEShapeRect(30f, 3f));
sealed class VacuumWave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VacuumWave, new AOEShapeCone(30f, 90f.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.WIP,
    StatesType = typeof(NecrophobiaStates),
    ConfigType = null, // replace null with typeof(NecrophobiaConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = typeof(TetherID),
    IconIDType = typeof(IconID),
    PrimaryActorOID = (uint)OID.Necrophobia,
    Contributors = "gynorhino",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14503u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class Necrophobia(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 800f), new ArenaBoundsCircle(24f));
