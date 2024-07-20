namespace BossMod.ClassShared;

public enum AID : uint
{
    None = 0,
    Sprint = 3,

    // Tank
    ShieldWall = 197, // LB1, instant, range 0, AOE 50 circle, targets=self, animLock=1.930
    Stronghold = 198, // LB2, instant, range 0, AOE 50 circle, targets=self, animLock=3.860
    Rampart = 7531, // L8, instant, 90.0s CD (group 46), range 0, single-target, targets=self
    LowBlow = 7540, // L12, instant, 25.0s CD (group 41), range 3, single-target, targets=hostile
    Provoke = 7533, // L15, instant, 30.0s CD (group 42), range 25, single-target, targets=hostile
    Interject = 7538, // L18, instant, 30.0s CD (group 43), range 3, single-target, targets=hostile
    Reprisal = 7535, // L22, instant, 60.0s CD (group 44), range 0, AOE 5 circle, targets=self
    Shirk = 7537, // L48, instant, 120.0s CD (group 49), range 25, single-target, targets=party

    // Healer
    HealingWind = 206, // LB1, 2.0s cast, range 0, AOE 50 circle, targets=self, castAnimLock=2.100
    BreathOfTheEarth = 207, // LB2, 2.0s cast, range 0, AOE 50 circle, targets=self, castAnimLock=5.130
    Repose = 16560, // L8, 2.5s cast, GCD, range 30, single-target, targets=hostile
    Esuna = 7568, // L10, 1.0s cast, GCD, range 30, single-target, targets=self/party/alliance/friendly
    Rescue = 7571, // L48, instant, 120.0s CD (group 49), range 30, single-target, targets=party

    // Melee
    Braver = 200, // LB1, 2.0s cast, range 8, single-target, targets=hostile, castAnimLock=3.860
    Bladedance = 201, // LB2, 3.0s cast, range 8, single-target, targets=hostile, castAnimLock=3.860
    LegSweep = 7863, // L10, instant, 40.0s CD (group 41), range 3, single-target, targets=hostile
    Bloodbath = 7542, // L12, instant, 90.0s CD (group 46), range 0, single-target, targets=self
    Feint = 7549, // L22, instant, 90.0s CD (group 47), range 10, single-target, targets=hostile
    TrueNorth = 7546, // L50, instant, 45.0s CD (group 45/50) (2 charges), range 0, single-target, targets=self

    // PhysRanged
    BigShot = 4238, // LB1, 2.0s cast, range 30, AOE 30+R width 4 rect, targets=hostile, castAnimLock=3.100
    Desperado = 4239, // LB2, 3.0s cast, range 30, AOE 30+R width 5 rect, targets=hostile, castAnimLock=3.100
    LegGraze = 7554, // L6, instant, 30.0s CD (group 42), range 25, single-target, targets=hostile
    FootGraze = 7553, // L10, instant, 30.0s CD (group 41), range 25, single-target, targets=hostile
    Peloton = 7557, // L20, instant, 5.0s CD (group 40), range 0, AOE 30 circle, targets=self
    HeadGraze = 7551, // L24, instant, 30.0s CD (group 43), range 25, single-target, targets=hostile

    // Caster
    Skyshard = 203, // LB1, 2.0s cast, range 25, AOE 8 circle, targets=area, castAnimLock=3.100
    Starstorm = 204, // LB2, 3.0s cast, range 25, AOE 10 circle, targets=area, castAnimLock=5.100
    Addle = 7560, // L8 BLM/SMN/RDM/BLU, instant, 90.0s CD (group 46), range 25, single-target, targets=hostile
    Sleep = 25880, // L10 BLM/SMN/RDM/BLU, 2.5s cast, GCD, range 30, AOE 5 circle, targets=hostile

    // Multi-role actions
    SecondWind = 7541, // L8 MNK/DRG/BRD/NIN/MCH/SAM/DNC/RPR, instant, 120.0s CD (group 49), range 0, single-target, targets=self
    LucidDreaming = 7562, // L14 WHM/BLM/SMN/SCH/AST/RDM/BLU/SGE, instant, 60.0s CD (group 45), range 0, single-target, targets=self
    Swiftcast = 7561, // L18 WHM/BLM/SMN/SCH/AST/RDM/BLU/SGE, instant, 60.0s CD (group 44), range 0, single-target, targets=self
    ArmsLength = 7548, // L32 PLD/MNK/WAR/DRG/BRD/NIN/MCH/DRK/SAM/GNB/DNC/RPR, instant, 120.0s CD (group 48), range 0, single-target, targets=self
    Surecast = 7559, // L44 WHM/BLM/SMN/SCH/AST/RDM/BLU/SGE, instant, 120.0s CD (group 48), range 0, single-target, targets=self

