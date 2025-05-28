namespace BossMod.Shadowbringers.Alliance.A36FalseIdol;

public enum OID : uint
{
    Boss = 0x318D, // R26.000, x1
    BossP2 = 0x3190, // R5.999, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, x21, Helper type
    LighterNoteEW = 0x318E, // R1.000, x0 (spawn during fight)
    LighterNoteNS = 0x318F, // R1.000, x0 (spawn during fight)
    RedGirl = 0x3191, // R3.450, x0 (spawn during fight)
    Energy = 0x3192, // R1.000, x0 (spawn during fight)
    MagicalInterference = 0x1EB169,
    WhiteDissonance = 0x1EB16A,
    BlackDissonance = 0x1EB16B,
    RecreateStructure = 0x1EB16C,
}

public enum AID : uint
{
    _AutoAttack_ = 24572, // Boss->player, no cast, single-target
    _Spell_ScreamingScore = 23517, // Boss->self, 5.0s cast, range 60 circle
    _Spell_MadeMagic = 23511, // Boss->self, 7.0s cast, range 50 width 30 rect
    _Spell_MadeMagic1 = 23510, // Boss->self, 7.0s cast, range 50 width 30 rect
    _Spell_LighterNote = 23512, // Boss->self, 3.0s cast, single-target
    _Spell_LighterNote1 = 23513, // Helper->location, no cast, range 6 circle
    _Spell_LighterNote2 = 23514, // Helper->location, no cast, range 6 circle
    _Spell_RhythmRings = 23508, // Boss->self, 3.0s cast, single-target
    _Spell_MagicalInterference = 23509, // Helper->self, no cast, range 50 width 10 rect
    _Spell_SeedOfMagic = 23518, // Boss->self, 3.0s cast, single-target
    _Spell_ScatteredMagic = 23519, // Helper->location, 3.0s cast, range 4 circle
    _Spell_DarkerNote = 23515, // Boss->self, 5.0s cast, single-target
    _Spell_DarkerNote1 = 23516, // Helper->player, 5.0s cast, range 6 circle
    _Spell_Eminence = 24021, // Boss->location, 5.0s cast, range 60 circle
    _AutoAttack_1 = 24575, // BossP2->player, no cast, single-target
    _Spell_Pervasion = 23520, // BossP2->self, 3.0s cast, single-target
    _Spell_RecreateStructure = 23521, // BossP2->self, 3.0s cast, single-target
    _Weaponskill_UnevenFooting = 23522, // Helper->self, 1.9s cast, range 80 width 30 rect
    _Spell_RecreateSignal = 23523, // BossP2->self, 3.0s cast, single-target
    _Spell_MixedSignals = 23524, // BossP2->self, 3.0s cast, single-target
    _Weaponskill_Crash = 23525, // Helper->self, 0.8s cast, range 50 width 10 rect
    _Spell_LighterNote3 = 23564, // BossP2->self, 3.0s cast, single-target
    _Spell_ScreamingScore1 = 23541, // BossP2->self, 5.0s cast, range 71 circle
    _Spell_DarkerNote2 = 23562, // BossP2->self, 5.0s cast, single-target
    _Weaponskill_HeavyArms = 23534, // BossP2->self, 7.0s cast, single-target
    _Weaponskill_HeavyArms1 = 23535, // Helper->self, 7.0s cast, range 44 width 100 rect
    _Weaponskill_HeavyArms2 = 23533, // BossP2->self, 7.0s cast, range 100 width 12 rect
    _Spell_Distortion = 23529, // BossP2->self, 3.0s cast, range 60 circle
    _Spell_TheFinalSong = 23530, // BossP2->self, 3.0s cast, single-target
    _Spell_PlaceOfPower = 23565, // Helper->location, 3.0s cast, range 6 circle
    _Spell_WhiteDissonance = 23531, // Helper->self, no cast, range 60 circle
    _Spell_BlackDissonance = 23532, // Helper->self, no cast, range 60 circle
    _Weaponskill_PillarImpact = 23536, // BossP2->self, 10.0s cast, single-target
    _Weaponskill_Shockwave = 23538, // Helper->self, 6.5s cast, range 71 circle, distance 35 kb, can be invulned
    _Weaponskill_Shockwave1 = 23537, // Helper->self, 6.5s cast, range 7 circle
    _Weaponskill_PillarImpact1 = 23566, // BossP2->self, no cast, single-target
    _Weaponskill_Towerfall = 23539, // BossP2->self, 3.0s cast, single-target
    _Weaponskill_Towerfall1 = 23540, // Helper->self, 3.0s cast, range 70 width 14 rect
    _Ability_ = 23526, // 3191->self, no cast, single-target
    _Spell_Distortion1 = 24664, // BossP2->self, 3.0s cast, range 60 circle
    _Spell_ScatteredMagic1 = 23528, // 3192->player, no cast, single-target
    _Ability_1 = 23527, // 3191->self, no cast, single-target
    _Spell_RhythmRings1 = 23563, // BossP2->self, 3.0s cast, single-target
}

