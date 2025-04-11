namespace BossMod.Dawntrail.Savage.RM06SSugarRiot;

class ColorRiot(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private readonly List<Actor> Casters = [];

    private bool IceClose;
    private DateTime Activation;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ColorRiotIceClose:
                Casters.Add(caster);
                Activation = Module.CastFinishAt(spell, 2.2f);
                IceClose = true;
                break;
            case AID.ColorRiotFireClose:
                Casters.Add(caster);
                Activation = Module.CastFinishAt(spell, 2.2f);
                IceClose = false;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CoolBomb or AID.WarmBomb)
        {
            NumCasts++;
            Casters.Clear();
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();
        if (Casters.Count == 0)
            return;

        AddBait(Raid.WithoutSlot().Closest(Casters[0].Position));
        AddBait(Raid.WithoutSlot().Farthest(Casters[0].Position));
    }

    enum Preference { None, Close, Far }

    private Preference GetPreference(Actor actor)
    {
        if (Casters.Count == 0)
            return Preference.None;

        if (actor.FindStatus(SID.CoolTint) != null)
            return IceClose ? Preference.Far : Preference.Close;
        if (actor.FindStatus(SID.WarmTint) != null)
            return IceClose ? Preference.Close : Preference.Far;

        return Preference.None;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        switch (GetPreference(actor))
        {
            case Preference.Close:
                hints.Add("Bait close!", Raid.WithoutSlot().Closest(Casters[0].Position) != actor);
                break;
            case Preference.Far:
                hints.Add("Bait far!", Raid.WithoutSlot().Farthest(Casters[0].Position) != actor);
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Casters.Count == 0)
            return;

        switch (GetPreference(actor))
        {
            case Preference.Close:
                var closest = Raid.WithoutSlot().Exclude(actor).Closest(Casters[0].Position)!;
                hints.AddForbiddenZone(ShapeContains.Donut(Casters[0].Position, closest.DistanceToPoint(Casters[0].Position), 100), CurrentBaits[0].Activation);
                break;
            case Preference.Far:
                var farthest = Raid.WithoutSlot().Exclude(actor).Farthest(Casters[0].Position)!;
                hints.AddForbiddenZone(ShapeContains.Circle(Casters[0].Position, farthest.DistanceToPoint(Casters[0].Position)), CurrentBaits[0].Activation);
                break;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Casters.Count > 0)
            hints.Add(IceClose ? "Ice close, fire far" : "Ice far, fire close");
    }

    private void AddBait(Actor? target)
    {
        if (target != null)
            CurrentBaits.Add(new(Casters[0], target, new AOEShapeCircle(4), Activation));
    }
}
