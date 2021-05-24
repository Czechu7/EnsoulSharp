using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;
using SharpDX.Direct3D9;
using Font = SharpDX.Direct3D9.Font;
using System.Net;
using System.Reflection;

namespace RVMPReborn
{
    class Program
    {
        private static AIHeroClient Player => ObjectManager.Player;
        static void Main(string[] args)
        {
            GameEvent.OnGameLoad += GameEventOnOnGameLoad;
        }


        private static void GameEventOnOnGameLoad()
        {
            switch (Player.CharacterName)
            {
                case "Ashe":
                    RVMPRebornAshe.Program.OnGameLoad();
                    break;
                case "Cassipeia":
                    RVMPRebornCassiopeia.Program.OnGameLoad();
                    break;

            }
            //Checkgit();
        }
        public static void Checkgit()
        {
            using (var wb = new WebClient())
            {
                var raw = wb.DownloadString("linkdogithuba");

                System.Version Version = Assembly.GetExecutingAssembly().GetName().Version;

                if (raw != Version.ToString())
                {
                    Game.Print("Oudated", raw);
                }
                else
                    Game.Print("Updated", Version.ToString());
            }
        }

    }
}
