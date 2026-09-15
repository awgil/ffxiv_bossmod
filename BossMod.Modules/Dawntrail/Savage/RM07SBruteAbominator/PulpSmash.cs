namespace BossMod.Dawntrail.Savage.RM07SBruteAbominator;

class PulpSmash(BossModule module) : Components.StackWithIcon(module, (uint)IconID.PulpSmash, AID.PulpSmashStack, 6, 5.1f);

class PulpSmashProtean(BossModule module) : Components.GenericBaitAway(module)
{
    private bool Risky;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.PulpSmash)
        {
            foreach (var player in Raid.WithoutSlot().Exclude(actor))
                CurrentBaits.Add(new(actor, player, new AOEShapeCone(60, 15.Degrees()), WorldState.FutureTime(7.1f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.PulpSmashStack)
        {
            Risky = true;
            for (var i = 0; i < CurrentBaits.Count; i++)
                CurrentBaits.Ref(i).Source = Module.PrimaryActor;
            CurrentBaits.Add(new(Module.PrimaryActor, WorldState.Actors.Find(spell.MainTargetID)!, new AOEShapeCone(60, 15.Degrees()), CurrentBaits[0].Activation));
        }

        if ((AID)spell.Action.ID == AID.TheUnpotted)
        {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Risky)
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Risky)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class ItCameFromTheDirt(BossModule module) : Components.StandardAOEs(module, AID.ItCameFromTheDirt, 6);
