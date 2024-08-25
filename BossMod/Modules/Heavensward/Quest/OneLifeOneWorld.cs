namespace BossMod.Heavensward.Quest.OneLifeForOneWorld;

public enum OID : uint
{
    Boss = 0x17CD,
    _Gen_FirstWard = 0x18D6, // R0.500, x7
    _Gen_RangerOfDarkness = 0x17D0, // R0.500, x1
    _Gen_DevoutOfDarkness = 0x17D1, // R0.500, x1
    _Gen_KnightOfDarkness = 0x17CE, // R0.500, x1
    _Gen_MagusOfDarkness = 0x17CF, // R0.500, x1
}

public enum AID : uint
{
    _Ability_TheWanderersMinuet = 3559, // 17D0->self, no cast, single-target
    _AutoAttack_Attack = 873, // 17D0->17C8, no cast, single-target
    _AutoAttack_Attack1 = 870, // 17CE/Boss->player/17CB, no cast, single-target
    _Weaponskill_Windbite = 113, // 17D0->17C8, no cast, single-target
    _Weaponskill_HeavySwing = 6682, // Boss->player, no cast, single-target
    _Spell_FireIII = 152, // 17CF->17CA, 3.5s cast, single-target
    _Ability_Enochian = 3575, // 17CF->self, no cast, single-target
    _Spell_CureII = 135, // 17D1->self/17D1/Boss/17CF/17D0/17CE, 2.0s cast, single-target
    _Ability_Cover = 6693, // 17CE->Boss, no cast, single-target
    _Weaponskill_VenomousBite = 100, // 17D0->17C8, no cast, single-target
    _Weaponskill_FastBlade = 9, // 17CE->17CB, no cast, single-target
    _Spell_FireIV = 3577, // 17CF->17CA, 2.8s cast, single-target
    _Weaponskill_IronJaws = 3560, // 17D0->17C8, no cast, single-target
    _Weaponskill_Overpower = 6683, // Boss->self, 2.5s cast, range 6+R 90-degree cone
    _Weaponskill_RiotBlade = 15, // 17CE->17CB, no cast, single-target
    _Spell_Fire = 141, // 17CF->17CA, 2.5s cast, single-target
    _Weaponskill_GoringBlade = 3538, // 17CE->17CB, no cast, single-target
    _Ability_CircleOfScorn = 23, // 17CE->self, no cast, range 5 circle
    _Spell_BlizzardIII = 154, // 17CF->17CA, 3.5s cast, single-target
    _Spell_Thunder = 144, // 17CF->17CA, no cast, single-target
    _Weaponskill_UnlitCyclone = 6684, // Boss->self, 4.0s cast, range 5+R circle
    _Spell_UnlitCyclone = 6685, // 18D6->location, 4.0s cast, range 9 circle
    _Weaponskill_Skydrive = 6686, // Boss->player, 5.0s cast, single-target
    _Weaponskill_Skydrive1 = 6687, // 18D6->player, no cast, range 5 circle
    _Ability_Convalescence = 12, // _Gen_KnightOfDarkness->self, no cast, single-target
    _Ability_Sentinel = 17, // _Gen_KnightOfDarkness->self, no cast, single-target
    _Ability_UnlitChain = 6688, // Boss->self, 4.0s cast, single-target
    _Spell_StoneIII = 3568, // _Gen_DevoutOfDarkness->17CC, 1.5s cast, single-target
    _Spell_StoneII = 127, // _Gen_DevoutOfDarkness->17CC, 1.5s cast, single-target
    _Ability_Awareness = 29167, // _Gen_KnightOfDarkness->self, no cast, single-target
    _Ability_UtterDestruction = 6689, // Boss->self, 2.5s cast, single-target
    _Spell_UtterDestruction = 6690, // _Gen_FirstWard->self, 3.0s cast, range 20+R circle
    _Weaponskill_RollingBlade = 6691, // Boss->self, 3.0s cast, range 7 circle
    _Weaponskill_RollingBlade1 = 6692, // _Gen_FirstWard->self, 3.0s cast, range 60+R 30-degree cone
}

