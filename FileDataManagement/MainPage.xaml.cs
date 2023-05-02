using Microsoft.Data.Sqlite;
using System.Collections.ObjectModel;

namespace FileDataManagement;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

    async private void ShowDatabase(object sender, EventArgs e) {
        string mainDir = FileSystem.Current.AppDataDirectory;
        string fileName = "DB_Libri_App.db";
        string bundleFileName = "DB_Libri.db";
        string targetFile = Path.Combine(mainDir, fileName);

        if (!File.Exists(targetFile)) {
            //Se il file non esiste nel file system della app, to copia dal bundle 
            try {
                using FileStream outputStream = File.OpenWrite(targetFile); 
                using Stream fs = await FileSystem.Current.OpenAppPackageFileAsync(bundleFileName);
                using var writer = new BinaryWriter(outputStream);
                using var reader = new BinaryReader(fs);
                var bytesRead = 0;
                int bufferSize = 1024;
                var buffer = new byte[bufferSize];
                using (fs) {
                    do {
                        buffer = reader.ReadBytes(bufferSize);
                        bytesRead = buffer.Length;
                        writer.Write(buffer);
                    }
                    while (bytesRead > 0);
                }
            } catch (Exception ex) {
                await DisplayAlert("Errore", ex.Message, "Ok");
                return;
            }
        }

        var connection = new SqliteConnection("Data Source =" + targetFile);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = $"SELECT {QueryFieldsList.Text} FROM Book WHERE {QueryCondition.Text}";

        try {
            using var reader = command.ExecuteReader();

            ISBN.IsVisible = true;
            Titolo.IsVisible = true;
            Anno.IsVisible = true;
            Prezzo.IsVisible = true;
            Editore.IsVisible = true;
            Genere.IsVisible = true;

            var Books = new ObservableCollection<DTO_Book>();

            while(reader.Read()) {
                var b = new DTO_Book(reader);
                Books.Add(b);
            }
            DBDisplay.ItemsSource = Books;
        } catch (Exception ex) {
            await DisplayAlert("Error", ex.Message, "Ok");
        }

        connection.Close();
    }
}

