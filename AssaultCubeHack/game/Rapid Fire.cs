using Examples;
using RL.Properties;
using System.Threading.Tasks;

namespace VECRPROJECT.game
{
    class Rapid_Fire
    {

        private static bool FireButtonClicked = false;

        public static async void Fire()
        {
            if (FireButtonClicked == false)
            {
                FireButtonClicked = true;
                Example.mousecontrol.Mouse.LeftButtonClick();
                await Task.Delay(Settings.Default.AUTO_FIRE_MS);
                FireButtonClicked = false;
            }
        }
    }
}
