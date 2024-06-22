using ai;
using ai.behaviours;
using HarmonyLib;
using NoMoreParthenogenesis_NativeModloader.Resourses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnitsLogger_BepInEx;
using UnityEngine;

namespace NoMoreParthenogenesis_NativeModloader
{
    public class WorldBoxMod : MonoBehaviour
    {
        public void Awake()
        {
            Debug.Log("NoMoreParthenogenesis_NativeModloader loading...");
            string path = Path.Combine(Application.streamingAssetsPath, "Mods");
            path = Path.Combine(path, "stuffthatjeansmodsuse");
            if (!Directory.Exists(path))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                directoryInfo.Create();
                directoryInfo.Attributes |= FileAttributes.Hidden;
            }

            File.WriteAllBytes(Path.Combine(path, "0Harmony.dll"), Resource._0Harmony);
            File.WriteAllBytes(Path.Combine(path, "Mono.Cecil.dll"), Resource.Mono_Cecil);
            File.WriteAllBytes(Path.Combine(path, "MonoMod.Utils.dll"), Resource.MonoMod_Utils);
            File.WriteAllBytes(Path.Combine(path, "MonoMod.RuntimeDetour.dll"), Resource.MonoMod_RuntimeDetour);

            Assembly.LoadFrom(Path.Combine(path, "0Harmony.dll"));
            Assembly.LoadFrom(Path.Combine(path, "Mono.Cecil.dll"));
            Assembly.LoadFrom(Path.Combine(path, "MonoMod.Utils.dll"));
            Assembly.LoadFrom(Path.Combine(path, "MonoMod.RuntimeDetour.dll"));

