namespace BossMod.Endwalker.Savage.P12S1Athena;

class Dialogos(BossModule module) : Components.UniformStackSpread(module, 6, 6, 7, alwaysShowSpreads: true)
{
    public enum Type { None, TankOutPartyIn, TankInPartyOut }

    public int NumCasts { get; private set; } // first is always tank, second is stack
    private Type _type;
    private DateTime _tankActivation; // party activation is +1s

    public override void Update()
    {
        Stacks.Clear();
        Spreads.Clear();
        if (_type != Type.None && NumCasts < 2)
        {
            var closest = Raid.WithoutSlot().Closest(Module.PrimaryActor.Position);
            var farthest = Raid.WithoutSlot().Farthest(Module.PrimaryActor.Position);
            if (closest != null && farthest != null)
            {
                if (NumCasts == 0)
                    AddSpread(_type == Type.TankOutPartyIn ? farthest : closest, _tankActivation);
                if (NumCasts <= 1)
                    AddStack(_type == Type.TankOutPartyIn ? closest : farthest, _tankActivation.AddSeconds(1));
            }
        }
        base.Update();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (Spreads.Count > 0 && Spreads[0].Target.Role != Role.Tank)
        {
            if (Spreads[0].Target == actor)
                hints.Add(_type == Type.TankOutPartyIn ? "Move closer!" : "Move farther!");
            else if (actor.Role == Role.Tank)
                hints.Add(_type == Type.TankOutPartyIn ? "Move farther!" : "Move closer!");
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_type != Type.None && NumCasts < 2)
            hints.Add(_type == Type.TankOutPartyIn ? "Tank out, party in" : "Tank in, party out");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
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

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ApodialogosAOE or AID.PeridialogosAOE or AID.Dialogos)
            ++NumCasts;
    }
}
