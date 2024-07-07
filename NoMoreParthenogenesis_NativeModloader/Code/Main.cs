using ai.behaviours;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnitsLogger_BepInEx;
using UnityEngine;

namespace NoMoreParthenogenesis_NativeModloader
{
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
}
