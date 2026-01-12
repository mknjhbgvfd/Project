using Microsoft.AspNetCore.Http;

namespace UnitTests;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestEncryptTrue()
    {
        Code_WebAdapter code = new Code_WebAdapter();

        const string text = "qwerty";
        const string key = "abc";

        string result = code.Encrypt(text, key);

        Assert.IsTrue(result == "qxgrua");
    }
    [TestMethod]
    public void TestEncryptFalse()
    {
        Code_WebAdapter code = new Code_WebAdapter();

        const string text = "qwerty";
        const string key = "abc123";

        string result = code.Encrypt(text, key);

        Assert.IsTrue(result == "");
    }
    
    [TestMethod]
    public void TestDecryptTrue()
    {
        Code_WebAdapter code = new Code_WebAdapter();

        const string text = "qxgrua";
        const string key = "abc";

        string result = code.Decrypt(text, key);

        Assert.IsTrue(result == "qwerty");
    }
    [TestMethod]
    public void TestDecryptFalse()
    {
        Code_WebAdapter code = new Code_WebAdapter();

        const string text = "qxgrua123";
        const string key = "abc";

        string result = code.Decrypt(text, key);

        Assert.IsTrue(result == "");
    }

    [TestMethod]
    public void TestTokenTrue()
    {
        Code_WebAdapter code = new Code_WebAdapter();

        const string login = "user";
        const string password = "password";

        var result = code.Login(login, password);
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<IResult>(result);

        var value = result.GetType().GetProperty("Value");
        Assert.IsNotNull(value);
        var response = value.GetValue(result);
        Assert.IsNotNull(response);
    }
    [TestMethod]
    public void TestTokenFalse()
    {
        Code_WebAdapter code = new Code_WebAdapter();

        const string login = "";
        const string password = "password";

        var result = code.Login(login, password);
        Assert.IsNotNull(result);
        
        var value = result.GetType().GetProperty("Value");
        Assert.IsNull(value);
    }
}