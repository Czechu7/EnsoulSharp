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

namespace RVMPRebornAshe
{
    internal class Program{

        public static Font TextBold;
        private static Spell Q,W,E,R;

        private static Menu mainMenu;
        private static AIHeroClient Player => ObjectManager.Player;

        public static void OnGameLoad()
        {

            TextBold = new Font(
    Drawing.Direct3DDevice, new FontDescription
    {
        FaceName = "Tahoma",
        Height = 15,
        Weight = FontWeight.ExtraBold,
        OutputPrecision = FontPrecision.Default,
        Quality = FontQuality.ClearType,
    }
);

            if (GameObjects.Player.CharacterName != "Ashe") return;
            Q = new Spell(SpellSlot.Q);

            W = new Spell(SpellSlot.W,1200f);
            W.SetSkillshot(0.25f, 40f, 1500f, true, SpellType.Line);

            E = new Spell(SpellSlot.E);

            R = new Spell(SpellSlot.R,float.MaxValue);
            R.SetSkillshot(0.25f, 260f, 1600f, false, SpellType.Line);


            mainMenu = new Menu("Ashe", "[RVMP:R] Ashe", true); 
            
            var Combo = new Menu("Combo", "[RVMP:R] Combo Settings");
            Combo.Add(new MenuBool("Quse", "Use Q", true)); 
            Combo.Add(new MenuBool("Wuse", "Use W", true));
            Combo.Add(new MenuBool("Ruse", "Use R", true));
            Combo.Add(new MenuSlider("Rcount", "^If enemy count is:", 2, 1, 5));
            mainMenu.Add(Combo);

            var Harass = new Menu("Harass", "[RVMP:R] Harass Settings");
            Harass.Add(new MenuBool("Wuse", "Use W", true));
            Harass.Add(new MenuBool("Euse", "Use E", true));
            Harass.Add(new MenuBool("onlyr", "^ Only if target is out of auto attack range", true));
            Harass.Add(new MenuSlider("mana%", "Mana percentage", 50, 0, 100));
            mainMenu.Add(Harass);
            
            var KS = new Menu("KS", "[RVMP:R] Kill Steal Settings");
            KS.Add(new MenuBool("Wuse", "Auto W to kill", true));
            KS.Add(new MenuBool("Ruse", "Auto R to kill", true));
            KS.Add(new MenuSlider("Renemy", "^ Use if less than enemies in range", 1, 1, 5));
            mainMenu.Add(KS);

            var Farm = new Menu("Farm", "[RVMP:R] LaneClear Settings");
            Farm.Add(new MenuBool("Wuse", "Use W", false));
            Farm.Add(new MenuSlider("Eminions", "^ Minions to cast W", 3, 1, 7));
            Farm.Add(new MenuSlider("mana%", "Mana percentage", 50, 0, 100));
            mainMenu.Add(Farm);

            var Jungle = new Menu("Jungle", "[RVMP:R] JungleClear Settings");
            Jungle.Add(new MenuBool("Quse", "Use Q", true));
            Jungle.Add(new MenuBool("Wuse", "Use W", true));
            mainMenu.Add(Jungle);
            

            var Misc = new Menu("Misc", "[RVMP:R] Miscellaneous Settings");
            Misc.Add(new MenuKeyBind("back", "Silent back key", Keys.B, KeyBindType.Press));
            Misc.Add(new MenuSlider("skins", "Set skin", 1, 0, 17));
            Misc.Add(new MenuKeyBind("setskin", "^ Press to set skin", Keys.U, KeyBindType.Press));
            mainMenu.Add(Misc);

            var Draw = new Menu("Draw", "[RVMP:R] Draw Settinga");
            Draw.Add(new MenuBool("rangeW","W range",true));
            Draw.Add(new MenuBool("anav","Draw only if spell is ready",true));
            Draw.Add(new MenuBool("Qtime", "Show Q timer", true));
            mainMenu.Add(Draw);

            mainMenu.Attach();
            GameEvent.OnGameTick += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            Console.Write("[KDAio Loaded] By CnG ");
            Game.Print("<font color = '#00a6d6'>[RVMP:R] :: <font color = '#FFFF00'>has been loaded! Enjoy.\n");
        }

        public static void SetSkin(){
            if (mainMenu["Misc"].GetValue<MenuKeyBind>("setskin").Active){
                GameObjects.Player.SetSkin(mainMenu["Misc"].GetValue<MenuSlider>("skins").Value);
            }
        }
        public static void ComboLogic()
        {
            //Q CAST
            if (mainMenu["Combo"].GetValue<MenuBool>("Quse").Enabled && Q.IsReady() && Orbwalker.GetTarget() != null)
                {
                    var target = Orbwalker.GetTarget() as AIHeroClient;
                    if (target != null && !target.IsDead && target.InAutoAttackRange())
                    {
                        if (Player.HasBuff("asheqcastready"))
                        {
                            Q.Cast();
                        }
                    }
                }
            //R CAST
            if (mainMenu["Combo"].GetValue<MenuBool>("Ruse").Enabled && R.IsReady())
            {
                var target = TargetSelector.GetTarget(W.Range, DamageType.Magical);
                if (Player.CountEnemyHeroesInRange(W.Range) >= mainMenu["Combo"].GetValue<MenuSlider>("Rcount").Value && target.IsValidTarget(W.Range))
                {
                    R.Cast(target);
                }
            }

            //W CAST
            var targetW = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            if (mainMenu["Combo"].GetValue<MenuBool>("Wuse").Enabled && W.IsReady() && targetW.IsValidTarget(W.Range))
                {
                    W.Cast(targetW);
                }
            
        }

