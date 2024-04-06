namespace BossMod.Global.MaskedCarnivale.Stage26.Act1;

public enum OID : uint
{
    Boss = 0x2C84, //R=2.55
    Helper = 0x233C,
};

public enum AID : uint
{
    AutoAttack = 6498, // 2C84->player, no cast, single-target
    AlternatePlumage = 18686, // 2C84->self, 3,0s cast, single-target, armor up, needs dispel
    RuffledFeathers = 18685, // 2C84->player, no cast, single-target
    Gust = 18687, // 2C84->location, 2,5s cast, range 3 circle
    CaberToss = 18688, // 2C84->player, 5,0s cast, single-target, interrupt or wipe
};

public enum SID : uint
{
    VulnerabilityDown = 63, // Boss->Boss, extra=0x0
    Windburn = 269, // Boss->player, extra=0x0

};


class Gust : Components.LocationTargetedAOEs
{
    public Gust() : base(ActionID.MakeSpell(AID.Gust), 3) { }
}

class AlternatePlumage : Components.CastHint
{
    public AlternatePlumage() : base(ActionID.MakeSpell(AID.AlternatePlumage), "Prepare to dispel buff") { }
}

class CaberToss : Components.CastHint
{
    public CaberToss() : base(ActionID.MakeSpell(AID.CaberToss), "Interrupt or wipe!") { }
}

class Hints : BossComponent
{
    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        hints.Add($"{module.PrimaryActor.Name} will cast Alternate Plumage, which makes him almost\nimmune to damage. Use Eerie Soundwave to dispel it. Caber Toss must be\ninterrupted or you wipe.\nAdditionally Exuviation and earth spells are recommended for act 2.");
    }
}

class Hints2 : BossComponent
{
    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        var armorbuff = module.Enemies(OID.Boss).Where(x => x.FindStatus(SID.VulnerabilityDown) != null).FirstOrDefault();
        if (armorbuff != null)
            hints.Add($"Dispel {module.PrimaryActor.Name} with Eerie Soundwave!");
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        var windburn = actor.FindStatus(SID.Windburn);
        if (windburn != null)
            hints.Add("Windburn on you! Cleanse it with Exuviation.");
    }
}

class Stage26Act1States : StateMachineBuilder
{
    public Stage26Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CaberToss>()
            .ActivateOnEnter<Gust>()
            .ActivateOnEnter<AlternatePlumage>()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 695, NameID = 9230, SortOrder = 1)]
public class Stage26Act1 : BossModule
{
    public Stage26Act1(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
    {
        ActivateComponent<Hints>();
    }
}
