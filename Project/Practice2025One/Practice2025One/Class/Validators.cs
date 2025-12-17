using System;
using System.Text.RegularExpressions;

namespace Practice2025One.Class
{
    public static class Validators
    {
        public static bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, pattern);
        }
        
        public static bool ValidatePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;
            
            // Удаляем все нецифровые символы для проверки
            string digitsOnly = Regex.Replace(phone, @"\D", "");
            return digitsOnly.Length >= 10;
        }
        
        public static bool ValidatePassword(string password)
        {
            return !string.IsNullOrWhiteSpace(password) && password.Length >= 6;
        }
        
        public static bool ValidateRequired(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }
        
        public static bool ValidatePositiveNumber(int? value)
        {
            return value.HasValue && value.Value > 0;
        }
        
        public static bool ValidateNonNegativeNumber(int? value)
        {
            return value.HasValue && value.Value >= 0;
        }
    }
}

