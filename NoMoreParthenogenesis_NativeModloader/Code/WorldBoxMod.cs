using System.IO;
using System.Reflection;
using UnityEngine;

namespace NoMoreParthenogenesis_NativeModloader
{
    public class WorldBoxMod : MonoBehaviour
    {
        public void Awake()
        {
            Debug.Log($"{MethodBase.GetCurrentMethod().DeclaringType.Namespace} loading...");
            string path = Path.Combine(Application.streamingAssetsPath, "Mods");
            path = Path.Combine(path, "stuffthatjeansmodsuse");
            if (!Directory.Exists(path))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                directoryInfo.Create();
                directoryInfo.Attributes |= FileAttributes.Hidden;
            }

            if (!File.Exists(Path.Combine(path, "0Harmony.dll"))) { File.WriteAllBytes(Path.Combine(path, "0Harmony.dll"), Assemblies.Resource._0Harmony); }
            if (!File.Exists(Path.Combine(path, "Mono.Cecil.dll"))) { File.WriteAllBytes(Path.Combine(path, "Mono.Cecil.dll"), Assemblies.Resource.Mono_Cecil); }
            if (!File.Exists(Path.Combine(path, "MonoMod.Utils.dll"))) { File.WriteAllBytes(Path.Combine(path, "MonoMod.Utils.dll"), Assemblies.Resource.MonoMod_Utils); }
            if (!File.Exists(Path.Combine(path, "MonoMod.RuntimeDetour.dll"))) { File.WriteAllBytes(Path.Combine(path, "MonoMod.RuntimeDetour.dll"), Assemblies.Resource.MonoMod_RuntimeDetour); }

            Assembly.LoadFrom(Path.Combine(path, "0Harmony.dll"));
            //Assembly.LoadFrom(Path.Combine(path, "Mono.Cecil.dll"));
            //Assembly.LoadFrom(Path.Combine(path, "MonoMod.Utils.dll"));
            //Assembly.LoadFrom(Path.Combine(path, "MonoMod.RuntimeDetour.dll"));

            Debug.Log($"{MethodBase.GetCurrentMethod().DeclaringType.Namespace} loaded!");
            GameObject gameObject = new GameObject(MethodBase.GetCurrentMethod().DeclaringType.Namespace);
            DontDestroyOnLoad(gameObject);
            gameObject.AddComponent<Main>();
        }
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