public enum SID : uint
{
    _Gen_VulnerabilityUp = 1789, // Boss/Helper/3192/BossP2->player, extra=0x1/0x2/0x3/0x4/0x5
    _Gen_Weakness = 43, // none->player, extra=0x0
    _Gen_Transcendent = 418, // none->player, extra=0x0
    _Gen_DownForTheCount = 2408, // Boss->player, extra=0xEC7
    _Gen_ = 2056, // none->BossP2, extra=0xE1
    _Gen_Distorted = 2535, // BossP2->player, extra=0x0
    _Gen_BrinkOfDeath = 44, // none->player, extra=0x0
}

public enum IconID : uint
{
    LighterNote = 1, // player->self
    _Gen_Icon_target_ae_s5f = 139, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_m0354_0c = 54, // Helper/BossP2->BossP2/Helper
}

class ScreamingScore(BossModule module) : Components.RaidwideCast(module, AID._Spell_ScreamingScore);
class MadeMagic(BossModule module) : Components.GroupedAOEs(module, [AID._Spell_MadeMagic, AID._Spell_MadeMagic1], new AOEShapeRect(50, 15));
class ScatteredMagic(BossModule module) : Components.StandardAOEs(module, AID._Spell_ScatteredMagic, 4);
class DarkerNote(BossModule module) : Components.BaitAwayCast(module, AID._Spell_DarkerNote1, new AOEShapeCircle(6), centerAtTarget: true);
class Eminence(BossModule module) : Components.RaidwideCast(module, AID._Spell_Eminence, "Knockback + stun");

class RecreateStructure(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_UnevenFooting)
{
    private readonly List<(Actor Actor, DateTime Activation)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _casters.Select(c => new AOEInstance(new AOEShapeRect(15, 80, 15), c.Actor.Position, c.Actor.Rotation, c.Activation));

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID == OID.RecreateStructure && state == 0x00010002)
            _casters.Add((actor, WorldState.FutureTime(9)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _casters.Clear();
        }
    }
}

class HeavyArms1(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_HeavyArms1, new AOEShapeRect(44, 50));
class HeavyArms2(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_HeavyArms2, new AOEShapeRect(100, 6));

class A36FalseIdolStates : StateMachineBuilder
{
    public A36FalseIdolStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ScreamingScore>()
            .ActivateOnEnter<MadeMagic>()
            .ActivateOnEnter<LighterNote>()
            .ActivateOnEnter<LighterNoteSpread>()
            .ActivateOnEnter<MagicalInterference>()
            .ActivateOnEnter<ScatteredMagic>()
            .ActivateOnEnter<DarkerNote>()
            .ActivateOnEnter<Eminence>()
            .ActivateOnEnter<RecreateStructure>()
            .ActivateOnEnter<MixedSignals>()
            .ActivateOnEnter<HeavyArms1>()
            .ActivateOnEnter<HeavyArms2>()
            .ActivateOnEnter<Distortion>()
            .ActivateOnEnter<Dissonance>()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed
                && module.Enemies(OID.BossP2).All(i => i.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9948)]
public class A36FalseIdol(WorldState ws, Actor primary) : BossModule(ws, primary, new(-700, -700), new ArenaBoundsSquare(24.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.BossP2), ArenaColor.Enemy);
    }
}
