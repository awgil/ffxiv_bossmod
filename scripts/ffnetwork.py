import ida_bytes
import ida_ida
import ida_search
import idaapi

packet_names = {}
next_packet_id = 0


def add_packet(name, skip_ids=0):
    global next_packet_id
    next_packet_id += skip_ids
    packet_names[next_packet_id] = name
    next_packet_id += 1


add_packet("Ping", 2)
add_packet("Init")  # 7.0: not sure
add_packet("Logout", 4)  # 7.0: not sure
add_packet("CFCancel", 2)  # 7.0: not sure
add_packet("CFDutyInfo", 1)  # 7.0: not sure
add_packet("CFNotify")  # 7.0: not sure
add_packet("CFPreferredRole", 3)  # 7.0: not sure
add_packet("CrossWorldLinkshellList", 62)  # 7.0: not sure
add_packet("FellowshipList", 7)  # 7.0: not sure
add_packet("Playtime", 21)  # 7.0: not sure
add_packet("CFRegistered")  # 7.0: not sure
add_packet("Chat", 2)  # 7.0: not sure
add_packet("RSVData", 12)
add_packet("RSFData")
add_packet("SocialMessage")  # not sure
add_packet("SocialMessage2")  # not sure
add_packet("SocialList", 1)  # not sure
add_packet("SocialRequestResponse")  # not sure
add_packet("ExamineSearchInfo")  # not sure
add_packet("UpdateSearchInfo")  # not sure
add_packet("InitSearchInfo")  # not sure
add_packet("ExamineSearchComment")  # not sure
add_packet("ServerNoticeShort", 2)  # not sure
add_packet("ServerNotice")  # not sure
add_packet("SetOnlineStatus")  # not sure
add_packet("LogMessage")  # not sure
add_packet("Countdown", 3)
add_packet("CountdownCancel")
add_packet("PartyMessage", 4)  # not sure
add_packet("PlayerAddedToBlacklist", 1)  # not sure
add_packet("PlayerRemovedFromBlacklist")  # not sure
add_packet("BlackList")  # not sure
add_packet("LinkshellList", 5)  # not sure
add_packet("MailDeleteRequest")  # not sure
add_packet("MarketBoardItemListingCount", 4)
add_packet("MarketBoardItemListing")
add_packet("PlayerRetainerInfo", 2) # this moved in 7.51
add_packet("MarketBoardPurchase")
add_packet("MarketBoardSale")
add_packet("MarketBoardItemListingHistory")
add_packet("RetainerSaleHistory")
add_packet("RetainerState")
add_packet("MarketBoardSearchResult")
add_packet("FreeCompanyInfo", 1)  # not sure
add_packet("ExamineFreeCompanyInfo", 1)  # not sure
add_packet("FreeCompanyDialog")  # not sure
add_packet("StatusEffectList", 25)
add_packet("StatusEffectListEureka")
add_packet("StatusEffectListBozja")
add_packet("StatusEffectListForay3")
add_packet("StatusEffectListDouble")
add_packet("EffectResult1", 1)
add_packet("EffectResult4")
add_packet("EffectResult8")
add_packet("EffectResult16")
add_packet("EffectResultBasic1", 1)
add_packet("EffectResultBasic4")
add_packet("EffectResultBasic8")
add_packet("EffectResultBasic16")
add_packet("EffectResultBasic32")
add_packet("EffectResultBasic64")
add_packet("ActorControl")
add_packet("ActorControlSelf")
add_packet("ActorControlTarget")
add_packet("UpdateHpMpTp")
add_packet("ActionEffect1")
add_packet("ActionEffect8", 2)
add_packet("ActionEffect16")
add_packet("ActionEffect24")
add_packet("ActionEffect32")
add_packet("StatusEffectListPlayer", 2)
add_packet("StatusEffectListPlayerDouble")
add_packet("UpdateRecastTimes", 1)
add_packet("UpdateDutyRecastTimes")  # old, 2 cdgroups
add_packet("UpdateDutyRecastTimes5")  # 5 cdgroups, 7.1 new foray?..
add_packet("UpdateAllianceNormal")
add_packet("UpdateAllianceSmall")
add_packet("UpdatePartyMemberPositions")
add_packet("UpdateAllianceNormalMemberPositions")
add_packet("UpdateAllianceSmallMemberPositions")
add_packet("GCAffiliation", 2)  # not sure
add_packet("SpawnPlayer", 17)
add_packet("SpawnNPC")
add_packet("SpawnBoss")
add_packet("DespawnCharacter")
add_packet("ActorMove")
add_packet("Transfer", 1)  # not sure
add_packet("ActorSetPos")
add_packet("ActorCast", 2)
add_packet("PlayerUpdateLook")  # not sure
add_packet("UpdateParty")
add_packet("InitZone")
add_packet("ApplyIDScramble")
add_packet("UpdateHate")
add_packet("UpdateHater")
add_packet("SpawnObject")
add_packet("DespawnObject")
add_packet("UpdateClassInfo")
add_packet("UpdateClassInfoEureka")
add_packet("UpdateClassInfoBozja")
add_packet("UpdateClassInfoForay3")
add_packet("PlayerSetup")
add_packet("PlayerStats")
add_packet("FirstAttack")
add_packet("PlayerStateFlags")
add_packet("PlayerClassInfo")
add_packet("PlayerBlueMageActions")
add_packet("ModelEquip")  # 7.1: no idea
add_packet("Examine")  # 7.1: no idea
add_packet("CharaNameReq", 2)  # 7.1: no idea
add_packet("RetainerSummary", 3)
add_packet("RetainerInformation")
add_packet("ItemMarketBoardSummary")
add_packet("ItemMarketBoardInfo")
add_packet("ItemInfo", 1)
add_packet("ContainerInfo")
add_packet("InventoryTransactionFinish")  # not sure
add_packet("InventoryTransaction")  # not sure
add_packet("CurrencyCrystalInfo")  # not sure
add_packet("InventoryActionAck", 1)  # not sure
add_packet("UpdateInventorySlot")  # not sure
add_packet("OpenTreasure", 1)  # not sure
add_packet("LootMessage", 2)  # not sure
add_packet("CreateTreasure", 3)  # not sure
add_packet("TreasureFadeOut")  # not sure
add_packet("HuntingLogEntry")  # not sure
add_packet("EventPlay", 1)
add_packet("EventPlay4")
add_packet("EventPlay8")
add_packet("EventPlay16")
add_packet("EventPlay32")
add_packet("EventPlay64")
add_packet("EventPlay128")
add_packet("EventPlay255")
add_packet("EventStart", 1)  # not sure
add_packet("EventFinish")  # not sure
add_packet("EventContinue", 10)  # not sure
add_packet("ResultDialog", 1)  # not sure
add_packet("DesynthResult")  # not sure
add_packet("QuestActiveList", 4)  # not sure
add_packet("QuestUpdate")  # not sure
add_packet("QuestCompleteList")  # not sure
add_packet("QuestFinish")  # not sure
add_packet("MSQTrackerComplete", 2)  # not sure
add_packet("QuestTracker", 1)  # not sure
add_packet("Mount")  # not sure
add_packet("DirectorVars", 1)  # not sure
add_packet("ContentDirectorSync")  # not sure
add_packet("ServerRequestCallbackResponse1", 7)
add_packet("ServerRequestCallbackResponse2")
add_packet("ServerRequestCallbackResponse3")
add_packet("MapEffect1", 21)
add_packet("MapEffect2")
add_packet("MapEffect3")
add_packet("MapEffect4")
add_packet("MapEffect5")
add_packet("MapEffect6")
add_packet("SystemLogMessage1", 2)
add_packet("SystemLogMessage2")
add_packet("SystemLogMessage4")
add_packet("SystemLogMessage8")
add_packet("SystemLogMessage16")
add_packet("BattleTalk2", 1)  # not sure
add_packet("BattleTalk4")  # not sure
add_packet("BattleTalk8")  # not sure
add_packet("MapUpdate", 1)  # not sure
add_packet("MapUpdate4")  # not sure
add_packet("MapUpdate8")  # not sure
add_packet("MapUpdate16")  # not sure
add_packet("MapUpdate32")  # not sure
add_packet("MapUpdate64")  # not sure
add_packet("MapUpdate128")  # not sure
add_packet("BalloonTalk2", 1)  # not sure
add_packet("BalloonTalk4")  # not sure
add_packet("BalloonTalk8")  # not sure
add_packet("WeatherChange", 1)  # not sure
add_packet("PlayerTitleList")  # not sure
add_packet("Discovery")  # not sure
add_packet("EorzeaTimeOffset", 1)  # not sure
add_packet("EquipDisplayFlags", 12)  # not sure
add_packet("NpcYell")
add_packet("FateInfo", 4)  # not sure
add_packet("CompletedAchievements", 4)
add_packet("LandSetInitialize", 8)  # not sure
add_packet("LandUpdate")  # not sure
add_packet("YardObjectSpawn")  # not sure
add_packet("HousingIndoorInitialize")  # not sure
add_packet("LandAvailability")  # not sure
add_packet("LandPriceUpdate", 1)  # not sure
add_packet("LandInfoSign")  # not sure
add_packet("LandRename")  # not sure
add_packet("HousingEstateGreeting")  # not sure
add_packet("HousingUpdateLandFlagsSlot")  # not sure
add_packet("HousingLandFlags")  # not sure
add_packet("HousingShowEstateGuestAccess")  # not sure
add_packet("HousingObjectInitialize", 1)  # not sure
add_packet("HousingInternalObjectSpawn")  # not sure
add_packet("HousingWardInfo", 1)  # not sure
add_packet("HousingObjectMove")  # not sure
add_packet("HousingObjectDye")  # not sure
add_packet("SharedEstateSettingsResponse", 11)  # not sure
add_packet("DailyQuests", 11)  # not sure
add_packet("DailyQuestRepeatFlags", 1)  # not sure
add_packet("LandUpdateHouseName", 1)  # not sure
add_packet("AirshipTimers", 10)  # not sure
add_packet("PlaceMarker", 7)
add_packet("WaymarkPreset")
add_packet("Waymark")
add_packet("UnMount", 2)  # not sure
add_packet("CeremonySetActorAppearance", 2)  # not sure
add_packet("AirshipStatusList", 5)  # not sure
add_packet("AirshipStatus")  # not sure
add_packet("AirshipExplorationResult")  # not sure
add_packet("SubmarineStatusList")  # not sure
add_packet("SubmarineProgressionStatus")  # not sure
add_packet("SubmarineExplorationResult")  # not sure
add_packet("SubmarineTimers", 1)  # not sure
add_packet("DeepDungeonMap", 6)
add_packet("DeepDungeonItems", 1)
add_packet("DeepDungeonParty")
add_packet("DeepDungeonChests")
add_packet("PrepareZoning", 18)  # not sure
add_packet("ActorGauge")
add_packet("CharaVisualEffect")  # not sure
add_packet("LandSetMap")  # not sure
add_packet("Fall")  # not sure
add_packet("PlayMotionSync", 48)  # not sure
add_packet("PlayActionTimelineSync", 2)
add_packet("CEDirector", 5)  # not sure
add_packet("IslandWorkshopDemandResearch", 18)  # not sure
add_packet("IslandWorkshopSupplyDemand", 2)  # not sure
add_packet("IslandWorkshopFavors", 15)  # not sure

