using System;
using System.Linq;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp;
using SharpDX;
using Color = System.Drawing.Color;
using static EnsoulSharp.SDK.Items;
using static RVMPPrediction.RVMPPrediction;
using static RVMPExtension.Extensions;


namespace RVMPRebornCassiopeia
{
    static class Program
    {
        private static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        public static Spell _Q;
        public static Spell _W;
        public static Spell _E;
        public static Spell _R;
        public static Spell _Ignite;
        public static Item Seraph;
        public static Item Zhonia;
        private static float lastQTime = 0f;
        private static long LastQCast = 0;
        private static long LastECast = 0;
        private static Vector3 QPosition;

        private static Menu PredictionMenu, StartMenu, ComboMenu, LastHitM, DebugC, DrawingsMenu, JungleMenu, ClearMenu, UtilityMenu, RSet, ESet, WSet, QSet, otheroptions;



        public static void OnGameLoad()
        {
            if (!_Player.CharacterName.Contains("Cassiopeia"))
            {
                return;
            }
            Game.Print("<font color = '#00a6d6'>[RVMP:R] :: <font color = '#FFFF00'>has been loaded! Enjoy.\n");
            _Q = new Spell(SpellSlot.Q, 825);
            _W = new Spell(SpellSlot.W, 825);
            _E = new Spell(SpellSlot.E, 700);
            _R = new Spell(SpellSlot.R, 825);

            _Q.SetSkillshot(0.6f, 75f, float.MaxValue, false, SpellType.Circle);
            _W.SetSkillshot(0.25f, 90f, float.MaxValue, false, SpellType.Circle);
            _E.SetTargetted(0.25f, float.MaxValue);
            _R.SetSkillshot(0.5f, (float)(80 * Math.PI / 180), float.MaxValue, false, SpellType.Cone);


            Zhonia = new Item((int)ItemId.Zhonyas_Hourglass, 450);
            Seraph = new Item((int)ItemId.Seraphs_Embrace, 450);
            _Ignite = new Spell(ObjectManager.Player.GetSpellSlot("summonerdot"), 600);
            PredictionMenu = new Menu("PredictionMenu", "RVMP:Prediction Menu", true);
            PredictionMenu.Add(new MenuList("PredSelect", "Select Prediction:", new[] { "RVMP:R Prediction", "Common Prediction" }, 1));
            StartMenu = new Menu("Cassiopeia", "RVMP:Cassiopeia", true);
            ComboMenu = new Menu("General/Combo Settings", "General/Combo Settings");
            ClearMenu = new Menu("Clearing Menu", "Clearing Menu");
            JungleMenu = new Menu("JClearing Menu", "JClearing Menu");
            DrawingsMenu = new Menu("Drawing Settings", "Drawing Settings");
            DebugC = new Menu("Debug", "Debug");
            ComboMenu.Add(new MenuSeparator("Cassiopeia RVMP:R", "Cassiopeia RVMP:R"));
            ComboMenu.Add(new MenuBool("DisableAA", "Enable AA in Combo", true));
            QSet = new Menu("Q Spell Settings", "Q Spell Settings");
            QSet.Add(new MenuBool("UseQ", "Use [Q]"));
            QSet.Add(new MenuBool("UseQH", "Use [Q] in Harass"));
            QSet.Add(new MenuBool("UseQI", "Use always [Q] if enemy is immobile?"));
            QSet.Add(new MenuBool("UseQPok", "Use always [Q] if enemy is killable by Poison?"));
            QSet.Add(new MenuBool("QComboDash", "Always use [Q] on Dash end position?"));
            ComboMenu.Add(QSet);
            WSet = new Menu("W Spell Settings", "W Spell Settings");
            WSet.Add(new MenuBool("UseW", "Use [W]"));
            WSet.Add(new MenuBool("UseWH", "Use [W] in Harass", false));

            ComboMenu.Add(WSet);
            ESet = (new Menu("E Spell Settings", "E Spell Settings"));
            ESet.Add(new MenuBool("UseE", "Use [E]"));
            ESet.Add(new MenuBool("UseEH", "Use [E] in Harass"));
            ESet.Add(new MenuBool("UseEK", "Use [E] always if enemy is killable?"));
            ESet.Add(new MenuSlider("Edelay", "E Delay", 0, 0, 5));
            ComboMenu.Add(ESet);
            RSet = new Menu("R Spell Settings", "R Spell Settings NOT IMPLEMENTED YET");
            RSet.Add(new MenuBool("UseR", "Use [R]"));
            RSet.Add(new MenuBool("UseRFace", "Use [R] only on facing enemy ?"));
            RSet.Add(new MenuBool("RGapClose", "Try use [R] for Gapclosing enemy ?", false));
            RSet.Add(new MenuBool("Rint", "Try use [R] for interrupt enemy ?"));
            RSet.Add(new MenuBool("UseRG", "Use [R] use minimum enemys for R ?"));
            RSet.Add(new MenuSlider("UseRGs", "Minimum people for R", 1, 1, 5));
            ComboMenu.Add(RSet);
            otheroptions = (new Menu("Other options", "Other options"));
            otheroptions.Add(new MenuBool("Ignite", "Use Summoner Ignite if target is killable ?"));
            otheroptions.Add(new MenuBool("Zhonya", "Use Zhonya if dangerous ?"));
            otheroptions.Add(new MenuSlider("ZhonyaHP", "Zhonya Health for use?  %", 25));
            otheroptions.Add(new MenuBool("Seraph", "Use Seraph"));
            otheroptions.Add(new MenuSlider("SeraphHP", "Seraph's Health for use? %", 35));
            otheroptions.Add(new MenuSlider("SeraphMana", "Minimum Mana for Seraph's use? %", 40));
            ComboMenu.Add(otheroptions);
            //     ClearMenu.Add("EMode", new MenuList("Clear E mode", 0, "Always", "Poisoned"));
            StartMenu.Add(ComboMenu);
            ClearMenu.Add(new MenuBool("UseQCL", "Use [Q] in clear ?"));
            ClearMenu.Add(new MenuBool("UseWCL", "Use [W] in clear ?", false));
            ClearMenu.Add(new MenuBool("UseECL", "Use [E] in clear ?"));
            ClearMenu.Add(new MenuBool("UseQLH", "Use [Q] in LastHit ?", false));
            ClearMenu.Add(new MenuBool("UseWLH", "Use [W] in LastHit ?", false));
            ClearMenu.Add(new MenuBool("UseELH", "Use [E] in LastHit ?"));
            ClearMenu.Add(new MenuSlider("ClearMana", "Minimum mana for clear %", 50));
            StartMenu.Add(ClearMenu);
            JungleMenu.Add(new MenuBool("UseJQCL", "Use [Q] in Jclear ?"));
            JungleMenu.Add(new MenuBool("UseJWCL", "Use [W] in Jclear ?", false));
            JungleMenu.Add(new MenuBool("UseJECL", "Use [E] in Jclear ?"));
            JungleMenu.Add(new MenuBool("UseJQLH", "Use [Q] in JLastHit ?", false));
            JungleMenu.Add(new MenuBool("UseJWLH", "Use [W] in JLastHit ?", false));
            JungleMenu.Add(new MenuBool("UseJELH", "Use [E] in JLastHit ?"));
            JungleMenu.Add(new MenuSlider("ClearManaJ", "Minimum mana J for clear %", 50));
            StartMenu.Add(JungleMenu);
            DrawingsMenu.Add(new MenuSeparator("Drawing Settings", "Drawing Settings"));
            DrawingsMenu.Add(new MenuSeparator("Tick for enable/disable spell drawings", "Tick for enable/disable spell drawings"));
            DrawingsMenu.Add(new MenuBool("DQ", "Draw [Q] range"));
            DrawingsMenu.Add(new MenuBool("QPred", "Draw [Q] Prediction"));
            DrawingsMenu.Add(new MenuBool("QTarg", "Draw current target"));
            DrawingsMenu.Add(new MenuBool("DW", "Draw [W] range"));
            DrawingsMenu.Add(new MenuBool("DE", "Draw [E] range"));
            DrawingsMenu.Add(new MenuBool("DR", "Draw [R] range"));
            StartMenu.Add(DrawingsMenu);
            DebugC.Add(new MenuBool("Debug", "Debug Console+Chat", false));
            DebugC.Add(new MenuBool("DrawStatus1", "Debug Curret Orbawlker mode"));
            StartMenu.Add(DebugC);
            PredictionMenu.Attach();
            StartMenu.Attach();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            Interrupter.OnInterrupterSpell += Interruptererer;
            Dash.OnDash += Dash_OnDash;
            AntiGapcloser.OnGapcloser += Gapcloser_OnGapcloser;



        }



