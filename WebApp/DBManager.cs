using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
public class DBManadger
{
    private SqliteConnection? connection = null;
    private string HashPassword (string password)
    {
        using (var algorithm = SHA256.Create())
        {
            var bytes_hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes_hash);
        }
    }
    public bool ConnectToDB(string path)
    {
        Console.WriteLine("Connection to DB...");

        try
        {
            connection = new SqliteConnection("Data Source=" + path);
            connection.Open();
            if (connection.State != System.Data.ConnectionState.Open) { Console.WriteLine("Failed!"); return false; }
        } 
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }

        Console.WriteLine("Done");
        return true;
        }
    public void Disconnect()
    {
        if (null == connection) { return; }
        if (connection.State != System.Data.ConnectionState.Open) { return; }
        connection.Close();
        Console.WriteLine("Disconnect from db");
    }
    public bool AddUser(string login, string password, string key)
    {
        if (null == connection || connection.State != System.Data.ConnectionState.Open) { return false; }
        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(key)) {return false;}
        string REQUEST  = $"INSERT INTO users (Login, Password, \"Key\") VALUES ('{login}', '{HashPassword(password)}', '{key}')";
        var command = new SqliteCommand(REQUEST, connection);
        int result = 0;
        try
        {
            result = command.ExecuteNonQuery();}
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }

        if (1 == result) { return true; }
        else { return false; }
        
    }

    public bool CheckUser(string login, string password)
    {
        if (null == connection) { return false; }
        if (connection.State != System.Data.ConnectionState.Open) { return false; }

        string REQUEST = "SELECT Login, Password FROM users WHERE Login='" + login + "' AND Password='" + HashPassword(password) + "'";
        var command = new SqliteCommand(REQUEST, connection);

        try
        {
            var reader = command.ExecuteReader();
            if (reader.HasRows) { return true; }
            else { return false; }
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }
    }

    public Json? KeyValue(string login)
    {
        if (null == connection) { return null;}
        if (connection.State != System.Data.ConnectionState.Open) { return null;}
        
        string REQUEST = $"SELECT Key FROM users WHERE Login='{login}'";
        var command = new SqliteCommand(REQUEST, connection);
        try
        {
            var reader = command.ExecuteReader();
            if (reader.Read()) {return new Json{key =reader.GetString(0)};}
            else { return null; }
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return null;
        }
    }

    public Json? TextValue(string login, int id)
    {
        if (null == connection) { return null;}
        if (connection.State != System.Data.ConnectionState.Open) { return null;}
        
        string REQUEST = $"SELECT Text FROM texts WHERE Login='{login}' AND ID_text={id}";
        var command = new SqliteCommand(REQUEST, connection);
        try
        {
            var reader = command.ExecuteReader();
            if (reader.Read()) {return new Json{text =reader.GetString(0)};}
            else { return null; }
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return null;
        }
    }

    public Json? IDValue(string login, string text)
    {
        if (null == connection) { return null;}
        if (connection.State != System.Data.ConnectionState.Open) { return null;}
        
        string REQUEST = $"SELECT ID_text FROM texts WHERE Login='{login}' AND Text='{text}'";
        var command = new SqliteCommand(REQUEST, connection);
        try
        {
            var reader = command.ExecuteReader();
            if (reader.Read()) {return new Json{id =reader.GetInt32(0)};}
            else { return null; }
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return null;
        }
    }

    public bool AddText(string login, string text)
    {
        if (null == connection || connection.State != System.Data.ConnectionState.Open) { return false; }
        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(text)) {return false;}
        string REQUEST  = $"INSERT INTO texts (Login, Text, indicator) VALUES ('{login}', '{text}', 0)";
        var command = new SqliteCommand(REQUEST, connection);
        int result = 0;
        try
        {
            result = command.ExecuteNonQuery();}
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }

        if (1 == result) { return true; }
        else { return false; }
        
    }

    public bool EditText(string login, int id, string text)
    {
        if (null == connection) { return false; }
        if (connection.State != System.Data.ConnectionState.Open) { return false; }
        
        string REQUEST = $"UPDATE texts SET Text='{text}' WHERE Login='{login}' AND ID_text={id} AND indicator=0";
        var command = new SqliteCommand(REQUEST, connection);
        int result = 0;
        try
        {
            result = command.ExecuteNonQuery();}
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }

        if (1 == result) { return true; }
        else { return false; }
    }

    public bool DelText(string login, int id)
    {
        if (null == connection) { return false; }
        if (connection.State != System.Data.ConnectionState.Open) { return false; }
        
        string REQUEST = $"DELETE FROM texts WHERE Login='{login}' AND ID_text={id}";
        var command = new SqliteCommand(REQUEST, connection);
        int result = 0;
        try
        {
            result = command.ExecuteNonQuery();}
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }

        if (1 == result) { return true; }
        else { return false; }
    }

    public Json? SeeText(string login, int id)
    {
        if (null == connection) { return null;}
        if (connection.State != System.Data.ConnectionState.Open) { return null;}
        
        string REQUEST = $"SELECT Text FROM texts WHERE Login='{login}' AND ID_text={id}";
        var command = new SqliteCommand(REQUEST, connection);
        try
        {
            var reader = command.ExecuteReader();
            if (reader.Read()) {return new Json{text =reader.GetString(0)};}
            else { return null; }
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return null;
        }
    }

    public List<Json>? SeeAll(string login)
    {
        
        if (null == connection) { return null;}
        if (connection.State != System.Data.ConnectionState.Open) { return null;}
        
        string REQUEST = $"SELECT ID_text, Text FROM texts WHERE Login='{login}'";
        var command = new SqliteCommand(REQUEST, connection);
        var texts = new List<Json>();
        try
        {
            var reader = command.ExecuteReader();
            while (reader.Read())
            {var text = new Json
            {
                id = reader.GetInt32(0),
                text = reader.GetString(1)
            };
            texts.Add(text);}
            return texts;
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return null;
        }
    }
    public bool EnText(string login, int id, string text)
    {
        if (null == connection) { return false; }
        if (connection.State != System.Data.ConnectionState.Open) { return false; }
        
        string REQUEST = $"UPDATE texts SET Text='{text}', indicator=1 WHERE Login='{login}' AND ID_text={id} AND indicator=0";
        var command = new SqliteCommand(REQUEST, connection);
        int result = 0;
        try
        {
            result = command.ExecuteNonQuery();}
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }

        if (1 == result) { return true; }
        else { return false; }
    }

    public bool DeText(string login, int id, string text)
    {
        if (null == connection) { return false; }
        if (connection.State != System.Data.ConnectionState.Open) { return false; }
        
        string REQUEST = $"UPDATE texts SET Text='{text}', indicator=0 WHERE Login='{login}' AND ID_text={id} AND indicator=1";
        var command = new SqliteCommand(REQUEST, connection);
        int result = 0;
        try
        {
            result = command.ExecuteNonQuery();}
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }

        if (1 == result) { return true; }
        else { return false; }
    }
    public bool AddHistory(string login, string action, int id)
    {
        
        if (null == connection || connection.State != System.Data.ConnectionState.Open) { return false; }
        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(action)) {return false;}
        string REQUEST  = $"INSERT INTO history (Login, ID_text, Action) VALUES ('{login}', {id}, '{action}')";
        var command = new SqliteCommand(REQUEST, connection);
        int result = 0;
        try
        {
            result = command.ExecuteNonQuery();}
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }

        if (1 == result) { return true; }
        else { return false; }
    }

    public List<Json>? SeeHistory(string login)
    {
        
        if (null == connection) { return null;}
        if (connection.State != System.Data.ConnectionState.Open) { return null;}
        
        string REQUEST = $"SELECT ID_text, Action FROM history WHERE Login='{login}'";
        var command = new SqliteCommand(REQUEST, connection);
        var stories = new List<Json>();
        try
        {
            var reader = command.ExecuteReader();
            while (reader.Read())
            {var history = new Json
            {
                id = reader.GetInt32(0),
                action = reader.GetString(1)
            };
            stories.Add(history);}
            return stories;
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return null;
        }
    }
    public bool DelHistory(string login)
    {
        if (null == connection) { return false; }
        if (connection.State != System.Data.ConnectionState.Open) { return false; }
        
        string REQUEST = $"DELETE FROM history WHERE Login='{login}'";
        var command = new SqliteCommand(REQUEST, connection);
        int result = 0;
        try
        {
            result = command.ExecuteNonQuery();}
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }

        if (0 != result) { return true; }
        else { return false; }
    }

    public bool NewPassword(string login, string password)
    {
        if (null == connection) { return false; }
        if (connection.State != System.Data.ConnectionState.Open) { return false; }
        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password)) {return false;}
        
        string REQUEST = $"UPDATE users SET Password='{HashPassword(password)}' WHERE Login='{login}'";
        var command = new SqliteCommand(REQUEST, connection);
        int result = 0;
        try
        {
            result = command.ExecuteNonQuery();}
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }

        if (1 == result) { return true; }
        else { return false; }
    }
}