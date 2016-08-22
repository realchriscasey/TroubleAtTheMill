using System;
using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.API;
using System.Windows;

namespace TroubleAtTheMill
{
    public class FatiguePlugin : IPlugin
    {
        public string Name => "Trouble at the Mill";
        public string Description
            =>
                "Displays the number of card draws until each player dies from fatigue.";

        public string ButtonText => "DO NOT PUSH THIS BUTTON!";
        public string Author => "realchriscasey";
        public Version Version => new Version(0, 1, 2);
        public System.Windows.Controls.MenuItem MenuItem => null;        

        private List<UIElement> _displayElements;
        private List<FatigueCalculator> _calculators;

        void IPlugin.OnButtonPress()
        {
            /*NOP*/
        }

        void IPlugin.OnLoad()
        {
            _displayElements = new List<UIElement>();
            _calculators = new List<FatigueCalculator>();

            newCalculator(true);
            newCalculator(false);
        }

        private void newCalculator(bool isLocal)
        {
            FatigueDisplay display = new FatigueDisplay(isLocal);
            Core.OverlayCanvas.Children.Add(display);
            _displayElements.Add(display);

            FatigueCalculator fatigueCalculator = new FatigueCalculator(display, isLocal);
            _calculators.Add(fatigueCalculator);

            GameEvents.OnGameStart.Add(fatigueCalculator.GameStart);
            GameEvents.OnInMenu.Add(fatigueCalculator.InMenu);            
        }

        void IPlugin.OnUnload()
        {
            foreach (UIElement element in _displayElements)
            {
                Core.OverlayCanvas.Children.Remove(element);
            }
            _displayElements.Clear();
            _calculators.Clear(); //this probably memory leaks. not sure how to clean up the calculators from the notifications
        }

        //my apologies, I kept digging through and finding more places I needed to subscribe to an event.
        //I wish there was an easy way to hook '# of cards in deck' and 'health' changes directly.
        void IPlugin.OnUpdate()
        {
            foreach (FatigueCalculator calculator in _calculators)
            {
                calculator.Update();
            }
        }
    }
}
