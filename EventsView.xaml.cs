using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace TimetableFH
{
    public sealed partial class EventsView : UserControl
    {
        private static readonly TimeSpan grdRowDuration = TimeSpan.FromMinutes(5);

        public static readonly DependencyProperty ReferenceDateProperty =
            DependencyProperty.Register("ReferenceDate", typeof(DateTime), typeof(EventsView),
                new PropertyMetadata(DateTime.Now, new PropertyChangedCallback(OnReferenceDatePropertyChanged)));

        private static void OnReferenceDatePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var s = (EventsView)sender;

            s.UpdateHeaders();
            s.UpdateViewEvents();
        }

        public static readonly DependencyProperty ViewDurationProperty =
            DependencyProperty.Register("ViewDuration", typeof(TimeSpan), typeof(EventsView),
                new PropertyMetadata(TimeSpan.FromHours(8), new PropertyChangedCallback(OnViewDurationPropertyChanged)));

        private static void OnViewDurationPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var s = (EventsView)sender;

            s.UpdateRowsHeight();
        }

        public static readonly DependencyProperty IsSingleDayProperty =
            DependencyProperty.Register("IsSingleDay", typeof(bool), typeof(EventsView),
                new PropertyMetadata(default(bool), OnIsSingleDayPropertyChanged));

        private static void OnIsSingleDayPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            EventsView s = (EventsView)sender;
            bool value = (bool)e.NewValue;
        }

        public static readonly DependencyProperty DaysOfWeekProperty =
            DependencyProperty.Register("DaysOfWeek", typeof(DaysOfWeek), typeof(EventsView),
                new PropertyMetadata(default(DayOfWeek), OnDaysOfWeekPropertyChanged));

        private static void OnDaysOfWeekPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            EventsView s = (EventsView)sender;

            s.UpdateColumnsCount();
            s.UpdateViewEvents();
        }

        public static readonly DependencyProperty AllEventsProperty =
            DependencyProperty.Register("AllEvents", typeof(IEnumerable<Event>), typeof(EventsView),
                new PropertyMetadata(null, new PropertyChangedCallback(OnEventsPropertyChanged)));

        private static void OnEventsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var s = (EventsView)sender;
            var value = (IEnumerable<Event>)e.NewValue;

            s.Unsubscribe(s.events);
            s.events = value.ToArray();
            s.Subscribe(s.events);

            s.UpdateViewEvents();
        }

        private Event[] events;
        private readonly Dictionary<Event, EventControl> eventControls;

        public DateTime ReferenceDate
        {
            get { return (DateTime)GetValue(ReferenceDateProperty); }
            set { SetValue(ReferenceDateProperty, value); }
        }

        public TimeSpan ViewDuration
        {
            get { return (TimeSpan)GetValue(ViewDurationProperty); }
            set { SetValue(ViewDurationProperty, value); }
        }

        public bool IsSingleDay
        {
            get => (bool)GetValue(IsSingleDayProperty);
            set => SetValue(IsSingleDayProperty, value);
        }

        public DaysOfWeek DaysOfWeek
        {
            get => (DaysOfWeek)GetValue(DaysOfWeekProperty);
            set => SetValue(DaysOfWeekProperty, value);
        }

        public IEnumerable<Event> AllEvents
        {
            get { return (IEnumerable<Event>)GetValue(AllEventsProperty); }
            set { SetValue(AllEventsProperty, value); }
        }

        public IEnumerable<Event> ViewEvents => events?.Where(ViewEvent) ?? Enumerable.Empty<Event>();

        public EventsView()
        {
            this.InitializeComponent();

            eventControls = new Dictionary<Event, EventControl>();

            UpdateColumnsCount();
            UpdateHeaders();
            UpdateRowsHeight();
            UpdateViewEvents();
        }

        private int GetViewDaysCount()
        {
            int no = (int)DaysOfWeek;
            int count = 0;

            while (no > 0)
            {
                if ((no & 1) > 0) count++;

                no = no >> 1;
            }

            return count;
        }

        private void Subscribe(IEnumerable<Event> events)
        {
            if (events == null) return;

            foreach (Event fhEvent in events) Subscribe(fhEvent);
        }

        private void Unsubscribe(IEnumerable<Event> events)
        {
            if (events == null) return;

            foreach (Event fhEvent in events) Unsubscribe(fhEvent);
        }

        private void Subscribe(Event fhEvent)
        {
            if (fhEvent == null) return;

            fhEvent.PropertyChanged += FhEvent_PropertyChanged;
        }

        private void Unsubscribe(Event fhEvent)
        {
            if (fhEvent == null) return;

            fhEvent.PropertyChanged -= FhEvent_PropertyChanged;
        }

        private void FhEvent_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Event fhEvent = (Event)sender;

            if (e.PropertyName == nameof(fhEvent.Begin) || e.PropertyName == nameof(fhEvent.End))
            {
                if (ViewEvents.Contains(fhEvent)) AddOrUpdateControl(fhEvent);
                else RemoveControlIfExists(fhEvent);
            }
        }

        private void AddOrUpdateControl(Event fhEvent)
        {
            EventControl control = GetEventControl(fhEvent);

            if (control != null)
            {
                SetSizeAndPosition(fhEvent, control);
                control.Visibility = Visibility.Visible;
            }
            else
            {
                control = new EventControl();
                SetSizeAndPosition(fhEvent, control);

                eventControls.Add(fhEvent, control);
                grdEvents.Children.Add(control);
            }
        }

        private void RemoveControlIfExists(Event fhEvent)
        {
            EventControl control = GetEventControl(fhEvent);

            if (control != null) control.Visibility = Visibility.Collapsed;
        }

        private EventControl GetEventControl(Event fhEvent)
        {
            EventControl control;

            return eventControls.TryGetValue(fhEvent, out control) ? control : null;
        }

        private void SetSizeAndPosition(Event fhEvent, EventControl control)
        {
            int columnIndex = GetColumnIndex(fhEvent.Begin.DayOfWeek);

            TimeSpan offset = fhEvent.Begin.TimeOfDay - ReferenceDate.TimeOfDay;
            int offsetRows = (int)Math.Max(Math.Floor(offset.TotalDays / grdRowDuration.TotalDays), 0);

            TimeSpan duration = fhEvent.End - fhEvent.Begin;
            int durationRows = (int)Math.Ceiling(duration.TotalDays / grdRowDuration.TotalDays);

            control.DataContext = fhEvent;
            control.SetValue(Grid.ColumnProperty, columnIndex);
            control.SetValue(Grid.RowProperty, offsetRows);
            control.SetValue(Grid.RowSpanProperty, durationRows);
        }

        private int GetColumnIndex(DayOfWeek day)
        {
            DateTime date = ReferenceDate;
            int index = 0;

            while (date.DayOfWeek != day)
            {
                if (DaysOfWeek.Contains(date.DayOfWeek)) index++;

                date = date.AddDays(1);
            }

            return index;
        }

        private DateTime GetDate(int columnIndex)
        {
            DateTime date = ReferenceDate;

            while (true)
            {
                if (!DaysOfWeek.Contains(date.DayOfWeek)) date = date.Date.AddDays(1);
                else if (columnIndex > 0) columnIndex--;
                else return date;
            }
        }

        private bool ViewEvent(Event fhEvent)
        {
            int deltaDays = (fhEvent.Begin.Date - ReferenceDate.Date).Days;

            return deltaDays >= 0 && deltaDays < 7 && DaysOfWeek.Contains(fhEvent.Begin.Date.DayOfWeek);
        }

        private void UpdateViewEvents()
        {
            IEnumerable<Event> curViewEvents = grdEvents.Children.OfType<EventControl>()
                .Select(c => c.DataContext).OfType<Event>();

            IEnumerable<Event> removeEvents = curViewEvents.ToArray().Except(ViewEvents);

            foreach (Event fhEvent in removeEvents) RemoveControlIfExists(fhEvent);
            foreach (Event fhEvent in ViewEvents) AddOrUpdateControl(fhEvent);

            UpdateRowsCount();
        }

        private void UpdateColumnsCount()
        {
            while (grdEvents.ColumnDefinitions.Count < GetViewDaysCount())
            {
                AddDay();
            }

            while (grdEvents.ColumnDefinitions.Count > GetViewDaysCount())
            {
                RemoveDay();
            }
        }

        private void AddDay()
        {
            if (grdHeaders.ColumnDefinitions.Count > 0)
            {
                grdHeaders.ColumnDefinitions.Add(GetColumnDefinition(2));
                grdHeaders.Children.Add(GetVerticalLine(grdHeaders.ColumnDefinitions.Count / 2));
            }

            grdHeaders.ColumnDefinitions.Add(GetStarColumnDefinition());
            grdHeaders.Children.Add(GetHeaderTextBlock((grdHeaders.ColumnDefinitions.Count - 1) / 2));

            SetColumnSpan();

            grdEvents.ColumnDefinitions.Add(GetStarColumnDefinition());
        }

        private void RemoveDay()
        {
            ((TextBlock)grdHeaders.Children.Last()).Tapped -= TblHeader_Tapped;
            grdHeaders.Children.RemoveAt(grdHeaders.Children.Count - 1);
            grdHeaders.ColumnDefinitions.RemoveAt(grdEvents.ColumnDefinitions.Count - 1);

            if (grdHeaders.ColumnDefinitions.Count > 0)
            {
                grdHeaders.Children.RemoveAt(grdHeaders.Children.Count - 1);
                grdHeaders.ColumnDefinitions.RemoveAt(grdHeaders.ColumnDefinitions.Count - 1);
            }

            SetColumnSpan();

            grdEvents.ColumnDefinitions.RemoveAt(grdEvents.ColumnDefinitions.Count - 1);
        }

        private void SetColumnSpan()
        {
            int span = Math.Max(grdHeaders.ColumnDefinitions.Count, 1);
            rectBack.SetValue(Grid.ColumnSpanProperty, span);
            rectHori.SetValue(Grid.ColumnSpanProperty, span);
        }

        private static ColumnDefinition GetStarColumnDefinition()
        {
            return new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };
        }

        private static ColumnDefinition GetColumnDefinition(double width)
        {
            return new ColumnDefinition() { Width = new GridLength(width) };
        }

        private static Rectangle GetVerticalLine(int day)
        {
            Rectangle rect = new Rectangle()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Fill = (Brush)Application.Current.Resources["ApplicationForegroundThemeBrush"]
            };

            rect.SetValue(Grid.ColumnProperty, day * 2 - 1);
            rect.SetValue(Grid.RowSpanProperty, 3);

            return rect;
        }

        private TextBlock GetHeaderTextBlock(int dayOffset)
        {
            TextBlock tbl = new TextBlock()
            {
                Text = GetDayOfWeekIn(dayOffset),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };

            tbl.SetValue(Grid.ColumnProperty, dayOffset * 2);
            tbl.Tapped += TblHeader_Tapped;

            return tbl;
        }

        private void TblHeader_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (IsSingleDay)
            {
                IsSingleDay = false;
                ReferenceDate = Settings.GetCurrentMonday(ReferenceDate, DaysOfWeek);
            }
            else
            {
                int index = (int)((UIElement)sender).GetValue(Grid.ColumnProperty);
                ReferenceDate = GetDate(index);
                IsSingleDay = true;
            }
        }

        private string GetDayOfWeekIn(int dayOffset)
        {
            DateTime date = ReferenceDate;

            while (true)
            {
                if (!DaysOfWeek.Contains(date.DayOfWeek)) date = date.AddDays(1);
                else if (dayOffset > 0)
                {
                    dayOffset--;
                    date = date.AddDays(1);
                }
                else break;
            }

            return string.Format("{0}, {1}. {2}", GetShortDayOfWeek(date.DayOfWeek), date.Day, GetShortMonth(date.Month));
        }

        private static string GetShortDayOfWeek(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Friday:
                    return "Fr";

                case DayOfWeek.Monday:
                    return "Mo";

                case DayOfWeek.Saturday:
                    return "Sa";

                case DayOfWeek.Sunday:
                    return "So";

                case DayOfWeek.Thursday:
                    return "Do";

                case DayOfWeek.Tuesday:
                    return "Di";

                case DayOfWeek.Wednesday:
                    return "Mi";
            }

            throw new ArgumentException("Value is not implemented", nameof(day));
        }

        private static string GetShortMonth(int month)
        {
            switch (month)
            {
                case 1:
                    return "Jan.";

                case 2:
                    return "Feb.";

                case 3:
                    return "März";

                case 4:
                    return "April";

                case 5:
                    return "Mai";

                case 6:
                    return "Juni";

                case 7:
                    return "Juli";

                case 8:
                    return "Aug.";

                case 9:
                    return "Sep.";

                case 10:
                    return "Okt.";

                case 11:
                    return "Nov.";

                case 12:
                    return "Dez.";
            }

            throw new ArgumentException("Value is not implemented", nameof(month));
        }

        private void UpdateHeaders()
        {
            int i = 0;
            foreach (TextBlock tbl in grdHeaders.Children.OfType<TextBlock>())
            {
                tbl.Text = GetDayOfWeekIn(i++);
            }
        }

        private void UpdateRowsHeight()
        {
            GridLength rowHeight = new GridLength(GetRowHeight());

            foreach (RowDefinition rowDefinition in grdEvents.RowDefinitions) rowDefinition.Height = rowHeight;
        }

        private void UpdateRowsCount()
        {
            int count;
            if (ViewEvents.Any())
            {
                double min = ReferenceDate.TimeOfDay.TotalDays / grdRowDuration.TotalDays;
                double max = ViewEvents.Max(e => e.End.TimeOfDay.TotalDays / grdRowDuration.TotalDays);

                count = (int)Math.Ceiling(max) - (int)Math.Floor(min);
            }
            else count = 0;

            while (grdEvents.RowDefinitions.Count < count)
            {
                grdEvents.RowDefinitions.Add(GetRowDefinition());
            }

            while (grdEvents.RowDefinitions.Count > count)
            {
                grdEvents.RowDefinitions.RemoveAt(grdEvents.RowDefinitions.Count - 1);
            }
        }

        private RowDefinition GetRowDefinition()
        {
            return new RowDefinition() { Height = new GridLength(GetRowHeight()) };
        }

        private double GetRowHeight()
        {
            return ActualHeight / ViewDuration.TotalDays * grdRowDuration.TotalDays;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.PreviousSize.Height != e.NewSize.Height) UpdateRowsHeight();
        }
    }
}
