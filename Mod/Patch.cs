using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lilly.MaxFishPopulation
{
    public static class Patch
    {
        public static HarmonyX harmony = null;
        public static string harmonyId = "Lilly.";

        public static void OnPatch(bool repatch = false)
        {
            if (repatch)
            {
                Unpatch();
            }
            if (harmony != null || !Settings.onPatch) return;
            harmony = new HarmonyX(harmonyId);
            try
            {
                harmony.PatchAll();
                MyLog.Message($"Patch <color=#00FF00FF>Succ</color>");
            }
            catch (System.Exception e)
            {
                MyLog.Error($"Patch Fail");
                MyLog.Error(e.ToString());
                MyLog.Error($"Patch Fail");
            }
            
        }

        public static void Unpatch()
        {
            MyLog.Message($"UnPatch");
            if (harmony == null) return;
            harmony.UnpatchSelf();
            harmony = null;
        }

        // PostLoad는 구현되지 않아 못써먹음
        //[HarmonyPatch(typeof(BiomeDef), "PostLoad")]
        //[HarmonyPostfix]
        public static void Postfix(BiomeDef __instance)
        {
            MyLog.Message($"{__instance.defName} {__instance.maxFishPopulation} {__instance.fileName}");
            __instance.maxFishPopulation = Settings.maxFishPopulation; // 원하는 값으로 설정                
            MyLog.Message($"{__instance.defName} {__instance.maxFishPopulation}");
        }

        // PlayDataLoader.DoPlayLoad() 메서드가 def 로딩 후 호출되므로 효과 없음
        //[HarmonyPatch(typeof(PlayDataLoader), "DoPlayLoad")]
        //[HarmonyPostfix]
        public static void SetMaxFishPopulation()
        {
            MyLog.Message("수정 시작");

            foreach (var biome in DefDatabase<BiomeDef>.AllDefs)
            {
                MyLog.Message($"{biome.defName} {biome.maxFishPopulation}");
                // 조건 없이 전체 Biome 수정
                biome.maxFishPopulation = Settings.maxFishPopulation; // 원하는 값으로 설정                

                // 또는 특정 Biome만 수정
                // if (biome.defName == "TemperateForest")
                //     biome.maxFishPopulation = 999f;

            }

            MyLog.Message("수정 완료");
        }       

        [HarmonyPatch(typeof(WaterBody), "SetFishTypes")]
        [HarmonyPrefix]
        public static bool SetFishTypes(WaterBody __instance)
        {
            if (__instance.map.Biome.fishTypes == null)
            {
                return false; ;
            }

            var type = __instance.GetType();
            // 🔍 private 필드 접근
            var commonFishField = type.GetField("commonFish", BindingFlags.NonPublic | BindingFlags.Instance);
            var uncommonFishField = type.GetField("uncommonFish", BindingFlags.NonPublic | BindingFlags.Instance);

            var commonFish = (List<ThingDef>)commonFishField.GetValue(__instance);
            var uncommonFish = (List<ThingDef>)uncommonFishField.GetValue(__instance);

            commonFish.Clear();
            uncommonFish.Clear();

            var shouldHaveFishField = type.GetField("shouldHaveFish", BindingFlags.NonPublic | BindingFlags.Instance);
            shouldHaveFishField.SetValue(__instance, true);

            //BiomeFishTypes fishTypes = __instance.map.Biome.fishTypes;
            commonFish.AddRange(ThingCategoryDefOf.Fish.childThingDefs);
            uncommonFish.AddRange(ThingCategoryDefOf.Fish.childThingDefs);
            //commonFish.AddRange(from fishChance in fishTypes.freshwater_Common select fishChance.fishDef);
            //commonFish.AddRange(from fishChance in fishTypes.saltwater_Common select fishChance.fishDef);
            //uncommonFish.AddRange(from fishChance in fishTypes.freshwater_Uncommon select fishChance.fishDef);
            //uncommonFish.AddRange(from fishChance in fishTypes.saltwater_Uncommon select fishChance.fishDef);
            MyLog.Message("수정 완료");
            return false;
        }

        [HarmonyPatch(typeof(Zone_Fishing), ".ctor", new[] { typeof(ZoneManager) })]
        [HarmonyPostfix]
        public static void AfterConstruct(Zone_Fishing __instance)
        {
            MyLog.Message("구역 설정 시본값 변경");
            __instance.repeatMode = FishRepeatMode.DoForever;
        }
    }
}
