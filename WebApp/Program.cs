using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();

bool CustomLifetimeValidator(DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters)
{
    if (expires == null) { return false; }
    return expires > DateTime.UtcNow;
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = AuthOptions.ISSUER,
        ValidateAudience = true,
        ValidAudience = AuthOptions.AUDIENCE,
        ValidateLifetime = true,
        LifetimeValidator = CustomLifetimeValidator,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = AuthOptions.GetKey()
    };
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

Code_WebAdapter code_v = new Code_WebAdapter();
DBManadger db = new DBManadger();

app.MapGet("/", () => "text encrypt/decrypt");
app.MapPost("/encrypt", [Authorize] ( [FromBody] Json j) => {
    j.text = code_v.Encrypt(j.text, j.key);
    if (db.EnText(j.login, j.id, j.text))
    { return Results.Ok(); }
    else {return Results.BadRequest();}
});
app.MapPost("/decrypt", [Authorize] ([FromBody] Json j) =>
{
    j.text = code_v.Decrypt(j.text, j.key);
    if (db.DeText(j.login, j.id, j.text))
    { return Results.Ok(); }
    else {return Results.BadRequest();}
});
app.MapPost("/login", ([FromBody] Json j) =>
{
    if (!db.CheckUser(j.login, j.password)) { return Results.Unauthorized(); }
    return code_v.Login(j.login, j.password);
});
app.MapPost("/signup", ([FromBody] Json j) =>
{
    if (string.IsNullOrWhiteSpace(j.key)) 
    { return Results.BadRequest("key is required and cannot be empty or whitespace"); }
    foreach (char i in j.key)
    {
        if (!"abcdefghijklmnopqrstuvwxyz".Contains(i)) { return Results.Conflict("incorrect key"); }
    }
    if (db.AddUser(j.login, j.password, j.key))
    { Console.WriteLine("User "+j.login + " registred successfull"); return code_v.Login(j.login, j.password); }
    else { return Results.Problem("Failed to register user "+j.login); }
});
app.MapPost("/keyvalue", [Authorize] ([FromBody] Json j) =>
{
    Json? json = db.KeyValue(j.login);
    if (json != null)
    {return Results.Ok(json);}
    else {return Results.NotFound();}
});
app.MapPost("/textvalue", [Authorize] ([FromBody] Json j) =>
{
    Json? json = db.TextValue(j.login, j.id);
    if (json != null)
    {return Results.Ok(json);}
    else {return Results.NotFound();}
});
app.MapPost("/idvalue", [Authorize] ([FromBody] Json j) =>
{
    Json? json = db.IDValue(j.login, j.text);
    if (json != null)
    {return Results.Ok(json);}
    else {return Results.NotFound();}
});
app.MapPost("/addtext", [Authorize] ([FromBody] Json j) =>
{   if (j.text == "") { return Results.BadRequest("text = null"); }
    foreach (char i in j.text)
    {
        if (!"abcdefghijklmnopqrstuvwxyz".Contains(i)) { return Results.Conflict("incorrect text"); }
    }
    if (db.AddText(j.login, j.text))
    { return Results.Ok(); }
    else {return Results.BadRequest();}
});
app.MapPatch("/edittext", [Authorize] ([FromBody] Json j) =>
{
    if (j.text == "") { return Results.BadRequest("text = null"); }
    foreach (char i in j.text)
    {
        if (!"abcdefghijklmnopqrstuvwxyz".Contains(i)) { return Results.Conflict("incorrect text"); }
    }
    if (db.EditText(j.login, j.id, j.text))
    { return Results.Ok(); }
    else {return Results.BadRequest();}
});
app.MapDelete("/deltext", [Authorize] ([FromBody] Json j) =>
{
    if (db.DelText(j.login, j.id))
    {return Results.Ok(); }
    else {return Results.BadRequest();}
});
app.MapGet("/seetext", [Authorize] ([FromBody] Json j) =>
{
    Json? json = db.SeeText(j.login, j.id);
    if (json != null)
    { return Results.Ok(json);}
    else {return Results.NotFound();}
});
app.MapGet("/seeall", [Authorize] ([FromBody] Json j) =>
{
    var texts = db.SeeAll(j.login); 
    if (texts == null || texts.Count == 0)
    {return Results.NotFound();}
    else {return Results.Ok(texts);}
});
app.MapPost("/addhistory", [Authorize] ([FromBody] Json j) =>
{
    if (db.AddHistory(j.login, j.action, j.id))
    { return Results.Ok(); }
    else {return Results.BadRequest();}
});
app.MapGet("/seehistory", [Authorize] ([FromBody] Json j) =>
{
    var stories = db.SeeHistory(j.login);
    if (stories == null)
    {return Results.NotFound();}
    else { return Results.Ok(stories);}
});
app.MapDelete("/delhistory", [Authorize] ([FromBody] Json j) =>
{
    if (db.DelHistory(j.login))
    {return Results.Ok(); }
    else {return Results.BadRequest();}
});
app.MapPatch("/newpassword", [Authorize] ([FromBody] Json j) =>
{
    if (!db.NewPassword(j.login, j.password)) {return Results.BadRequest();}
    return code_v.Login(j.login, j.password);
});

