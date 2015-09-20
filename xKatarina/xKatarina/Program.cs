using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using Color = System.Drawing.Color;
using SharpDX;

namespace xKatarina
{
    class Program
    {
       
        private static Spell.Targeted Q;
        private static Spell.Active W;
        private static Spell.Targeted E;
        private static Spell.Active R;
        private static Menu xMenu, ComboMenu, HarassMenu, KillstealMenu;
        public static AIHeroClient Player
        {
            get { return ObjectManager.Player; }

        }
        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoad;
        }
        private static void OnLoad(EventArgs args)
        {
            if (Player.ChampionName != "Katarina") return;
            TS.Init();
            Bootstrap.Init(null);
            Q = new Spell.Targeted(SpellSlot.Q, 675);
            W = new Spell.Active(SpellSlot.W, 375);
            E = new Spell.Targeted(SpellSlot.E, 700);
            R = new Spell.Active(SpellSlot.R, 550);

            xMenu = MainMenu.AddMenu("PROJECT-X: xKatarina", "PROJECT-X");
            xMenu.AddGroupLabel("xKatarina");
            xMenu.AddSeparator();
            xMenu.AddLabel("PROJECT-X by Xer0");
            ComboMenu = xMenu.AddSubMenu("Combo", "Combo");
            ComboMenu.AddGroupLabel("Combo-Settings");
            ComboMenu.Add("CQ", new CheckBox("Use Q", true));
            ComboMenu.Add("CW", new CheckBox("Use W", true));
            ComboMenu.Add("CE", new CheckBox("Use E", true));
            ComboMenu.Add("CR", new CheckBox("Use R", true));
            HarassMenu = xMenu.AddSubMenu("Harass", "Harass");
            HarassMenu.AddGroupLabel("Harass-Settings");
            HarassMenu.Add("HQ", new CheckBox("Use Q", true));
            HarassMenu.Add("HW", new CheckBox("Use W", false));
            HarassMenu.Add("HE", new CheckBox("Use E", false));
            KillstealMenu = xMenu.AddSubMenu("KillSteal", "Killsteal");
            KillstealMenu.AddGroupLabel("KillSteal Settings");
            KillstealMenu.Add("smartKS", new CheckBox("SmartKS", true));
            KillstealMenu.AddLabel("There are no options since everything is predicted + automated");
            
            Game.OnTick += Game_OnTick;
            Chat.Print("Project-X: Katarina(1.0) loaded.");
        }
        private static void Game_OnTick(EventArgs args)
        {
            if (Player.IsDead) return;
            if (!Player.HasBuff("katarinarsound"))
            {
                Orbwalker.DisableAttacking = false;
                Orbwalker.DisableMovement = false;
            }
            else
            {
                Orbwalker.DisableAttacking = true;
                Orbwalker.DisableMovement = true;
            }
                Killsteal();
               
            switch (Orbwalker.ActiveModesFlags)
            {
                case Orbwalker.ActiveModes.Combo:
                    Combo(ComboMenu["CQ"].Cast<CheckBox>().CurrentValue, ComboMenu["CW"].Cast<CheckBox>().CurrentValue, ComboMenu["CE"].Cast<CheckBox>().CurrentValue, ComboMenu["CR"].Cast<CheckBox>().CurrentValue);
                    break;

                case Orbwalker.ActiveModes.Harass:
                    Harass(HarassMenu["HQ"].Cast<CheckBox>().CurrentValue, HarassMenu["HW"].Cast<CheckBox>().CurrentValue, HarassMenu["HE"].Cast<CheckBox>().CurrentValue);
                    break;

                //case Orbwalker.ActiveModes.LaneClear:
                    //Laneclear();

                   // break;


            }
        }
        private double MarkDmg(AIHeroClient target)
        {
            return target.HasBuff("katarinaqmark") ? Player.GetSpellDamage(target, SpellSlot.Q) : 0;
        }
        private float GetComboDamage(AIHeroClient enemy)
        {
            double damage = 0d;

            if (Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            

            if (W.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (R.IsReady() || (R.State == SpellState.Surpressed && R.Level > 0))
                damage += Player.GetSpellDamage(enemy, SpellSlot.R) * 8;

            

            return (float)damage;
        }
        private static void Harass(bool useQ, bool useW, bool useE)
        {
           var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
           

            if (!target.HasBuffOfType(BuffType.Invulnerability) && !target.IsZombie && !target.IsDead)
            {
                if (useQ && Q.IsReady() && Player.Distance(target.Position) <= Q.Range)
                {
                    Q.Cast(target);
                }
                if (useE && E.IsReady() && Player.Distance(target.Position) < E.Range && !Q.IsReady())
                {


                    E.Cast(target);

                }
                if (useW && W.IsReady() && Player.Distance(target.Position) <= W.Range)
                {
                    W.Cast();
                }
            }
        }
        private static void Combo(bool useQ, bool useW, bool useE, bool useR)
        {
           var target = TargetSelector.GetTarget(E.Range, DamageType.Magical);
           

            if (!target.HasBuffOfType(BuffType.Invulnerability) && !target.IsZombie && !target.IsDead)
            {
                if (useQ && Q.IsReady() && Player.Distance(target.Position) <= Q.Range)
                {
                    Q.Cast(target);
                }
                if (useE && E.IsReady() && Player.Distance(target.Position) < E.Range && !Q.IsReady())
                {
                    
                    
                    E.Cast(target);
                    
                }
                if (useW && W.IsReady() && Player.Distance(target.Position) <= W.Range)
                {
                    W.Cast();
                }
                if (useR && R.IsReady() &&
                    Player.CountEnemiesInRange(R.Range) > 0)
                {
                    if (!Q.IsReady() && !E.IsReady() && !W.IsReady())
                    {
                        Orbwalker.DisableMovement = true;
                        Orbwalker.DisableAttacking = true;
                        R.Cast();
                    }
                }
                
                
            }
            
            

        }
        private static void Killsteal()
        {
            var smartks = KillstealMenu["smartKS"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            {
               foreach(AIHeroClient enemy in HeroManager.Enemies)
               {
                if (target != null && target.IsValidTarget(E.Range) && !target.IsDead && target.HealthPercent <= 40)
                { //QEW
                    if (Player.Distance(target.ServerPosition) <= E.Range && 
                        (Player.GetSpellDamage(target, SpellSlot.E) + Player.GetSpellDamage(target, SpellSlot.Q)  +
                         Player.GetSpellDamage(target, SpellSlot.W)) > target.Health + 20)
                    {
                        if (E.IsReady() && Q.IsReady() && W.IsReady())
                        {
                          
                            Q.Cast(target);
                            E.Cast(target);
                            
                            if (Player.Distance(target.ServerPosition) < W.Range)
                                W.Cast();
                            return;
                        }
                    }

                    if (Player.Distance(target.ServerPosition) <= E.Range && 
                       (Player.GetSpellDamage(target, SpellSlot.E) + Player.GetSpellDamage(target, SpellSlot.W)) >
                       target.Health + 20)
                    {
                        if (E.IsReady() && W.IsReady())
                        {
                            
                            E.Cast(target);
                           
                            if (Player.Distance(target.ServerPosition) < W.Range)
                                W.Cast();
                           
                            return;
                        }
                    }
                    if (Player.Distance(target.ServerPosition) <= E.Range && 
                        (Player.GetSpellDamage(target, SpellSlot.E) + Player.GetSpellDamage(target, SpellSlot.Q)) >
                        target.Health + 20)
                    {
                        if (E.IsReady() && Q.IsReady())
                        {
                           
                            E.Cast(target);
                            
                            Q.Cast(target);
                            //Game.PrintChat("ks 6");
                            return;
                        }
                    }

                    if ((Player.GetSpellDamage(target, SpellSlot.Q)) > target.Health + 20)
                    {
                        if (Q.IsReady() && Player.Distance(target.ServerPosition) <= Q.Range)
                        {
                            
                            Q.Cast(target);
                            
                            return;
                        }
                        
                    }
                    if (Player.Distance(target.ServerPosition) <= E.Range  &&
                        (Player.GetSpellDamage(target, SpellSlot.E)) > target.Health + 20)
                    {
                        if (E.IsReady())
                        {
                           
                            E.Cast(target);
                           
                           
                            return;
                        }
                    }

                    //R
                    if (Player.Distance(target.ServerPosition) <= E.Range &&
                        (Player.GetSpellDamage(target, SpellSlot.R) * 5) > target.Health +20)
                    {
                        if (R.IsReady())
                        {
                            Orbwalker.DisableAttacking = true;
                            Orbwalker.DisableMovement = true;
                            R.Cast();
                          
                            return;
                        }
                    }
                }





                }
            }
        }
    }
}
