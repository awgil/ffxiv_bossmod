namespace BossMod.Dawntrail.Ultimate.UMAD;

class P1FlagrantFireIII(BossModule module) : Components.UniformStackSpread(module, 6, 5, 4, 4)
{
    readonly UMADConfig _config = Service.Config.Get<UMADConfig>();

    enum Mechanic { None, Stack, Spread }
    enum Lying { Unsure, No, Yes }

    public int Iteration; // 0 for first mech, 1 for arrows

    Mechanic _displayed;
    Lying _lying;

    BitMask _stackTargets; // empty if boss is lying or if the displayed mechanic is spread
    bool _blizzardHappened;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID.FireSpread:
                _displayed = Mechanic.Spread;
                Init();
                break;
            case IconID.FireStack:
                _stackTargets.Set(Raid.FindSlot(targetID));
                _displayed = Mechanic.Stack;
                Init();
                break;
            case IconID.MysteryMagicFireLie:
                _lying = Lying.Yes;
                Init();
                break;
            case IconID.MysteryMagicFireTruth:
                _lying = Lying.No;
                Init();
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FlagrantFireIIIStack:
                Stacks.Clear();
                break;
            case AID.FlagrantFireIIISpread:
                Spreads.Clear();
                break;
            case AID.BlizzardIIIBlowout1:
            case AID.BlizzardIIIBlowout2:
                _blizzardHappened = true;
                break;
        }
    }

    void Init()
    {
        if (Stacks.Count > 0 || Spreads.Count > 0)
            return;

        switch ((_displayed, _lying))
        {
            case (Mechanic.Stack, Lying.Yes):
            case (Mechanic.Spread, Lying.No):
                AddSpreads();
                break;
            case (Mechanic.Stack, Lying.No):
            case (Mechanic.Spread, Lying.Yes):
                AddStacks();
                break;
        }
    }

    void AddStacks()
    {
        // wait for other stack target to be telegraphed
        if (_stackTargets.NumSetBits() == 1)
            return;

        BitMask maskLeft = new(), maskRight = new();
        foreach (var (slot, group) in _config.P1WaveCannonConga.Resolve(Raid))
        {
            if (group < 4)
                maskLeft.Set(slot);
            else
                maskRight.Set(slot);
        }

        void addMasked(int slot, Actor target)
        {
            var allowedMask = maskLeft[slot] ? maskLeft : maskRight[slot] ? maskRight : default;
            AddStack(target, WorldState.FutureTime(5.8f), allowedMask.Any() ? ~allowedMask : default);
        }

        foreach (var (slot, knownTarget) in Raid.WithSlot().IncludedInMask(_stackTargets))
            addMasked(slot, knownTarget);

        if (Stacks.Count == 0)
        {
            var partySorted = Raid.WithSlot().OrderBy(r => r.Item2.Class.IsDD()).ToList();
            addMasked(partySorted[0].Item1, partySorted[0].Item2);
            addMasked(partySorted[^1].Item1, partySorted[^1].Item2);
        }
    }

    void AddSpreads()
    {
        foreach (var player in Raid.WithoutSlot())
            AddSpread(player, WorldState.FutureTime(5.8f));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Iteration == 0)
            AddGraven1Hints(slot, actor, assignment, hints);

        if (Iteration == 1)
            AddTelePortentHints(slot, actor, assignment, hints);
    }

    void AddGraven1Hints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var myOrder = _config.P1WaveCannonConga[assignment];
        if (myOrder < 0 || _blizzardHappened)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            return;
        }

        var (isKB, kbAt) = Module.FindComponent<P1PulseWave>() is { } pw ? (pw.Targets[slot], pw.Activation) : (actor.PendingKnockbacks.Count > 0, default);

        WPos mySpot;

        var isSpread = Spreads.Count > 0;

        if (isSpread)
            // space players evenly-ish across the arena
            mySpot = Arena.Center + new WDir((myOrder - 3.5f) * 5, 0);
        else
            mySpot = Arena.Center + new WDir(myOrder > 3 ? 6 : -6, 0);

        var safeDirZ = 1;

        if (Module.FindComponent<P1BlizzardIIIBlowout>()?.Check(slot, actor, mySpot + new WDir(0, safeDirZ)) == true)
            safeDirZ = -safeDirZ;

        if (isSpread)
        {
            var bossDirX = myOrder > 3 ? -1 : 1;

            switch (myOrder)
            {
                // MT/M1 can move in horizontally to relative n/s to give other melees space
                case 3:
                case 4:
                    mySpot += new WDir(1.5f * bossDirX, 8 * safeDirZ);
                    break;
                // OT/M2 can also move in horizontally, staying relative e/w
                case 2:
                case 5:
                    mySpot += new WDir(1.5f * bossDirX, safeDirZ);
                    break;
                // ranged 1s move further away from center line to give partner space
                case 1:
                case 6:
                    mySpot.Z += safeDirZ * 3;
                    break;
                default:
                    mySpot.Z += safeDirZ;
                    break;
            }
        }
        else
        {
            mySpot.Z += safeDirZ * 2;
        }

        if (isKB)
        {
            var dirToStatue = P1PulseWave.Origin - mySpot;
            // if KB destination is south, we can preposition exactly; if it's north, it might actually be outside the arena (for m1/r1)
            var factor = mySpot.Z > 100 ? 1 : 0.75f;
            mySpot += dirToStatue.Normalized() * P1PulseWave.Distance * factor;
        }

        hints.AddForbiddenZone(ShapeContains.InvertedCircle(mySpot, isSpread ? 1 : 4), kbAt);

    }

    void AddTelePortentHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (IsSpreadTarget(player))
            return PlayerPriority.Danger;
        if (IsStackTarget(player))
            return PlayerPriority.Interesting;
        return Active ? PlayerPriority.Normal : PlayerPriority.Irrelevant;
    }
}
