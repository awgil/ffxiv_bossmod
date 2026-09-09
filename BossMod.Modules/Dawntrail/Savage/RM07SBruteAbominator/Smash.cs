namespace BossMod.Dawntrail.Savage.RM07SBruteAbominator;

class P1Stoneringer(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? NextAOE;

    public bool Risky;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.P1StoneringerClub:
                NextAOE = new(new AOEShapeCircle(12), caster.Position, default, Module.CastFinishAt(spell, 9.8f));
                break;
            case AID.P1StoneringerSword:
                NextAOE = new(new AOEShapeDonut(9, 60), caster.Position, default, Module.CastFinishAt(spell, 9.8f));
                break;
            case AID.P1BrutishSwingOut:
            case AID.P1BrutishSwingIn:
                Risky = true;
                NextAOE = NextAOE!.Value with { Activation = Module.CastFinishAt(spell) };
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.P1BrutishSwingOut or AID.P1BrutishSwingIn)
        {
            NumCasts++;
            NextAOE = null;
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(NextAOE).Select(a => a with { Risky = Risky, Origin = Module.PrimaryActor.Position });
}

class P1Smash(BossModule module) : Components.GenericSharedTankbuster(module, default, 6)
{
    enum Smash
    {
        None,
        Close,
        Far
    }

    private Smash NextSmash;

    public override void Update()
    {
        switch (NextSmash)
        {
            case Smash.None:
                Source = null;
                Target = null;
                break;
            case Smash.Close:
                Source = Module.PrimaryActor;
                Target = Raid.WithoutSlot().Closest(Source.Position);
                break;
            case Smash.Far:
                Source = Module.PrimaryActor;
                Target = Raid.WithoutSlot().Farthest(Source.Position);
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.SmashHere:
                NextSmash = Smash.Close;
                break;
            case AID.SmashThere:
                NextSmash = Smash.Far;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BrutalSmash1 or AID.BrutalSmash2)
        {
            NumCasts++;
            NextSmash = Smash.None;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (NextSmash != Smash.None)
            hints.Add(NextSmash == Smash.Close ? "Tankbuster close" : "Tankbuster far");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Target == actor && actor.Role != Role.Tank)
            hints.Add(NextSmash == Smash.Close ? "Go far from boss!" : "Go close to boss!");
        else
            base.AddHints(slot, actor, hints);
    }
}
