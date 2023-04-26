using Microsoft.Data.Sqlite;

namespace FileDataManagement;

public class DTO_Book {
    public long? ISBN { get; set; }
    public string Titolo { get; set; }
    public int? Anno { get; set; }
    public double? Prezzo { get; set; }
    public int? Editore { get; set; }
    public int? Genere { get; set; }

    public DTO_Book(SqliteDataReader r) {
        ISBN = null;
        Titolo = null; 
        Anno = null; 
        Prezzo = null; 
        Editore = null; 
        Genere = null;

        try { ISBN    = r.GetInt64 (r.GetOrdinal("ISBN"   )); } catch { ISBN    = null; }
        try { Titolo  = r.GetString(r.GetOrdinal("Titolo" )); } catch { Titolo  = null; }
        try { Anno    = r.GetInt32 (r.GetOrdinal("Anno"   )); } catch { Anno    = null; }
        try { Prezzo  = r.GetDouble(r.GetOrdinal("Prezzo" )); } catch { Prezzo  = null; }
        try { Editore = r.GetInt32 (r.GetOrdinal("Editore")); } catch { Editore = null; }
        try { Genere  = r.GetInt32 (r.GetOrdinal("Genere" )); } catch { Genere  = null; }
    }
}