using System;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace TimtableFH
{
    public sealed partial class EventControl : UserControl
    {
        public EventControl()
        {
            this.InitializeComponent();
        }

        private object MulTime_Convert(object input0, object input1)
        {
            DateTime begin = ((DateTime?)input0).GetValueOrDefault();
            DateTime end = ((DateTime?)input1).GetValueOrDefault();

            return string.Format("{0:00}:{1:00} - {2:00}:{3:00}", begin.TimeOfDay.Hours,
                begin.TimeOfDay.Minutes, end.TimeOfDay.Hours, end.TimeOfDay.Minutes);
        }

        private object MulColor_Convert(object input0, object input1)
        {
            string name = (string)input0;
            string group = (string)input1;

            switch (group)
            {
                case "PA":
                case "PAB":
                case "PAM":
                    return new SolidColorBrush(Colors.Pink);

                case "SO":
                    return new SolidColorBrush(Colors.LimeGreen);
            }

            if (name != null)
            {
                if (name.StartsWith("DATSTR")) return new SolidColorBrush(Colors.Orange);
                if (name.StartsWith("DBDESIGN")) return new SolidColorBrush(Colors.Aqua);
                if (name.StartsWith("KONFIG")) return new SolidColorBrush(Colors.LightBlue);
                if (name.StartsWith("MATH2")) return new SolidColorBrush(Colors.RosyBrown);
                if (name.StartsWith("PMuAR")) return new SolidColorBrush(Colors.LightGreen);
                if (name.StartsWith("PROFENG")) return new SolidColorBrush(Colors.Yellow);
                if (name.StartsWith("PROG2")) return new SolidColorBrush(Colors.YellowGreen);
                if (name.StartsWith("RelDB")) return new SolidColorBrush(Colors.Aqua);
                if (name.StartsWith("WEBTECH")) return new SolidColorBrush(Colors.LightSeaGreen);
            }

            return new SolidColorBrush(Colors.LightGray);
        }

        private object SvcName_Convert(object input)
        {
            string name = (string)input;

            if (name == null) return null;

            if (name.StartsWith("DATSTR")) return "Da&Al";
            if (name.StartsWith("DBDESIGN")) return "DB";
            if (name.StartsWith("KONFIG")) return "Konfi";
            if (name.StartsWith("MATH2")) return "Mathe";
            if (name.StartsWith("PMuAR")) return "Recht";
            if (name.StartsWith("PROFENG")) return "Englisch";
            if (name.StartsWith("PROG2")) return "SwDev";
            if (name.StartsWith("WEBTECH")) return "Web";

            return name;
        }

        private object SvcRoom_Convert(object input)
        {
            string room = (string)input;

            if (room == null) return null;

            if (room.StartsWith("G.AP"))
            {
                int index = 4, no;

                if (HasNumber(room, ref index, out _) && room.Length > index && room[index++] == '.')
                {
                    if (HasNumber(room, ref index, out no) && room.Length > index && room[index++] == ' ')
                    {
                        string roomType = room.Substring(index);

                        if (roomType.Length == 7 && roomType.EndsWith("rsaal")) return string.Format("HS{0:000}", no);
                        else if (roomType == "Seminarraum") return string.Format("SR{0:000}", no);
                    }
                }
            }

            return room;
        }

        private bool HasNumber(string text, ref int index, out int no)
        {
            string noText = string.Empty;

            while (text.Length > index && char.IsDigit(text[index])) noText += text[index++];

            return int.TryParse(noText, out no);
        }

        protected async override void OnTapped(TappedRoutedEventArgs e)
        {
            Event fhEvent = (Event)DataContext;
            string date = fhEvent.Begin.GetDateTimeFormats()[0];
            string time = tblTime.Text;

            string message = date + " " + time + "\r\n" + fhEvent.Name + "\r\n" +
                fhEvent.Professor + "\r\n" + fhEvent.Room + "\r\n" + fhEvent.Group;

            await new MessageDialog(message).ShowAsync();
        }
    }
}
