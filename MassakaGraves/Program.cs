using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using System.Threading.Tasks;

namespace MassakaGraves
{
    class Program
    {
        private static Menu nmenu;
        static Spell Q;
        static Spell W;
        static Spell E;
        static Spell R;
        private const int Hpid = 2003;
        private const int Mpid = 2004;
        private static Orbwalking.Orbwalker orbwalk;
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        public static Orbwalking.Orbwalker Orbwalker;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Game.OnUpdate += OnUpdate;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {

            /*if (Player.ChampionName != "Graves")
            {
                return;
            }*/

            GravesSpells();


            nmenu = new Menu("Graves", "Massaka Graves", true);
            var tsmenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(tsmenu);
            nmenu.AddSubMenu(tsmenu);
            {
                nmenu.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
                Orbwalker = new Orbwalking.Orbwalker(nmenu.SubMenu("Orbwalking"));



                nmenu.AddSubMenu(new Menu("Combo", "Combo"));
                nmenu.SubMenu("Combo").AddItem(new MenuItem("SemiR", "Semi-Manual Cast R").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
                nmenu.SubMenu("Combo").AddItem(new MenuItem("useE", "Use E in Combo")).SetValue(true);
                nmenu.SubMenu("Combo").AddItem(new MenuItem("autopots", "Smart Auto Pots").SetValue(true));
                nmenu.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

                nmenu.AddSubMenu(new Menu("Harrass", "Harrass"));
                nmenu.SubMenu("Harrass").AddItem(new MenuItem("useQHarrass", "Use Q").SetValue(true));
                nmenu.SubMenu("Harrass").AddItem(new MenuItem("autoqharrass", "Auto Q").SetValue(false));

                nmenu.AddSubMenu(new Menu("KS", "KS"));
                nmenu.SubMenu("KS").AddItem(new MenuItem("ksQ", "KS with Q").SetValue(true));
                nmenu.SubMenu("KS").AddItem(new MenuItem("ksR", "KS with R").SetValue(false));
                nmenu.SubMenu("KS").AddItem(new MenuItem("eforks", "Use E to get in KS Range").SetValue(true));

                nmenu.AddSubMenu(new Menu("Draw", "Draw"));
                nmenu.SubMenu("Draw").AddItem(new MenuItem("onlyifrdy", "Draw only if Ready").SetValue(false));
                nmenu.SubMenu("Draw").AddItem(new MenuItem("DrawQ", "Draw Q").SetValue(new Circle(true, Color.Green)));
                nmenu.SubMenu("Draw").AddItem(new MenuItem("DrawW", "Draw W").SetValue(new Circle(true, Color.Green)));
                nmenu.SubMenu("Draw").AddItem(new MenuItem("DrawE", "Draw E").SetValue(new Circle(true, Color.Green)));
                nmenu.SubMenu("Draw").AddItem(new MenuItem("DrawR", "Draw R").SetValue(new Circle(true, Color.Green)));
                
                Game.OnUpdate += OnUpdate;
                nmenu.AddToMainMenu();
            }

        }

        private static void GravesSpells()
        {

            Q = new Spell(SpellSlot.Q, 910f);
            Q.SetSkillshot(0.26f, 10f * 2 * (float)Math.PI / 180, 1950, false, SkillshotType.SkillshotCone);

            W = new Spell(SpellSlot.W, 1100f);
            W.SetSkillshot(0.30f, 250f, 1650f, false, SkillshotType.SkillshotCircle);

            R = new Spell(SpellSlot.R, 1100f);
            R.SetSkillshot(0.22f, 150f, 2100, true, SkillshotType.SkillshotLine);
        }

        public void Drawing_OnDraw(EventArgs args)
        {
            //Q Range
            if (Program.nmenu.Item("DrawQ").GetValue<bool>())
                if (Q.IsReady())
                    Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.HotPink);

            //W Range
            if (Program.nmenu.Item("DrawW").GetValue<bool>())
                if (W.IsReady())
                    Utility.DrawCircle(ObjectManager.Player.Position, W.Range, Color.HotPink);

            //E Range
            if (Program.nmenu.Item("DrawE").GetValue<bool>())
                if (E.IsReady())
                    Utility.DrawCircle(ObjectManager.Player.Position, E.Range, Color.HotPink);

            //R Range
            if (nmenu.Item("DrawR").GetValue<bool>())
                if (R.IsReady())
                    Utility.DrawCircle(ObjectManager.Player.Position, R.Range, Color.HotPink);
        }

