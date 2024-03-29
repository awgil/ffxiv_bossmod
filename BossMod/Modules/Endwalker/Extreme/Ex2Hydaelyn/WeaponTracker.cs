namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

// component tracking boss weapon/stance switches; also draws imminent aoe after each switch
// note: we could rely on invisible buff 2273 to select weapon (n/a for sword, 1B4 for staff, 1B5 for chakram), it appears slightly earlier than 'official' buff
class WeaponTracker : BossComponent
{
    public enum Stance { None, Sword, Staff, Chakram }
    public Stance CurStance { get; private set; }
    public bool AOEImminent { get; private set; }

    private static readonly AOEShapeRect _aoeSword = new(20, 5, 20);
    private static readonly AOEShapeCircle _aoeStaff = new(10);
    private static readonly AOEShapeDonut _aoeChakram = new(5, 40);

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (!AOEImminent)
            return;

        bool inAOE = CurStance switch
        {
            Stance.Sword => _aoeSword.Check(actor.Position, module.PrimaryActor.Position, 0.Degrees()) || _aoeSword.Check(actor.Position, module.PrimaryActor.Position, 90.Degrees()),
            Stance.Staff => _aoeStaff.Check(actor.Position, module.PrimaryActor.Position),
            Stance.Chakram => _aoeChakram.Check(actor.Position, module.PrimaryActor.Position),
            _ => false
        };
        if (inAOE)
            hints.Add("GTFO from weapon aoe!");
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (!AOEImminent)
            return;

        switch (CurStance)
        {
            case Stance.Sword:
                _aoeSword.Draw(arena, module.PrimaryActor.Position, 0.Degrees());
                _aoeSword.Draw(arena, module.PrimaryActor.Position, 90.Degrees());
                break;
            case Stance.Staff:
                _aoeStaff.Draw(arena, module.PrimaryActor.Position);
                break;
            case Stance.Chakram:
                _aoeChakram.Draw(arena, module.PrimaryActor.Position);
                break;
        }
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if (actor != module.PrimaryActor)
            return;

        var newStance = (SID)status.ID switch
        {
            SID.HerosMantle => Stance.Sword,
            SID.MagosMantle => Stance.Staff,
            SID.MousaMantle => Stance.Chakram,
            _ => Stance.None
        };

        if (newStance == Stance.None || newStance == CurStance)
            return;

        AOEImminent = CurStance != Stance.None;
        CurStance = newStance;
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WeaponChangeAOEChakram or AID.WeaponChangeAOEStaff or AID.WeaponChangeAOESword)
            AOEImminent = false;
    }
}
