using System.Collections;
using System.Collections.Generic;
//using Unity.Services.Analytics;
using UnityEngine;

public static class AnalyticAssistant
{
    public static void DeathEvent(Vector3 player, Vector3 enemy, string pWeapon, string eWeapon, float eHealth)
    {
        Dictionary<string, object> DEDict = new Dictionary<string, object>()
        {
            {"dpWeapon",pWeapon},
            {"kpWeapon",eWeapon},
            {"killerHealth",eHealth},
            {"kPosX",enemy.x},
            {"kPosY",enemy.y},
            {"kPosZ",enemy.z},
            {"posX",player.x},
            {"posY",player.y},
            {"posZ",player.z}
        };
        //AnalyticsService.Instance.CustomData("deathEvent", DEDict);
    }
}
