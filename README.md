# FileDataManagement
 
## Obiettivo

Lo scopo dell'applicazione è la semplice gestione di un database attraverso query di base. 
La particolarità delle query che andremo ad effettuare è che dovranno essere gestite tramite codice in un applicazione **.NET MAUI**. 
In aggiunta per vari linguaggi di programmazione sono state sviluppate delle librerie che ci aiutano con la gestione dei database e delle query. 
Nel nostro caso per applicazioni "dotnet" sono stati sviluppati i pacchetti in aggiunta prendono il nome di **NuGet** di nostra scelta è stato `Microsoft.Data.SQlite`.

## Processo di Sviluppo

La prima cosa da chiarire è su quale database vogliamo operare, nel nostro caso abbiamo un database SQL di libri, di nome `DB_Libri.db`.
Il database in questione viene fornito all'applicazione come **Bundled Resource**, ossia dei file che vengono inclusi nell'applicazione e che possono essere utilizzati durante l'esecuzione dell'applicazione stessa.
Il prossimo passo è accedere a suddetto database.

### Problema dei File Read Only

I bundled files, specialmente quelli di tipo database, sono solamente accessibili in lettura.
Per poter effettuare modifiche su di essi dobbiamo effettuarne una **copia**.
Per effettuare una copia possiamo optare per 2 approcci :
- **Cache Directory** $\rightarrow$ Permette di memorizzare delle informazioni in una cache.
- **App Data Directory** $\rightarrow$ Memoria permanente sul disco fisso nella propria cartella `Users > AppData`.

Tuttavia nell'approccio a Cache Directory il sistema operativo può eventualmente decidere di cancellare le informazioni contenute in **cache** nel caso abbia bisogno di spazio, perchè per l'appunto si tratta di una **memoria temporanea**.
Per questo motivo abbiamo scelto la App Data Directory, perchè è una memoria permanente.

### Risoluzione dei File Read Only

Dichiariamo alcune variabili essenziali :

```cs
string mainDir = FileSystem.Current.AppDataDirectory; // 1
string fileName = "DB_Libri_App.db";                  // 2
string bundleFileName = "DB_Libri.db";                // 3
string targetFile = Path.Combine(mainDir, fileName);  // 4
```

Dove :
1) La directory della nostra cartella in `AppData`.
2) Il nome che dovrà avere la copia del database.
3) Il nome del nostro database originale.
4) L'**URI** completo dato dall'unione dell'`AppData` e la copia del database.

Successivamente effettuiamo un controllo sull'esistenza della copia nel nostro `AppData` :

```cs
if (!File.Exists(targetFile)) { . . . }
```

Se questa non esiste allora dobbiamo iniziare un processo di copiatura del nostro bundled file :

```cs
using FileStream outputStream = File.OpenWrite(targetFile); 
using Stream fs = await FileSystem.Current.OpenAppPackageFileAsync(bundleFileName);
using var writer = new BinaryWriter(outputStream);
using var reader = new BinaryReader(fs);
```

Sfruttiamo l'uso della classe `FileStream` per istanziare un collegamento tra il codice e il file system per prepararlo alla scrittura del file.
Invece con l'uso della classe `Stream` andiamo a leggere il file che dobbiamo copiare.

Per effettuare la copia del file stesso invece sfruttiamo un **buffer** di dimensioni fisse a 1024 byte :

```cs
int bufferSize = 1024;
var buffer = new byte[bufferSize];
```

Sfruttando questo buffer possiamo poi leggere il nostro bundle file in **blocchi** di 1024 byte alla volta e scrive i dati letti in un'output stream.
Questo approccio è spesso utilizzato per copiare grandi quantità di dati da un file a un'altra destinazione senza dover caricare tutto il file in memoria contemporaneamente.

### Ado (Active Database Object)

Per effettuare una query al database dobbiamo come prima cosa istanziare una connessione con esso attraverso l'uso della classe `SqliteConnection` provveduta dalla libreria `Microsoft.Data.SQlite`.
Questa libreria è conveniente perchè riesce a fornirci delle interfacce e delle classi di tipo **Ado (Active Database Object)**, tipi di classi che consentono agli sviluppatori di accedere a origini dati, come database, utilizzando oggetti e metodi.

