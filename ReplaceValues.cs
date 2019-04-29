using System.Collections.ObjectModel;

namespace TimtableFH
{
    public class ReplaceValues : ObservableCollection<ReplaceValue>
    {
        public ObservableCollection<string> Examples { get; }

        public ReplaceValues()
        {
            Examples = new ObservableCollection<string>();
        }
    }
}