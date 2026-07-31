namespace BossMod.Shadowbringers.Alliance.A32HanselGretel;

[SkipLocalsInit]
sealed class WailLamentation(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.Wail1, (uint)AID.Wail2, (uint)AID.Lamentation1, (uint)AID.Lamentation2]);
[SkipLocalsInit]
sealed class CripplingBlow(BossModule module) : Components.SingleTargetDelayableCasts(module, [(uint)AID.CripplingBlow1, (uint)AID.CripplingBlow2]);

[SkipLocalsInit]
sealed class BloodySweep(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BloodySweep1, (uint)AID.BloodySweep2,
(uint)AID.BloodySweep3, (uint)AID.BloodySweep4], new AOEShapeRect(50, 12.5f));

[SkipLocalsInit]
sealed class PassingLance(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PassingLance, new AOEShapeRect(50f, 12f));
[SkipLocalsInit]
sealed class UnevenFooting(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UnevenFooting, 23f);
[SkipLocalsInit]
sealed class HungryLance(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.HungryLance1, (uint)AID.HungryLance2], new AOEShapeCone(40f, 60f.Degrees()));

[SkipLocalsInit]
sealed class Breakthrough(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Breakthrough, new AOEShapeRect(53f, 16f));
[SkipLocalsInit]
sealed class SeedOfMagicBeta(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SeedOfMagicBeta, 5f);
[SkipLocalsInit]
sealed class UpgradedShield(BossModule module) : Components.DirectionalParry(module, [(uint)OID.Gretel, (uint)OID.Hansel])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.UpgradedShield1 or (uint)AID.UpgradedShield2)
        {
            PredictParrySide(caster.InstanceID, Side.All ^ Side.Front);
        }
    }
}

[SkipLocalsInit]
sealed class MagicalConfluence(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> voidzones = module.Enemies((uint)OID.MagicalConfluence);
    private readonly AOEShapeCircle circle = new(4f);
    private readonly AOEShapeArcCapsule arcCW = new(4f, 30f.Degrees(), module.Arena.Center), arcCCW = new(4f, -30f.Degrees(), module.Arena.Center);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = voidzones.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = new AOEInstance[count];
        var center = Arena.Center;
        for (var i = 0; i < count; ++i)
        {
            var vz = voidzones[i];
            var pos = vz.Position;
            if (vz.LastFrameMovement == default)
            {
                aoes[i] = new(circle, pos.Quantized());
            }
            else
            {
                var dir = pos - center;
                var ccw = vz.Rotation.ToDirection().OrthoR().Dot(dir) < 0f;
                aoes[i] = new(ccw ? arcCCW : arcCW, pos.Quantized());
            }
        }
        return aoes;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = voidzones.Count;
        if (count == 0)
        {
            return;
        }
        var forbiddenNearFuture = WorldState.FutureTime(1.1d);
        var forbiddenSoon = WorldState.FutureTime(3d);
        var forbiddenFarFuture = DateTime.MaxValue;
        var center = Arena.Center;
        var a15 = 15f.Degrees();
        var a25 = 25f.Degrees();
        var a35 = 35f.Degrees();
        for (var i = 0; i < count; ++i)
        {
            var vz = voidzones[i];
            var pos = vz.Position;
            var dir = pos - center;
            var ccw = vz.Rotation.ToDirection().OrthoR().Dot(dir) < 0f;
            var mult = ccw ? -1f : 1f;
            var mov = vz.LastFrameMovement != default;
            if (mov)
            {
                hints.AddForbiddenZone(new SDArcCapsule(pos, center, mult * a15, 4f), forbiddenNearFuture);
                hints.AddForbiddenZone(new SDArcCapsule(pos, center, mult * a25, 4f), forbiddenSoon);
                hints.AddForbiddenZone(new SDArcCapsule(pos, center, mult * a35, 4f), forbiddenFarFuture);
            }
            hints.TemporaryObstacles.Add(new SDCircle(pos.Quantized(), mov ? 4f : 5f));
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", PrimaryActorOID = (uint)OID.Gretel, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9990, SortOrder = 2)]
[SkipLocalsInit]
public sealed class A32HanselGretel(WorldState ws, Actor primary) : BossModule(ws, primary, new(-800f, -951.03119f), new ArenaBoundsCircle(24.5f))
{
    public Actor? BossHansel;

    protected override void UpdateModule()
    {
        BossHansel ??= GetActor((uint)OID.Hansel);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(BossHansel);
    }
}