```cs
var connection = new SqliteConnection("Data Source =" + targetFile);
connection.Open();

var command = connection.CreateCommand();
command.CommandText = $"SELECT * FROM Book";
```

E l'unico comando che andremo a provare sarà semplicemente un display di tutta la tabella `Book` dal nostro database copiato in `AppData`.

```cs
using var reader = command.ExecuteReader();
```

Il comando che verrà eseguito avrà poi il suo contenuto restituito nella variabile `reader`.

### Creazione Classe DTO_Book

I **DTO (Data Transfer Object)** sono classi che contengono solamente degli attributi e ci torna utile perchè in questo modo possiamo sfruttare le **analogie** tra la programmazione a oggetti e i database per contenere i nostri dati.
La classe creata rappresenterà un singolo record della tabella con i suoi relativi attributi (campi) e per contenere l'intera tabella creeremo una `ObservableCollection` di `DTO_Book` :

```cs
var Books = new ObservableCollection<DTO_Book>();
```

Una `ObservableCollection` ci può tornare utile perchè provvede nelle "notifiche" di cambiamento della collezione ogni volta che un elemento viene aggiunto o rimosso da esso, ma non è comunque mondo differente da una normale `Collection` o `List`.

La classe `DTO_Book` avrà questo aspetto :

```cs
public class DTO_Book {
    public long? ISBN { get; set; }
    public string Titolo { get; set; }
    public int? Anno { get; set; }
    public double? Prezzo { get; set; }
    public int? Editore { get; set; }
    public int? Genere { get; set; }
	
	public DTO_Book(SqliteDataReader r) {
		. . .
	}
}
```

E conterrà appunto i campi della tabella.

Con un semplice ciclo di `while` che itera tante volte quanto è il numero di record ci permette di riempire la collection :

```cs
while(reader.Read()) {
	var b = new DTO_Book(reader);
	Books.Add(b);
}
```

### Display della Tabella Book

Per mostrare il contenuto intero della tabella dobbiamo impostare nel `MainPage.xaml` un ambiente `<CollectionView>` :

```xml
<CollectionView x:Name="DBDisplay">
	<CollectionView.ItemTemplate>
		<DataTemplate>
			. . .
		</DataTemplate>
	</CollectionView.ItemTemplate>
</CollectionView>
```

In questo **Controller** impostiamo come devono essere visualizzati tutti i dati di una **collezione**.
Per far ciò utilizziamo un `DataTemplate`, dove in esso bisogna specificare come ogni singolo elemento di una `Collection` verrà visualizzato :

```cs
<Grid>
	<Grid.ColumnDefinitions> . . . </Grid.ColumnDefinitions>
	
	<Label Grid.Column="0" Text="{Binding ISBN}"   />
	<Label Grid.Column="1" Text="{Binding Titolo}" />
	<Label Grid.Column="2" Text="{Binding Anno}"   />
	<Label Grid.Column="3" Text="{Binding Prezzo}" />
	<Label Grid.Column="4" Text="{Binding Editore}"/>
	<Label Grid.Column="5" Text="{Binding Genere}" />
</Grid>
```

Questo è il contenuto del `DataTemplate`, una griglia da **1 riga** e **7 colonne**.
Ogni singola colonna è riservata alla mostra di un singolo campo dell'oggetto `DTO_Book` attraverso l'istruzione di `Binding`.

Proprio qui viene introdotto in concetto di **MVVM (Model View View Model)**, un modello che è capace di presentare un'interfaccia e di unirsi ad essa.
Infatti con il `Binding` uniamo proprio i campi dell'oggetto con l'interfaccia grafica.

Infine nel codice :

```cs
DBDisplay.ItemsSource = Books;
```

Sarà questa istruzione che permetterà di collegare la `ObservableCollection` di oggetti `DTO_Book` precedentemente dichiarata con la `CollectionView`.
