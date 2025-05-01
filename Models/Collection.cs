using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CollectionManager.Models
{
    public class Collection : INotifyPropertyChanged
    {
        private string _name;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Collection()
        {
            Items = new ObservableCollection<Item>();
        }

        public Collection(string name)
        {
            _name = name;
            Items = new ObservableCollection<Item>();
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Item> Items { get; set; }

        public void SortItems()
        {
            var sortedItems = Items.OrderBy(item => item.IsSold).ToList();

            Items.Clear();
            foreach (var item in sortedItems)
            {
                Items.Add(item);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
