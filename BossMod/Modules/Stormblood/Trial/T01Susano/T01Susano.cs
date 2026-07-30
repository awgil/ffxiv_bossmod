namespace BossMod.Stormblood.Trial.T01Susano;

public enum OID : uint
{
    Susano = 0x1AF7,
    Helper = 0x233C,
    AmaNoIwato = 0x1BA1, // R0.500, x?, mixed types
    SusanoBig = 0x1AF8, // R0.000, x?
    _Gen_Actor1e8536 = 0x1E8536, // R2.000, x?, EventObj type
    _Gen_Actor1ea478 = 0x1EA478, // R2.000, x?, EventObj type
    BladesShadow = 0x1EA479, // R2.000, x?, EventObj type
    _Gen_Exit = 0x1E850B, // R0.500, x?, EventObj type
    DarkCloud = 0x1F53, // R3.000, x?
    _Gen_2 = 0x1BA2, // R1.000, x? : Crystal flower item that starts the clash possibly.
    AmeNoMurakumo = 0x1C84, // R8.000, x?, Part type
    DarkLevin = 0x1B9B, // R1.000, x? : Lightning orbs
    AmaNoIwato1 = 0x1C20, // R1.800, x? Stones
}

public enum AID : uint
{
    _AutoAttack_Attack = 870, // 1AF7->player, no cast, single-target
    Assail = 8220, // 1AF7->player, no cast, single-target
    RasenKaikyo = 8222, // 1BA1->self, 3.0s cast, range 6 circle
    RasenKaikyoVisual = 8221, // 1AF7->self, 3.0s cast, single-target
    YataNoKagami = 8223, // 1AF7->player, no cast, single-target
    Brightstorm = 8224, // 1AF7->players, no cast, range 6 circle
    YasakaniNoMagatamaVisual = 9633, // 1AF7->self, no cast, single-target : Summons the giant Susano
    ThePartingClouds = 9631, // 1F53->self, 3.5s cast, range 50+R width 10 rect
    SeasplitterVisual = 9661, // 1BA1->self, 2.9s cast, single-target
    Seasplitter = 8232, // 1BA1->self, 3.0s cast, range 21+R width 40 rect
    Seasplitter1 = 8233, // 1BA1->self, no cast, range 7+R width 40 rect
    Seasplitter2 = 8234, // 1BA1->self, no cast, range 7+R width 40 rect
    Seasplitter3 = 8235, // 1BA1->self, no cast, range 7+R width 40 rect
    _Weaponskill_ = 8646, // 1AF8->self, no cast, single-target
    SheerForce = 8225, // 1BA1->self, no cast, range 40+R circle
    Shock = 8259, // 1B9B->self, no cast, range 6 circle
    AmeNoMurakumoRaidwide = 8226, // 1AF8->self, no cast, range 40+R circle
    AmeNoMurakumoRectAOE = 8588, // 1BA1->self, 4.0s cast, range 40+R width 6 rect
    // Rectangle tank buster with icon
    Stormsplitter = 8227, // 1AF7->self/player, 5.0s cast, range 20+R width 4 rect
    // The gates are the rock game.
    _Ability_TheHiddenGate = 8228, // 1AF7->self, no cast, single-target
    _Ability_TheAlteredGate = 8333, // 1BA1->1C20, no cast, ???
    TheSealedGate = 8229, // 1C20->self, 15.0s cast, single-target
    Ukehi = 8230, // 1AF7->self, 4.0s cast, range 40+R circle
    _Weaponskill_Ukehi1 = 8231, // 1AF7->self, no cast, range 40+R circle
    AmeNoMurakumoWipe = 9506, // 1C84->self, 24.0s cast, single-target - Enrage cast for not beating big sword
}

public enum SID : uint
{
    _Gen_LightningResistanceDown = 898, // AmaNoIwato->player, extra=0x1
    _Gen_Paralysis = 216, // DarkCloud->player, extra=0x0
    _Gen_Clashing = 1271, // Susano->player, extra=0x17E3
    _Gen_FleshWound = 624, // Susano->player, extra=0x0
    _Gen_Fetters = 292, // Susano->player, extra=0x0

}

public enum IconID : uint
{
    _Gen_Icon_lockon5_t0h = 23, // player->self : Knockback icon?
    _Gen_Icon_com_share0c = 62, // player->self
    StormsplitterIcon = 230, // player->self : Caution Tankbuster: After icon is cast it is followed up with Spell 8227 'Stormsplitter'
    _Gen_Icon_m0372trg_t2j = 112, // AmaNoIwato1->self : 1C20/Ama-no-iwato
}

public enum TetherID : uint
{
    _Gen_Tether_chn_m0372_01j = 66, // AmaNoIwato1->Susano
}

sealed class RasenKaikyo(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RasenKaikyo, 6f);

sealed class YataNoKagami(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.YataNoKagami, 20f, kind: Kind.AwayFromOrigin);

sealed class Ukehi(BossModule module) : Components.RaidwideCast(module, (uint)AID.Ukehi);

