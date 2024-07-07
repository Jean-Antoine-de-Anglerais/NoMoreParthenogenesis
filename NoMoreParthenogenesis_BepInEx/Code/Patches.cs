using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace NoMoreParthenogenesis_BepInEx
{
    public class Patches
    {
        public static IEnumerable<CodeInstruction> produceNewCitizen_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = new List<CodeInstruction>(instructions);

            int index = codes.FindLastIndex(instruction => instruction.opcode == OpCodes.Stloc_2);

            if (index == -1)
            {
                Console.WriteLine("produceNewCitizen_Transpiler: index not found");
                return codes.AsEnumerable();
            }

            // Найти метод op_Equality
            //MethodInfo opEqualityMethod = typeof(UnityEngine.Object).GetMethod("op_Equality", new Type[] { typeof(UnityEngine.Object), typeof(UnityEngine.Object) });

            // Создать метку для IL_004c
            //Label label = generator.DefineLabel();

            Label label004c = generator.DefineLabel();

            // Пометить кодовую инструкцию, соответствующую IL_004c
            //codes[index + 7].labels.Add(label);

            codes[codes.FindIndex(instr => instr.opcode == OpCodes.Ldloc_2) - 1].labels.Add(label004c);


            // Вставить новые инструкции
            codes.Insert(index + 1, new CodeInstruction(OpCodes.Ldloc_0));
            codes.Insert(index + 2, new CodeInstruction(OpCodes.Ldloc_1));
            codes.Insert(index + 3, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StaticStuff), nameof(StaticStuff.ifCantProduceNewCitizen))));
            codes.Insert(index + 4, new CodeInstruction(OpCodes.Brfalse_S, label004c));
            codes.Insert(index + 5, new CodeInstruction(OpCodes.Ldc_I4_0));
            codes.Insert(index + 6, new CodeInstruction(OpCodes.Ret));


            //codes.Insert(index + 1, new CodeInstruction(OpCodes.Ldloc_1));
            //codes.Insert(index + 2, new CodeInstruction(OpCodes.Ldnull));
            //codes.Insert(index + 3, new CodeInstruction(OpCodes.Call, opEqualityMethod));
            //codes.Insert(index + 4, new CodeInstruction(OpCodes.Brfalse_S, label));
            //codes.Insert(index + 5, new CodeInstruction(OpCodes.Ldc_I4_0));
            //codes.Insert(index + 6, new CodeInstruction(OpCodes.Ret));
            //
            //codes.Insert(index + 7, new CodeInstruction(OpCodes.Ldloc_0));
            //codes.Insert(index + 8, new CodeInstruction(OpCodes.Ldfld, typeof(ActorBase).GetField("data")));
            //codes.Insert(index + 9, new CodeInstruction(OpCodes.Ldfld, typeof(ActorData).GetField("gender")));
            //codes.Insert(index + 10, new CodeInstruction(OpCodes.Ldloc_1));
            //codes.Insert(index + 11, new CodeInstruction(OpCodes.Ldfld, typeof(ActorBase).GetField("data")));
            //codes.Insert(index + 12, new CodeInstruction(OpCodes.Ldfld, typeof(ActorData).GetField("gender")));
            //codes.Insert(index + 13, new CodeInstruction(OpCodes.Bne_Un_S, label004c));
            //codes.Insert(index + 14, new CodeInstruction(OpCodes.Ldc_I4_0));
            //codes.Insert(index + 15, new CodeInstruction(OpCodes.Ret));


            // if (((ActorBase)actor).data.gender == ((ActorBase)actor2).data.gender)
            // IL_0057: ldloc.0
            // IL_0058: ldfld class ['Assembly-CSharp']ActorData ['Assembly-CSharp']ActorBase::data
            // IL_005d: ldfld valuetype ['Assembly-CSharp']ActorGender ['Assembly-CSharp']ActorData::gender
            // IL_0062: ldloc.1
            // IL_0063: ldfld class ['Assembly-CSharp']ActorData ['Assembly-CSharp']ActorBase::data
            // IL_0068: ldfld valuetype ['Assembly-CSharp']ActorGender ['Assembly-CSharp']ActorData::gender
            // IL_006d: bne.un.s IL_0071
            // 
            // // return false;
            // IL_006f: ldc.i4.0
            // IL_0070: ret

            //foreach (var item in codes)
            //{
            //    Console.WriteLine(item?.opcode.Name + "    " + item?.operand?.ToString());
            //}

            return codes.AsEnumerable();
        }

        public static IEnumerable<CodeInstruction> OnEnable_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = new List<CodeInstruction>(instructions);

            int index = codes.FindLastIndex(instr => instr.opcode == OpCodes.Callvirt && ((MethodInfo)instr.operand).Name == "set_sprite");

            if (index == -1)
            {
                Console.WriteLine("OnEnable_Transpiler: index not found");
                return codes.AsEnumerable();
            }

            codes.Insert(index + 1, new CodeInstruction(OpCodes.Ldarg_0));
            codes.Insert(index + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StaticStuff), nameof(StaticStuff.showGender))));
            return codes.AsEnumerable();
        }

        /*public static bool produceNewCitizen_Prefix(CityBehProduceUnit __instance, ref bool __result, Building pBuilding, City pCity)
        {
            Actor actor = __instance._possibleParents.Pop();
            if (actor == null)
            {
                __result = false;
                return false;
            }
            if (!Toolbox.randomChance(((BaseSimObject)actor).stats[S.fertility]))
            {
                __result = false;
                return false;
            }
            Actor actor2 = null;
            if (__instance._possibleParents.Count > 0)
            {
                actor2 = __instance._possibleParents.Pop();
            }
            if (actor2 == null)
            {
                __result = false;
                return false;
            }
            if (actor.data.gender == actor2.data.gender)
            {
                __result = true;
                return false;
            }
            ResourceAsset foodItem = pCity.getFoodItem((string)null);
            pCity.eatFoodItem(foodItem.id);
            pCity.status.housingFree--;
            pCity.data.born++;
            if (pCity.kingdom != null)
            {
                pCity.kingdom.data.born++;
            }
            ActorAsset asset = actor.asset;
            ActorData actorData = new ActorData();
            actorData.created_time = BehaviourActionBase<City>.world.getCreationTime();
            actorData.cityID = pCity.data.id;
            actorData.id = BehaviourActionBase<City>.world.mapStats.getNextId("unit");
            actorData.asset_id = asset.id;
            ActorBase.generateCivUnit(actor.asset, actorData, ((ActorBase)actor).race);
            actorData.generateTraits(asset, ((ActorBase)actor).race);
            actorData.inheritTraits(((ActorBase)actor).data.traits);
            actorData.hunger = asset.maxHunger / 2;
            ((ActorBase)actor).data.makeChild(BehaviourActionBase<City>.world.getCurWorldTime());
            if (actor2 != null)
            {
                actorData.inheritTraits(((ActorBase)actor2).data.traits);
                ((ActorBase)actor2).data.makeChild(BehaviourActionBase<City>.world.getCurWorldTime());
            }
            Clan clan = CityBehProduceUnit.checkGreatClan(actor, actor2);
            actorData.skin = ActorTool.getBabyColor(actor, actor2);
            actorData.skin_set = ((ActorBase)actor).data.skin_set;
            Culture babyCulture = CityBehProduceUnit.getBabyCulture(actor, actor2);
            if (babyCulture != null)
            {
                actorData.culture = babyCulture.data.id;
                actorData.level = babyCulture.getBornLevel();
            }
            if (clan != null)
            {
                Actor pActor = pCity.spawnPopPoint(actorData, actor.currentTile);
                clan.addUnit(pActor);
            }
            else
            {
                pCity.addPopPoint(actorData);
            }
            __result = true;
            return false;
        }*/
    }
}
