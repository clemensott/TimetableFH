using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace TimtableFH
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class ReplaceValueEditPage : Page
    {
        private ReplaceValueEditPageViewModel viewModel;

        public ReplaceValueEditPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DataContext = viewModel = (ReplaceValueEditPageViewModel)e.Parameter;

            base.OnNavigatedTo(e);
        }

        private void AbbBack_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void AutoSuggestBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ((AutoSuggestBox)sender).IsSuggestionListOpen = true;
        }
    }

    class ReplaceValueEditPageViewModel : INotifyPropertyChanged
    {
        private const string partToReplace = "??";
        private static readonly string[] inserts = new string[] { "ä", "Ä", "ö", "Ö", "ü", "Ü" };

        private ReplaceValue replaceValue;
        private IEnumerable<string> suggestedExamples, suggestedReplacements;

        public ReplaceValue ReplaceValue
        {
            get { return replaceValue; }
            set
            {
                if (value == replaceValue) return;

                if (replaceValue != null) replaceValue.PropertyChanged -= ReplaceValue_PropertyChanged;
                replaceValue = value;
                if (replaceValue != null) replaceValue.PropertyChanged += ReplaceValue_PropertyChanged;

                OnPropertyChanged(nameof(ReplaceValue));

                SetSuggestedExamples();
                SetSuggestedReplacements();
            }
        }

        public IEnumerable<string> Examples { get; }

        public IEnumerable<string> SuggestedExamples
        {
            get { return suggestedExamples; }
            private set
            {
                if (value == suggestedExamples) return;

                suggestedExamples = value;
                OnPropertyChanged(nameof(SuggestedExamples));
            }
        }

        public IEnumerable<string> SuggestedReplacements
        {
            get { return suggestedReplacements; }
            private set
            {
                if (value == suggestedReplacements) return;

                suggestedReplacements = value;
                OnPropertyChanged(nameof(SuggestedReplacements));
            }
        }

        public ReplaceValueEditPageViewModel(ReplaceValue replaceValue, IEnumerable<string> examples)
        {
            Examples = examples;
            ReplaceValue = replaceValue;
        }

        private void ReplaceValue_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ReplaceValue.Reference))
            {
                SetSuggestedExamples();
                SetSuggestedReplacements();
            }
            else if (e.PropertyName == nameof(ReplaceValue.Replacement)) SetSuggestedReplacements();
        }

        private void SetSuggestedExamples()
        {
            SuggestedExamples = Examples.Filter(ReplaceValue?.Reference?.ToLower());
        }

        private void SetSuggestedReplacements()
        {
            SuggestedReplacements = GetReplaceSuggestions(ReplaceValue?.Reference, ReplaceValue?.Replacement);
        }

        private static IEnumerable<string> GetReplaceSuggestions(string reference, string replacement)
        {
            if (string.IsNullOrEmpty(reference)) return Enumerable.Empty<string>();

            string lowerReplacement = replacement?.ToLower();
            IEnumerable<string> suggestions = string.IsNullOrEmpty(replacement) ? GetReplaceSuggestions(reference) :
                GetReplaceSuggestions(reference).Where(s => s.ToLower().Contains(lowerReplacement));

            return suggestions.Take(20).OrderBy(s => s);
        }

        private static IEnumerable<string> GetReplaceSuggestions(string reference)
        {
            int index = reference.IndexOf(partToReplace);

            if (index == -1) return new string[] { reference };

            string beforePart = reference.Remove(index);
            string afterPart = reference.Substring(index + partToReplace.Length);

            return GetReplaceSuggestions(afterPart).SelectMany(a => inserts.Select(i => beforePart + i + a));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
