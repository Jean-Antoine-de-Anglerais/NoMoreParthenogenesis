using HarmonyLib;
using NCMS;
using System.Reflection;
using UnityEngine;
using ai.behaviours;

namespace NoMoreParthenogenesis_NCMS
{
    [ModEntry]
    class Main : MonoBehaviour
    {
        public static Harmony harmony = new Harmony(MethodBase.GetCurrentMethod().DeclaringType.Namespace);

        void Awake()
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
        }
    }
}
