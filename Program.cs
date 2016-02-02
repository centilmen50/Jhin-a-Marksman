using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using JhinaMarksman;

namespace JhinaMarksman
{
    internal class Program
    {
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static bool ultimate = false;
        public static int ultimatecount = 0;
        public static Spell.Targeted Q;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        public static Spell.Skillshot R1;
        static Item Healthpot;
        public static DamageIndicator Indicator;
        public static readonly string[] JungleMobsList = { "SRU_Red", "SRU_Blue", "SRU_Dragon", "SRU_Baron", "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak", "SRU_Krug", "Sru_Crab" };
        public static Menu Menu, ComboSettings, HarassSettings, ClearSettings, AutoSettings, DrawMenu, Items, Skin;
        
        private static void Loading_OnLoadingComplete(EventArgs args)
        {
           if (Player.Instance.Hero != Champion.Jhin)
            {
                return;
            }
            Teleport.OnTeleport += Teleport_OnTeleport;
            
            Indicator = new DamageIndicator();
            Healthpot = new Item(2003, 0);
            Q = new Spell.Targeted(SpellSlot.Q, 550);
            W = new Spell.Skillshot(SpellSlot.W, 3000, SkillShotType.Linear,250 , 1200, 60)
            {
                MinimumHitChance = HitChance.Medium,
                AllowedCollisionCount = int.MaxValue
            };
            E = new Spell.Skillshot(SpellSlot.E, 750, SkillShotType.Circular, 250, 3000, 1);
            R = new Spell.Skillshot(SpellSlot.R, 25000, SkillShotType.Cone, 250, 1200, 500);
            R1 = new Spell.Skillshot(SpellSlot.R, 25000, SkillShotType.Linear, 250, 1200, 60); 

            Menu = MainMenu.AddMenu("Jhin a Marksman", "JhinaMarksman");

            ComboSettings = Menu.AddSubMenu("Combo Settings", "ComboSettings");
            ComboSettings.Add("useQCombo", new CheckBox("Use Q"));
            ComboSettings.Add("useQEnemyCount", new Slider("Q Enemy Count >= ", 1, 1, 5));
            ComboSettings.Add("useWCombo", new CheckBox("Use W"));
            ComboSettings.Add("useWComboOnlyCC", new CheckBox("Use W Only CC", false));
            ComboSettings.Add("useECombo", new CheckBox("Use E"));
            ComboSettings.Add("useEDistance", new CheckBox("Auto E for Enemy Distance"));
            ComboSettings.Add("EMaxDistance", new Slider("Enemy Distance < ", 400, 100, 900));
            ComboSettings.Add("useRCombo", new CheckBox("Use R"));
            ComboSettings.Add("RKey", new KeyBind("Press Key to Activate Auto R", false, KeyBind.BindTypes.HoldActive, 'G'));

            HarassSettings = Menu.AddSubMenu("Harass Settings", "HarassSettings");
            HarassSettings.Add("useQHarass", new CheckBox("Use Q"));
            HarassSettings.Add("HarassQEnemyCount", new Slider("Q Enemy Count >= ", 1, 1, 5));
            HarassSettings.Add("useQHarassMana", new Slider("Q Mana > %", 20, 0, 100));
            HarassSettings.Add("useWHarass", new CheckBox("Use W"));
            HarassSettings.Add("useWHarassMana", new Slider("W Mana > %", 20, 0, 100));
            HarassSettings.Add("useEHarass", new CheckBox("Use E"));
            HarassSettings.Add("useEHarassMana", new Slider("E Mana > %", 35, 0, 100));
            HarassSettings.AddSeparator();
            HarassSettings.AddLabel("Auto Harass");
            HarassSettings.Add("autoQHarass", new CheckBox("Auto Q for Harass", false));
            HarassSettings.Add("autoQHarassEnemyCount", new Slider("Q Enemy Count >= ", 2, 0, 5));
            HarassSettings.Add("autoQHarassMana", new Slider("Q Mana > %", 35, 0, 100));
            HarassSettings.Add("autoWHarass", new CheckBox("Auto W for Harass", false));
            HarassSettings.Add("autoWHarassMana", new Slider("W Mana > %", 35, 0, 100));

            ClearSettings = Menu.AddSubMenu("Lane Clear Settings", "FarmSettings");
            ClearSettings.AddLabel("Lane Clear");
            ClearSettings.Add("useQFarm", new CheckBox("Use Q"));
            ClearSettings.Add("useQFarmCount", new Slider("Q Min. Minion Count", 3, 1, 4));
            ClearSettings.Add("FarmQMana", new Slider("Q Mana > %", 35, 0, 100));
            ClearSettings.AddSeparator();
            ClearSettings.AddLabel("Last Hit");
            ClearSettings.Add("useQLastHit", new CheckBox("Use Q Killable Minions"));
            ClearSettings.Add("LastHitQCount", new Slider("Min Minion Count >", 2, 0, 4));
            ClearSettings.Add("LastHitQMana", new Slider("Q Mana > %", 35, 0, 100));
            ClearSettings.AddSeparator();
            ClearSettings.AddLabel("Jungle Clear");
            ClearSettings.Add("useQJungle", new CheckBox("Use Q"));
            ClearSettings.Add("useQJungleMana", new Slider("Q Mana > %", 20, 0, 100));
            ClearSettings.Add("useWJungle", new CheckBox("Use W"));
            ClearSettings.Add("useWJungleMana", new Slider("W Mana > %", 20, 0, 100));
            ClearSettings.AddSeparator();
            ClearSettings.Add("RJungleSteal", new CheckBox("Jungle Steal(partially working now)",false));
            ClearSettings.AddSeparator();
            ClearSettings.AddLabel("Epics");
            ClearSettings.Add("SRU_Baron", new CheckBox("Baron"));
            ClearSettings.Add("SRU_Dragon", new CheckBox("Dragon"));
            ClearSettings.AddLabel("Buffs");
            ClearSettings.Add("SRU_Blue", new CheckBox("Blue"));
            ClearSettings.Add("SRU_Red", new CheckBox("Red"));

            AutoSettings = Menu.AddSubMenu("Misc Settings", "MiscSettings");
            AutoSettings.Add("gapcloser", new CheckBox("Auto E for Gapcloser"));
            AutoSettings.Add("interrupter", new CheckBox("Auto E for Interrupter"));
            AutoSettings.Add("CCE", new CheckBox("Auto E on Enemy CC"));
            AutoSettings.Add("UsePassive", new CheckBox("Use Passive"));
            AutoSettings.AddLabel("LaneClear,LastHit Mods 4.Passive Stacks Auto Harass to Enemy");

            Skin = Menu.AddSubMenu("Skin Changer", "SkinChanger");
            Skin.Add("checkSkin", new CheckBox("Use Skin Changer"));
            Skin.Add("skin.Id", new Slider("Skin", 1, 0, 1));

            Items = Menu.AddSubMenu("Item Settings", "ItemSettings");
            Items.Add("useHP", new CheckBox("Use Health Potion"));
            Items.Add("useHPV", new Slider("HP < %", 40, 0, 100));
            Items.AddSeparator();
            Items.Add("useBOTRK", new CheckBox("Use BOTRK"));
            Items.Add("useBotrkMyHP", new Slider("My Health < ", 60, 1, 100));
            Items.Add("useBotrkEnemyHP", new Slider("Enemy Health < ", 60, 1, 100));
            Items.Add("useYoumu", new CheckBox("Use Youmu"));
            Items.Add("useQSS", new CheckBox("Use QSS"));

            DrawMenu = Menu.AddSubMenu("Drawing Settings");
            DrawMenu.Add("drawRange", new CheckBox("Draw AA Range",false));
            DrawMenu.Add("drawQ", new CheckBox("Draw Q Range"));
            DrawMenu.Add("drawW", new CheckBox("Draw W Range"));
            DrawMenu.Add("drawE", new CheckBox("Draw E Range"));
            DrawMenu.Add("drawR", new CheckBox("Draw R Range",false));
            DrawMenu.Add("drawStatus", new CheckBox("Draw R KeyBind"));
            DrawMenu.AddSeparator();
            DrawMenu.AddLabel("Damage Calculation");
            DrawMenu.Add("draw.Damage", new CheckBox("Draw Damage"));
            DrawMenu.Add("draw.Q", new CheckBox("Q Calculate"));
            DrawMenu.Add("draw.W", new CheckBox("W Calculate"));
            DrawMenu.Add("draw.E", new CheckBox("E Calculate"));
            DrawMenu.Add("draw.R", new CheckBox("R Calculate"));
            DrawMenu.AddSeparator();
            DrawMenu.AddLabel("Recall Tracker");
            DrawMenu.Add("draw.Recall", new CheckBox("Chat Print",false));

            Game.OnTick += Game_OnTick;
            Game.OnUpdate += OnGameUpdate;
            Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs e)
        {
            if (AutoSettings["interrupter"].Cast<CheckBox>().CurrentValue && sender.IsEnemy &&
                e.DangerLevel >= DangerLevel.Medium && sender.IsValidTarget(900))
            {
                E.Cast(sender);
            }
        }
        //Recall Tracker Start
        private static string FormatTime(double time)
        {
            var t = TimeSpan.FromSeconds(time);
            return string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
        }

