namespace BossMod.Endwalker.Savage.P12S1Athena;

class Dialogos : Components.UniformStackSpread
{
    public enum Type { None, TankOutPartyIn, TankInPartyOut }

    public int NumCasts { get; private set; } // first is always tank, second is stack
    private Type _type;
    private DateTime _tankActivation; // party activation is +1s

    public Dialogos() : base(6, 6, 7, alwaysShowSpreads: true) { }

    public override void Update(BossModule module)
    {
        Stacks.Clear();
        Spreads.Clear();
        if (_type != Type.None && NumCasts < 2)
        {
            var closest = module.Raid.WithoutSlot().Closest(module.PrimaryActor.Position);
            var farthest = module.Raid.WithoutSlot().Farthest(module.PrimaryActor.Position);
            if (closest != null && farthest != null)
            {
                if (NumCasts == 0)
                    AddSpread(_type == Type.TankOutPartyIn ? farthest : closest, _tankActivation);
                if (NumCasts <= 1)
                    AddStack(_type == Type.TankOutPartyIn ? closest : farthest, _tankActivation.AddSeconds(1));
            }
        }
        base.Update(module);
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        base.AddHints(module, slot, actor, hints, movementHints);
        if (Spreads.Count > 0 && Spreads[0].Target.Role != Role.Tank)
        {
            if (Spreads[0].Target == actor)
                hints.Add(_type == Type.TankOutPartyIn ? "Move closer!" : "Move farther!");
            else if (actor.Role == Role.Tank)
                hints.Add(_type == Type.TankOutPartyIn ? "Move farther!" : "Move closer!");
        }
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (_type != Type.None && NumCasts < 2)
            hints.Add(_type == Type.TankOutPartyIn ? "Tank out, party in" : "Tank in, party out");
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        var type = (AID)spell.Action.ID switch
        {
            AID.Apodialogos => Type.TankOutPartyIn,
            AID.Peridialogos => Type.TankInPartyOut,
            _ => Type.None
        };
        if (type != Type.None)
        {
            _type = type;
            _tankActivation = spell.NPCFinishAt.AddSeconds(0.2);
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ApodialogosAOE or AID.PeridialogosAOE or AID.Dialogos)
            ++NumCasts;
    }
}
