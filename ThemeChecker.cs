using StdOttUwp;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace TimetableFH
{
    public class ThemeChecker
    {
        private int count;
        private Task<bool> askTask;
        private Settings lastSettings;

        public ThemeChecker()
        {
            count = 0;
        }

        public async Task<bool> Check(Settings settings)
        {
            bool change;

            if (settings == lastSettings) return false;

            ApplicationTheme oldTheme = settings.Theme;
            ApplicationTheme newTheme = Application.Current.RequestedTheme;
            settings.Theme = newTheme;

            if (oldTheme == newTheme) return false;

            int currentCount = ++count;

            if (askTask != null) change = await askTask;
            else
            {
                askTask = Ask();
                change = await askTask;

                askTask = null;
                lastSettings = null;
            }

            if (currentCount != count) return false;

            return change;
        }

        private async Task<bool> Ask()
        {
            const string message = "A change of the Application Theme has been detected, would you like to change colors?";
            UICommand cmdYes = new UICommand("Yes", null);
            UICommand cmdNo = new UICommand("No", null);
            MessageDialog dialog = new MessageDialog(message);
            dialog.Commands.Add(cmdYes);
            dialog.Commands.Add(cmdNo);
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;

            IUICommand cmdResult = await dialog.ShowSafeAsync();

            return cmdResult == cmdYes;
        }
    }
}