public enum SID : uint
{
    _Gen_TheWanderersMinuet = 2216, // _Gen_RangerOfDarkness->_Gen_RangerOfDarkness, extra=0x0
    _Gen_Thunderhead = 3870, // _Gen_MagusOfDarkness->_Gen_MagusOfDarkness, extra=0x0
    _Gen_Windbite = 129, // _Gen_RangerOfDarkness->17C8, extra=0x14
    _Gen_KissOfTheViper = 490, // 17CB->17CB, extra=0x0
    _Gen_Invincibility = 325, // _Gen_KnightOfDarkness->Boss/_Gen_FirstWard, extra=0x0
    _Gen_VenomousBite = 124, // _Gen_RangerOfDarkness->17C8, extra=0x14
    _Gen_BioII = 189, // 17C8->_Gen_RangerOfDarkness, extra=0x0
    _Gen_VulnerabilityUp = 638, // 17CB->_Gen_KnightOfDarkness, extra=0x0
    _Gen_Firestarter = 165, // _Gen_MagusOfDarkness->_Gen_MagusOfDarkness, extra=0x0
    _Gen_Bio = 179, // 17C8->_Gen_RangerOfDarkness, extra=0x0
    _Gen_CircleOfScorn = 248, // _Gen_KnightOfDarkness->17CB, extra=0x0
    _Gen_Thunder = 161, // _Gen_MagusOfDarkness->17CA, extra=0x0
    _Gen_Bleeding = 642, // none->player, extra=0x0
    _Gen_Miasma = 180, // 17C8->_Gen_RangerOfDarkness, extra=0x0
}

class Overpower(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Overpower), new AOEShapeCone(7, 45.Degrees()));
class UnlitCyclone(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_UnlitCyclone), new AOEShapeCircle(6));
class UnlitCyclone2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_UnlitCyclone), 9);

class Skydrive(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(5), 23, ActionID.MakeSpell(AID._Weaponskill_Skydrive), centerAtTarget: true);
class SkydrivePuddle(BossModule module) : Components.PersistentVoidzone(module, 5, m => m.Enemies(0x1EA19C).Where(x => x.EventState != 7));
class RollingBlade(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_RollingBlade), new AOEShapeCircle(7));
class RollingBlade2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_RollingBlade1), new AOEShapeCone(60, 15.Degrees()));

class BladeOfLight(BossModule module) : BossComponent(module)
{
    public Actor? Blade => WorldState.Actors.FirstOrDefault(x => x.OID == 0x1EA19E && x.IsTargetable);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Blade != null)
            Arena.Actor(Blade, ArenaColor.Vulnerable);
    }
}

class Adds(BossModule module) : Components.AddsMulti(module, [0x17CE, 0x17CF, 0x17D0, 0x17D1]);
class Target(BossModule module) : BossComponent(module)
{
    private Actor? Knight => Module.Enemies(OID._Gen_KnightOfDarkness).FirstOrDefault();
    private Actor? Covered => WorldState.Actors.FirstOrDefault(s => s.FindStatus(SID._Gen_Invincibility) != null);
    private Actor? BladeOfLight => WorldState.Actors.FirstOrDefault(s => s.OID == 0x1EA19E && s.IsTargetable);

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Knight != null && Covered != null)
            Arena.AddLine(Knight.Position, Covered.Position, ArenaColor.Danger, 1);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (BladeOfLight != null)
        {
            var playerIsAttacked = false;

            foreach (var e in hints.PotentialTargets)
            {
                if (e.Actor.TargetID == actor.InstanceID)
                {
                    playerIsAttacked = true;
                    e.Priority = 0;
                }
                else
                {
                    e.Priority = -1;
                }
            }

            if (!playerIsAttacked)
            {
                if (actor.DistanceToHitbox(BladeOfLight) > 5.5f)
                    hints.ForcedMovement = (BladeOfLight!.Position - actor.Position).ToVec3();
                else
                    hints.InteractWithTarget = BladeOfLight;
            }
        }
        else
        {
            foreach (var e in hints.PotentialTargets)
            {
                if (e.Actor == Knight)
                    e.Priority = 2;
                else if (e.Actor == Covered)
                    e.Priority = 0;
                else
                    e.Priority = 1;
            }
        }
    }
}

class UtterDestruction(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_UtterDestruction), new AOEShapeDonut(10, 20));

class WarriorOfDarknessStates : StateMachineBuilder
{
    public WarriorOfDarknessStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Overpower>()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<Target>()
            .ActivateOnEnter<UnlitCyclone>()
            .ActivateOnEnter<UnlitCyclone2>()
            .ActivateOnEnter<Skydrive>()
            .ActivateOnEnter<SkydrivePuddle>()
            .ActivateOnEnter<BladeOfLight>()
            .ActivateOnEnter<RollingBlade>()
            .ActivateOnEnter<RollingBlade2>()
            .ActivateOnEnter<UtterDestruction>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 194, NameID = 5240)]
public class WarriorOfDarkness(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(20));
