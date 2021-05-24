using System.Linq;
using System.Collections.Generic;
using EnsoulSharp.SDK;
using EnsoulSharp;

namespace RVMPExtension
{
    public static class Extensions
    {
        public static bool PoisonWillExpire(this AIBaseClient target, float time)
        {
            var buff = target.Buffs.OrderByDescending(x => x.EndTime).FirstOrDefault(x => x.Type == BuffType.Poison && x.IsActive && x.IsValid);
            return buff == null || time > (buff.EndTime - Game.Time) * 1000f;
        }
        public static bool Immobile(AIBaseClient unit)
        {
            return unit.HasBuffOfType(BuffType.Charm) || unit.HasBuffOfType(BuffType.Stun) ||
                   unit.HasBuffOfType(BuffType.Knockup) || unit.HasBuffOfType(BuffType.Snare) ||
                   unit.HasBuffOfType(BuffType.Taunt) || unit.HasBuffOfType(BuffType.Suppression) ||
                   unit.HasBuffOfType(BuffType.Polymorph);
        }
        public static List<AIMinionClient> GetGenericJungleMinionsTargets()
        {
            return GetGenericJungleMinionsTargetsInRange(float.MaxValue);
        }
        public static List<AIMinionClient> GetGenericJungleMinionsTargetsInRange(float range)
        {
            return GameObjects.Jungle.Where(m => !GameObjects.JungleSmall.Contains(m) && m.IsValidTarget(range)).ToList();
        }
        public static List<AIHeroClient> GetAllyHeroesTargets()
        {
            return GetAllyHeroesTargetsInRange(float.MaxValue);
        }
        public static List<AIHeroClient> GetAllyHeroesTargetsInRange(float range)
        {
            return GameObjects.AllyHeroes.Where(h => h.IsValidTarget(range)).ToList();
        }
        public static List<AIMinionClient> GetAllyLaneMinionsTargets()
        {
            return GetAllyLaneMinionsTargetsInRange(float.MaxValue);
        }
        public static List<AIMinionClient> GetAllyLaneMinionsTargetsInRange(float range)
        {
            return GameObjects.AllyMinions.Where(m => m.IsValidTarget(range, true)).ToList();
        }
        public static AIHeroClient GetBestAllyHeroTargetInRange(float range)
        {
            var target = TargetSelector.GetTarget(range);
            if (target != null && target.IsValidTarget() && !target.IsInvulnerable)
            {
                return target;
            }
            return null;
        }
        public static List<AIMinionClient> GetEnemyLaneMinionsTargets()
        {
            return GetEnemyLaneMinionsTargetsInRange(float.MaxValue);
        }

        public static List<AIMinionClient> GetEnemyLaneMinionsTargetsInRange(float range)
        {
            return GameObjects.EnemyMinions.Where(m => m.IsValidTarget(range)).ToList();
        }
        public static List<AIHeroClient> GetEnemyHeroesTargets()
        {
            return GetEnemyHeroesTargetsInRange(float.MaxValue);
        }

        public static List<AIHeroClient> GetEnemyHeroesTargetsInRange(float range)
        {
            return GameObjects.EnemyHeroes.Where(h => h.IsValidTarget(range)).ToList();
        }

        public static bool CheckWalls(AIBaseClient player, AIBaseClient enemy)
        {
            var distance = player.Position.Distance(enemy.Position);
            for (int i = 1; i < 6; i++)
            {
                if (player.Position.Extend(enemy.Position, distance + 60 * i).IsWall())
                {
                    return true;
                }
            }
            return false;
        }


    }

}