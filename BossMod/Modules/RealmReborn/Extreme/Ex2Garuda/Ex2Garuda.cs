namespace BossMod.RealmReborn.Extreme.Ex2Garuda;

class DownburstBoss : Components.Cleave
{
    public DownburstBoss() : base(ActionID.MakeSpell(AID.Downburst1), new AOEShapeCone(11.7f, 60.Degrees())) { } // TODO: verify angle
}

class DownburstSuparna : Components.Cleave
{
    public DownburstSuparna() : base(ActionID.MakeSpell(AID.Downburst1), new AOEShapeCone(11.36f, 60.Degrees()), (uint)OID.Suparna) { } // TODO: verify angle
}

class DownburstChirada : Components.Cleave
{
    public DownburstChirada() : base(ActionID.MakeSpell(AID.Downburst2), new AOEShapeCone(11.36f, 60.Degrees()), (uint)OID.Chirada) { } // TODO: verify angle
}

class Slipstream : Components.SelfTargetedAOEs
{
    public Slipstream() : base(ActionID.MakeSpell(AID.Slipstream), new AOEShapeCone(11.7f, 45.Degrees())) { }
}

class FrictionAdds : Components.LocationTargetedAOEs
{
    public FrictionAdds() : base(ActionID.MakeSpell(AID.FrictionAdds), 5) { }
}

class FeatherRain : Components.LocationTargetedAOEs
{
    public FeatherRain() : base(ActionID.MakeSpell(AID.FeatherRain), 3) { }
}

class AerialBlast : Components.RaidwideCast
{
    public AerialBlast() : base(ActionID.MakeSpell(AID.AerialBlast)) { }
}

class MistralShriek : Components.RaidwideCast
{
    public MistralShriek() : base(ActionID.MakeSpell(AID.MistralShriek)) { }
}

class Gigastorm : Components.SelfTargetedAOEs
{
    public Gigastorm() : base(ActionID.MakeSpell(AID.Gigastorm), new AOEShapeCircle(6.5f)) { }
}

class GreatWhirlwind : Components.LocationTargetedAOEs
{
    public GreatWhirlwind() : base(ActionID.MakeSpell(AID.GreatWhirlwind), 8) { }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 65, NameID = 1644)]
public class Ex2Garuda : BossModule
{
    public IReadOnlyList<Actor> Monoliths;
    public IReadOnlyList<Actor> RazorPlumes;
    public IReadOnlyList<Actor> SpinyPlumes;
    public IReadOnlyList<Actor> SatinPlumes;
    public IReadOnlyList<Actor> Chirada;
    public IReadOnlyList<Actor> Suparna;

    public Ex2Garuda(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(0, 0), 22))
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
