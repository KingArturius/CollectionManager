using CollectionManager.Models;
using Microsoft.Maui.Graphics;
using System.Linq;
using System.Collections.ObjectModel;

namespace CollectionManager.Pages;

public partial class CollectionDetailsPage : ContentPage
{
    private Collection _collection;

    public CollectionDetailsPage(Collection collection)
    {
        InitializeComponent();
        _collection = collection;
        CollectionNameLabel.Text = _collection.Name;
        RefreshItemsList();
    }

    private void RefreshItemsList()
    {
        var sortedItems = _collection.Items
            .OrderBy(item => item.Status == "Sprzedany")
            .ThenByDescending(item => item.Quantity > 0)
            .ThenBy(item => item.Name)
            .Select(item =>
            {
                var backgroundColor = item.Status == "Sprzedany"
                    ? Colors.LightGray
                    : Colors.White;

                return new
                {
                    item.Name,
                    item.Price,
                    item.Status,
                    item.Quantity,
                    BackgroundColor = backgroundColor,
                    OriginalItem = item
                };
            })
            .ToList();

        ItemsCollectionView.ItemsSource = sortedItems;
    }

    private async void OnItemSelected(object sender, SelectionChangedEventArgs e)
    {
        var selected = e.CurrentSelection.FirstOrDefault();
        if (selected == null) return;

        var selectedItem = selected.GetType().GetProperty("OriginalItem")?.GetValue(selected) as Item;
        if (selectedItem != null)
        {
            await Navigation.PushAsync(new ItemDetailsPage(selectedItem, _collection));
        }

        ItemsCollectionView.SelectedItem = null;
    }

    private async void OnAddItemClicked(object sender, EventArgs e)
    {
        string itemName = "Nowy przedmiot";
        if (_collection.Items.Any(i => i.Name == itemName))
        {
            await DisplayAlert("Błąd", $"Przedmiot o nazwie \"{itemName}\" już istnieje.", "OK");
            return;
        }

        var newItem = new Item(itemName, "Nowy", 0, 1);
        _collection.Items.Add(newItem);

        RefreshItemsList();
        await Navigation.PushAsync(new ItemDetailsPage(newItem, _collection));
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefreshItemsList();
    }
}