            Debug.Log("NoMoreParthenogenesis_NativeModloader loaded!");
            GameObject gameObject = new GameObject(MethodBase.GetCurrentMethod().DeclaringType.Namespace);
            DontDestroyOnLoad(gameObject);
            gameObject.AddComponent<Main>();
        }
    }

    internal class Main : MonoBehaviour
    {
        public static Harmony harmony = new Harmony(MethodBase.GetCurrentMethod().DeclaringType.Namespace);
        private bool _initialized = false;

        public void Update()
        {
            if (global::Config.gameLoaded && !_initialized)
            {
                Localizer.SetLocalization("ru", "creature_statistics_gender", "Пол");
                Localizer.SetLocalization("en", "creature_statistics_gender", "Gender");

                Localizer.SetLocalization("ru", "gender_unknown", "Неизвестно");
                Localizer.SetLocalization("en", "gender_unknown", "Unknown");

                Localizer.SetLocalization("ru", "gender_male", "Мужской");
                Localizer.SetLocalization("en", "gender_male", "Male");

                Localizer.SetLocalization("ru", "gender_female", "Женский");
                Localizer.SetLocalization("en", "gender_female", "Female");

                harmony.Patch(AccessTools.Method(typeof(CityBehProduceUnit), "produceNewCitizen"),
                transpiler: new HarmonyMethod(AccessTools.Method(typeof(Patches), "produceNewCitizen_Transpiler")));

                

                harmony.Patch(AccessTools.Method(typeof(WindowCreatureInfo), "OnEnable"),
                transpiler: new HarmonyMethod(AccessTools.Method(typeof(Patches), "OnEnable_Transpiler")));

                //harmony.Patch(AccessTools.Method(typeof(CityBehProduceUnit), "produceNewCitizen"),
                //prefix: new HarmonyMethod(AccessTools.Method(typeof(Patches), "produceNewCitizen_Prefix")));

                _initialized = true;
            }
        }
    }

    public static class StaticStuff
    {
        public static bool ifCantProduceNewCitizen(Actor parent1, Actor parent2)
        {
            if (parent2 == null)
            {
                return true;
            }

            else if (parent1 == null)
            {
                return true;
            }

            else if (parent1.data.gender == parent2.data.gender)
            {
                return true;
            }
            return false;
        }

        public static void showGender(WindowCreatureInfo instance)
        {
            instance.showStat("creature_statistics_gender", LocalizedTextManager.getText("gender_" + instance.actor.data.gender.ToString().ToLower(), null));
        }
    }

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

    /*public class CityBehProduceUnitTest : BehaviourActionCity
    {
        private List<Actor> _possibleParents = new List<Actor>();

        private bool unitProduced;

        public override BehResult execute(City pCity)
        {
            if (!BehaviourActionBase<City>.world.worldLaws.world_law_civ_babies.boolVal)
            {
                return BehResult.Stop;
            }
            if (!pCity.hasAnyFood())
            {
                return BehResult.Stop;
            }
            if (!isCityCanProduceUnits(pCity))
            {
                return BehResult.Stop;
            }
            unitProduced = false;
            findPossibleParents(pCity);
            if (_possibleParents.Count == 0)
            {
                return BehResult.Stop;
            }
            int pMaxExclusive = pCity.status.population / 7 + 1;
            int num = Toolbox.randomInt(1, pMaxExclusive);
            if (DebugConfig.isOn(DebugOption.CityFastPopGrowth) && num < 100)
            {
                num = 100;
            }
            for (int i = 0; i < num; i++)
            {
                if (_possibleParents.Count == 0)
                {
                    break;
                }
                if (!isCityCanProduceUnits(pCity))
                {
                    break;
                }
                tryToProduceUnit(pCity);
            }
            _possibleParents.Clear();
            if (unitProduced)
            {
                return BehResult.Continue;
            }
            return BehResult.Stop;
        }

        private bool isCityCanProduceUnits(City pCity)
        {
            if (pCity.status.housingFree == 0)
            {
                return false;
            }
            if (BehaviourActionBase<City>.world.worldLaws.world_law_civ_limit_population_100.boolVal && pCity.getPopulationTotal() >= 100)
            {
                return false;
            }
            return true;
        }

        private void findPossibleParents(City pCity)
        {
            _possibleParents.Clear();
            List<Actor> simpleList = pCity.units.getSimpleList();
            double num = BehaviourActionBase<City>.world.getCurWorldTime() / 5.0;
            for (int i = 0; i < simpleList.Count; i++)
            {
                Actor actor = simpleList[i];
                if (actor.isAlive() && !(((BaseSimObject)actor).stats[S.fertility] <= 0f) && !(num - ((ActorBase)actor).data.had_child_timeout / 5.0 < 8.0))
                {
                    _possibleParents.Add(actor);
                }
            }
            _possibleParents.Shuffle();
        }

        private void tryToProduceUnit(City pCity)
        {
            if (pCity.getFoodItem((string)null) != null)
            {
                Building buildingType = pCity.getBuildingType(SB.type_house, pCountOnlyFinished: true, pRandom: false);
                if ((object)buildingType != null && produceNewCitizen(buildingType, pCity))
                {
                    unitProduced = true;
                }
            }
        }

        private bool produceNewCitizen(Building pBuilding, City pCity)
        {
            Actor actor = _possibleParents.Pop();
            if (actor == null)
            {
                return false;
            }
            if (!Toolbox.randomChance(((BaseSimObject)actor).stats[S.fertility]))
            {
                return false;
            }
            Actor actor2 = null;
            if (_possibleParents.Count > 0)
            {
                actor2 = _possibleParents.Pop();
            }
            if (Patches.ifCantProduceNewCitizen(actor, actor2))
            {
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
            Clan clan = checkGreatClan(actor, actor2);
            actorData.skin = ActorTool.getBabyColor(actor, actor2);
            actorData.skin_set = ((ActorBase)actor).data.skin_set;
            Culture babyCulture = getBabyCulture(actor, actor2);
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
            return true;
        }

        private static Clan checkGreatClan(Actor pParent1, Actor pParent2)
        {
            string text = string.Empty;
            if (string.IsNullOrEmpty(text))
            {
                if (pParent1.isKing())
                {
                    text = ((ActorBase)pParent1).data.clan;
                }
                else if (pParent2 != null && pParent2.isKing())
                {
                    text = ((ActorBase)pParent2).data.clan;
                }
            }
            if (string.IsNullOrEmpty(text))
            {
                if (pParent1.isCityLeader() && pParent2 != null && pParent2.isCityLeader())
                {
                    text = ((!Toolbox.randomBool()) ? ((ActorBase)pParent2).data.clan : ((ActorBase)pParent1).data.clan);
                }
                else if (pParent1 != null && pParent1.isCityLeader())
                {
                    text = ((ActorBase)pParent1).data.clan;
                }
                else if (pParent2 != null && pParent2.isCityLeader())
                {
                    text = ((ActorBase)pParent2).data.clan;
                }
            }
            Clan result = null;
            if (!string.IsNullOrEmpty(text))
            {
                result = BehaviourActionBase<City>.world.clans.get(text);
            }
            return result;
        }

        private static Culture getBabyCulture(Actor pActor1, Actor pActor2)
        {
            string text = ((ActorBase)pActor1).data.culture;
            string text2 = text;
            if (pActor2 != null)
            {
                text2 = ((ActorBase)pActor2).data.culture;
            }
            if (string.IsNullOrEmpty(text))
            {
                text = pActor1.city?.data.culture;
            }
            if (string.IsNullOrEmpty(text2) && pActor2 != null)
            {
                text2 = pActor2.city?.data.culture;
            }
            Culture culture = pActor1.currentTile.zone.culture;
            if (culture != null && culture.data.race == ((ActorBase)pActor1).race.id && Toolbox.randomChance(culture.stats.culture_spread_convert_chance.value))
            {
                text = culture.data.id;
            }
            if (Toolbox.randomBool())
            {
                return BehaviourActionBase<City>.world.cultures.get(text);
            }
            return BehaviourActionBase<City>.world.cultures.get(text2);
        }
    }*/
}
