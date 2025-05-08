namespace BossMod.Endwalker.Savage.P12S2PallasAthena;

class EkpyrosisProximityV(BossModule module) : Components.StandardAOEs(module, AID.EkpyrosisProximityV, 19); // TODO: verify falloff
class EkpyrosisProximityH(BossModule module) : Components.StandardAOEs(module, AID.EkpyrosisProximityH, new AOEShapeCircle(19)); // TODO: verify falloff

class EkpyrosisExaflare(BossModule module) : Components.Exaflare(module, 6)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.EkpyrosisExaflareFirst)
        {
            Lines.Add(new() { Next = caster.Position, Advance = 8 * spell.Rotation.ToDirection(), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 2.1f, ExplosionsLeft = 5, MaxShownExplosions = 2 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.EkpyrosisExaflareFirst or AID.EkpyrosisExaflareRest)
        {
            ++NumCasts;
            int index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1));
            if (index == -1)
            {
                ReportError($"Failed to find entry for {caster.InstanceID:X}");
                return;
            }

            AdvanceLine(Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}

class EkpyrosisSpread : Components.UniformStackSpread
{
    public EkpyrosisSpread(BossModule module) : base(module, 0, 6)
    {
        foreach (var p in Raid.WithoutSlot(true))
            AddSpread(p, module.StateMachine.NextTransitionWithFlag(StateMachine.StateHint.Raidwide));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.EkpyrosisSpread)
            Spreads.Clear();
    }
}
