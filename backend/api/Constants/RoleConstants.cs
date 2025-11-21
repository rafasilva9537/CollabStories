namespace api.Constants;

public class RoleConstants {
    // User role exists to create a way to block accounts, if you need to revoke the entire auth access of an account from a normal user, just remove the role
    // #TODO: remove User role and use other way to block access, like IsActive property or User Lockout
    public const string Admin = "Admin";
    public const string User = "User";
}
