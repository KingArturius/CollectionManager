using CollectionManager.Models;

namespace CollectionManager.Pages;

public partial class ItemDetailsPage : ContentPage
{
    private readonly Item _item;
    private readonly Collection _collection;

    public ItemDetailsPage(Item item, Collection collection)
    {
        InitializeComponent();
        _item = item;
        _collection = collection;
        LoadItemData();
    }

    private void LoadItemData()
    {
        NameEntry.Text = _item.Name;
        StatusPicker.SelectedItem = _item.Status;
        PriceEntry.Text = _item.Price.ToString();
        QuantityEntry.Text = _item.Quantity.ToString();

        if (!string.IsNullOrEmpty(_item.ImageBase64))
        {
            byte[] imageBytes = Convert.FromBase64String(_item.ImageBase64);
            ItemImage.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        string newName = NameEntry.Text?.Trim();
        bool nameExists = _collection.Items.Any(i => i != _item && i.Name.Equals(newName, StringComparison.OrdinalIgnoreCase));

        if (nameExists)
        {
            await DisplayAlert("B³¹d", $"Przedmiot o nazwie \"{newName}\" ju¿ istnieje w tej kolekcji.", "OK");
            return;
        }

        _item.Name = newName;
        _item.Status = StatusPicker.SelectedItem?.ToString();
        _item.Price = double.TryParse(PriceEntry.Text, out var price) ? price : 0;
        _item.Quantity = int.TryParse(QuantityEntry.Text, out var quantity) ? quantity : 0;

        await DisplayAlert("Zapisano", "Dane przedmiotu zosta³y zaktualizowane.", "OK");
        await Navigation.PopAsync();
    }

    private async void OnLoadImageClicked(object sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Wybierz zdjêcie",
            FileTypes = FilePickerFileType.Images
        });

        if (result != null)
        {
            using var stream = await result.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();
            _item.ImageBase64 = Convert.ToBase64String(bytes);
            ItemImage.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Potwierdzenie", "Czy na pewno chcesz usun¹æ ten przedmiot?", "Tak", "Nie");
        if (confirm)
        {
            _collection.Items.Remove(_item);
            await Navigation.PopAsync();
        }
    }
}
