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
            Localizer.SetLocalization("cz", "creature_statistics_gender", "性别");
           Localizer.SetLocalization("ch", "creature_statistics_gender", "性別");

            Localizer.SetLocalization("ru", "gender_unknown", "Неизвестно");
            Localizer.SetLocalization("en", "gender_unknown", "Unknown");
            Localizer.SetLocalization("cz", "gender_unknown", "不便透露");
            Localizer.SetLocalization("ch", "gender_unknown", ":(");

            Localizer.SetLocalization("ru", "gender_male", "Мужской");
            Localizer.SetLocalization("en", "gender_male", "Male");
            Localizer.SetLocalization("cz", "gender_male", "男");
            Localizer.SetLocalization("ch", "gender_male", "男");

            Localizer.SetLocalization("ru", "gender_female", "Женский");
            Localizer.SetLocalization("en", "gender_female", "Female");
            Localizer.SetLocalization("cz", "gender_female", "女");
            Localizer.SetLocalization("ch", "gender_female", "女");

            harmony.Patch(AccessTools.Method(typeof(CityBehProduceUnit), "produceNewCitizen"),
            transpiler: new HarmonyMethod(AccessTools.Method(typeof(Patches), "produceNewCitizen_Transpiler")));

            harmony.Patch(AccessTools.Method(typeof(WindowCreatureInfo), "OnEnable"),
            transpiler: new HarmonyMethod(AccessTools.Method(typeof(Patches), "OnEnable_Transpiler")));
        }
    }
}
