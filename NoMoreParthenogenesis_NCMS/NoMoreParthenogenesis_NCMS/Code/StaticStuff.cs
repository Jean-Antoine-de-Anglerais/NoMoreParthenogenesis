namespace NoMoreParthenogenesis_NCMS
{
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
}
