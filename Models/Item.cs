using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CollectionManager.Models
{
    public class Item : INotifyPropertyChanged
    {
        private string _name;
        private string _status;
        private double _price;
        private int _quantity;
        private string _imageBase64;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Item() { }
        public Item(string name, string status, double price, int quantity)
        {
            _name = name;
            _status = status;
            _price = price;
            _quantity = quantity;
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsSold));
                }
            }
        }

        public double Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(); }
        }

        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); }
        }

        public string ImageBase64
        {
            get => _imageBase64;
            set { _imageBase64 = value; OnPropertyChanged(); }
        }

        public bool IsSold => string.Equals(_status?.Trim(), "sprzedany", StringComparison.OrdinalIgnoreCase);

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
