using System.Reflection.PortableExecutable;
using System;
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

        await DisplayAlert("Info", $"The book database is located in {targetFile}", "Ok");

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

        using (var connection = new SqliteConnection("Data Source =" + targetFile)) {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
		    SELECT *
		    FROM Book
	        ";
            using (var reader = command.ExecuteReader()) {
                while (reader.Read()) {
                    for (int i = 0; i < reader.FieldCount; i++) {
                        Console.Write($"{reader.GetString(i)}, ");
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}

