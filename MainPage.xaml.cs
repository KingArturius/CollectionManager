using CollectionManager.Models;
using CollectionManager.Pages;
using System.Text;

namespace CollectionManager;

public partial class MainPage : ContentPage
{
    public List<Collection> Collections { get; set; } = new();

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private async void OnAddCollectionClicked(object sender, EventArgs e)
    {
        string name = await DisplayPromptAsync("Nowa kolekcja", "Podaj nazwę kolekcji:");

        if (!string.IsNullOrWhiteSpace(name) && !Collections.Any(c => c.Name == name))
        {
            var newCollection = new Collection { Name = name };
            Collections.Add(newCollection);
            RefreshCollectionView();
        }
        else
        {
            await DisplayAlert("Błąd", "Nazwa kolekcji jest pusta lub już istnieje.", "OK");
        }
    }

    private void RefreshCollectionView()
    {
        CollectionsView.SelectedItem = null;
        CollectionsView.ItemsSource = null;
        CollectionsView.ItemsSource = Collections;
    }

    private async void OnCollectionSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Collection selected)
        {
            await Navigation.PushAsync(new CollectionDetailsPage(selected));

            CollectionsView.SelectedItem = null;
        }
    }

    private async void OnExportCollectionClicked(object sender, EventArgs e)
    {
        if (Collections.Count == 0)
        {
            await DisplayAlert("Błąd", "Brak kolekcji do eksportu.", "OK");
            return;
        }

        var names = Collections.Select(c => c.Name).ToArray();

        string selectedName = await DisplayActionSheet("Wybierz kolekcję do eksportu:", "Anuluj", null, names);

        if (string.IsNullOrEmpty(selectedName) || selectedName == "Anuluj")
            return;

        var selected = Collections.FirstOrDefault(c => c.Name == selectedName);
        if (selected == null)
        {
            await DisplayAlert("Błąd", "Nie znaleziono wybranej kolekcji.", "OK");
            return;
        }

        string folder = FileSystem.AppDataDirectory;
        string filePath = Path.Combine(folder, $"{selected.Name}.txt");

        try
        {
            var sb = new StringBuilder();
            sb.AppendLine(selected.Name);

            foreach (var item in selected.Items)
            {
                sb.AppendLine($"{item.Name}|{item.Status}|{item.Price}|{item.Quantity}");
            }

            File.WriteAllText(filePath, sb.ToString());

            await DisplayAlert("Sukces", $"Kolekcja wyeksportowana do pliku:\n{filePath}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Nie udało się zapisać pliku: {ex.Message}", "OK");
        }
    }


    private async void OnImportCollectionClicked(object sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync();

        if (result == null)
            return;

        if (Path.GetExtension(result.FileName).ToLower() != ".txt")
        {
            await DisplayAlert("Błąd", "Wybierz plik tekstowy (.txt).", "OK");
            return;
        }

        try
        {
            var stream = await result.OpenReadAsync();
            using var reader = new StreamReader(stream);
            var lines = new List<string>();

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (line != null)
                    lines.Add(line);
            }

            if (lines.Count == 0)
            {
                await DisplayAlert("Błąd", "Plik jest pusty.", "OK");
                return;
            }

            var importedName = lines[0];
            var existingCollection = Collections.FirstOrDefault(c => c.Name == importedName);

            if (existingCollection == null)
            {
                var newCollection = new Collection { Name = importedName };

                for (int i = 1; i < lines.Count; i++)
                {
                    var parts = lines[i].Split('|');
                    if (parts.Length != 4) continue;

                    var name = parts[0];
                    var status = parts[1];
                    var price = double.TryParse(parts[2], out double p) ? p : 0;
                    var quantity = int.TryParse(parts[3], out int q) ? q : 0;

                    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(status)) continue;

                    var item = new Item(name, status, price, quantity);
                    newCollection.Items.Add(item);
                }

                Collections.Add(newCollection);
            }
            else
            {
                for (int i = 1; i < lines.Count; i++)
                {
                    var parts = lines[i].Split('|');
                    if (parts.Length != 4) continue;

                    var name = parts[0];
                    var status = parts[1];
                    var price = double.TryParse(parts[2], out double p) ? p : 0;
                    var quantity = int.TryParse(parts[3], out int q) ? q : 0;

                    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(status)) continue;

                    var existingItem = existingCollection.Items.FirstOrDefault(x =>
                        x.Name == name && x.Status == status && x.Price == price);

                    if (existingItem != null)
                    {
                        existingItem.Quantity += quantity;
                    }
                    else
                    {
                        var item = new Item(name, status, price, quantity);
                        existingCollection.Items.Add(item);
                    }
                }
            }

            CollectionsView.ItemsSource = null;
            CollectionsView.ItemsSource = Collections;

            await DisplayAlert("Sukces", "Import zakończony.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Nie udało się wczytać pliku: {ex.Message}", "OK");
        }
    }

}
