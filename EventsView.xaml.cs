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

namespace TimtableFH
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

        public static readonly DependencyProperty ViewDaysCountProperty =
            DependencyProperty.Register("ViewDaysCount", typeof(int), typeof(EventsView),
                new PropertyMetadata(5, new PropertyChangedCallback(OnViewDaysCountPropertyChanged)));

        private static void OnViewDaysCountPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var s = (EventsView)sender;

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
            s.Subscribe(value);

            s.UpdateViewEvents();
        }

        private Event[] events;
        private Dictionary<Event, EventControl> eventControls;

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

        public int ViewDaysCount
        {
            get { return (int)GetValue(ViewDaysCountProperty); }
            set { SetValue(ViewDaysCountProperty, value); }
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

            return grdEvents.Children.OfType<EventControl>().FirstOrDefault(c => c.DataContext == fhEvent);
        }

        private void SetSizeAndPosition(Event fhEvent, EventControl control)
        {
            int deltaDays = (fhEvent.Begin.Date - ReferenceDate.Date).Days;

            TimeSpan offset = fhEvent.Begin.TimeOfDay - ReferenceDate.TimeOfDay;
            int offsetRows = (int)Math.Floor(offset.TotalDays / grdRowDuration.TotalDays);

            TimeSpan duration = fhEvent.End - fhEvent.Begin;
            int durationRows = (int)Math.Ceiling(duration.TotalDays / grdRowDuration.TotalDays);

            control.DataContext = fhEvent;
            control.SetValue(Grid.ColumnProperty, deltaDays);
            control.SetValue(Grid.RowProperty, offsetRows);
            control.SetValue(Grid.RowSpanProperty, durationRows);
        }

        private bool ViewEvent(Event fhEvent)
        {
            int deltaDays = (fhEvent.Begin.Date - ReferenceDate.Date).Days;

            return deltaDays >= 0 && deltaDays < ViewDaysCount;
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
            while (grdEvents.ColumnDefinitions.Count < ViewDaysCount)
            {
                AddDay();
            }

            while (grdEvents.ColumnDefinitions.Count > ViewDaysCount)
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

        private ColumnDefinition GetStarColumnDefinition()
        {
            return new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };
        }

        private ColumnDefinition GetColumnDefinition(double width)
        {
            return new ColumnDefinition() { Width = new GridLength(width) };
        }

        private Rectangle GetVerticalLine(int day)
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
            if (ViewDaysCount == 1)
            {
                ReferenceDate = ViewModel.GetMondayMorningBefore(ReferenceDate);
                ViewDaysCount = 5;
            }
            else
            {
                int index = (int)((UIElement)sender).GetValue(Grid.ColumnProperty);
                ReferenceDate = ReferenceDate.AddDays(index / 2);
                ViewDaysCount = 1;
            }
        }

        private string GetDayOfWeekIn(int dayOffset)
        {
            DateTime day = ReferenceDate.AddDays(dayOffset);

            return string.Format("{0}, {1}. {2}", GetShortDayOfWeek(day.DayOfWeek), day.Day, GetShortMonth(day.Month));
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
                tbl.Text = GetDayOfWeekIn(i++).ToString();
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
