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
            if (this.player == null || this.hero == null || !isMouseHoveringDecks())
            {
                fatigueDisplay.Hide();
                return;
            }            

            int playerHealth = this.hero.Health + this.hero.GetTag(HearthDb.Enums.GameTag.ARMOR);
            if (playerHealth <= 0)
            {
                fatigueDisplay.UpdateText("dead");
                fatigueDisplay.UpdateDetail(playerHealth.ToString());

                fatigueDisplay.Show();
                return;
            }

            int playerCardsRemaining = this.player.DeckCount;
            int playerFatigue = this.player.Fatigue + 1;

            int drawsLeft = drawsUntilFatigue(playerCardsRemaining, playerFatigue, playerHealth);

            fatigueDisplay.UpdateText(drawsLeft + " draws remaining");
            fatigueDisplay.UpdateDetail(getDetailText(drawsLeft - playerCardsRemaining, playerFatigue, playerHealth));

            fatigueDisplay.Show();
        }


        /* coordinate system */
        /* center of board is (0,0). top of board is (0,100). left of board is (-133,0). */
        /* precise to approximately three digits */

        private bool isMouseHoveringDecks()
        {
            Point mouse = getMousePos();

            if (mouse.X > 112.2 && mouse.X < 131.5 && 
                mouse.Y > 13.74 && mouse.Y < 43.74)
            {
                //mouse is hovering the opponent deck
                return true;
            }

            if (mouse.X > 110.5 && mouse.X < 132.9 &&
                mouse.Y > -39.9 && mouse.Y < -10.3)
            {
                // mouse is hovering the player's deck
                return true;
            }

            return false;
        }

        private static double GLOBAL_SCALE = 100.0;
        private static double EMPIRICAL_X_OFFSET = 0;
        private static double EMPIRICAL_Y_OFFSET = -0.07 * GLOBAL_SCALE;

        private Point getMousePos()
        {
            System.Drawing.Point pos = User32.GetMousePos();
            System.Drawing.Rectangle gameBoard = User32.GetHearthstoneRect(false);

            double yCenter = gameBoard.Height / 2;
            double xCenter = gameBoard.Width / 2;
            double scale = GLOBAL_SCALE / yCenter;

            double xOffset = EMPIRICAL_X_OFFSET;
            double yOffset = EMPIRICAL_Y_OFFSET;

            Point local = new Point(
                (pos.X - xCenter) * scale + xOffset,
                (pos.Y - yCenter) * scale * -1 + yOffset);  //invert y axis (up should be positive, dammit)

            return local;
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