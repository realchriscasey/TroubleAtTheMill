using Hearthstone_Deck_Tracker.API;
using System;
using System.Windows;
using System.Windows.Controls;

namespace TroubleAtTheMill
{
    /// <summary>
    /// Interaction logic for FatigueDisplay.xaml
    /// </summary>
    public partial class FatigueDisplay : UserControl
    {
        private bool isLocal;     
        public FatigueDisplay(bool isLocal)
        {
            this.isLocal = isLocal;

            InitializeComponent();
            UpdatePosition();
        }

        public void UpdateText(String text)
        {
            this.textBlock.Text = text;
            UpdatePosition();
        }

        public void UpdateDetail(String text)
        {
            this.detailBlock.Text = text;
        }

        private double ScreenRatio => (4.0 / 3.0) / (Core.OverlayCanvas.Width / Core.OverlayCanvas.Height);
        public void UpdatePosition()
        {
            /* OLD CODE; positions near top of screen
            Canvas.SetTop(this, Core.OverlayCanvas.Height * 3 / 100);
            var xPos = Hearthstone_Deck_Tracker.Helper.GetScaledXPos(8.0 / 100, (int)Core.OverlayCanvas.Width, ScreenRatio);

            if (isLocal)
            {                
                Canvas.SetRight(this, xPos);
            }
            else
            {
                Canvas.SetLeft(this, xPos);
            }
            */

            Canvas.SetRight(this, Hearthstone_Deck_Tracker.Helper.GetScaledXPos(5.0 / 100, (int)Core.OverlayCanvas.Width, ScreenRatio));
            if (isLocal)
            {
                Canvas.SetTop(this, Core.OverlayCanvas.Height * 65 / 100);
            } else {
                Canvas.SetBottom(this, Core.OverlayCanvas.Height * 75 / 100);
            }
        }

        public void Show()
        {
            this.Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            this.Visibility = Visibility.Hidden;
        }
    }
}
