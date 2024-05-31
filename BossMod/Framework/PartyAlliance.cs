using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.Interop;

namespace BossMod;

// similar to dalamud's PartyList, except that it works with alliances properly
class PartyAlliance
{
    public static int MaxAllianceMembers = 20;

    private readonly unsafe GroupManager* _groupManager = GroupManager.Instance();

    public unsafe int NumPartyMembers => _groupManager->MemberCount;
    public unsafe bool IsAlliance => (_groupManager->AllianceFlags & 1) != 0;
    public unsafe bool IsSmallGroupAlliance => (_groupManager->AllianceFlags & 2) != 0; // alliance containing 6 groups of 4 members rather than 3x8

    public unsafe PartyMember* PartyMember(int index) => (index >= 0 && index < NumPartyMembers) ? _groupManager->PartyMembers.GetPointer(index) : null;
    public unsafe PartyMember* AllianceMember(int rawIndex) => (rawIndex is >= 0 and < 20) ? AllianceMemberIfValid(rawIndex) : null;
    public unsafe PartyMember* AllianceMember(int group, int index)
    {
        if (IsSmallGroupAlliance)
            return group is >= 0 and < 5 && index is >= 0 and < 4 ? AllianceMemberIfValid(4 * group + index) : null;
        else
            return group is >= 0 and < 2 && index is >= 0 and < 8 ? AllianceMemberIfValid(8 * group + index) : null;
    }

    public unsafe PartyMember* FindPartyMember(ulong contentID)
    {
        for (int i = 0; i < NumPartyMembers; ++i)
        {
            var m = _groupManager->PartyMembers.GetPointer(i);
            if ((ulong)m->ContentId == contentID)
                return m;
        }
        return null;
    }

    private unsafe PartyMember* AllianceMemberIfValid(int rawIndex)
    {
        var p = _groupManager->AllianceMembers.GetPointer(rawIndex);
        return (p->Flags & 1) != 0 ? p : null;
    }
}
