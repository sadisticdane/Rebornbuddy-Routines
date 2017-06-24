using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using ff14bot.Enums;
using ff14bot.Navigation;
using ff14bot.NeoProfiles;
using Sadistic.Helpers;
using Sadistic.Settings;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Objects;
using Sadistic.Settings;
using TreeSharp;
using ff14bot.AClasses;
using Color = System.Drawing.Color;
using Action = TreeSharp.Action;

namespace Sadistic
{
    public abstract partial class SadisticRoutine : CombatRoutine
    {
        //public override virtual string Name { get { return "Sadistic [" + GetType().Name + "]"; } }

        private Form _configForm;

        public override bool WantButton
        {
            get { return true; }
        }


        public static SadisticSettings WindowSettings = SadisticSettings.Instance;

        public override void OnButtonPress()
        {
            if (_configForm == null || _configForm.IsDisposed || _configForm.Disposing)
                _configForm = new SettingsForm();

            _configForm.ShowDialog();
        }

        #region CombatRoutine implementation

        public virtual void OnPulse()
        {
        }

        public static bool WantHealing = false;

        public override sealed void Pulse()
        {
            SadisticExtensions.DoubleCastPreventionDict.RemoveAll(t => DateTime.UtcNow > t);

            TimeToDeathExtension.TTDStorage.RemoveAllKeys(r=> !r.IsValid);

            if (WantHealing)
                HealTargeting.Instance.Pulse();

            foreach (var attacker in GameObjectManager.Attackers)
            {
                attacker.TimeToDeath();
            }


            OnPulse();
        }


        protected Composite SummonChocobo()
        {
            return new PrioritySelector(
                new Decorator(r => WindowSettings.SummonChocobo && !Chocobo.Summoned && Chocobo.CanSummon,
                    new Action(r =>
                    {
                        if (MovementManager.IsMoving)
                            Navigator.PlayerMover.MoveStop();

                        Chocobo.Summon();
                        return RunStatus.Failure;
                    }))
                );
        }



        #region Hidden Overrides
        //Seal this class so that all our child classes will have this logic be called
        public sealed override void ShutDown()
        {
            Logger.Write("Shutingdown " + Name);
            //Clear the events
            GameEvents.OnMapChanged -= OnGameEventsOnOnMapChanged;
            TreeHooks.Instance.OnHooksCleared -= OnInstanceOnOnHooksCleared;
            OnGameContextChanged -= OnOnGameContextChanged;


            _lastContext = GameContext.None;

            CompositeBuilder._methods.Clear();
            OnShutdown();
        }

        //These two functions allow child classes to have specialized initalize and shutdown functions
        public virtual void OnInitialize()
        {
        }

        public virtual void OnShutdown()
        {
        }

        internal static SadisticRoutine Instance;

        public override sealed void Initialize()
        {
            Logger.Write("Starting " + Name);
            Instance = this;
            SetupEnergyFunction();
            GameEvents.OnMapChanged += OnGameEventsOnOnMapChanged;

            TreeHooks.Instance.OnHooksCleared += OnInstanceOnOnHooksCleared;


            UpdateContext();

            // NOTE: Hook these events AFTER the context update.
            OnGameContextChanged += OnOnGameContextChanged;


            

            if (!RebuildBehaviors(true))
            {
                return;
            }

            Logger.WriteDiagnostic(Color.White, "Verified behaviors can be created!");
            Logger.Write("Initialization complete!");
            OnInitialize();
        }

        private void OnInstanceOnOnHooksCleared(object s, EventArgs e)
        {
            Logger.Write("Hooks cleared, re-creating behaviors");
            RebuildBehaviors(true);
        }

        private void OnGameEventsOnOnMapChanged(object s, EventArgs e)
        {
            UpdateContext();
        }

        private void OnOnGameContextChanged(object orig, GameContextEventArg ne)
        {
            Logger.Write("Context changed, re-creating behaviors");
            RebuildBehaviors();
        }



        #endregion

        #endregion

        #region Unit Wrappers

        protected BattleCharacter GetTargetMissingDebuff(GameObject near, string debuff, float distance = -1f)
        {
            return
                UnfriendlyUnits.FirstOrDefault(
                    u => u.Location.Distance3D(near.Location) <= distance && !u.HasAura(debuff, true));
        }

        protected int EnemiesNearTarget(float range)
        {
            var target = Core.Player.CurrentTarget;
            if (target == null)
                return 0;
            var tarLoc = target.Location;
            return GameObjectManager.GetObjectsOfType<BattleCharacter>().Count(u => u.ObjectId != target.ObjectId && !u.IsDead && u.IsTargetable && u.IsVisible&& u.CanAttack && u.Location.Distance3D(tarLoc) <= range);
        }
        



        protected int EnemiesNearPlayer(float range, Func<BattleCharacter, bool> predicate)
        {
            var player = Core.Player.ObjectId;
            var tarLoc = Core.Player.Location;
            return GameObjectManager.GetObjectsOfType<BattleCharacter>().Count(u => u.ObjectId != player && !u.IsDead && u.IsTargetable && u.IsVisible && u.CanAttack && u.Location.Distance3D(tarLoc) <= range && predicate(u));
        }
        protected int EnemiesNearPlayer(float range)
        {
            var player = Core.Player.ObjectId;
            var tarLoc = Core.Player.Location;
            return GameObjectManager.GetObjectsOfType<BattleCharacter>().Count(u => u.ObjectId != player && !u.IsDead && u.IsTargetable && u.IsVisible && u.CanAttack && u.Location.Distance3D(tarLoc) <= range);
        }

        //protected IEnumerable<GameObject> UnfriendlyMeleeUnits { get { return UnfriendlyUnits.Where(u => ActionManager.InSpellInRangeLOS()); } }

        protected IEnumerable<BattleCharacter> UnfriendlyUnits
        {
            get
            {
                return
                    GameObjectManager.GetObjectsOfType<BattleCharacter>().Where(u => !u.IsDead && u.CanAttack);
            }
        }

        #endregion

        #region Spell Casting

        protected Composite EnsureTarget
        {
            get
            {
                return
                    new Decorator(
                        r =>
                            Core.Player.SpellCastInfo != null &&
                            Core.Player.SpellCastInfo.TargetId != GameObjectManager.EmptyGameObject,
                        new PrioritySelector(
                            r => GameObjectManager.GetObjectByObjectId(Core.Player.SpellCastInfo.TargetId),
                            new Decorator(r => (r as Character) != null && (r as Character).IsDead,
                                new PrioritySelector(
                                    new FailLogger(
                                        r =>
                                            string.Format("Stop casting at {0} because it is dead.",
                                                (r as GameObject).Name)),
                                    new Action(r => ActionManager.StopCasting()))
                                )));
            }
        }



        #endregion

        protected delegate T Selection<out T>(object context);
        protected delegate T Selectionz<out T>();
    }


}