        private static void Teleport_OnTeleport(Obj_AI_Base sender, Teleport.TeleportEventArgs args)
        {
            if (sender.Team == _Player.Team || !DrawMenu["draw.Recall"].Cast<CheckBox>().CurrentValue) return;

            if (args.Status == TeleportStatus.Start)
            {
                Chat.Print("<font color='#ffffff'>[" + FormatTime(Game.Time) + "]</font> " + sender.BaseSkinName + " has <font color='#00ff66'>started</font> recall.");
            }

            if (args.Status == TeleportStatus.Abort)
            {
                Chat.Print("<font color='#ffffff'>[" + FormatTime(Game.Time) + "]</font> " + sender.BaseSkinName + " has <font color='#ff0000'>aborted</font> recall.");
            }

            if (args.Status == TeleportStatus.Finish)
            {
                Chat.Print("<font color='#ffffff'>[" + FormatTime(Game.Time) + "]</font> " + sender.BaseSkinName + " has <font color='#fdff00'>finished</font> recall.");
            }
        }
        //Recall Tracker Finish
        private static void Game_OnTick(EventArgs args)
        {

            var HPpot = Items["useHP"].Cast<CheckBox>().CurrentValue;
            var HPv = Items["useHPv"].Cast<Slider>().CurrentValue;
            Orbwalker.ForcedTarget = null;

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                ComboMedium();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                WaveClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                LastHit();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                //JungleClear();
            }
            
