using Microsoft.Data.Sqlite;


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

        //await DisplayAlert("Info", $"The book database is located in {targetFile}", "Ok");

        if (!File.Exists(targetFile)) {
            //Se it file non esiste nel file system della app, to copia dal bundle 
            try {
                using FileStream outputStream = File.OpenWrite(targetFile); 
                using Stream fs = await FileSystem.Current.OpenAppPackageFileAsync(bundleFileName);
                using BinaryWriter writer = new BinaryWriter(outputStream); 
                using (BinaryReader reader = new BinaryReader(fs)) {
                    var bytesRead = 0;
                    int bufferSize = 1024; 
                    byte[] bytes; 
                    var buffer = new byte[bufferSize]; 
                    using (fs) {
                        do {
                            buffer = reader.ReadBytes(bufferSize); 
                            bytesRead = buffer.Count(); writer.Write(buffer);
                        }
                        while (bytesRead > 0) ;
                    }
                }
            } catch (Exception ex) {
                await DisplayAlert("Errore", ex.Message, "Ok");
                return;
            }
        }

        using var connection = new SqliteConnection("Data Source =" + targetFile);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = QueryInput.Text;

        using (var reader = command.ExecuteReader()) {
            string output = "";
            for (int k = 0; k < reader.FieldCount; k++) {
                DBDisplay.AddColumnDefinition(new ColumnDefinition());
            }
            int i = 0;
            while (reader.Read()) {
                DBDisplay.AddRowDefinition(new());

                for (int j = 0; j < reader.FieldCount; j++) {
                    DBDisplay.Add(new Label {
                        Text = reader.GetString(j)
                    }, j, i);
                    output += $"{reader.GetString(j)}, ";
                    i++;
                }
                output += "\n";
            }
            await DisplayAlert("Database", output, "Ok");
        }

        connection.Close();
    }
}