        public static void HarassLogic()
        {
            /*
            var target = TargetSelector.GetTarget(E.Range);
            var input = W.GetPrediction(target);
            if(target == null) return;
            if (mainMenu["Harass"].GetValue<MenuBool>("Wuse").Enabled)
            {
                if (target.IsValidTarget(W.Range) && W.IsReady() && input.Hitchance >= HitChance.High)
                {
                    W.Cast(input.UnitPosition);
                }
            }

            if (mainMenu["Harass"].GetValue<MenuSlider>("mana%").Value <= GameObjects.Player.ManaPercent)
            {
                if (mainMenu["Harass"].GetValue<MenuBool>("onlyr").Enabled)
                {
                    if (mainMenu["Harass"].GetValue<MenuBool>("Euse").Enabled && (GameObjects.Player.Distance(target.Position) > ObjectManager.Player.AttackRange) && target.IsValidTarget(E.Range) && target.HasBuff("TwitchDeadlyVenom"))
                    {
                        E.Cast();
                    }
                }
                else
                {
                    if (mainMenu["Harass"].GetValue<MenuBool>("Euse").Enabled && target.IsValidTarget(E.Range) && target.HasBuff("TwitchDeadlyVenom"))
                    {
                        E.Cast();
                    }
                }
            }
            */
        }

        public static void KSLogic()
        {
            /*
            foreach (var target in GameObjects.EnemyHeroes.Where(x=> x.IsValidTarget(E.Range)))
            {
                if(target==null) return;
                if (mainMenu["KS"].GetValue<MenuBool>("Euse").Enabled && E.IsReady())
                {
                    if (GameObjects.EnemyHeroes.Any(x => E.GetDamage(target) > target.Health - ObjectManager.Player.CalculateDamage(target, DamageType.Mixed, 1)))
                    {
                        E.Cast();
                    }
                }
            }
            */
        }

        private static void LaneClearLogic()
        {
            /*
            int cont = 0;
            foreach (var minion in GameObjects.EnemyMinions.Where(x=> x.IsValidTarget(E.Range)))
            {
                if(minion == null) return;
                
                var Edmg = E.GetDamage(minion);
                if (Edmg > minion.Health - ObjectManager.Player.CalculateDamage(minion, DamageType.Mixed, 1) &&
                    minion.HasBuff("TwitchDeadlyVenom"))
                {
                    cont++;
                }

                if (cont > 0)
                {
                    if (mainMenu["Farm"].GetValue<MenuBool>("Euse").Enabled)
                    {
                        if (cont >= mainMenu["Farm"].GetValue<MenuSlider>("Eminions").Value && E.IsReady())
                        {
                            E.Cast();
                        }
                    }
                }
            }
            */
        }

        private static void JungleClearLogic()
        {
            /*
            var mobs = GameObjects.Jungle.FirstOrDefault(x => x.IsValidTarget(E.Range));
            var inpput = W.GetPrediction(mobs);
            if(mobs == null) return;
            if(mainMenu["Jungle"].GetValue<MenuBool>("Euse").Enabled){
                var Edmg = E.GetDamage(mobs);
                if(Edmg>mobs.Health-ObjectManager.Player.CalculateDamage(mobs,DamageType.Mixed,1)){
                    E.Cast();
                }
            }
            if(mainMenu["Jungle"].GetValue<MenuBool>("Wuse").Enabled){
                if(mobs.IsValidTarget(W.Range) && inpput.Hitchance >= HitChance.High){
                    W.Cast(inpput.UnitPosition);
                }
            }
            */
        }
        private static void OnGameUpdate(EventArgs args){
            if(GameObjects.Player.IsDead) return;
            KSLogic();
            SetSkin();
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    ComboLogic();
                    break;
                case OrbwalkerMode.Harass:
                    HarassLogic();
                    break;
                case OrbwalkerMode.LaneClear:
                    LaneClearLogic();
                    JungleClearLogic();
                    break;
                
            }
            
        }
        public static void DrawText(Font fuente, String text, float posx, float posy, SharpDX.ColorBGRA color){
            fuente.DrawText(null,text,(int)posx,(int)posy,color);
        }


        public static double QTime(AIBaseClient target)
        {
            if (target.HasBuff("AsheQAttack"))
            {
                return Math.Max(0, target.GetBuff("AsheQAttack").EndTime) - Game.Time;
            }

            return 0;
        }
        
        
        public static void OnDraw(EventArgs args){
            if(mainMenu["Draw"].GetValue<MenuBool>("anav").Enabled){
                if(mainMenu["Draw"].GetValue<MenuBool>("rangeW").Enabled){
                    if(W.IsReady()){
                        Render.Circle.DrawCircle(GameObjects.Player.Position,W.Range,System.Drawing.Color.Cyan);
                    }
                }

            }else{
                if(mainMenu["Draw"].GetValue<MenuBool>("rangeW").Enabled){
                    Render.Circle.DrawCircle(GameObjects.Player.Position,W.Range,System.Drawing.Color.Cyan);
                }
            }


            if (mainMenu["Draw"].GetValue<MenuBool>("Qtime").Enabled && GameObjects.Player.HasBuff("AsheQAttack"))
            {
                var PlayerPos = Drawing.WorldToScreen(ObjectManager.Player.Position);
                
                DrawText(TextBold,"Q Time: "+Math.Round(QTime(ObjectManager.Player),MidpointRounding.ToEven),PlayerPos.X,PlayerPos.Y+50,SharpDX.Color.White);
            }
        }
    }
}
