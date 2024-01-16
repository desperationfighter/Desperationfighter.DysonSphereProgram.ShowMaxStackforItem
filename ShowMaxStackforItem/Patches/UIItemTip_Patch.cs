using HarmonyLib;

//transpiller
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;

namespace ShowMaxStackforItem.Patches
{
    [HarmonyPatch(typeof(UIItemTip))]
    [HarmonyPatch(nameof(UIItemTip.SetTip))]
    public static class UIItemTip_SetTip_Patch
    {
        //Debug Logging - Deactivate before shipping
        private static bool debuglogging = false;
        //Deep Logging
        private static bool deeplogging = false;

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            string classnamefordocu = "UIItemTip_SetTip_Patch";
            if (debuglogging && !deeplogging)
            {
                Plugin.Logger.LogDebug("Deeploging deactivated");
            }

            if (debuglogging)
            {
                Plugin.Logger.LogDebug($"Start Transpiler - {classnamefordocu}");
            }

            bool found = false;
            var Index = -1;
            var codes = new List<CodeInstruction>(instructions);

            //logging before
            if (debuglogging && deeplogging)
            {
                Plugin.Logger.LogDebug("Deep Logging pre-transpiler:");
                for (int k = 0; k < codes.Count; k++)
                {
                    Plugin.Logger.LogDebug((string.Format("0x{0:X4}", k) + $" : {codes[k].opcode.ToString()}	{(codes[k].operand != null ? codes[k].operand.ToString() : "")}"));
                }
            }

            //analyse the code to find the right place for injection
            if (debuglogging)
            {
                Plugin.Logger.LogDebug("Start code analyses");
            }
            for (var i = 0; i < codes.Count; i++)
            {
/*
[Debug  :Show Max Stack for Item] 0x021E : callvirt	Void set_anchoredPosition(UnityEngine.Vector2)      -5
                                                                                                        NEW
[Debug  :Show Max Stack for Item] 0x021F : ldloc.s	System.Int32 (10)                                   -4 below                                                                                                        
[Debug  :Show Max Stack for Item] 0x0220 : ldloc.s	System.Int32 (13)                                   -3 top
[Debug  :Show Max Stack for Item] 0x0221 : add	                                                        -2
[Debug  :Show Max Stack for Item] 0x0222 : ldc.i4.2	                                                    -1
[Debug  :Show Max Stack for Item] 0x0223 : cgt	                                                        0
[Debug  :Show Max Stack for Item] 0x0224 : ldc.i4.0	                                                    1
[Debug  :Show Max Stack for Item] 0x0225 : ceq	                                                        2
[Debug  :Show Max Stack for Item] 0x0226 : dup	                                                        3
*/

                if (codes[i].opcode == OpCodes.Cgt && codes[i +2].opcode == OpCodes.Ceq && codes[i +3].opcode == OpCodes.Dup && codes[i-2].opcode == OpCodes.Add)
                {
                    if (debuglogging)
                    {
                        Plugin.Logger.LogDebug("Found IL Code Line for Index");
                        Plugin.Logger.LogDebug($"Index = {Index.ToString()}");
                    }
                    found = true;
                    Index = i;
                    break;
                }
            }

            if (debuglogging)
            {
                if (found)
                {
                    Plugin.Logger.LogDebug("found true");
                }
                else
                {
                    Plugin.Logger.LogDebug("found false");
                }
            }

            if (Index > -1)
            {
                if (debuglogging)
                {
                    Plugin.Logger.LogDebug("Index1 > -1");
                }
                Plugin.Logger.LogInfo($"Transpiler injectection position found for - {classnamefordocu}");
    
                if(Plugin.Modus.Value != 1)
                {
                    Plugin.Logger.LogDebug("Adding New Line to UI of ToolTip");
                    int diffforchange = 4;
                    LocalBuilder builder = codes[Index - 4].operand as LocalBuilder;
                    //codes.Insert(Index - diffforchange, new CodeInstruction(OpCodes.Stloc_S, 10));
                    codes.Insert(Index - diffforchange, new CodeInstruction(OpCodes.Stloc_S, builder));
                    codes.Insert(Index - diffforchange, new CodeInstruction(OpCodes.Add));
                    codes.Insert(Index - diffforchange, new CodeInstruction(OpCodes.Ldc_I4_1));
                    //codes.Insert(Index - diffforchange, new CodeInstruction(OpCodes.Ldloc_S, 10));
                    codes.Insert(Index - diffforchange, new CodeInstruction(OpCodes.Ldloc_S, builder));
                }
            }
            else
            {
                Plugin.Logger.LogError("Index was not found");
            }

            //logging after
            if (debuglogging && deeplogging)
            {
                Plugin.Logger.LogDebug("Deep Logging after-transpiler:");
                for (int k = 0; k < codes.Count; k++)
                {
                    Plugin.Logger.LogDebug((string.Format("0x{0:X4}", k) + $" : {codes[k].opcode.ToString()}	{(codes[k].operand != null ? codes[k].operand.ToString() : "")}"));
                }
            }

            if (debuglogging)
            {
                Plugin.Logger.LogDebug("Transpiler end going to return");
            }
            return codes.AsEnumerable();
        }

        [HarmonyPostfix]
        static void Postfix(UIItemTip __instance)
        {
            ItemProto itemProto = LDB.items.Select(__instance.showingItemId);
            if(itemProto != null)
            {
                Plugin.Logger.LogDebug("Item Proto found");
                if (Plugin.Modus.Value != 2)
                {
                    Plugin.Logger.LogDebug("Modus 1 runs");
                    switch (Plugin.Modus_1_Setting.Value)
                    {
                        case 1:
                            __instance.nameText.text = __instance.nameText.text + " (Stack Size:" + itemProto.StackSize.ToString() + ")";
                            break;
                        case 2:
                            __instance.nameText.text = __instance.nameText.text + " (Stack:" + itemProto.StackSize.ToString() + ")";
                            break;
                        case 3:
                            __instance.nameText.text = __instance.nameText.text + " (" + itemProto.StackSize.ToString() + ")";
                            break;
                    }
                }

                if (Plugin.Modus.Value != 1)
                {
                    Plugin.Logger.LogDebug("Modus 2 runs");
                    switch (Plugin.Modus_2_Setting.Value)
                    {
                        case 1:
                            __instance.propsText.text = "Stack Size" + "\r\n" + __instance.propsText.text;
                            __instance.valuesText.text = itemProto.StackSize.ToString() + "\r\n" + __instance.valuesText.text;
                            break;
                        case 2:
                            __instance.propsText.text = __instance.propsText.text + "Stack Size";
                            __instance.valuesText.text = __instance.valuesText.text + itemProto.StackSize.ToString();
                            break;
                    }
               }
            }
        }
    }
}