for v, n in packet_names.items():
    print(f'{n} = {v},')
print('------')


def find_next_func_by_sig(ea, pattern):
    return ida_search.find_binary(ea, ida_ida.inf_get_max_ea(), pattern, 16,
                                  ida_search.SEARCH_DOWN)


def find_single_func_by_sig(pattern):
    ea_first = find_next_func_by_sig(ida_ida.inf_get_min_ea(), pattern)
    if ea_first == idaapi.BADADDR:
        print(f'Could not find function by pattern {pattern}')
        return 0
    if find_next_func_by_sig(ea_first + 1, pattern) != idaapi.BADADDR:
        print(f'Multiple functions match pattern {pattern}')
        return 0
    return ea_first


def read_signed_byte(ea):
    v = ida_bytes.get_byte(ea)
    return v - 0x100 if v & 0x80 else v


def read_signed_dword(ea):
    v = ida_bytes.get_dword(ea)
    return v - 0x100000000 if v & 0x80000000 else v


def read_rva(ea):
    return ea + 4 + read_signed_dword(ea)


def get_vfoff_for_body(body):
    # assume each case has the following body:
    # mov rax, [rcx]
    # lea r9, [r10+10h]
    # jmp qword ptr [rax+<vfoff>]
    if ida_bytes.get_byte(body) != 0x48 or ida_bytes.get_byte(
            body + 1) != 0x8B or ida_bytes.get_byte(body + 2) != 0x01:
        return -1
    if ida_bytes.get_byte(body + 3) != 0x4D or ida_bytes.get_byte(
            body + 4) != 0x8D or ida_bytes.get_byte(
                body + 5) != 0x4A or ida_bytes.get_byte(body + 6) != 0x10:
        return -1
    if ida_bytes.get_byte(body + 7) != 0x48 or ida_bytes.get_byte(body +
                                                                  8) != 0xFF:
        return -1
    sz = ida_bytes.get_byte(body + 9)
    if sz == 0x60:
        return read_signed_byte(body + 10)
    elif sz == 0xA0:
        return read_signed_dword(body + 10)
    else:
        return -1


