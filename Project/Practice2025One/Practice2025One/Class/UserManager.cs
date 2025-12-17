using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Practice2025One.AppData;

namespace Practice2025One.Class
{
    public static class UserManager
    {
        public static string HashPassword(string password)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
        
        public static bool Register(string fullName, string email, string phone, string password)
        {
            try
            {
                // Проверка уникальности email
                if (AppConnect.Model1.Users.Any(u => u.Email == email))
                {
                    throw new Exception("Пользователь с таким email уже существует");
                }
                
                // Проверка уникальности телефона
                if (AppConnect.Model1.Users.Any(u => u.Phone == phone))
                {
                    throw new Exception("Пользователь с таким телефоном уже существует");
                }
                
                // Получение роли "Пользователь"
                var userRole = AppConnect.Model1.Roles.FirstOrDefault(r => r.RoleName == "Пользователь");
                if (userRole == null)
                {
                    throw new Exception("Роль 'Пользователь' не найдена в базе данных");
                }
                
                Users newUser = Users.CreateUsers(0, fullName, email, phone, HashPassword(password), 
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), 1);
                EFHelper.SetUserRole(newUser, userRole);
                
                AppConnect.Model1.AddToUsers(newUser);
                AppConnect.Model1.SaveChanges();
                
                return true;
            }
            catch
            {
                throw;
            }
        }
        
        public static bool Login(string login, string password)
        {
            try
            {
                string passwordHash = HashPassword(password);
                
                var user = AppConnect.Model1.Users
                    .FirstOrDefault(u => (u.Email == login || u.Phone == login) 
                                        && u.PasswordHash == passwordHash 
                                        && u.IsActive == 1);
                
                if (user != null)
                {
                    CurrentUser.User = user;
                    // Загружаем связанную роль
                    if (!user.RolesReference.IsLoaded)
                    {
                        user.RolesReference.Load();
                    }
                    CurrentUser.Role = user.Roles;
                    if (CurrentUser.Role == null && user.RolesReference.EntityKey != null)
                    {
                        int roleId = (int)user.RolesReference.EntityKey.EntityKeyValues[0].Value;
                        CurrentUser.Role = AppConnect.Model1.Roles.FirstOrDefault(r => r.RoleID == roleId);
                    }
                    return true;
                }
                
                return false;
            }
            catch
            {
                throw;
            }
        }
        
        public static bool UpdateUser(int userId, string fullName, string phone)
        {
            try
            {
                var user = AppConnect.Model1.Users.FirstOrDefault(u => u.UserID == userId);
                if (user == null)
                    return false;
                
                // Проверка уникальности телефона (если изменился)
                if (user.Phone != phone && AppConnect.Model1.Users.Any(u => u.Phone == phone && u.UserID != userId))
                {
                    throw new Exception("Пользователь с таким телефоном уже существует");
                }
                
                user.FullName = fullName;
                user.Phone = phone;
                
                AppConnect.Model1.SaveChanges();
                
                // Обновляем текущего пользователя
                if (CurrentUser.User != null && CurrentUser.User.UserID == userId)
                {
                    CurrentUser.User = user;
                }
                
                return true;
            }
            catch
            {
                throw;
            }
        }
    }
}

