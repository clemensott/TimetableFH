using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Shapes;
using StdOttStandard.Linq;
using TimetableFH.Helpers;
using TimetableFH.Models;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace TimetableFH.ViewEvents
{
    public sealed partial class EventsView : UserControl
    {
        private static readonly TimeSpan grdRowDuration = TimeSpan.FromMinutes(5);

        public static readonly DependencyProperty ReferenceDateProperty =
            DependencyProperty.Register(nameof(ReferenceDate), typeof(DateTime), typeof(EventsView),
                new PropertyMetadata(DateTime.Now, new PropertyChangedCallback(OnReferenceDatePropertyChanged)));

        private static void OnReferenceDatePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var s = (EventsView)sender;

            s.UpdateHeaders();
            s.UpdateViewEvents();
        }

        public static readonly DependencyProperty ViewDurationProperty =
            DependencyProperty.Register(nameof(ViewDuration), typeof(TimeSpan), typeof(EventsView),
                new PropertyMetadata(TimeSpan.FromHours(8), new PropertyChangedCallback(OnViewDurationPropertyChanged)));

        private static void OnViewDurationPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var s = (EventsView)sender;

            s.UpdateRowsHeight();
        }

        public static readonly DependencyProperty ViewDaysProperty =
            DependencyProperty.Register(nameof(ViewDays), typeof(DaysOfWeek), typeof(EventsView),
                new PropertyMetadata(default(DaysOfWeek), OnViewDaysPropertyChanged));

        private static void OnViewDaysPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            EventsView s = (EventsView)sender;

            s.UpdateDayColumns();
            s.UpdateHeaders();
            s.UpdateViewEvents();
        }

        public static readonly DependencyProperty BaseDaysProperty =
            DependencyProperty.Register(nameof(BaseDays), typeof(DaysOfWeek), typeof(EventsView),
                new PropertyMetadata(default(DayOfWeek), OnDaysOfWeekPropertyChanged));

        private static void OnDaysOfWeekPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            EventsView s = (EventsView)sender;
            DaysOfWeek oldValue = (DaysOfWeek)e.OldValue;
            DaysOfWeek newValue = (DaysOfWeek)e.NewValue;

            if (oldValue == s.ViewDays) s.ViewDays = newValue;
        }

        public static readonly DependencyProperty AllEventsProperty =
            DependencyProperty.Register(nameof(AllEvents), typeof(IEnumerable<Event>), typeof(EventsView),
                new PropertyMetadata(null, new PropertyChangedCallback(OnEventsPropertyChanged)));

        private static void OnEventsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var s = (EventsView)sender;
            var value = (IEnumerable<Event>)e.NewValue;

            s.Unsubscribe(s.events);
            s.events = value.ToArray();
            s.Subscribe(s.events);

            s.UpdateViewEvents(true);
        }

        private Event[] events;
        private readonly Dictionary<Event, EventControl> eventControls;
        private readonly Dictionary<DayOfWeek, Grid> dayGrids;

        public event EventHandler<Event> SetColorClick, SetNameClick, AddAdmittedClick;

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

        public DaysOfWeek ViewDays
        {
            get => (DaysOfWeek)GetValue(ViewDaysProperty);
            set => SetValue(ViewDaysProperty, value);
        }

        public DaysOfWeek BaseDays
        {
            get => (DaysOfWeek)GetValue(BaseDaysProperty);
            set => SetValue(BaseDaysProperty, value);
        }

        public IEnumerable<Event> AllEvents
        {
            get { return (IEnumerable<Event>)GetValue(AllEventsProperty); }
            set { SetValue(AllEventsProperty, value); }
        }

        public EventsView()
        {
            this.InitializeComponent();

            eventControls = new Dictionary<Event, EventControl>();
            dayGrids = new Dictionary<DayOfWeek, Grid>();

            UpdateDayColumns();
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
                if (ViewEvent(fhEvent)) AddOrUpdateControl(fhEvent);
                else RemoveControlIfExists(fhEvent, false);
            }
        }

        private void AddOrUpdateControl(Event fhEvent)
        {
            EventControl control = eventControls.GetOrAdd(fhEvent, c =>
            {
                c.RightTapped += EventControl_RightTapped;
                c.Holding += EventControl_Holding;
            });

            Grid dayGrid = dayGrids[fhEvent.Begin.DayOfWeek];
            if (!dayGrid.Children.Contains(control)) dayGrid.Children.Add(control);

            SetSizeAndPosition(fhEvent, control);
            control.Visibility = Visibility.Visible;
        }

        private void RemoveControlIfExists(Event fhEvent, bool delete)
        {
            EventControl control;
            if (!eventControls.TryGetValue(fhEvent, out control)) return;

            if (delete)
            {
                Grid dayGrid;
                if (dayGrids.TryGetValue(fhEvent.Begin.DayOfWeek, out dayGrid))
                {
                    dayGrid.Children.Remove(control);
                }

                eventControls.Remove(fhEvent);
            }
            else control.Visibility = Visibility.Collapsed;
        }

        private void EventControl_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            OpenFlyout((FrameworkElement)sender);
        }

        private void EventControl_Holding(object sender, HoldingRoutedEventArgs e)
        {
            OpenFlyout((FrameworkElement)sender);
        }

        private void OpenFlyout(FrameworkElement element)
        {
            FlyoutBase flyout = FlyoutBase.GetAttachedFlyout(element);

            if (flyout == null)
            {
                flyout = CreateEventControlFlyout();
                FlyoutBase.SetAttachedFlyout(element, flyout);
            }

            FlyoutBase.ShowAttachedFlyout(element);
        }

        private void SetSizeAndPosition(Event fhEvent, EventControl control)
        {
            TimeSpan offset = fhEvent.Begin.TimeOfDay - ReferenceDate.TimeOfDay;
            int offsetRows = (int)Math.Max(Math.Floor(offset.TotalDays / grdRowDuration.TotalDays), 0);

            TimeSpan duration = fhEvent.End - fhEvent.Begin;
            int durationRows = (int)Math.Ceiling(duration.TotalDays / grdRowDuration.TotalDays);

            control.DataContext = fhEvent;
            control.SetValue(Grid.RowProperty, offsetRows);
            control.SetValue(Grid.RowSpanProperty, durationRows);
        }

        private bool ViewEvent(Event fhEvent)
        {
            int deltaDays = (fhEvent.Begin.Date - ReferenceDate.Date).Days;

            return deltaDays >= 0 && deltaDays < 7 && ViewDays.Contains(fhEvent.Begin.Date.DayOfWeek);
        }

        private void UpdateViewEvents(bool delete = false)
        {
            if (delete)
            {
                foreach (Event fhEvent in eventControls.Keys.ToArray())
                {
                    RemoveControlIfExists(fhEvent, delete);
                }
            }
            else
            {
                foreach (Event fhEvent in eventControls.Keys.Where(e => !ViewEvent(e)).ToArray())
                {
                    RemoveControlIfExists(fhEvent, delete);
                }
            }

            foreach (Event fhEvent in AllEvents.ToNotNull().Where(ViewEvent))
            {
                AddOrUpdateControl(fhEvent);
            }

            UpdateRowsCount();
        }

        private void UpdateDayColumns()
        {
            DayOfWeek[] days = GetOrderedViewDays().ToArray();
            while (grdEvents.ColumnDefinitions.Count < days.Length)
            {
                AddDayColumn();
            }

            while (grdEvents.ColumnDefinitions.Count > days.Length)
            {
                RemoveDayColumn();
            }

            foreach ((int index, DayOfWeek day) in days.WithIndex())
            {
                dayGrids.GetOrAdd(day, g => grdEvents.Children.Add(g))
                    .SetValue(Grid.ColumnProperty, index);
            }

            foreach ((DayOfWeek day, Grid dayGrid) in dayGrids.Where(p => !days.Contains(p.Key)).ToArray().ToTuples())
            {
                dayGrid.Children.Clear();
                grdEvents.Children.Remove(dayGrid);
                dayGrids.Remove(day);
            }
        }

        private void AddDayColumn()
        {
            if (grdHeaders.ColumnDefinitions.Count > 0)
            {
                grdHeaders.ColumnDefinitions.Add(GetColumnDefinition(2));
                grdHeaders.Children.Add(GetVerticalLine(grdHeaders.ColumnDefinitions.Count / 2));
            }

            grdHeaders.ColumnDefinitions.Add(GetStarColumnDefinition());
            grdHeaders.Children.Add(GetHeaderTextBlock((grdHeaders.ColumnDefinitions.Count - 1) / 2));

            grdEvents.ColumnDefinitions.Add(GetStarColumnDefinition());

            SetColumnSpanVerticalLine();
        }

        private void RemoveDayColumn()
        {
            ((UIElement)grdHeaders.Children.Last()).Tapped -= TblHeader_Tapped;
            grdHeaders.Children.RemoveAt(grdHeaders.Children.Count - 1);
            grdHeaders.ColumnDefinitions.RemoveAt(grdHeaders.ColumnDefinitions.Count - 1);

            if (grdHeaders.ColumnDefinitions.Count > 0)
            {
                grdHeaders.Children.RemoveAt(grdHeaders.Children.Count - 1);
                grdHeaders.ColumnDefinitions.RemoveAt(grdHeaders.ColumnDefinitions.Count - 1);
            }

            SetColumnSpanVerticalLine();

            grdEvents.ColumnDefinitions.RemoveAt(grdEvents.ColumnDefinitions.Count - 1);
        }

        private void SetColumnSpanVerticalLine()
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

        private Rectangle GetVerticalLine(int day)
        {
            Rectangle rect = new Rectangle()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            Binding fillBinding = new Binding()
            {
                ElementName = nameof(rectHori),
                Path = new PropertyPath(nameof(Shape.Fill)),
            };
            rect.SetBinding(Shape.FillProperty, fillBinding);
            rect.SetValue(Grid.ColumnProperty, day * 2 - 1);
            rect.SetValue(Grid.RowSpanProperty, 3);

            return rect;
        }

        private TextBlock GetHeaderTextBlock(int dayOffset)
        {
            DateTime date = GetDateIn(dayOffset);
            TextBlock tbl = new TextBlock()
            {
                Text = FormatHeader(date),
                DataContext = date,
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
            if (ViewDays != BaseDays) ViewDays = BaseDays;
            else
            {
                DateTime date = (DateTime)((FrameworkElement)sender).DataContext;
                ViewDays = date.DayOfWeek.Convert();
            }
        }

        private IEnumerable<DayOfWeek> GetOrderedViewDays()
        {
            DayOfWeek refDayOfWeek = ReferenceDate.DayOfWeek;
            return ViewDays.GetDaysOfWeek().OrderBy(d => d >= refDayOfWeek).ThenBy(d => d);
        }

        private DateTime GetDateIn(int dayOffset)
        {
            DateTime date = ReferenceDate;

            while (true)
            {
                if (!ViewDays.Contains(date.DayOfWeek)) date = date.AddDays(1);
                else if (dayOffset > 0)
                {
                    dayOffset--;
                    date = date.AddDays(1);
                }
                else break;
            }

            return date;
        }

        private string FormatHeader(DateTime date)
        {
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
            foreach ((int index, TextBlock tbl) in grdHeaders.Children.OfType<TextBlock>().WithIndex())
            {
                DateTime date = GetDateIn(index);
                tbl.Text = FormatHeader(date);
                tbl.DataContext = date;
            }
        }

        private void UpdateRowsHeight()
        {
            GridLength rowHeight = new GridLength(GetRowHeight());

            foreach (Grid dayGrid in dayGrids.Values)
            {
                foreach (RowDefinition rowDefinition in dayGrid.RowDefinitions) rowDefinition.Height = rowHeight;
            }
        }

        private void UpdateRowsCount()
        {
            foreach ((DayOfWeek day, Grid dayGrid) in dayGrids.ToTuples())
            {
                int count;
                Event[] dayEvents = eventControls.Keys.Where(e => e.Begin.DayOfWeek == day).ToArray();
                if (dayEvents.Length > 0)
                {
                    double min = ReferenceDate.TimeOfDay.TotalDays / grdRowDuration.TotalDays;
                    double max = dayEvents.Max(e => e.End.TimeOfDay.TotalDays / grdRowDuration.TotalDays);

                    count = (int)Math.Ceiling(max) - (int)Math.Floor(min);
                }
                else count = 0;

                while (dayGrid.RowDefinitions.Count < count)
                {
                    dayGrid.RowDefinitions.Add(GetRowDefinition());
                }

                while (dayGrid.RowDefinitions.Count > count)
                {
                    dayGrid.RowDefinitions.RemoveAt(dayGrid.RowDefinitions.Count - 1);
                }
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

        private FlyoutBase CreateEventControlFlyout()
        {
            MenuFlyout flyout = new MenuFlyout();
            MenuFlyoutItem setColorItem = new MenuFlyoutItem() { Text = "Set Color" };
            setColorItem.Click += SetColorItem_Click;
            MenuFlyoutItem setNameItem = new MenuFlyoutItem() { Text = "Set Name" };
            setNameItem.Click += SetNameItem_Click;
            MenuFlyoutItem addAdmittedItem = new MenuFlyoutItem() { Text = "Add to admitted classes" };
            addAdmittedItem.Click += AddAdmittedItem_Click;

            flyout.Items.Add(setColorItem);
            flyout.Items.Add(setNameItem);
            flyout.Items.Add(addAdmittedItem);

            return flyout;
        }

        private void SetColorItem_Click(object sender, RoutedEventArgs e)
        {
            Event fhEvent = (Event)((FrameworkElement)sender).DataContext;
            SetColorClick?.Invoke(this, fhEvent);
        }

        private void SetNameItem_Click(object sender, RoutedEventArgs e)
        {
            Event fhEvent = (Event)((FrameworkElement)sender).DataContext;
            SetNameClick?.Invoke(this, fhEvent);
        }

        private void AddAdmittedItem_Click(object sender, RoutedEventArgs e)
        {
            Event fhEvent = (Event)((FrameworkElement)sender).DataContext;
            AddAdmittedClick?.Invoke(this, fhEvent);
        }
    }
}