sealed class Brightstorm(BossModule module) : Components.StackWithIcon(module, (uint)IconID._Gen_Icon_com_share0c, (uint)AID.Brightstorm, 6f, 8);

sealed class ThePartingClouds(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThePartingClouds, new AOEShapeRect(65f, 5f));

sealed class AmeNoMurakumoRaidwide(BossModule module) : Components.RaidwideInstant(module, (uint)AID.AmeNoMurakumoRaidwide);

sealed class AmeNoMurakumoRectAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AmeNoMurakumoRectAOE, new AOEShapeRect(65f, 3f));

sealed class Shock(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shock, 6f);

sealed class AmeNoMurakumoOrbs(BossModule module) : BossComponent(module)
{
    public static List<Actor> GetOrbs(BossModule module)
    {
        var orbs = module.Enemies((uint)OID.DarkLevin);
        var count = orbs.Count;
        if (count == 0)
            return [];

        var filteredorbs = new List<Actor>(count);
        for (var i = 0; i < count; ++i)
        {
            var z = orbs[i];
            if (!z.IsDead)
                filteredorbs.Add(z);
        }
        return filteredorbs;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        var orbs = GetOrbs(Module);
        var count = orbs.Count;
        if (count != 0)
            hints.Add("OT soaks orbs!");
    }

    // Draw the dark levin as circles on radar.
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var orbs = GetOrbs(Module);
        var count = orbs.Count;
        for (var i = 0; i < count; ++i)
            Arena.ZoneCircle(orbs[i].Position, 1f, Colors.Danger);
    }
}

sealed class SeaSplitterCast(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Seasplitter, new AOEShapeRect(41f, 20f));

sealed class SeaSplitter1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Seasplitter1, new AOEShapeRect(27f, 20f));
sealed class SeaSplitter2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Seasplitter2, new AOEShapeRect(27f, 20f));
sealed class SeaSplitter3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Seasplitter3, new AOEShapeRect(27f, 20f));

sealed class StormSplitter(BossModule module) : Components.IconSharedTankbuster(module, (uint)IconID.StormsplitterIcon, (uint)AID.Stormsplitter, new AOEShapeRect(40f, 2f));

sealed class SheerForce(BossModule module) : Components.RaidwideInstant(module, (uint)AID.SheerForce);

[SkipLocalsInit]
sealed class SusanoStates : StateMachineBuilder
{
    public SusanoStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        SimpleState(id + 0xFF0000u, 10000f, "???")
            .ActivateOnEnter<RasenKaikyo>()
            .ActivateOnEnter<YataNoKagami>()
            .ActivateOnEnter<Ukehi>()
            .ActivateOnEnter<Brightstorm>()
            .ActivateOnEnter<ThePartingClouds>()
            .ActivateOnEnter<AmeNoMurakumoRaidwide>()
            .ActivateOnEnter<AmeNoMurakumoRectAOE>()
            .ActivateOnEnter<Shock>()
            .ActivateOnEnter<AmeNoMurakumoOrbs>()
            .ActivateOnEnter<SeaSplitterCast>()
            .ActivateOnEnter<SeaSplitter1>()
            .ActivateOnEnter<SeaSplitter2>()
            .ActivateOnEnter<SeaSplitter3>()
            .ActivateOnEnter<StormSplitter>()
            .ActivateOnEnter<SheerForce>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(SusanoStates),
    ConfigType = null, // replace null with typeof(SusanoConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = typeof(TetherID),
    IconIDType = typeof(IconID),
    PrimaryActorOID = (uint)OID.Susano,
    Contributors = "wen",
    Expansion = BossModuleInfo.Expansion.Stormblood,
    Category = BossModuleInfo.Category.Trial,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 243u,
    NameID = 6221u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public class Susano(WorldState ws, Actor primary)
    : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(20f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        if (PrimaryActor.IsTargetable)
        {
            if (PrimaryActor.Position.InCircle(Arena.Center, 20f))
                Arena.ActorInsideBounds(Arena.Center - new WDir(0, 20), PrimaryActor.Rotation, Colors.Enemy);
            else
                Arena.ActorOutsideBounds(Arena.Center - new WDir(0, 20f), PrimaryActor.Rotation, Colors.Enemy);
        }

        Arena.Actors(Enemies((uint)OID.SusanoBig), Colors.Enemy);
        Arena.Actors(Enemies((uint)OID.AmaNoIwato), Colors.Enemy);
        Arena.Actors(Enemies((uint)OID.AmaNoIwato1), Colors.Enemy); // One of these AmaNoIwato rocks hides the player
        Arena.Actors(Enemies((uint)OID.AmeNoMurakumo), Colors.Enemy);
        Arena.Actors(Enemies((uint)OID.BladesShadow), Colors.Enemy);
        Arena.Actors(Enemies((uint)OID.DarkCloud), Colors.Object, true); // Shoots wide line AOE The parting clouds
        Arena.Actors(Enemies((uint)OID.DarkLevin), Colors.Enemy); // clouds that should be dodged unless you are offtank.
    }
}