    // Misc
    Resurrection = 173, // L12 SMN/SCH, 8.0s cast, GCD, range 30, single-target, targets=party/alliance/friendly
}

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.Sprint);

        // Tank
        d.RegisterSpell(AID.ShieldWall, instantAnimLock: 1.93f);
        d.RegisterSpell(AID.Stronghold, instantAnimLock: 3.86f);
        d.RegisterSpell(AID.Rampart);
        d.RegisterSpell(AID.LowBlow);
        d.RegisterSpell(AID.Provoke);
        d.RegisterSpell(AID.Interject);
        d.RegisterSpell(AID.Reprisal);
        d.RegisterSpell(AID.Shirk);

        // Healer
        d.RegisterSpell(AID.HealingWind, castAnimLock: 2.10f);
        d.RegisterSpell(AID.BreathOfTheEarth, castAnimLock: 5.13f);
        d.RegisterSpell(AID.Repose); // animLock=???
        d.RegisterSpell(AID.Esuna);
        d.RegisterSpell(AID.Rescue);

        // Melee
        d.RegisterSpell(AID.Braver, castAnimLock: 3.86f);
        d.RegisterSpell(AID.Bladedance, castAnimLock: 3.86f);
        d.RegisterSpell(AID.LegSweep);
        d.RegisterSpell(AID.Bloodbath);
        d.RegisterSpell(AID.Feint);
        d.RegisterSpell(AID.TrueNorth);

        // PhysRanged
        d.RegisterSpell(AID.BigShot, true, castAnimLock: 3.10f);
        d.RegisterSpell(AID.Desperado, true, castAnimLock: 3.10f);
        d.RegisterSpell(AID.LegGraze, true);
        d.RegisterSpell(AID.FootGraze, true);
        d.RegisterSpell(AID.Peloton, true);
        d.RegisterSpell(AID.HeadGraze, true);

        // Caster
        d.RegisterSpell(AID.Skyshard, castAnimLock: 3.10f);
        d.RegisterSpell(AID.Starstorm, castAnimLock: 5.10f);
        d.RegisterSpell(AID.Addle);
        d.RegisterSpell(AID.Sleep); // animLock=???

        // Multi-role actions
        d.RegisterSpell(AID.SecondWind);
        d.RegisterSpell(AID.LucidDreaming);
        d.RegisterSpell(AID.Swiftcast);
        d.RegisterSpell(AID.ArmsLength);
        d.RegisterSpell(AID.Surecast);

        // Misc
        d.RegisterSpell(AID.Resurrection);

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        d.Spell(AID.Interject)!.ForbidExecute = (_, _, target, _) => !(target?.CastInfo?.Interruptible ?? false); // don't use interject if target is not casting interruptible spell
        d.Spell(AID.Reprisal)!.ForbidExecute = (_, player, _, hints) => !hints.PotentialTargets.Any(e => e.Actor.Position.InCircle(player.Position, 5 + e.Actor.HitboxRadius)); // don't use reprisal if no one would be hit; TODO: consider checking only target?..
        d.Spell(AID.Shirk)!.SmartTarget = ActionDefinitions.SmartTargetCoTank;

        //d.Spell(AID.Repose)!.EffectDuration = 30;

        //d.Spell(AID.LegSweep)!.EffectDuration = 3;
        //d.Spell(AID.Bloodbath)!.EffectDuration = 20;
        //d.Spell(AID.Feint)!.EffectDuration = 10;
        //d.Spell(AID.TrueNorth)!.EffectDuration = 10;

        d.Spell(AID.Peloton)!.ForbidExecute = (_, player, _, _) => player.InCombat;
        d.Spell(AID.HeadGraze)!.ForbidExecute = (_, _, target, _) => !(target?.CastInfo?.Interruptible ?? false);

        //d.Spell(AID.Addle)!.EffectDuration = 10;
        //d.Spell(AID.Sleep)!.EffectDuration = 30;

        //d.Spell(AID.LucidDreaming)!.EffectDuration = 21;
        //d.Spell(AID.Swiftcast)!.EffectDuration = 10;
        //d.Spell(AID.Surecast)!.EffectDuration = 6;
    }
}
