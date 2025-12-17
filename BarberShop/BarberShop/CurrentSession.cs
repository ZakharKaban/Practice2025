using System;

namespace BarberShop
{
    /// <summary>
    /// Хранит информацию о текущем вошедшем пользователе.
    /// </summary>
    public static class CurrentSession
    {
        public static Users CurrentUser { get; set; }

        public static bool IsAuthenticated
        {
            get { return CurrentUser != null; }
        }

        public static bool IsInRole(string roleName)
        {
            if (CurrentUser == null || CurrentUser.UserRoles == null || string.IsNullOrWhiteSpace(roleName))
            {
                return false;
            }

            return string.Equals(CurrentUser.UserRoles.RoleName, roleName, StringComparison.OrdinalIgnoreCase);
        }
    }
}


