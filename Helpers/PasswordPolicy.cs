using System.Linq;

namespace SparshaERP.Helpers
{
    public static class PasswordPolicy
    {
        public static bool IsStrong(string password, out string error)
        {
            error = "";

            if (password.Length < 8)
                error = "Password must be at least 8 characters long";
            else if (!password.Any(char.IsUpper))
                error = "Password must contain at least one uppercase letter";
            else if (!password.Any(char.IsLower))
                error = "Password must contain at least one lowercase letter";
            else if (!password.Any(char.IsDigit))
                error = "Password must contain at least one number";
            else if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
                error = "Password must contain at least one special character";

            return error == "";
        }
    }
}
