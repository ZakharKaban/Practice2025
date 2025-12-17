using System;
using Practice2025One.AppData;

namespace Practice2025One.Class
{
    public static class CurrentUser
    {
        public static Users User { get; set; }
        public static Roles Role { get; set; }
        
        public static bool IsGuest()
        {
            return User == null;
        }
        
        public static bool IsUser()
        {
            return Role != null && Role.RoleName == "Пользователь";
        }
        
        public static bool IsManager()
        {
            return Role != null && Role.RoleName == "Менеджер";
        }
        
        public static bool IsAdmin()
        {
            return Role != null && Role.RoleName == "Администратор";
        }
        
        public static void Logout()
        {
            User = null;
            Role = null;
        }
    }
}