        private static void Game_OnUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead || ObjectManager.Player.IsRecalling() || MenuGUI.IsChatOpen || ObjectManager.Player.IsWindingUp)
            {
                return;
            }
            var target = TargetSelector.GetTarget(_Q.Range, DamageType.Magical);
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
                case OrbwalkerMode.LastHit:
                    LastHit();
                    break;
                case OrbwalkerMode.LaneClear:
                    LaneClear();
                    JungleClear();
                    break;
            }
            ImmobileQ();
            KillSteal();
            Zhonya();
            SeraphsEmbrace();
        }
        public static void LastHit()

        {
            var minion = GameObjects.EnemyMinions.Where(a => a.Distance(ObjectManager.Player) <= _E.Range).OrderBy(a => a.Health).FirstOrDefault();
            if (minion != null)
            {
                if (ClearMenu["UseELH"].GetValue<MenuBool>().Enabled && _E.IsReady() && minion.IsValidTarget(_E.Range) && minion.Health < ObjectManager.Player.GetSpellDamage(minion, SpellSlot.E))
                {
                    _E.Cast(minion);
                }
            }
            var MHR = GameObjects.EnemyMinions.Where(a => a.Distance(ObjectManager.Player) <= _Q.Range).OrderBy(a => a.Health).FirstOrDefault();
            if (MHR != null)
            {



                if (ClearMenu["UseQLH"].GetValue<MenuBool>().Enabled && _Q.IsReady() && ObjectManager.Player.ManaPercent > ClearMenu["ClearMana"].GetValue<MenuSlider>().Value && MHR.IsValidTarget(_Q.Range) &&
                    ObjectManager.Player.GetSpellDamage(MHR, SpellSlot.Q) >= MHR.Health)

                {
                    _Q.Cast(MHR.Position);
                }


                if (ClearMenu["UseWLH"].GetValue<MenuBool>().Enabled && _W.IsReady() && ObjectManager.Player.GetSpellDamage(MHR, SpellSlot.W) >= MHR.Health &&
                    ObjectManager.Player.ManaPercent > ClearMenu["ClearMana"].GetValue<MenuSlider>().Value)
                {
                    _W.Cast(MHR.Position);
                }
            }
        }

        public static void JungleClear()
        {
            var MHR = GameObjects.Jungle.Where(a => a.Distance(ObjectManager.Player) <= _Q.Range).OrderBy(a => a.Health).FirstOrDefault();
            if (MHR != null)
            {
                if (_Q.IsReady() && _Player.ManaPercent > JungleMenu["ClearManaJ"].GetValue<MenuSlider>().Value && JungleMenu["UseJQCL"].GetValue<MenuBool>().Enabled && MHR.IsValidTarget(_Q.Range))
                {
                    _Q.Cast(MHR);
                }
            }

            if (_W.IsReady() && _Q.IsReady() == false && _Player.ManaPercent > JungleMenu["ClearManaJ"].GetValue<MenuSlider>().Value && JungleMenu["UseJWCL"].GetValue<MenuBool>().Enabled && MHR.IsValidTarget(_W.Range))
            {
                _W.Cast(MHR.Position);
            }
            if (_E.IsReady() && _Player.ManaPercent > JungleMenu["ClearManaJ"].GetValue<MenuSlider>().Value && JungleMenu["UseJECL"].GetValue<MenuBool>().Enabled && MHR.IsValidTarget(_E.Range))
            {
                _E.Cast(MHR);
            }
        }

        public static void LaneClear()

        {
            if (_Q.IsReady() && ClearMenu["UseQCL"].GetValue<MenuBool>().Enabled)
            {
                foreach (var minion in GetEnemyLaneMinionsTargetsInRange(_Q.Range))
                {

                    if (minion.IsValidTarget(_Q.Range) && minion != null && ClearMenu["UseQCL"].GetValue<MenuBool>().Enabled)
                    {
                        _Q.CastOnUnit(minion);
                    }
                }
            }
            var MHR = GameObjects.EnemyMinions.Where(a => a.Distance(ObjectManager.Player) <= _Q.Range).OrderBy(a => a.Health).FirstOrDefault();
            if (MHR != null)

                if (ClearMenu["UseWCL"].GetValue<MenuBool>().Enabled)
                {
                    if (_W.IsReady())
                    {
                        _W.Cast(MHR.Position);
                    }

                }

            if (ClearMenu["UseECL"].GetValue<MenuBool>().Enabled && _E.IsReady() && ObjectManager.Player.ManaPercent > ClearMenu["ClearMana"].GetValue<MenuSlider>().Value && MHR.IsValidTarget(_E.Range))

            {
                _E.Cast(MHR);
            }
            foreach (var minion in GetEnemyLaneMinionsTargetsInRange(_E.Range))
            {

                if (minion.Health <= GameObjects.Player.GetSpellDamage(minion, SpellSlot.E))
                {
                    if (ClearMenu["UseELH"].GetValue<MenuBool>().Enabled)
                    {
                        if (minion.Distance(GameObjects.Player) > 250)
                        {
                            _E.CastOnUnit(minion);
                        }
                    }
                    if (ClearMenu["UseELH"].GetValue<MenuBool>().Enabled)
                    {
                        _E.CastOnUnit(minion);
                    }

                }

            }

        }

        public static void Harass()
        {
            var target = TargetSelector.GetTarget(_Q.Range, DamageType.Magical);
            if (target == null) return;
            if (ESet["UseEH"].GetValue<MenuBool>().Enabled)
            {
                if (!target.IsValidTarget(_E.Range) && !_E.IsReady())
                    return;
                {
                    if (_E.IsReady() && ESet["UseES"].GetValue<MenuBool>().Enabled)
                    {

                        if (Environment.TickCount >= LastECast + (1 * 100))
                            _E.Cast(target);

                    }

                }
            }
            if (WSet["UseWH"].GetValue<MenuBool>().Enabled)
            {
                if (!_W.IsReady() && _Player.Distance(target) >= 500) return;
                {


                    if (_W.IsReady() && Environment.TickCount > LastQCast + _Q.Delay * 1000)
                    {
                        _W.Cast(PreCastPos(target, _Player.Position.Distance(target.Position) / _W.Speed));

                    }

                }
            }

            if (QSet["UseQH"].GetValue<MenuBool>().Enabled)
            {
                if (_Q.IsReady())
                {
                    _Q.Cast(PreCastPos(target, 0.6f));
                }
            }

        }

        private static void Zhonya()
        {
            var Zhonya = otheroptions["Zhonya"].GetValue<MenuBool>().Enabled;
            var ZhonyaHP = otheroptions["ZhonyaHP"].GetValue<MenuSlider>().Value;
            if (!Zhonya || !Zhonia.IsReady || !Zhonia.IsOwned()) return;
            if (_Player.HealthPercent <= ZhonyaHP && _Player.CountEnemyHeroesInRange(500) >= 1)
            {
                Zhonia.Cast();
            }
        }
        private static void SeraphsEmbrace()
        {
            if (Seraph.IsReady && Seraph.IsOwned())
            {
                var embrace = otheroptions["Seraph"].GetValue<MenuBool>().Enabled;
                var shealth = otheroptions["SeraphHP"].GetValue<MenuSlider>().Value;
                var smana = otheroptions["SeraphMana"].GetValue<MenuSlider>().Value;
                if (!embrace || !Zhonia.IsReady || !Zhonia.IsOwned()) return;
                if (_Player.HealthPercent <= shealth && _Player.ManaPercent >= smana && _Player.CountEnemyHeroesInRange(500) >= 1)
                {
                    Seraph.Cast();
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var target = TargetSelector.GetTarget(_Q.Range, DamageType.Magical);
            var Combo = Orbwalker.ActiveMode == OrbwalkerMode.Combo;
            var LastHit = Orbwalker.ActiveMode == OrbwalkerMode.LastHit;
            var LaneClear = Orbwalker.ActiveMode == OrbwalkerMode.LaneClear;
            var Harass = Orbwalker.ActiveMode == OrbwalkerMode.Harass;

            if (DrawingsMenu["DQ"].GetValue<MenuBool>().Enabled && _Q.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _Q.Range, Color.Lime, 1);
            }
            if (DrawingsMenu["DE"].GetValue<MenuBool>().Enabled && _E.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _E.Range, Color.Lime, 1);
            }
            if (DrawingsMenu["DR"].GetValue<MenuBool>().Enabled && _R.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _R.Range, Color.Lime, 1);
            }
            if (DrawingsMenu["QPred"].GetValue<MenuBool>().Enabled && _Q.IsReady())
            {
                if (target == null)
                    return;
                Drawing.DrawCircle(_Q.GetPrediction(target).CastPosition, _Q.Width, System.Drawing.Color.Violet);
                {
                    if (Variables.GameTimeTickCount - lastQTime > 1000)
                        return;

                    Drawing.DrawCircle(QPosition, _Q.Width, System.Drawing.Color.Green);
                }
            }
            if (DrawingsMenu["QTarg"].GetValue<MenuBool>().Enabled && _Q.IsReady())
            {
                if (target == null)
                    return;
                Drawing.DrawCircle(target.Position, 150, Color.Red);
            }
        }

        private static void Combo()
        {

            var EDelay = ESet["Edelay"].GetValue<MenuSlider>().Value;
            var target = TargetSelector.GetTarget(_Q.Range, DamageType.Magical);
            var targetQ2 = TargetSelector.GetTarget(_Q.Range, DamageType.Magical);
            if (target == null)
            {
                return;
            }
            if (PredictionMenu["PredSelect"].GetValue<MenuList>().Index == 0)
            {


                if (_E.IsReady() & target.IsValidTarget(_E.Range))
                {
                    if (Environment.TickCount >= LastECast + (EDelay * 100))
                        _E.Cast(target);
                }
                if (_Q.IsReady())
                {

                    _Q.Cast(PreCastPos(target, 0.6f));

                }
                if (_W.IsReady() && Environment.TickCount > LastQCast + _Q.Delay * 1000)
                {
                    _W.Cast(PreCastPos(target, _Player.Position.Distance(target.Position) / _W.Speed));

                }
            }
            if (PredictionMenu["PredSelect"].GetValue<MenuList>().Index == 1)
            {
                if (_E.IsReady() & target.IsValidTarget(_E.Range))
                {

                    _E.Cast(target);
                }
                if (_Q.IsReady())
                {

                    _Q.Cast(target);

                }
                if (_W.IsReady() && Environment.TickCount > LastQCast + _Q.Delay * 1000)
                {
                    _W.Cast(target);

                }
            }

        }
        private static void Interruptererer(AIBaseClient sender, Interrupter.InterruptSpellArgs args)
        {
            var target = TargetSelector.GetTarget(_R.Range, DamageType.Magical);
            if (target == null)
            {
                return;
            }
            var RintTarget = TargetSelector.GetTarget(_R.Range, DamageType.Magical);
            if (RintTarget == null) return;
            if (_R.IsReady() && sender.IsValidTarget(_R.Range) && ComboMenu["Rint"].GetValue<MenuBool>().Enabled)
                _R.Cast(RintTarget);

        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs e)
        {
            var target = TargetSelector.GetTarget(_R.Range, DamageType.Magical);
            if (target == null)
            {
                return;
            }
            if (!ComboMenu["RGapClose"].GetValue<MenuBool>().Enabled) return;
            if (sender.IsEnemy)
                _R.Cast(sender);
        }
        private static void Dash_OnDash(AIBaseClient sender, Dash.DashArgs e)
        {
            var target = TargetSelector.GetTarget(_Q.Range, DamageType.Magical);
            if (target == null)
            {
                return;
            }
            if (!QSet["QComboDash"].GetValue<MenuBool>().Enabled) return;
            if (!sender.IsEnemy) return;
            if (!_Q.IsReady()) return;
            if (e.EndPos.IsValid())
                _Q.Cast(e.EndPos);
        }
        public static void KillSteal()
        {
            var targetQ = TargetSelector.GetTarget(_Q.Range, DamageType.Magical);
            var targetE = TargetSelector.GetTarget(_E.Range, DamageType.Magical);
            if (targetQ == null)
            {
                return;
            }
            if (targetE == null)
            {
                return;
            }
            if (QSet["UseQPok"].GetValue<MenuBool>().Enabled)
            {
                var Qpred = _Q.GetPrediction(targetQ);
                if (Qpred.Hitchance >= HitChance.High && targetQ.IsValidTarget(_Q.Range))
                {
                    if (targetQ.Health + targetQ.PhysicalShield < _Player.GetSpellDamage(targetQ, SpellSlot.Q))
                    {
                        if (!targetQ.IsValidTarget(_Q.Range) && !_Q.IsReady()) return;
                        {
                            _Q.Cast(targetQ);
                        }
                    }
                }
            }

            if (ESet["UseEK"].GetValue<MenuBool>().Enabled)
            {
                if (targetE.Health + targetE.PhysicalShield < _Player.GetSpellDamage(targetE, SpellSlot.E))
                {
                    if (!targetE.IsValidTarget(_E.Range) && !_E.IsReady()) return;
                    {
                        {
                            _E.Cast(targetE);
                        }
                    }
                }

            }
        }
        private static void ImmobileQ()
        {

            var target = TargetSelector.GetTarget(_Q.Range, DamageType.Magical);
            if (target == null)
            {
                return;
            }
            if (QSet["UseQ"].GetValue<MenuBool>().Enabled && QSet["UseQI"].GetValue<MenuBool>().Enabled)

            {
                if (_Q.IsReady())
                {

                    var Qpred = _Q.GetPrediction(target);
                    if (Qpred.Hitchance >= HitChance.Immobile && target.IsValidTarget(_Q.Range))
                    {
                        _Q.Cast(target);
                        if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                        {

                            Game.Print("Casting Q for immobile enemy");
                            Console.WriteLine("Casting Q for immobile enemy ");
                        }
                    }
                }
            }

        }

    }

}