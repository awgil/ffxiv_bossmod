#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Stormblood.Alliance.A35UltimaTheHighSeraph;

public enum OID : uint
{
    Boss = 0x25A9, // R8.000, x1
    Helper = 0x233C, // R0.500, x41
    BossP1 = 0x2604, // R10.400, x1
    _Gen_Dominion = 0x25AE, // R1.700, x0 (spawn during fight)
    _Gen_AuraciteShard = 0x2639, // R1.250, x0 (spawn during fight)
    _Gen_Dominion1 = 0x25B6, // R1.700, x0 (spawn during fight)
    _Gen_Dominion2 = 0x2689, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 14501, // Boss->player, no cast, single-target
    _Ability_DemiVirgo = 14530, // Boss->self, 3.0s cast, single-target
    _Ability_EastwardMarch = 14494, // Boss->self, 4.0s cast, single-target
    _Ability_EastwardMarch1 = 14496, // Helper->self, no cast, single-target
    _Weaponskill_RayOfLight = 14519, // Helper->self, no cast, range 70 width 10 rect
    _Weaponskill_RayOfLight1 = 14518, // 25AE->self, no cast, single-target
    _Weaponskill_Redemption = 14506, // Boss->player, 4.0s cast, single-target
    _Weaponskill_GrandCross = 14536, // Boss->self, 3.0s cast, single-target
    _Weaponskill_Plummet = 14509, // 2639->self, 3.5s cast, range 15 width 15 rect
    _Weaponskill_GrandCross1 = 14510, // 2639->self, 4.0s cast, range 60 width 15 cross
    _Weaponskill_Penultima = 14524, // 25B6->player, no cast, single-target
    _Weaponskill_Embrace = 14520, // 25AE->self, 3.0s cast, single-target
    _Weaponskill_Embrace1 = 14521, // Helper->location, 3.0s cast, range 3 circle
    _Weaponskill_Embrace2 = 14522, // Helper->self, no cast, range 7 circle
    _Weaponskill_LifeDrain = 14523, // Helper->player, no cast, single-target
    _Weaponskill_HolyIV = 14490, // Helper->location, 3.0s cast, range 6 circle
    _Weaponskill_HolyIV1 = 14534, // Boss->self, 3.0s cast, single-target
    _Weaponskill_HolyIV2 = 14491, // Helper->players, 5.5s cast, range 6 circle
    _Ability_Cataclysm = 14500, // Boss->self, no cast, single-target
    _Weaponskill_Shockwave = 14484, // Helper->self, no cast, range 80 circle
    _Weaponskill_Auralight = 14504, // Helper->self, 2.5s cast, range 50 width 10 rect
    _Weaponskill_Auralight1 = 14505, // Helper->self, 2.5s cast, range 25 width 10 rect
    _Weaponskill_Bombardment = 14526, // Helper->location, 2.5s cast, range 6 circle
    _Weaponskill_Holy = 14507, // Helper->location, 2.5s cast, range 2 circle
    _Ability_EastwardMarch2 = 14502, // Boss->self, 4.0s cast, single-target
    _Weaponskill_FlareIV = 14493, // Helper->players, 5.5s cast, range 80 circle
}

public enum SID : uint
{
    _Gen_VulnerabilityUp = 202, // 2639->player, extra=0x1
    _Gen_Clashing = 1271, // none->player, extra=0x17E3
    _Gen_Fearless = 1747, // 25B6->player, extra=0x0
    _Gen_Bind = 564, // Helper->player, extra=0x0
    _Gen_OutOfTheAction = 1284, // none->player, extra=0x63E
    _Gen_AccelerationBomb = 1072, // none->player, extra=0x0
}

public enum IconID : uint
{
    _Gen_Icon_tank_lockon01i = 198, // player->self
    _Gen_Icon_target_ae_s5f = 139, // player->self
    _Gen_Icon_m0328sp10trg0s = 75, // player->self
    _Gen_Icon_m0361trg_b0t = 87, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_teke01h = 84, // 25AE->player
}

class RayOfLight(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_RayOfLight)
{
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Ability_EastwardMarch)
            foreach (var actor in Module.Enemies(OID._Gen_Dominion).Where(d => d.ModelState.ModelState == 24))
                _predicted.Add(new(new AOEShapeRect(70, 5), actor.Position + new WDir(10, 0), actor.Rotation, WorldState.FutureTime(9.1f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
        }
    }
}
class Redemption(BossModule module) : Components.SingleTargetCast(module, AID._Weaponskill_Redemption);
class Plummet(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Plummet, new AOEShapeRect(15, 7.5f));

class GrandCross(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_GrandCross1)
{
    readonly List<AOEInstance> _predicted = [];
    DateTime Activation;
    bool Risky;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Select(p => p with { Risky = Risky });

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_Plummet)
        {
            if (Activation == default)
                Activation = WorldState.FutureTime(13.8f);
            var src = caster.Position;
            if (_predicted.Count == 2)
                Risky = true;
            else
                src += new WDir(15, 0);
            _predicted.Add(new(new AOEShapeCross(60, 7.5f), src, default, Activation));
        }

        if (spell.Action == WatchedAction)
        {
            if (_predicted.Count == 3)
                _predicted.RemoveAt(0);
            _predicted.Add(new(new AOEShapeCross(60, 7.5f), spell.LocXZ, default, Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
            if (_predicted.Count == 0)
            {
                Activation = default;
                Risky = false;
            }
        }
    }
}

class Penultima(BossModule module) : Components.GenericTowers(module, AID._Weaponskill_Penultima)
{
    public override void OnActorTimelineStateChange(Actor actor, uint state)
    {
        if (actor.OID == 0x1EAA53 && actor.TimelineState == 0x0040)
            Towers.Add(new(TowerPosition(actor), 1, activation: WorldState.FutureTime(8.8f)));

        if (actor.OID == 0x1EAA53 && actor.TimelineState == 4)
            Towers.RemoveAll(t => t.Position.AlmostEqual(TowerPosition(actor), 1));
    }

    static WPos TowerPosition(Actor eventObj) => eventObj.ActualPosition + eventObj.Rotation.ToDirection() * 14;
}

class EmbraceSpawn(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Embrace1, 3);
class EmbracePuddle(BossModule module) : Components.Voidzone(module, 7, OID._Gen_Dominion2);
class HolyIVPuddle(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_HolyIV, 6);
class HolyIVSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID._Weaponskill_HolyIV2, 6);

class Bombardment(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Bombardment, 6);
class Holy(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Holy, 2);

class A35UltimaTheHighSeraphStates : StateMachineBuilder
{
    public A35UltimaTheHighSeraphStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RayOfLight>()
            .ActivateOnEnter<Redemption>()
            .ActivateOnEnter<Plummet>()
            .ActivateOnEnter<GrandCross>()
            .ActivateOnEnter<Penultima>()
            .ActivateOnEnter<EmbraceSpawn>()
            .ActivateOnEnter<EmbracePuddle>()
            .ActivateOnEnter<HolyIVPuddle>()
            .ActivateOnEnter<HolyIVSpread>()
            .ActivateOnEnter<Bombardment>()
            .ActivateOnEnter<Holy>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7909)]
public class A35UltimaTheHighSeraph(ModuleInit init) : BossModule(init, new(600, -600), new ArenaBoundsSquare(29.5f));