            Auto();
            KS();
            AutoW();
            if (HPpot && _Player.HealthPercent < HPv)
            {
                if (Item.HasItem(Healthpot.Id) && Item.CanUseItem(Healthpot.Id) && !_Player.HasBuff("RegenerationPotion"))
                {
                    Healthpot.Cast();
                }
            }
        }

        private static void UltiActive()
        {
            var target = TargetSelector.GetTarget(R1.Range, DamageType.Physical);
            if (target == null) return;

            Orbwalker.DisableMovement = true;

                for (int i = 0; i <= 4; i++)
                {

                        R1.Cast(target);
                        ultimatecount = i;
                        if (i == 4)
                        {
                        ultimate = false;
                        }
                                  
                }        
        }

        private static void OnGameUpdate(EventArgs args)
        {
            if (checkSkin())
            {
                Player.SetSkinId(SkinId());
            }
            if (ComboSettings["RKey"].Cast<KeyBind>().CurrentValue)
            {
                UltiActive();
            }
            if(ultimate == false)
            {
                Orbwalker.DisableMovement = false;

            }
        }

        public static int SkinId()
        {
            return Skin["skin.Id"].Cast<Slider>().CurrentValue;
        }
        public static bool checkSkin()
        {
            return Skin["checkSkin"].Cast<CheckBox>().CurrentValue;
        }

