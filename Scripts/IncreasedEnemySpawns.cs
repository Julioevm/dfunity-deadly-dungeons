// Project:         IncreasedEnemySpawns mod for Daggerfall Unity (http://www.dfworkshop.net)
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Reaven

using System;
using UnityEngine;
using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Questing;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Utility;


namespace IncreasedEnemySpawns
{

    public class IncreasedEnemySpawns : MonoBehaviour
    {
        static Mod mod;
        static int extraEnemyChance;
        static int extraEnemyCount;
        static int additionalEnemyChance;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            var go = new GameObject(mod.Title);
            go.AddComponent<IncreasedEnemySpawns>();

            Debug.Log("Begin mod init: Deadly Dungeons");
            PlayerEnterExit.OnTransitionDungeonInterior += IESTransitionDungeonInterior;
        }

        void Awake()
        {
            var settings = mod.GetSettings();
            extraEnemyChance = settings.GetValue<int>("EnemySpawns", "extraEnemyChance");
            extraEnemyCount = settings.GetValue<int>("EnemySpawns", "extraEnemyCount");
            additionalEnemyChance = settings.GetValue<int>("EnemySpawns", "additionalEnemyChance");

            mod.LoadSettings();
            mod.IsReady = true;
        }

        private void LoadSettings(ModSettings settings, ModSettingsChange change)
        {
            if (change.HasChanged("Foo"))
            {
                var number = settings.GetValue<int>("Foo", "Number");
                var color = settings.GetValue<Color32>("Foo", "Color");
            }

            if (change.HasChanged("Bar", "Text"))
            {
                var text = settings.GetValue<string>("Bar", "Text");
            }
        }


        private static void IESTransitionDungeonInterior(PlayerEnterExit.TransitionEventArgs args)
        {
            PlayerEnterExit playerEnterExit = GameManager.Instance.PlayerEnterExit;
            SpawnExtraEnemies();

        }

        private static void SpawnExtraEnemies()
        {
            var entityBehaviours = FindObjectsOfType<DaggerfallEntityBehaviour>();
                var count = 0;
                foreach (var entityBehaviour in entityBehaviours)
                {
                    if (entityBehaviour.EntityType != EntityTypes.EnemyMonster &&
                        entityBehaviour.EntityType != EntityTypes.EnemyClass) continue;
                    if (!Dice100.SuccessRoll(extraEnemyChance)) continue;
                    // Spawn first extra enemy
                    SpawnEnemy(entityBehaviour);
                    count++;
                    // Roll for additional enemies
                    for (int i = 0; i < extraEnemyCount && Dice100.SuccessRoll(additionalEnemyChance); i++)
                    {
                        SpawnEnemy(entityBehaviour);
                        count++;
                    }
                }
                #if UNITY_EDITOR
                    Debug.LogFormat("Added: {0} extra enemies.", count);
                #endif
        }

        private static void SpawnEnemy(DaggerfallEntityBehaviour entityBehaviour)
        {
            var enemyEntity = entityBehaviour.Entity as EnemyEntity;
            var enemyMobileType = (MobileTypes)Enum.Parse(typeof(MobileTypes), enemyEntity.MobileEnemy.ID.ToString());
            GameObjectHelper.CreateEnemy(entityBehaviour.name,enemyMobileType,entityBehaviour.transform.position);
        }
    }
}
