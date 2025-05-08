namespace BossMod.RealmReborn.Extreme.Ex2Garuda;

class DownburstBoss(BossModule module) : Components.Cleave(module, AID.Downburst1, new AOEShapeCone(11.7f, 60.Degrees())); // TODO: verify angle
class DownburstSuparna(BossModule module) : Components.Cleave(module, AID.Downburst1, new AOEShapeCone(11.36f, 60.Degrees()), (uint)OID.Suparna); // TODO: verify angle
class DownburstChirada(BossModule module) : Components.Cleave(module, AID.Downburst2, new AOEShapeCone(11.36f, 60.Degrees()), (uint)OID.Chirada); // TODO: verify angle
class Slipstream(BossModule module) : Components.StandardAOEs(module, AID.Slipstream, new AOEShapeCone(11.7f, 45.Degrees()));
class FrictionAdds(BossModule module) : Components.StandardAOEs(module, AID.FrictionAdds, 5);
class FeatherRain(BossModule module) : Components.StandardAOEs(module, AID.FeatherRain, 3);
class AerialBlast(BossModule module) : Components.RaidwideCast(module, AID.AerialBlast);
class MistralShriek(BossModule module) : Components.RaidwideCast(module, AID.MistralShriek);
class Gigastorm(BossModule module) : Components.StandardAOEs(module, AID.Gigastorm, new AOEShapeCircle(6.5f));
class GreatWhirlwind(BossModule module) : Components.StandardAOEs(module, AID.GreatWhirlwind, 8);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 65, NameID = 1644)]
public class Ex2Garuda : BossModule
{
    public IReadOnlyList<Actor> Monoliths;
    public IReadOnlyList<Actor> RazorPlumes;
    public IReadOnlyList<Actor> SpinyPlumes;
    public IReadOnlyList<Actor> SatinPlumes;
    public IReadOnlyList<Actor> Chirada;
    public IReadOnlyList<Actor> Suparna;

    public Ex2Garuda(WorldState ws, Actor primary) : base(ws, primary, new(0, 0), new ArenaBoundsCircle(22))
    {
        Monoliths = Enemies(OID.Monolith);
        RazorPlumes = Enemies(OID.RazorPlume);
        SpinyPlumes = Enemies(OID.SpinyPlume);
        SatinPlumes = Enemies(OID.SatinPlume);
        Chirada = Enemies(OID.Chirada);
        Suparna = Enemies(OID.Suparna);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Monoliths.Where(a => !a.IsDead), ArenaColor.Object, true);
        Arena.Actors(RazorPlumes, ArenaColor.Enemy);
        Arena.Actors(SpinyPlumes, ArenaColor.Enemy);
        Arena.Actors(SatinPlumes, ArenaColor.Enemy);
        Arena.Actors(Chirada, ArenaColor.Enemy);
        Arena.Actors(Suparna, ArenaColor.Enemy);
    }
}