        public static void Auto()
        {//AUTO SETTİNGS START
            var useQSS = Items["useQSS"].Cast<CheckBox>().CurrentValue;
            var EonCC = AutoSettings["CCE"].Cast<CheckBox>().CurrentValue;
            if (EonCC)
            {
                foreach (var enemy in EntityManager.Heroes.Enemies)
                {
                    if (enemy.Distance(Player.Instance) < E.Range &&
                        (enemy.HasBuffOfType(BuffType.Stun)
                         || enemy.HasBuffOfType(BuffType.Snare)
                         || enemy.HasBuffOfType(BuffType.Suppression)
                         || enemy.HasBuffOfType(BuffType.Fear)
                         || enemy.HasBuffOfType(BuffType.Knockup)))
                    {
                        E.Cast(enemy);
                    }
                }
            }
           
          
            if (_Player.HasBuffOfType(BuffType.Fear) || _Player.HasBuffOfType(BuffType.Stun) || _Player.HasBuffOfType(BuffType.Taunt) || _Player.HasBuffOfType(BuffType.Polymorph))
            {
                
                if (useQSS && Item.HasItem(3140) && Item.CanUseItem(3140))
                    Item.UseItem(3140);

                if (useQSS && Item.HasItem(3139) && Item.CanUseItem(3139))
                    Item.UseItem(3139);
            }
        }//AUTO SETTİNGS END
        public static void AutoW()
        {//AUTO W and AUTO Q START
            var targetQ = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var qcount = HarassSettings["HarassQEnemyCount"].Cast<Slider>().CurrentValue;
            var targetW = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            var wPred = W.GetPrediction(targetW);
            foreach (var enemy in EntityManager.Heroes.Enemies.Where(
                                a => a.IsValidTarget(Q.Range))
                                .OrderBy(TargetSelector.GetPriority))
            {
                if (HarassSettings["autoQHarass"].Cast<CheckBox>().CurrentValue && enemy.CountEnemiesInRange(Q.Range) >= qcount &&
                 Q.IsReady() && targetQ.IsValidTarget(Q.Range) && _Player.ManaPercent > HarassSettings["autoQHarassMana"].Cast<Slider>().CurrentValue)
                {
                    Q.Cast(targetQ);
                }
            }
            if (HarassSettings["autoWHarass"].Cast<CheckBox>().CurrentValue &&
                wPred.HitChance >= HitChance.Medium && W.IsReady() && targetW.IsValidTarget(W.Range) && _Player.ManaPercent > HarassSettings["autoWHarassMana"].Cast<Slider>().CurrentValue)
            {
                W.Cast(targetW);
            }
        }//AUTO W and AUTO Q END
        public static void LastHit()
        {//LASTHİT START
            var useq = ClearSettings["useQLastHit"].Cast<CheckBox>().CurrentValue;
            var qcount = ClearSettings["LastHitQCount"].Cast<Slider>().CurrentValue;
            var autopassive = AutoSettings["UsePassive"].Cast<CheckBox>().CurrentValue;
            var buff = Player.HasBuff("jhinpassiveattackbuff");
            var unit =
                    EntityManager.MinionsAndMonsters.GetLaneMinions()
                        .Where(
                            a =>
                                a.IsValidTarget(Q.Range) &&
                                a.Health < _Player.GetAutoAttackDamage(a) * 1.1)
                        .FirstOrDefault(minion => EntityManager.MinionsAndMonsters.EnemyMinions.Count(
                            a => a.Distance(minion) < 300 && a.Health < _Player.GetAutoAttackDamage(a) * 1.1) > qcount);
                         
            if(unit != null && !unit.IsDead && !unit.IsZombie)
            {
                if(useq)
                {
                    Q.Cast(unit);
                }
            }

            foreach (var enemy in EntityManager.Heroes.Enemies.Where(
                               a => a.IsValidTarget(_Player.AttackRange))
                               .OrderBy(TargetSelector.GetPriority))
            {
                if (autopassive && buff)
                {
                    Orbwalker.ForcedTarget = enemy;
                    Harass();
                }
                else
                {
                    return;
                }
            }
        }//LASTHİT END

