using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using Sadistic.Settings;
using Sadistic.Helpers;
using TreeSharp;
using Action = TreeSharp.Action;

namespace Sadistic.Rotations
{
    public class GladiatorPaladin : SadisticRoutine
    {
        //DEVELOPERS REPLACE GetType().Name WITH YOUR CR'S NAME.
        public override string Name
        {
            get { return "Sadistic Routine"; }
        }

        public override float PullRange
        {
            get { return 2.5f; }
        }

        public override ClassJobType[] Class
        {
            get { return new ClassJobType[] {ClassJobType.Gladiator, ClassJobType.Paladin,}; }
        }

        [Behavior(BehaviorType.PreCombatBuffs)]
        public Composite CreateBasicPreCombatBuffs()
        {
            return SummonChocobo();
        }

        [Behavior(BehaviorType.Rest)]
        public Composite CreateBasicRest()
        {
            return DefaultRestBehavior(r => Core.Player.CurrentTPPercent);
        }

        [Behavior(BehaviorType.Pull)]
        public Composite CreateBasicPull()
        {
                        return new PrioritySelector(ctx => Core.Player.CurrentTarget as BattleCharacter,
                new Decorator(ctx => ctx != null,new PrioritySelector(
                        CommonBehaviors.MoveToLos(ctx => ctx as GameObject),
                        CommonBehaviors.MoveAndStop(ctx => (ctx as GameObject).Location, ctx => Core.Player.CombatReach + PullRange + (ctx as GameObject).CombatReach, true, "Moving to unit"),
                    Spell.PullCast("Shield Lob"),
                    Spell.PullCast("Fast Blade")
                )));
        }


        private readonly SpellData ShieldSwipe = DataManager.GetSpellData("Shield Swipe");
        [Behavior(BehaviorType.Combat)]
        public Composite CreateBasicCombat()
        {
            return new PrioritySelector(ctx => Core.Player.CurrentTarget as BattleCharacter,
                new Decorator(ctx => ctx != null,
                    new PrioritySelector(
                        CommonBehaviors.MoveToLos(ctx => ctx as BattleCharacter),
                        CommonBehaviors.MoveAndStop(ctx => (ctx as BattleCharacter).Location, ctx => PullRange + (ctx as BattleCharacter).CombatReach, true, "Moving to unit"),
                        Spell.Cast("Fight or Flight", r => Core.Player),
                        Spell.Cast("Circle of Scorn", r => Core.Player),
                        Spell.Cast("Goring Blade", r => ActionManager.LastSpell.Name == "Riot Blade" && !Core.Target.HasAura("Goring Blade", false)),
                        Spell.Cast("Goring Blade", r => ActionManager.LastSpell.Name == "Riot Blade" && !Core.Target.HasAura("Goring Blade", true, 7000)),
                        Spell.Cast("Royal Authority", r => ActionManager.LastSpell.Name == "Riot Blade"),
                        //Check for mana level at higher level when theother combo action is avail
                        Spell.Cast("Riot Blade", r => ActionManager.LastSpell.Name == "Fast Blade"),
                        Spell.Cast("Fast Blade", r => true),
                        Spell.Cast("Requiescat", r => true),
                        Spell.Cast("Shield Swipe", r => true),
                        Spell.Cast("Spirits Within", r => true),
                        Spell.Cast(r => "Sheltron", r => ShieldSwipe.Cooldown.TotalSeconds <= 5,r => Core.Player)
                        // r => ActionManager.LastSpellId == 0 || ActionManager.LastSpell.Name == "Full Thrust" )
                        )));
        }
    }
}