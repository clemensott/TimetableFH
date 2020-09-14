using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Shapes;
using StdOttStandard.Linq;
using TimetableFH.Models;
using StdOttStandard.Linq.DataStructures.Observable;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace TimetableFH.ViewEvents
{
    public sealed partial class EventsView : UserControl
    {
        public static readonly DependencyProperty BeginOfDayProperty =
            DependencyProperty.Register(nameof(BeginOfDay), typeof(TimeSpan), typeof(EventsView),
                new PropertyMetadata(TimeSpan.FromHours(6), new PropertyChangedCallback(OnBeginOfDayPropertyChanged)));

        private static void OnBeginOfDayPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            EventsView s = (EventsView)sender;
            s.UpdateAllSizeAndPositions();
        }

        public static readonly DependencyProperty ViewDurationProperty =
            DependencyProperty.Register(nameof(ViewDuration), typeof(TimeSpan), typeof(EventsView),
                new PropertyMetadata(TimeSpan.FromHours(8), new PropertyChangedCallback(OnViewDurationPropertyChanged)));

        private static void OnViewDurationPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            EventsView s = (EventsView)sender;
            s.UpdateAllSizeAndPositions();
        }

        public static readonly DependencyProperty EventDaysProperty =
            DependencyProperty.Register(nameof(EventDays), typeof(IReadOnlyObservableCollection<EventDay>), typeof(EventsView),
                new PropertyMetadata(default(IReadOnlyObservableCollection<EventDay>), OnEventDaysPropertyChanged));

        private static void OnEventDaysPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            EventsView s = (EventsView)sender;
            IReadOnlyObservableCollection<EventDay> oldValue = (IReadOnlyObservableCollection<EventDay>)e.OldValue;
            IReadOnlyObservableCollection<EventDay> newValue = (IReadOnlyObservableCollection<EventDay>)e.NewValue;

            s.Unsubscribe(oldValue);
            s.Subscrube(newValue);
            s.UpdateViewEvents(newValue);
        }

        private readonly Dictionary<EventControl, EventPosition> eventControls;

        public event EventHandler<Event> SetColorClick, SetNameClick, AddAdmittedClick;
        public event EventHandler<DateTime> DayHeaderClick;

        public TimeSpan BeginOfDay
        {
            get { return (TimeSpan)GetValue(BeginOfDayProperty); }
            set { SetValue(BeginOfDayProperty, value); }
        }

        public TimeSpan ViewDuration
        {
            get { return (TimeSpan)GetValue(ViewDurationProperty); }
            set { SetValue(ViewDurationProperty, value); }
        }

        public IReadOnlyObservableCollection<EventDay> EventDays
        {
            get => (IReadOnlyObservableCollection<EventDay>)GetValue(EventDaysProperty);
            set => SetValue(EventDaysProperty, value);
        }

        public EventsView()
        {
            this.InitializeComponent();

            eventControls = new Dictionary<EventControl, EventPosition>();
        }

        private void Subscrube(IReadOnlyObservableCollection<EventDay> list)
        {
            if (list == null) return;

            list.Added += EventDays_Added;
            list.Removed += EventDays_Removed;
            list.Set += EventDays_Set;
            list.Moved += EventDays_Moved;
            list.Cleared += EventDays_Cleared;
        }

        private void Unsubscribe(IReadOnlyObservableCollection<EventDay> list)
        {
            if (list == null) return;

            list.Added -= EventDays_Added;
            list.Removed -= EventDays_Removed;
            list.Set -= EventDays_Set;
            list.Moved -= EventDays_Moved;
            list.Cleared -= EventDays_Cleared;
        }

        private void EventDays_Added(object sender, SingleChangeEventArgs<EventDay> e)
        {
            AddDayColumn();

            for (int i = e.Index; i < EventDays.Count; i++)
            {
                SetEvents(i);
            }
        }

        private void EventDays_Removed(object sender, SingleChangeEventArgs<EventDay> e)
        {
            RemoveDayColumn();

            for (int i = e.Index; i < EventDays.Count; i++)
            {
                SetEvents(i);
            }
        }

        private void EventDays_Set(object sender, SetItemEventArgs<EventDay> e)
        {
            SetEvents(e.NewItem, e.Index);
        }

        private void EventDays_Moved(object sender, MoveItemEventArgs<EventDay> e)
        {
            int minIndex = Math.Min(e.OldIndex, e.NewIndex);
            int maxIndex = Math.Max(e.OldIndex, e.NewIndex);

            for (int i = minIndex; i < maxIndex; i++)
            {
                SetEvents(i);
            }
        }

        private void EventDays_Cleared(object sender, CollectionClearEventArgs<EventDay> e)
        {
            while (grdEvents.ColumnDefinitions.Count > 0)
            {
                RemoveDayColumn();
            }
        }

        private void UpdateViewEvents(IReadOnlyList<EventDay> eventDays)
        {
            int count = eventDays?.Count ?? 0;
            while (grdEvents.ColumnDefinitions.Count > count)
            {
                RemoveDayColumn();
            }

            while (grdEvents.ColumnDefinitions.Count < count)
            {
                AddDayColumn();
            }

            for (int i = 0; i < count; i++)
            {
                SetEvents(i);
            }
        }

        private void SetEvents(int index)
        {
            SetEvents(EventDays[index], index);
        }

        private void SetEvents(EventDay eventDay, int index)
        {
            Grid grid = (Grid)grdEvents.Children[index];

            for (int j = 0; j < eventDay.Events.Length; j++)
            {
                AddControl(eventDay.Events[j], grid, j);
            }

            foreach (EventControl control in grid.Children.Skip(eventDay.Events.Length))
            {
                control.Visibility = Visibility.Collapsed;
            }

            TextBlock tblHeader = grdHeaders.Children.OfType<TextBlock>().ElementAt(index);
            tblHeader.Text = FormatHeader(eventDay.Date);
            tblHeader.DataContext = eventDay.Date;
        }

        private void AddDayColumn()
        {
            if (grdHeaders.ColumnDefinitions.Count > 0)
            {
                grdHeaders.ColumnDefinitions.Add(GetColumnDefinition(2));
                grdHeaders.Children.Add(GetVerticalLine(grdHeaders.ColumnDefinitions.Count / 2));
            }

            grdHeaders.ColumnDefinitions.Add(GetStarColumnDefinition());
            grdHeaders.Children.Add(GetHeaderTextBlock(grdHeaders.ColumnDefinitions.Count - 1));

            grdEvents.ColumnDefinitions.Add(GetStarColumnDefinition());

            SetColumnSpanVerticalLine();

            Grid grid = new Grid()
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            grid.SetValue(Grid.ColumnProperty, grdEvents.Children.Count);
            grid.SizeChanged += DayGrid_SizeChanged;
            grdEvents.Children.Add(grid);
        }

        private void RemoveDayColumn()
        {
            grdHeaders.Children.Last().Tapped -= TblHeader_Tapped;
            grdHeaders.Children.RemoveLast();
            grdHeaders.ColumnDefinitions.RemoveLast();

            if (grdHeaders.ColumnDefinitions.Count > 0)
            {
                grdHeaders.Children.RemoveLast();
                grdHeaders.ColumnDefinitions.RemoveLast();
            }

            SetColumnSpanVerticalLine();

            Grid lastGrid = ((Grid)grdEvents.Children.Last());
            lastGrid.SizeChanged -= DayGrid_SizeChanged;

            foreach (EventControl control in lastGrid.Children)
            {
                eventControls.Remove(control);

                control.RightTapped -= EventControl_RightTapped;
                control.Holding -= EventControl_Holding;
            }

            grdEvents.ColumnDefinitions.RemoveLast();
            grdEvents.Children.RemoveLast();
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

        private TextBlock GetHeaderTextBlock(int column)
        {
            TextBlock tbl = new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };

            tbl.SetValue(Grid.ColumnProperty, column);
            tbl.Tapped += TblHeader_Tapped;

            return tbl;
        }

        private void TblHeader_Tapped(object sender, TappedRoutedEventArgs e)
        {
            DayHeaderClick?.Invoke(this, (DateTime)((FrameworkElement)sender).DataContext);
        }

        private void DayGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Grid grid = (Grid)sender;

            foreach (EventControl control in grid.Children)
            {
                SetSizeAndPosition(eventControls[control], control, grid.ActualWidth, svrEvents.ActualHeight);
            }
        }

        private void AddControl(EventPosition fhEvent, Grid dayGrid, int index)
        {
            EventControl control;

            if (dayGrid.Children.Count > index) control = (EventControl)dayGrid.Children[index];
            else
            {
                control = new EventControl()
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                };

                control.RightTapped += EventControl_RightTapped;
                control.Holding += EventControl_Holding;

                dayGrid.Children.Add(control);
            }

            double totalWidth = grdEvents.ActualWidth / grdEvents.ColumnDefinitions.Count;
            SetSizeAndPosition(fhEvent, control, totalWidth, svrEvents.ActualHeight);

            control.DataContext = fhEvent.Event;
            control.Visibility = Visibility.Visible;
            eventControls[control] = fhEvent;
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

        private void SetSizeAndPosition(EventPosition fhEvent, EventControl control, double totalWidth, double totalHeight)
        {
            double left = fhEvent.Rect.Left * totalWidth;
            double top = (fhEvent.Rect.Top - BeginOfDay.TotalHours) / ViewDuration.TotalHours * totalHeight;

            control.Margin = new Thickness(left, top, 0, 0);
            control.Width = fhEvent.Rect.Width * totalWidth;
            control.Height = fhEvent.Rect.Height / ViewDuration.TotalHours * totalHeight;
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

        private void SvrEvents_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateAllSizeAndPositions();
        }

        private void UpdateAllSizeAndPositions()
        {
            foreach ((EventControl control, EventPosition fhEvent) in eventControls.Where(p => p.Key.Visibility == Visibility.Visible).ToTuples())
            {
                double totalWidth = grdEvents.ActualWidth / grdEvents.ColumnDefinitions.Count;
                SetSizeAndPosition(fhEvent, control, totalWidth, svrEvents.ActualHeight);
            }
        }
    }
}
