using System.Collections.Generic;

namespace NoMoreParthenogenesis_BepInEx
{
    public static class Localizer
    {
        public static void SetLocalization(string planguage, string id, string name)
        {
            string language = LocalizedTextManager.instance.language;
            string templanguage;

            templanguage = language;

            if (templanguage != "ru" && templanguage != "en")
            {
                templanguage = "en";
            }

            if (planguage == templanguage)
            {
                Dictionary<string, string> localizedText = LocalizedTextManager.instance.localizedText;
                if (!localizedText.ContainsKey(id))
                {
                    localizedText.Add(id, name);
                }
                else if (localizedText.ContainsKey(id))
                {
                    localizedText.Remove(id);
                    localizedText.Add(id, name);
                }
            }
        }

        public static string GetLocalization(this string id)
        {
            Dictionary<string, string> localizedText = LocalizedTextManager.instance.localizedText;

            if (localizedText.ContainsKey(id))
            {
                return localizedText[id];
            }

            else
            {
                return id;
            }
        }
    }
}
