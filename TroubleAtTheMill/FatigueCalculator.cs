using System;
using System.Linq;
using System.Text;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

using CoreAPI = Hearthstone_Deck_Tracker.API.Core;
using Hearthstone_Deck_Tracker;
using System.Windows;

namespace TroubleAtTheMill
{
    internal class FatigueCalculator
    {
        private FatigueDisplay fatigueDisplay;
        private bool isLocal;

        private Player player => isLocal ? CoreAPI.Game.Player : CoreAPI.Game.Opponent;
        private Entity hero => player.Board.FirstOrDefault(x => x.IsHero);

        public FatigueCalculator(FatigueDisplay fatigueDisplay, bool isLocal)
        {
            this.fatigueDisplay = fatigueDisplay;
            this.isLocal = isLocal;
        }

        internal void GameStart()
        {
            fatigueDisplay.Show();
            fatigueDisplay.UpdateText(isLocal ? "you" : "opponent");
        }

        internal void InMenu()
        {
            fatigueDisplay.Hide();
        }

        internal void Update()
        {
            updateFatigue();
        }

        private void updateFatigue()
        {
            if (this.player == null || this.hero == null)
            {
                return;
            }

            var pos = User32.GetMousePos();
            System.Windows.Point relativeCanvas;
            try
            {
                relativeCanvas = CoreAPI.OverlayCanvas.PointFromScreen(new System.Windows.Point(pos.X, pos.Y));
            }
            catch (InvalidOperationException)
            {
                return;
            }

            if (isMouseHoveringDecks(relativeCanvas))
            {
                fatigueDisplay.Show();
            } else
            {
                fatigueDisplay.Hide();
            }


            int playerHealth = this.hero.Health + this.hero.GetTag(HearthDb.Enums.GameTag.ARMOR);
            if (playerHealth <= 0)
            {
                fatigueDisplay.UpdateText("ded");
                fatigueDisplay.UpdateDetail(playerHealth.ToString());

                return;
            }

            int playerCardsRemaining = this.player.DeckCount;
            int playerFatigue = this.player.Fatigue + 1;

            int drawsLeft = drawsUntilFatigue(playerCardsRemaining, playerFatigue, playerHealth);

            fatigueDisplay.UpdateText(drawsLeft + " draws remaining");
            fatigueDisplay.UpdateDetail(getDetailText(drawsLeft - playerCardsRemaining, playerFatigue, playerHealth));
        }

        private bool isMouseHoveringDecks(Point relativeCanvas)
        {
            double y_min = CoreAPI.OverlayCanvas.Height / 2 - 300;
            double y_max = CoreAPI.OverlayCanvas.Height / 2 + 200;

            double x_min = CoreAPI.OverlayCanvas.Width - 300;
            double x_max = CoreAPI.OverlayCanvas.Width - 200;

            return (relativeCanvas.Y > y_min &&
                relativeCanvas.Y < y_max &&
                relativeCanvas.X > x_min &&
                relativeCanvas.X < x_max);
        }

        private String getDetailText(int draws, int fatigue, int health)
        {
            int totalDamage = computeFatigueDamage(fatigue, draws);

            StringBuilder detail = new StringBuilder();
            detail.Append(health);
            detail.Append(" - ");
            for (int i = 0; i < draws; i++)
            {
                detail.Append(fatigue + i);
                detail.Append((i == draws - 1) ? " = " : " - ");
            }
            detail.Append(health - totalDamage);

            return detail.ToString();
        }

        private int computeFatigueDamage(int fatigueLevel, int numDraws)
        {
            // sum( f .. f+(d-1) )
            // (f + (f+d-1)) + (f+1 + (f+d-2)) + [...]
            //  ^1st ^last      ^2nd   ^2nd last
            // ((f + (f+d-1)) / 2) * d
            // ^ avg
            //(2f + d - 1) / 2 * d

            return (2 * fatigueLevel + numDraws - 1) * numDraws / 2;
        }

        private int drawsUntilFatigue(int cardsLeft, int fatigue, int health)
        {
            //(2f + d - 1)d / 2 = h
            //(2f + d - 1)d = 2h
            //2fd + d^2 - d = 2h
            //d^2 + (2f-1)d - 2h = 0
            //quadratic formula
            //a=1, b = (2f-1), c = (-2h)
            //
            // d = ( -(2f-1) (+/-) sqrt( (2f-1)^2-4*1*(-2h) ) ) / 2*1
            // d = ( -(2f-1) (+/-) sqrt( (2f-1)^2 + 8h ) ) / 2
            // d = ( -(2f-1) (+/-) sqrt( (2f-1)(2f-1) + 8h ) ) /2
            // d = ( -(2f-1) (+/-) sqrt (4f^2 - 4f + 1 + 8h) ) / 2

            return cardsLeft + (int)Math.Ceiling((Math.Sqrt((4 * fatigue * fatigue) - 4 * fatigue + 1 + 8 * health) - 2 * fatigue + 1) / 2.0);
        }
    }
}