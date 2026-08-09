namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V21Yozakura;

sealed class GloryNeverlasting(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.GloryNeverlasting);
sealed class ArtOfTheFireblossom(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArtOfTheFireblossom, 9f);
sealed class ArtOfTheWindblossom(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArtOfTheWindblossom, new AOEShapeDonut(5f, 60f));
sealed class KugeRantsuiOkaRanman(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.KugeRantsui, (uint)AID.OkaRanman]);
sealed class LevinblossomStrike(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LevinblossomStrike, 3f);

sealed class DriftingPetals(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.DriftingPetals, 15f, ignoreImmunes: true)
{
    private readonly Mudrain _aoe1 = module.FindComponent<Mudrain>()!;
    private readonly Witherwind _aoe2 = module.FindComponent<Witherwind>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            var aoes = _aoe1.ActiveAOEs(slot, actor);
            var origin = c.Origin;
            var a20 = 20f.Degrees();
            var len = aoes.Length;
            var forbidden = new ShapeDistance[len + 1];
            forbidden[len] = new SDInvertedCircle(origin, 5f);

            for (var i = 0; i < len; ++i)
            {
                ref readonly var aoe = ref aoes[i];
                forbidden[i] = new SDCone(origin, 20f, Angle.FromDirection(aoe.Origin - origin), a20);
            }
            hints.AddForbiddenZone(new SDUnion(forbidden), c.Activation);
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe1.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            if (aoes[i].Check(pos))
            {
                return true;
            }
        }
        var aoes2 = _aoe2.ActiveAOEs(slot, actor);
        var len2 = aoes2.Length;
        for (var i = 0; i < len2; ++i)
        {
            if (aoes2[i].Check(pos))
            {
                return true;
            }
        }
        return !Arena.InBounds(pos);
    }
}

sealed class Mudrain(BossModule module) : Components.VoidzoneAtCastTarget(module, 5f, (uint)AID.Mudrain, GetVoidzones, 0.7d)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.MudVoidzone);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}
sealed class Icebloom(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Icebloom, 6);
sealed class Shadowflight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shadowflight, new AOEShapeRect(10f, 3f));
sealed class MudPie(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MudPie, new AOEShapeRect(60f, 3f));
sealed class FireblossomFlare(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FireblossomFlare, 6f);
sealed class ArtOfTheFluff(BossModule module) : Components.CastGazes(module, [(uint)AID.ArtOfTheFluff1, (uint)AID.ArtOfTheFluff2]);
sealed class TatamiGaeshi(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TatamiGaeshi, new AOEShapeRect(40f, 5f));
sealed class AccursedSeedling(BossModule module) : Components.Voidzone(module, 4f, GetSeedlings)
{
    private static List<Actor> GetSeedlings(BossModule module) => module.Enemies((uint)OID.AccursedSeedling);
}

sealed class RootArrangement(BossModule module) : Components.StandardChasingAOEs(module, 4f, (uint)AID.RockRootArrangementFirst, (uint)AID.RockRootArrangementRest, 4, 1, 4, true, (uint)IconID.RootArrangement)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (TargetsMask[slot])
        {
            hints.AddForbiddenZone(new SDRect(Arena.Center + new WDir(19f, default), Arena.Center + new WDir(-19f, default), 20f), Activation);
        }
    }
}

sealed class Witherwind(BossModule module) : Components.Voidzone(module, 3f, GetWhirlwind, 20f)
{
    private static List<Actor> GetWhirlwind(BossModule module) => module.Enemies((uint)OID.AutumnalTempest);
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", PrimaryActorOID = (uint)OID.Yozakura, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 945u, NameID = 12325u, SortOrder = 2, Category = BossModuleInfo.Category.VariantCriterion, Expansion = BossModuleInfo.Expansion.Endwalker)]
public sealed class V21Yozakura(WorldState ws, Actor primary) : BossModule(ws, primary, primary.PosRot.X is var X && X < -700f ? new(-775f, 16f) : X > 700f ? new(737f, 220f) : new(47f, 93f), X < -700f ? new ArenaBoundsSquare(22.5f) : X > 700f ? new ArenaBoundsSquare(19.5f) : new ArenaBoundsSquare(22.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.LivingGaol));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.LivingGaol => 1,
                _ => 0
            };
        }
    }
}