def vfoff_to_index(vfoff):
    if vfoff < 0x10:
        return -1  # first two vfs are dtor and exec
    if (vfoff & 7) != 0:
        return -1  # vf contains qwords
    return (vfoff >> 3) - 2


class ffnetwork(idaapi.plugin_t):
    flags = idaapi.PLUGIN_UNL
    comment = 'Build opcode map'
    help = ''
    wanted_name = 'ffnetwork'
    wanted_hotkey = ''

    _unknown_in_output = False

    def init(self):
        return idaapi.PLUGIN_OK

    def run(self, arg=None):
        # assume func starts with:
        # mov rax, [r8+10h]
        # mov r10, [rax+38h]
        # movzx eax, word ptr [r10+2]
        # add eax, -<min_case>
        # cmp eax, <max_case-min_case>
        # ja <default_off>
        # lea r11, <__ImageBase_off>
        # cdqe
        # mov r9d, ds::<jumptable_rva>[r11+rax*4]
        func = find_single_func_by_sig(
            '49 8B 40 10  4C 8B 50 38  41 0F B7 42 02  83 C0 ??  3D ?? ?? ?? ??  0F 87 ?? ?? ?? ??  4C 8D 1D ?? ?? ?? ??  48 98  45 8B 8C 83 ?? ?? ?? ??'
        )
        if func == 0:
            return
        min_case = -read_signed_byte(func + 15)  # this is a negative
        jumptable_size = read_signed_dword(func + 17) + 1
        def_addr = read_rva(func + 23)
        imagebase = read_rva(func + 30)
        jumptable = imagebase + read_signed_dword(func + 40)
        opcodemap = {}
        for i in range(jumptable_size):
            body = imagebase + read_signed_dword(jumptable + 4 * i)
            if body == def_addr:
                continue
            case = i + min_case
            voff = get_vfoff_for_body(body)
            index = vfoff_to_index(voff)
            if index < 0:
                print(f'Unexpected body for case {case}')
                continue
            if index in opcodemap:
                print(
                    f'Multiple opcodes map to single operation {index}: {hex(opcodemap[index])} and {hex(case)}'
                )
                continue
            opcodemap[index] = case
        for k, v in sorted(opcodemap.items()):
            v -= 101
            if k in packet_names:
                print(f'{packet_names[k]} = 0x{v:0{4}X},')
            elif self._unknown_in_output:
                print(f'Packet{k} = 0x{v:0{4}X},')

    def term(self):
        pass


def PLUGIN_ENTRY():
    return ffnetwork()
