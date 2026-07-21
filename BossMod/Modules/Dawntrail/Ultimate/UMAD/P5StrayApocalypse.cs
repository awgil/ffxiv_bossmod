namespace BossMod.Dawntrail.Ultimate.UMAD;

class P5StrayApocalypse(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(6))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Ability_StrayApocalypse)
        {
            Lines.Add(new()
            {
                Next = caster.Position,
                Advance = caster.Rotation.ToDirection() * MathF.Sqrt(50),
                Rotation = caster.Rotation,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 1,
                ExplosionsLeft = 7,
                MaxShownExplosions = 4
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Ability_StrayApocalypse or AID._Ability_StrayApocalypse2)
        {
            NumCasts++;
            var ix = Lines.FindIndex(l => l.Next.AlmostEqual(caster.Position, 1));
            if (ix >= 0)
                AdvanceLine(Lines[ix], caster.Position);
        }
    }
}

class P5StrayEntropy(BossModule module) : Components.UniformStackSpread(module, 0, 5)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Ability_StrayEntropy)
            AddSpreads(Raid.WithoutSlot(), Module.CastFinishAt(spell, 0.9f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Ability_StrayEntropy1 && Spreads.Count > 0)
            Spreads.RemoveAt(0);
    }
}