const string DB_PATH = "/home/zebra/Project/WebApp/users.db";
if (!db.ConnectToDB(DB_PATH))
{
    Console.WriteLine("Failed to connect to db " + DB_PATH + "\nShutdown!");
    return;
}
app.Run();
db.Disconnect();

public struct Json
{
    public string key {get; set;}
    public string text { get; set; }
    public string login { get; set; }
    public string password { get; set; }
    public string action { get; set; }
    public int id {get; set;}
}

public class AuthOptions
{
    public const string ISSUER = "WebApp";
    public const string AUDIENCE = "WebAppAudience";
    public static SymmetricSecurityKey GetKey()
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes("WebAppPasswordWebAppPasswordWebAppPasswordWebAppPasswordWebAppPassword"));
    }
}

public class Code_WebAdapter
{
    public IResult Login(string login, string password)
    {
        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password)) {return Results.BadRequest();}
        var jwt = new JwtSecurityToken(
            issuer: AuthOptions.ISSUER,
            audience: AuthOptions.AUDIENCE,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: new SigningCredentials(AuthOptions.GetKey(), SecurityAlgorithms.HmacSha256)
        );
        var encodedToken = new JwtSecurityTokenHandler().WriteToken(jwt);
        var response = new
        {
            access_token = encodedToken,
            username = login
        };
        return Results.Ok(response);
        
    }
    private Code_Vizhenera code_v = new Code_Vizhenera();
    public string Encrypt(string text, string key)
    {
        foreach (char i in text)
        {
            if (!"abcdefghijklmnopqrstuvwxyz".Contains(i))
            { return ""; }
        }
        foreach (char i in key)
        {
            if (!"abcdefghijklmnopqrstuvwxyz".Contains(i))
            { return ""; }
        }
        return code_v.Encrypt(text, key);
    }
    public string Decrypt(string text, string key)
    {
        foreach (char i in text)
        {
            if (!"abcdefghijklmnopqrstuvwxyz".Contains(i))
            { return ""; }
        }
        foreach (char i in key)
        {
            if (!"abcdefghijklmnopqrstuvwxyz".Contains(i))
            { return ""; }
        }
        return code_v.Decrypt(text, key);
    }
}

public class Code_Vizhenera
{
    private Dictionary<char, int> code;
    public Code_Vizhenera()
    {
        code = new Dictionary<char, int>()
        {
            ['a'] = 0,
            ['b'] = 1,
            ['c'] = 2,
            ['d'] = 3,
            ['e'] = 4,
            ['f'] = 5,
            ['g'] = 6,
            ['h'] = 7,
            ['i'] = 8,
            ['j'] = 9,
            ['k'] = 10,
            ['l'] = 11,
            ['m'] = 12,
            ['n'] = 13,
            ['o'] = 14,
            ['p'] = 15,
            ['q'] = 16,
            ['r'] = 17,
            ['s'] = 18,
            ['t'] = 19,
            ['u'] = 20,
            ['v'] = 21,
            ['w'] = 22,
            ['x'] = 23,
            ['y'] = 24,
            ['z'] = 25
        };
    }
    public string Encrypt(string text, string key)
    {
        int len = key.Length;
        string res = "";
        for (int i = len; i < text.Length; i++)
        { key += key[i % len]; }
        for (int i = 0; i < text.Length; i++)
        {
            int x = (code[text[i]] + code[key[i]]) % 26;
            foreach (var j in code)
            {
                if (x == j.Value) { res += j.Key; }
            }
        }
        return res;
    }
    public string Decrypt(string text, string key)
    {
        int len = key.Length;
        string res = "";
        for (int i = len; i < text.Length; i++)
        { key += key[i % len]; }
        for (int i = 0; i < text.Length; i++)
        {
            int x = (code[text[i]] - code[key[i]] + 26) % 26;
            foreach (var j in code)
            {
                if (x == j.Value) { res += j.Key; }
            }
        }
        return res;
    }
}