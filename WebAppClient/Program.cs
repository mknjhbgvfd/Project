using System.ComponentModel;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

HttpClient client = new HttpClient();

Token? SignupOnServer(string? _username, string? _password, string? _key)
{
    if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password) || string.IsNullOrEmpty(_key))
    { return null; }

    var request_body = new { login = _username, password = _password, key = _key};
    var request = new StringContent(JsonSerializer.Serialize(request_body), Encoding.UTF8, "application/json");
    var response = client.PostAsync("/signup", request).Result;
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("Регистрация прошла успешно"); return response.Content.ReadFromJsonAsync<Token>().Result;
    }
    else
    {
        Console.WriteLine("Регистрация провалена"); return null;
    }
}
Token? LoginOnServer(string? _username, string? _password)
{
    if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password))
    { return null; }

    var request_body = new { login = _username, password = _password};
    var request = new StringContent(JsonSerializer.Serialize(request_body), Encoding.UTF8, "application/json");
    var response = client.PostAsync("/login", request).Result;
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("Авторизация прошла успешно"); return response.Content.ReadFromJsonAsync<Token>().Result;
    }
    else
    {
        Console.WriteLine("Авторизация провалена"); return null;
    }
}
string? Key_Value(string? _username)
{
    if (string.IsNullOrEmpty(_username))
    { return null; }
    else{
    var request_body = new { login = _username};
    var request = new StringContent(JsonSerializer.Serialize(request_body), Encoding.UTF8, "application/json");
    var response = client.PostAsync("/keyvalue", request).Result;
    if (response.IsSuccessStatusCode)
    {
        var response_body = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<Json>(response_body);
        return result.key;
    }
    else{return null;}}
}
string? Text_Value(string? _username, int? _id)
{
    if (string.IsNullOrEmpty(_username) || _id == null)
    { return null; }
    else{
    var request_body = new { login = _username, id = _id};
    var request = new StringContent(JsonSerializer.Serialize(request_body), Encoding.UTF8, "application/json");
    var response = client.PostAsync("/textvalue", request).Result;
    if (response.IsSuccessStatusCode)
    {
        var response_body = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<Json>(response_body);
        return result.text;
    }
    else{return null;}}
}
int? ID_Value(string? _username, string? _text)
{
    if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_text))
    { return null; }
    else{
    var request_body = new { login = _username, text = _text};
    var request = new StringContent(JsonSerializer.Serialize(request_body), Encoding.UTF8, "application/json");
    var response = client.PostAsync("/idvalue", request).Result;
    if (response.IsSuccessStatusCode)
    {
        var response_body = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<Json>(response_body);
        return result.id;
    }
    else{return null;}}
}
async Task Add_Text(string? _username, string? _text)
{
    if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_text))
    { Console.WriteLine("Ошибка ввода"); }
    else{
    var request_body = new { login = _username, text = _text};
    var request = new StringContent(JsonSerializer.Serialize(request_body), Encoding.UTF8, "application/json");
    var response = await client.PostAsync("/addtext", request);
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("Текст добавлен");
        int? id = ID_Value(_username, _text);
        if (id != null)
        {
            Add_History(_username, "добавлен", id);
        }
    }
    else
    {  Console.WriteLine("Что-то пошло не так. Повторите попытку");}}
}
async Task Edit_Text(string? _username, int? _id, string? _text)
{
    if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_text) || _id == null)
    { Console.WriteLine("Ошибка ввода");}
    else{
    var request_body = new { login = _username, id = _id, text = _text };
    var request = new StringContent(JsonSerializer.Serialize(request_body), Encoding.UTF8, "application/json");
    var response = await client.PatchAsync("/edittext", request);
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("Текст изменен");
        Add_History(_username, "изменен", _id);
    }
    else{ Console.WriteLine("Что-то пошло не так. Повторите попытку");}}
}
async Task Del_Text(string? _username, int? _id)
{
    if (string.IsNullOrEmpty(_username)||_id==null)
    { Console.WriteLine("Ошибка ввода");}
    else{
    var request_body = new {login = _username, id = _id};
    var request = new StringContent(JsonSerializer.Serialize(request_body), Encoding.UTF8, "application/json");
    var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/deltext") {Content =request });
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("Текст удален");
        Add_History(_username, "удален", _id);
    }
    else{ Console.WriteLine("Что-то пошло не так. Повторите попытку");}}
}
async Task See_Text(string? _username, int? _id)
{
    if (string.IsNullOrEmpty(_username)||_id==null)
    { Console.WriteLine("Ошибка ввода");}
    else{
    var request_body = new {login = _username, id = _id};
    var request = new StringContent(JsonSerializer.Serialize(request_body), Encoding.UTF8, "application/json");
    var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/seetext") {Content =request });
    if (response.IsSuccessStatusCode)
    {
        var response_body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Json>(response_body);
        Console.WriteLine($"Текст:\n{result.text}");
        Add_History(_username, "просмотрен", _id);
    }
    else
    { Console.WriteLine("Текст не найден. Повторите попытку");}}
}
async Task See_All(string? _username)
{
    if (string.IsNullOrEmpty(_username))
    {Console.WriteLine("Что-то пошло не такю Повторите попытку");}
    else
    {
        var request_body = new {login = _username};
        var request = new StringContent(JsonSerializer.Serialize(request_body), Encoding.UTF8, "application/json");
        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/seeall") {Content=request});
        if (response.IsSuccessStatusCode)
        {var response_body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<Json>>(response_body);
        foreach (Json r in result)
        {
        Console.WriteLine($"ID текста: {r.id}");
        Console.WriteLine($"Текст: {r.text}");}
        Add_History(_username, "Просмотрены все тексты", 0);}
        else
        { Console.WriteLine("У данного пользователя нет текстов");}
    }
}
async Task En_Text(string? _username, int? _id, string? _code)
{
    if (string.IsNullOrEmpty(_username)||_id==null || string.IsNullOrEmpty(_code))
    { Console.WriteLine("Ошибка ввода");}
    else
    {
        string? _text = Text_Value(_username, _id);
        if (_text!= null) {
            var request_body = new { login = _username, id = _id, text = _text, key = _code};
            var request = new StringContent(JsonSerializer.Serialize(request_body), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/encrypt", request);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Текст зашифрован");
                Add_History(_username, "зашифрован", _id);
            }else
            {  Console.WriteLine("Что-то пошло не так. Повторите попытку");}
        }
        else
        {  Console.WriteLine("Что-то пошло не так. Повторите попытку");}
    }
}
async Task De_Text(string? _username, int? _id, string? _code)
{
    if (string.IsNullOrEmpty(_username)||_id==null || string.IsNullOrEmpty(_code))
    { Console.WriteLine("Ошибка ввода");}
    else
    {
        string? _text = Text_Value(_username, _id);
        if (_text!= null) {
            var request_body = new { login = _username, id = _id, text = _text, key = _code};
            var request = new StringContent(JsonSerializer.Serialize(request_body), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/decrypt", request);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Текст расшифрован");
                Add_History(_username, "расшифрован", _id);
            }else
            {  Console.WriteLine("Что-то пошло не так. Повторите попытку");}
        }else
        {  Console.WriteLine("Что-то пошло не так. Повторите попытку");}
        
    }
}
async Task Add_History(string? _username, string? _action, int? _id)
{
    if (string.IsNullOrEmpty(_username))
    { Console.WriteLine("Ошибка ввода"); }
    else{
    var request_body = new { login = _username, action = _action, id = _id};
    var request = new StringContent(JsonSerializer.Serialize(request_body), Encoding.UTF8, "application/json");
    var response = await client.PostAsync("/addhistory", request);
    if (!response.IsSuccessStatusCode)
    { Console.WriteLine("Не удалось записать действие в историю");}}
}
async Task See_History(string? _username)
{
    if (string.IsNullOrEmpty(_username))
    {Console.WriteLine("Что-то пошло не так. Повторите попытку");}
    else
    {
        var request_body = new {login = _username};
        var request = new StringContent(JsonSerializer.Serialize(request_body), Encoding.UTF8, "application/json");
        var response =  await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/seehistory") {Content=request});
        if (response.IsSuccessStatusCode)
        {var response_body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<Json>>(response_body);
        if (result != null && result.Count > 0)
        {foreach (Json r in result)
        {
            if (r.id == 0)
            {Console.WriteLine(r.action);}
            else
            {
                Console.WriteLine($"Был {r.action} текст с ID: {r.id}");
            }
        }}else
        { Console.WriteLine("История пуста");}
        }else
        { Console.WriteLine("Что-то пошло не так");}
    }
}
async Task Del_History(string? _username)
{
    if (string.IsNullOrEmpty(_username))
    { Console.WriteLine("Ошибка ввода");}
    else{
    var request_body = new {login = _username};
    var request = new StringContent(JsonSerializer.Serialize(request_body), Encoding.UTF8, "application/json");
    var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/delhistory") {Content =request });
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("История удалена");
    }
    else{ Console.WriteLine("Что-то пошло не так. Повторите попытку");}}
}
Token? New_Password(string? _username, string? _password)
{
    if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password))
    { Console.WriteLine("Ошибка ввода"); return null;}
    else
    {
    var request_body = new { login = _username, password = _password };
    var request = new StringContent(JsonSerializer.Serialize(request_body), Encoding.UTF8, "application/json");
    var response = client.PatchAsync("/newpassword", request).Result;
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("Пароль изменен");
        Add_History(_username, "Изменен пароль", 0);
        return response.Content.ReadFromJsonAsync<Token>().Result;
    }
    else{ Console.WriteLine("Что-то пошло не так. Повторите попытку"); return null;}
    }
}
const string DEFAULT_SERVER_URL = "http://localhost:5093";
Console.WriteLine("Введите URL сервера (http://localhost:5093 по умолчанию):");
string? server_url = Console.ReadLine();
if (server_url == null || server_url.Length == 0)
{
    server_url = DEFAULT_SERVER_URL;
}
try
{
    client.BaseAddress = new Uri(server_url); Token? token = null; bool input = false;
    string name = ""; string code = "";
    do
    {
        bool k = true;
        if (!input)
        {
            Console.WriteLine("Нажмите 1, если хотите зарегистрироваться, 2 - если войти, 3 - выйти");
            string? number = Console.ReadLine();
            if (string.IsNullOrEmpty(number) || !"123".Contains(number)) { Console.WriteLine("Ошибка ввода. Повторите попытку"); continue; }
            if (number == "3") { return; }
            Console.WriteLine("Введите логин и пароль:");
            string? username = Console.ReadLine();
            if (string.IsNullOrEmpty(username)) {Console.WriteLine("Логин не может быть пустым"); k = false; continue;}
            foreach (char i in username)
                {
                    if (!"AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789".Contains(i) && k == true)
                    { Console.WriteLine("Логин может содержать только латинские буквы или цифры"); k=false; continue; }
                }
            string? password = Console.ReadLine();
            if (string.IsNullOrEmpty(password)) {Console.WriteLine("Пароль не может быть пустым"); k = false; continue;}
            foreach (char i in password)
                {
                    if (!"AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789".Contains(i) && k == true)
                    { Console.WriteLine("Пароль может содержать только латинские буквы или цифры"); k=false; continue; }
                }
            if (number == "1") {
                Console.WriteLine("Введите ключ для шифрования/дешифровки:");
                string? key = Console.ReadLine();
                if (string.IsNullOrEmpty(key)) {Console.WriteLine("Ключ не может быть пустым"); continue;}
                foreach (char i in key)
                {
                    if (!"abcdefghijklmnopqrstuvwxyz".Contains(i) && k == true)
                    { Console.WriteLine("Ключ может содержать только строчные латинские буквы"); k=false; continue; }
                }
                if (k == true)
                {token = SignupOnServer(username, password, key);}
                }
            else { 
                if (k==true) {token = LoginOnServer(username, password);} }
            if (token == null)
            {
                Console.WriteLine("Не удалось аутентифицироваться. Повторите попытку или зарегистрируйтесь:"); continue;
            }
            else {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value.access_token);
                input = true; name = username; code = Key_Value(name);}
        }
        Console.WriteLine("Введите:\n1 - Добавить текст\n2 - Изменить текст\n3 - Удалить текст\n4 - Просмотреть один текст\n5 - Просмотреть все тексты\n6 - Зашифровать текст\n7 - Расшифровать текст\n8 - Посмотреть историю запросов\n9 - Удалить историю запросов\n10 - Изменить пароль\n11 - Выйти");
        string? n = Console.ReadLine();
        if (n == null) { Console.WriteLine("Ошибка ввода. Повторите попытку"); continue; }
        if (n == "1") {
            Console.WriteLine("Введите текст:");
            string? text = Console.ReadLine();
            if (string.IsNullOrEmpty(text)) {Console.WriteLine("Текст не может быть пустым"); k=false; continue;}
            foreach (char i in text)
            {
                if (!"abcdefghijklmnopqrstuvwxyz".Contains(i) && k == true)
                { Console.WriteLine("Текст может содержать только строчные латинские буквы"); k= false; continue; }
            }
            if (k == true) {await Add_Text(name,text);}
        }
        if (n == "2")
        {
            Console.WriteLine("Введите ID текста:");
            string? _id = Console.ReadLine();
            if (string.IsNullOrEmpty(_id)) {{Console.WriteLine("ID не может быть пустым"); k = false; continue;}}
            foreach (char i in _id)
            {
                if (!"0123456789".Contains(i)&&k==true) {Console.WriteLine("ID должен быть числом");k=false; continue;}
            }
            Console.WriteLine("Введите измененный текст:");
            string? text = Console.ReadLine();
            if (string.IsNullOrEmpty(text)) {Console.WriteLine("Текст не может быть пустым"); k=false; continue;}
            foreach (char i in text)
            {
                if (!"abcdefghijklmnopqrstuvwxyz".Contains(i)&&k==true)
                { Console.WriteLine("Текст может содержать только строчные латинские буквы"); k=false;continue; }
            }
            if (k==true) {
                int id = int.Parse(_id);
                await Edit_Text(name, id, text);}
        }
        if (n == "3")
        {
            Console.WriteLine("Введите ID текста:");
            string? _id = Console.ReadLine();
            if (string.IsNullOrEmpty(_id)) {{Console.WriteLine("ID не может быть пустым"); k = false;continue;}}
            foreach (char i in _id)
            {
                if (!"0123456789".Contains(i)&&k==true) {Console.WriteLine("ID должен быть числом"); k = false; continue;}
            }
            if (k == true) {
                int id = int.Parse(_id);
                await Del_Text(name, id);}
        }
        if (n =="4")
        {
            Console.WriteLine("Введите ID текста:");
            string? _id = Console.ReadLine();
            if (string.IsNullOrEmpty(_id)) {{Console.WriteLine("ID не может быть пустым"); k = false; continue;}}
            foreach (char i in _id)
            {
                if (!"0123456789".Contains(i) && k == true) {Console.WriteLine("ID должен быть числом");k = false; continue;}
            }
            if (k == true)
            {
                int id = int.Parse(_id);
                await See_Text(name, id);
            }
        }
        if (n=="5")
        {
            await See_All(name);
        }
        if (n=="6")
        {
            Console.WriteLine("Введите ID текста:");
            string? _id = Console.ReadLine();
            if (string.IsNullOrEmpty(_id)) {{Console.WriteLine("ID не может быть пустым"); k = false; continue;}}
            foreach (char i in _id)
            {
                if (!"0123456789".Contains(i)&& k == true) {Console.WriteLine("ID должен быть числом"); k = false; continue;}
            }
            if (k == true){
                int id = int.Parse(_id);
                await En_Text(name, id, code);}
        }
        if (n=="7")
        {
            Console.WriteLine("Введите ID текста:");
            string? _id = Console.ReadLine();
            if (string.IsNullOrEmpty(_id)) {{Console.WriteLine("ID не может быть пустым"); k= false; continue;}}
            foreach (char i in _id)
            {
                if (!"0123456789".Contains(i)&& k == true) {Console.WriteLine("ID должен быть числом"); k = false;continue;}
            }
            if (k == true) {
                int id = int.Parse(_id);
                await De_Text(name, id, code);}
        }
        if (n=="8")
        {
            await See_History(name);
        }
        if (n=="9")
        {
            await Del_History(name);
        }
        if (n=="10")
        {
            Console.WriteLine("Введите новый пароль");
            string? password = Console.ReadLine();
            if (string.IsNullOrEmpty(password)) {Console.WriteLine("Пароль не должен быть пустым"); k = false; continue;}
            if (k == true) {
                Token? token_new = New_Password(name, password);
                if (token.Value.access_token!=token_new.Value.access_token && token_new!= null)
                {token = token_new;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value.access_token);}}
        }
        if (n =="11"){break;}
        
    }while (true);
}
catch (Exception exp)
{
    Console.WriteLine("Ошибка: " + exp.Message);
}


public struct Token
{
    public required string access_token { get; set; }
}
struct Json
{
    public string text { get; set; }
    public string key {get; set;}
    public string action {get; set;}
    public int id {get; set;}
}