        public static void Flee()
        {//Flee START
            
        }//Flee END

        public static void JungleClear()
        { //Jungle Clear START
            var menu = ClearSettings["useQJungle"].Cast<CheckBox>().CurrentValue;
                var unit =
                    EntityManager.MinionsAndMonsters.GetJungleMonsters()
                        .Where(
                            a =>
                                a.IsValidTarget(Q.Range) &&
                              a.Health < _Player.GetAutoAttackDamage(a) * 1.1);  
        } // Jungle Clear END

        public static void WaveClear()
        {//LANE CLEAR START
            var useq = ClearSettings["useQFarm"].Cast<CheckBox>().CurrentValue;
            var qcount = ClearSettings["UseQFarmCount"].Cast<Slider>().CurrentValue;
            var autopassive = AutoSettings["UsePassive"].Cast<CheckBox>().CurrentValue;
            var unit =
                    EntityManager.MinionsAndMonsters.GetLaneMinions()
                        .Where(
                            a =>
                                a.IsValidTarget(Q.Range))
                        .FirstOrDefault(minion => EntityManager.MinionsAndMonsters.EnemyMinions.Count(
                            a => a.Distance(minion) < 400) >= qcount);

            if (unit != null && !unit.IsDead && !unit.IsZombie)
            {
                if (useq && Q.IsReady())
                {
                    Q.Cast(unit);
                }
            }
            foreach (var enemy in EntityManager.Heroes.Enemies.Where(
                               a => a.IsValidTarget(_Player.AttackRange))
                               .OrderBy(TargetSelector.GetPriority))
            {
                if(autopassive && _Player.HasBuff("jhinpassiveattackbuff"))
                {                   
                    Orbwalker.ForcedTarget = enemy;
                    Harass();
                }
                else
                {
                    return;
                }
            }
            }//LANE CLEAR END

        public static void Harass()
        {//HARASS START
            var targetQ = TargetSelector.GetTarget(Q.Range, DamageType.Physical);         
            var qcount = HarassSettings["HarassQEnemyCount"].Cast<Slider>().CurrentValue;
            var targetW = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            var Wmana = HarassSettings["useWHarassMana"].Cast<Slider>().CurrentValue;
            var Qmana = HarassSettings["useQHarassMana"].Cast<Slider>().CurrentValue;

            if (Orbwalker.IsAutoAttacking) return;

            if (targetW != null)
            {
                // W out of range
                if (HarassSettings["useWHarass"].Cast<CheckBox>().CurrentValue && W.IsReady() &&
                    targetW.Distance(_Player) > _Player.AttackRange &&
                    targetW.IsValidTarget(W.Range) && _Player.ManaPercent > Wmana)
                {
                    W.Cast(targetW);
                }
            }
            if (targetQ != null)
            {
                foreach (var enemy in EntityManager.Heroes.Enemies.Where(
                                a => a.IsValidTarget(Q.Range))
                                .OrderBy(TargetSelector.GetPriority))
                {
                    if (HarassSettings["useQHarass"].Cast<CheckBox>().CurrentValue && enemy.CountEnemiesInRange(Q.Range) >= qcount &&
                     Q.IsReady() && targetQ.IsValidTarget(Q.Range) && _Player.ManaPercent > HarassSettings["useQHarassMana"].Cast<Slider>().CurrentValue)
                    {
                        Q.Cast(targetQ);
                    }
                }
            }
        }//HARASS END

