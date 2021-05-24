using System.Linq;
using System.Collections.Generic;
using EnsoulSharp.SDK;
using EnsoulSharp;
using SharpDX;

namespace RVMPPrediction
{
    public static class RVMPPrediction
    {
        private static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        public static Vector3 PreCastPos(AIHeroClient Hero, float Delay)
        {
            float value = 0f;
            if (Hero.IsFacing(_Player))
            {
                value = (50f - Hero.BoundingRadius);
            }
            else
            {
                value = -(100f - Hero.BoundingRadius);
            }
            var distance = Delay * Hero.MoveSpeed + value;
            var path = Hero.GetWaypoints();

            for (var i = 0; i < path.Count - 1; i++)
            {
                var a = path[i];
                var b = path[i + 1];
                var d = a.Distance(b);

                if (d < distance)
                {
                    distance -= d;
                }
                else
                {
                    return (a + distance * (b - a).Normalized()).ToVector3();
                }
            }


            return (path[path.Count - 1]).ToVector3();
        }

    }
}