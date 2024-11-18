namespace Configuration;

public class PasswordOptions
{
    public const string Position = "PasswordSaltDetails";
    public string Salt { get; set; }
    public int Iterations { get; set; }
}