        //EVENTS

        public static void Gapcloser_OnGapCloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (AutoSettings["gapcloser"].Cast<CheckBox>().CurrentValue && sender.IsEnemy &&
                e.End.Distance(_Player) < 200)
            {
                E.Cast(e.End);
            }
        }
        
        public static void KS()
        {// KİLLSTEAL START
            var targetQ = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var targetW = TargetSelector.GetTarget(W.Range, DamageType.Mixed);      
            var targetR = TargetSelector.GetTarget(R.Range, DamageType.Physical);
           
            if (ComboSettings["useWCombo"].Cast<CheckBox>().CurrentValue && W.IsReady() && targetW.IsValidTarget(W.Range) &&
                WDamage(targetW) >= targetW.Health)
            {
                W.Cast(targetW);
            }
            if (ComboSettings["useQCombo"].Cast<CheckBox>().CurrentValue && Q.IsReady() && targetQ.IsValidTarget(Q.Range) &&
                QDamage(targetQ) >= targetQ.Health)
            {
                Q.Cast(targetQ);
            }
            if (ComboSettings["useRCombo"].Cast<CheckBox>().CurrentValue && targetR.IsValidTarget(R.Range) && R.IsReady() && RDamage(targetR) >= targetR.Health)
            {
                R.Cast(targetR);
                ultimate = true;
            }



        }//KİLLSTEAL END

        public static void ComboMedium()
        {//COMBO MEDİUM PREDİCTİON START    
            var targetQ = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var targetE = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            var targetW = TargetSelector.GetTarget(W.Range, DamageType.Mixed);
            var rtarget = TargetSelector.GetTarget(R.Range, DamageType.Physical);
            var mtarget = TargetSelector.GetTarget(700, DamageType.Physical);
            var useYoumu = Items["useYoumu"].Cast<CheckBox>().CurrentValue;
            var useMahvolmus = Items["useBOTRK"].Cast<CheckBox>().CurrentValue;
            var useMahvolmusEV = Items["useBotrkEnemyHP"].Cast<Slider>().CurrentValue;
            var useMahvolmusHPV = Items["useBotrkMyHP"].Cast<Slider>().CurrentValue;

            if (Orbwalker.IsAutoAttacking) return;

            if (useMahvolmus && Item.HasItem(3153) && Item.CanUseItem(3153) && Item.HasItem(3144) && Item.CanUseItem(3144) && mtarget.HealthPercent < useMahvolmusEV && _Player.HealthPercent < useMahvolmusHPV)
                Item.UseItem(3153, mtarget);
            Item.UseItem(3144, mtarget);

            if (useYoumu && Item.HasItem(3142) && Item.CanUseItem(3142))
                Item.UseItem(3142);

            // Q LOGİC
            if (targetQ != null)
            {
                if (ComboSettings["useQCombo"].Cast<CheckBox>().CurrentValue &&
                 Q.IsReady() && targetQ.IsValidTarget(Q.Range))
                {
                    Q.Cast(targetQ);
                }
            }

            // E LOGİC

            if (ComboSettings["useECombo"].Cast<CheckBox>().CurrentValue && (targetE.HasBuffOfType(BuffType.Snare) ||  targetE.HasBuffOfType(BuffType.Stun) || targetE.HasBuffOfType(BuffType.Fear) || targetE.HasBuffOfType(BuffType.Knockup) || targetE.HasBuffOfType(BuffType.Taunt)))
            {
                E.Cast(targetE);
            }

            if (ComboSettings["useEDistance"].Cast<CheckBox>().CurrentValue && targetE.Distance(_Player) < ComboSettings["EMaxDistance"].Cast<Slider>().CurrentValue)
            {
                E.Cast(targetE);
            }

            // W LOGİC
            if (ComboSettings["useWCombo"].Cast<CheckBox>().CurrentValue && W.IsReady() && !targetW.IsValidTarget(_Player.AttackRange) &&
                targetW.IsValidTarget(W.Range))
            {
                W.Cast(targetW);
            }
            else if (ComboSettings["useWComboOnylCC"].Cast<CheckBox>().CurrentValue && W.IsReady() && targetW.IsValidTarget(W.Range))
            {
                if (targetW.HasBuff("jhinespotteddebuff"))
                {
                    W.Cast(targetW);
                }
            }

           

        } //COMBO MEDİUM PREDİCTİON END
       
        //DAMAGE HESAP
        public static int QDamage(Obj_AI_Base target)
        {
            return
                (int)
                    (new int[] { 60, 85, 110, 135, 160 }[Q.Level - 1] +
                     0.4 * (_Player.TotalAttackDamage) + 0.6 * (_Player.TotalMagicalDamage));
        }

        public static int WDamage(Obj_AI_Base target)
        {
            return
                (int)
                    (new int[] { 50, 85, 120, 155, 190 }[W.Level - 1] +
                     0.7 * (_Player.TotalAttackDamage));
        }

        public static int EDamage(Obj_AI_Base target)
        {
            return
                (int)
                    (new double[] { 15, 75, 135, 195, 255 }[E.Level - 1]
                                    + 1.30 * _Player.TotalAttackDamage + 1 * _Player.TotalMagicalDamage);
        }

        public static float RDamage(Obj_AI_Base target)
        {
            
            if (!JhinaMarksman.Program.R.IsLearned) return 0;
             var level = JhinaMarksman.Program.R.Level - 1;

                if (ultimatecount == 4)
                {
                    return _Player.CalculateDamageOnUnit(target, DamageType.Physical,
                        (float)
                            (new int[] { 150, 220, 450 }[R.Level - 1] +
                     1 * (_Player.TotalAttackDamage)));
            }

                return _Player.CalculateDamageOnUnit(target, DamageType.Physical,
                    (float)
                        (new int[] { 50, 125, 200 }[R.Level - 1] +
                     0.35 * (_Player.TotalAttackDamage)));


        }

        private static void JungleSteal()
        {
            var rRange = ClearSettings["RJungleSteal"].Cast<Slider>().CurrentValue;
                    var jungleMob =
                        EntityManager.MinionsAndMonsters.Monsters.FirstOrDefault(
                            u =>
                            u.IsVisible && JungleMobsList.Contains(u.BaseSkinName)
                            && WDamage(u) >= u.Health);

                    if (jungleMob == null)
                    {
                        return;
                    }

                    if (!ClearSettings[jungleMob.BaseSkinName].Cast<CheckBox>().CurrentValue)
                    {
                        return;
                    }
                                W.Cast(jungleMob);                                                                                  
        }

        //DRAWİNGS
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawMenu["drawRange"].Cast<CheckBox>().CurrentValue)
            {
               
            }
            if (DrawMenu["drawQ"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(Q.IsReady() ? Color.Gray : Color.Red, Q.Range, _Player.Position);
            }
            if (DrawMenu["drawW"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(W.IsReady() ? Color.Gray : Color.Red, W.Range, _Player.Position);
            }
            if (DrawMenu["drawE"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(E.IsReady() ? Color.Gray : Color.Red, E.Range, _Player.Position);
            }
            if (DrawMenu["drawR"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(R.IsReady() ? Color.Gray : Color.Red, R.Range, _Player.Position);
            }
            if (DrawMenu["drawStatus"].Cast<CheckBox>().CurrentValue)
            {
                var pos = Drawing.WorldToScreen(Player.Instance.Position);
                Drawing.DrawText((int)pos.X - 45, (int)pos.Y + 40, System.Drawing.Color.Aqua, "R Key: G");
            }
        }


    }


}