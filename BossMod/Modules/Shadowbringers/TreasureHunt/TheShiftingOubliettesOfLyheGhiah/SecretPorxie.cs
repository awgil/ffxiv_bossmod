namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretPorxie;

public enum OID : uint
{
    Boss = 0x3014, //R=1.2
    BossHelper = 0x233C,
    MagickedBroomHelper = 0x310A, // R=0.5
    MagickedBroom1 = 0x30F3, // R=3.125
    MagickedBroom2 = 0x30F2, // R=3.125
    MagickedBroom3 = 0x30F4, // R=3.125
    MagickedBroom4 = 0x30F1, // R=3.125
    MagickedBroom5 = 0x3015, // R=3.125
    MagickedBroom6 = 0x30F0, // R=3.125
    BonusAddKeeperOfKeys = 0x3034, // R3.230
    SecretQueen = 0x3021, // R0.840, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    SecretGarlic = 0x301F, // R0.840, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    SecretTomato = 0x3020, // R0.840, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    SecretOnion = 0x301D, // R0.840, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    SecretEgg = 0x301E, // R0.840, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/BonusAddKeeperOfKeys->player/Mandragoras, no cast, single-target
    BrewingStorm = 21670, // Boss->self, 2.5s cast, range 5 60-degree cone, knockback 10 away from source
    HarrowingDream = 21671, // Boss->self, 3.0s cast, range 6 circle, applies sleep
    BecloudingDust = 22935, // Boss->self, 3.0s cast, single-target
    BecloudingDust2 = 22936, // BossHelper->location, 3.0s cast, range 6 circle
    SweepStart = 22937, // 30F4/30F3/30F2/30F1/3015/30F0->self, 4.0s cast, range 6 circle
    SweepRest = 21672, // 30F4/30F3/30F2/30F1/3015/30F0->self, no cast, range 6 circle
    SweepVisual = 22508, // 30F4/30F3/30F2/30F1/3015/30F0->self, no cast, single-target, visual
    SweepVisual2 = 22509, // 30F4/30F3/30F2/30F1/3015/30F0->self, no cast, single-target

    Telega = 9630, // BonusAdds->self, no cast, single-target, bonus adds disappear
    Mash = 21767, // 3034->self, 3.0s cast, range 13 width 4 rect
    Inhale = 21770, // 3034->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
    Spin = 21769, // 3034->self, 4.0s cast, range 11 circle
    Scoop = 21768, // 3034->self, 4.0s cast, range 15 120-degree cone
    Pollen = 6452, // 2A0A->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // 2A06->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // 2A09->self, 3.5s cast, range 6+R circle
    PluckAndPrune = 6449, // 2A07->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // 2A08->self, 3.5s cast, range 6+R circle
}

class BrewingStorm(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BrewingStorm), new AOEShapeCone(5, 30.Degrees()));
class HarrowingDream(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HarrowingDream), new AOEShapeCircle(6));
class BecloudingDust(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.BecloudingDust2), 6);

class Sweep(BossModule module) : Components.Exaflare(module, 6)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SweepStart)
            Lines.Add(new() { Next = caster.Position, Advance = 12 * spell.Rotation.ToDirection(), NextExplosion = spell.NPCFinishAt.AddSeconds(0.9f), TimeToMove = 4.5f, ExplosionsLeft = 4, MaxShownExplosions = 3 });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Lines.Count > 0 && (AID)spell.Action.ID == AID.SweepRest)
        {
            int index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1));
            AdvanceLine(Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}

class Spin(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Spin), new AOEShapeCircle(11));
class Mash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Mash), new AOEShapeRect(13, 2));
class Scoop(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Scoop), new AOEShapeCone(15, 60.Degrees()));
class PluckAndPrune(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PluckAndPrune), new AOEShapeCircle(6.84f));
class TearyTwirl(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TearyTwirl), new AOEShapeCircle(6.84f));
class HeirloomScream(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HeirloomScream), new AOEShapeCircle(6.84f));
class PungentPirouette(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PungentPirouette), new AOEShapeCircle(6.84f));
class Pollen(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Pollen), new AOEShapeCircle(6.84f));

class PorxieStates : StateMachineBuilder
{
    public PorxieStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BrewingStorm>()
            .ActivateOnEnter<HarrowingDream>()
            .ActivateOnEnter<BecloudingDust>()
            .ActivateOnEnter<Sweep>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Scoop>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BonusAddKeeperOfKeys).All(e => e.IsDead) && module.Enemies(OID.SecretEgg).All(e => e.IsDead) && module.Enemies(OID.SecretQueen).All(e => e.IsDead) && module.Enemies(OID.SecretOnion).All(e => e.IsDead) && module.Enemies(OID.SecretGarlic).All(e => e.IsDead) && module.Enemies(OID.SecretTomato).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 745, NameID = 9795)]
public class Porxie(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(19))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.BonusAddKeeperOfKeys))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.SecretEgg))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.SecretTomato))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.SecretQueen))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.SecretGarlic))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.SecretOnion))
            Arena.Actor(s, ArenaColor.Vulnerable);
    }

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.SecretOnion => 6,
                OID.SecretEgg => 5,
                OID.SecretGarlic => 4,
                OID.SecretTomato => 3,
                OID.BonusAddKeeperOfKeys or OID.SecretQueen => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
