using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Authentication.Domain.Entities;

public class User
{
    public User(Guid id, string username, string password, string email, string firstName, string lastName)
    {
        Id = id;
        Username = username;
        Password = password;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        CreatedDate = DateTimeOffset.Now;
        LastModifiedDate =  DateTimeOffset.Now;
    }

    [Required]
    [Column(TypeName = "nvarchar(150)")]
    public Guid Id { get; set; }
    [Required]
    [Column(TypeName = "nvarchar(150)")]
    public string Username { get; set; }
    [Required]
    [Column(TypeName = "nvarchar(250)")]
    public string Password { get; set; }
    [Column(TypeName = "nvarchar(150)")]
    public string Email { get; set; }
    [Column(TypeName = "nvarchar(150)")]
    public string FirstName { get; set; }
    [Column(TypeName = "nvarchar(150)")]
    public string LastName { get; set; }
    [Column(TypeName = "nvarchar(150)")]
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset? LastModifiedDate { get; set; }
}