        private static void OnUpdate(EventArgs args)
        {

            Combo();
            KS();
        
        }

        public static void Combo()
        {
            if (nmenu.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                if (target.IsValidTarget() && Q.GetDamage(target) + W.GetDamage(target) + R.GetDamage(target) > target.Health)
                {
                    Q.Cast(target);
                    W.Cast(target);
                    R.Cast(target);
                }
                else if (target.IsValidTarget())
                {
                    Q.Cast(target);
                    W.Cast(target);
                    if (R.GetDamage(target) > target.Health)
                    {
                        R.Cast(target);
                    }
                }
                else
                {
                    return;
                }
            }
        }
        public static void KS()
        {
            if (nmenu.Item("KsQ").GetValue<bool>())
            {
                if (nmenu.Item("eforks").GetValue<bool>())
                {
                    var target = TargetSelector.GetTarget(Q.Range + E.Range, TargetSelector.DamageType.Physical);
                    var QERANGE = Q.Range + E.Range;
                    if (Player.Distance(target.ServerPosition) <= QERANGE && Q.GetDamage(target) > target.Health)
                        E.Cast(target);
                    Q.Cast(target);
                }
                else
                {
                    var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                    if (Q.GetDamage(target) > target.Health)
                    {
                        Q.Cast(target);
                    }
                }
            }
            else if (nmenu.Item("KsR").GetValue<bool>())
            {
                if (nmenu.Item("eforks").GetValue<bool>())
                {
                    var target = TargetSelector.GetTarget(R.Range + E.Range, TargetSelector.DamageType.Physical);
                    var RERANGE = R.Range + E.Range;
                    if (Player.Distance(target.ServerPosition) <= RERANGE && R.GetDamage(target) > target.Health)
                        E.Cast(target);
                    R.Cast(target);
                }
                else
                {
                    var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                    if (R.GetDamage(target) > target.Health)
                    {
                        R.Cast(target);
                    }
                }
            }
            else if (nmenu.Item("ksQ").GetValue<bool>() && nmenu.Item("ksR").GetValue<bool>())
            {
                if (nmenu.Item("eforks").GetValue<bool>())
                {
                    var target = TargetSelector.GetTarget(Q.Range + E.Range, TargetSelector.DamageType.Physical);
                    var QRERANGE = Q.Range + E.Range;
                    if (Player.Distance(target.ServerPosition) <= QRERANGE && Q.GetDamage(target) + R.GetDamage(target) > target.Health && target.Health > Q.GetDamage(target))
                    {
                        E.Cast(target);
                        Q.Cast(target);
                        R.Cast(target);
                    }
                }
                else
                {
                    var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                    var QDMG = Q.GetDamage(target);
                    var RDMG = R.GetDamage(target);
                    var QRDMG = QDMG + RDMG;
                    if (QRDMG > target.Health)
                    {
                        Q.Cast(target);
                        R.Cast(target);
                    }
                }
            }
            else
            {
                return;
            }
        }
    
        public static void AutoPots()
        {
            var QMANA = Q.Instance.ManaCost;
            var WMANA = W.Instance.ManaCost;
            var EMANA = E.Instance.ManaCost;
            var RMANA = R.Instance.ManaCost;
            var COMBOMANA = QMANA + WMANA + EMANA + RMANA;

            var MinHPL = ObjectManager.Player.Health / 100 * 50;
            var MinHPCombo = ObjectManager.Player.Health / 100 * 70;
            var PlayerHealth = ObjectManager.Player.Health;
            if (PlayerHealth <= MinHPL && Utility.CountEnemiesInRange((int)W.Range) <= 0 && nmenu.Item("autopots").GetValue<bool>())
            {
                Items.UseItem(Mpid);
            }
            else if (Utility.CountEnemiesInRange((int)W.Range) >= 0 && nmenu.Item("autopots").GetValue<bool>() && Player.Mana < COMBOMANA)
            {
                Items.UseItem(Mpid);
            }
        }

        public static void autoqharrass()
        {
            if (nmenu.Item("autoqharrass").GetValue<bool>() && Player.Mana > Player.Mana / 100 * 60)
            {
                var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                Q.Cast(target);
            }
        }

        public static void SemiR()
        {
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
            if (nmenu.Item("SemiR").GetValue<KeyBind>().Active && target.IsValidTarget())
            {
                R.CastIfWillHit(target, 1);
            }
        }
    }
}