using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class Validators
{
    // Константы для ограничений
    public const int MAX_EMAIL_LENGTH = 255;
    public const int MAX_PHONE_LENGTH = 20;
    public const int MIN_PASSWORD_LENGTH = 6;
    public const int MAX_PASSWORD_LENGTH = 100;
    public const int MAX_GENERAL_TEXT_LENGTH = 500;

    // Старые методы (сохранены для обратной совместимости)
    public static bool ValidateEmail(string email)
    {
        return ValidateEmail(email, MAX_EMAIL_LENGTH);
    }

    public static bool ValidatePhone(string phone)
    {
        return ValidatePhone(phone, MAX_PHONE_LENGTH);
    }

    public static bool ValidatePassword(string password)
    {
        return ValidatePassword(password, MIN_PASSWORD_LENGTH, MAX_PASSWORD_LENGTH);
    }

    public static bool ValidateRequired(string value)
    {
        return ValidateRequired(value, MAX_GENERAL_TEXT_LENGTH);
    }

    public static bool ValidatePositiveNumber(int? value)
    {
        return ValidatePositiveNumber(value, int.MaxValue);
    }

    public static bool ValidateNonNegativeNumber(int? value)
    {
        return ValidateNonNegativeNumber(value, int.MaxValue);
    }

    // Новые методы с параметрами для кастомизации
    public static bool ValidateEmail(string email, int maxLength = MAX_EMAIL_LENGTH)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Length > maxLength)
            return false;

        try
        {
            // Более точная проверка email согласно RFC
            string pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
                           + @"([-a-zA-Z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)"
                           + @"(?<!\.)@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$";

            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }
        catch (RegexMatchTimeoutException)
        {
            // В случае проблем с Regex возвращаем базовую проверку
            return !string.IsNullOrWhiteSpace(email) &&
                   email.Contains("@") &&
                   email.Contains(".") &&
                   email.Length <= maxLength;
        }
    }

    public static bool ValidatePhone(string phone, int maxLength = MAX_PHONE_LENGTH)
    {
        if (string.IsNullOrWhiteSpace(phone) || phone.Length > maxLength)
            return false;

        // Удаляем все нецифровые символы, кроме плюса в начале
        string digitsOnly = phone.StartsWith("+")
            ? "+" + Regex.Replace(phone.Substring(1), @"\D", "")
            : Regex.Replace(phone, @"\D", "");

        // Проверяем минимальную и максимальную длину
        int minDigits = phone.StartsWith("+") ? 11 : 10; // +7XXXXXXXXXX или XXXXXXXXXX
        int maxDigits = phone.StartsWith("+") ? 15 : 12; // Международные номера

        if (digitsOnly.Length < minDigits || digitsOnly.Length > maxDigits)
            return false;

        // Дополнительная проверка для российских номеров
        if (digitsOnly.StartsWith("+7") || digitsOnly.StartsWith("7") || digitsOnly.StartsWith("8"))
        {
            if (digitsOnly.Length < 11) return false;

            // Проверка кода оператора (900-999, 900-999 и т.д.)
            string operatorCode = digitsOnly.Length >= 11
                ? digitsOnly.Substring(digitsOnly.Length - 10, 3)
                : "";

            if (!Regex.IsMatch(operatorCode, @"^[9]\d{2}$") &&
                !Regex.IsMatch(operatorCode, @"^[3-8]\d{2}$"))
            {
                return false;
            }
        }

        return true;
    }

    public static bool ValidatePassword(string password,
                                        int minLength = MIN_PASSWORD_LENGTH,
                                        int maxLength = MAX_PASSWORD_LENGTH)
    {
        if (string.IsNullOrWhiteSpace(password) ||
            password.Length < minLength ||
            password.Length > maxLength)
            return false;

        // Дополнительные проверки сложности пароля (опционально)
        bool hasUpperCase = Regex.IsMatch(password, @"[A-Z]");
        bool hasLowerCase = Regex.IsMatch(password, @"[a-z]");
        bool hasDigits = Regex.IsMatch(password, @"\d");
        bool hasSpecialChar = Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]");

        // Минимальные требования: хотя бы одна буква и одна цифра
        return (hasUpperCase || hasLowerCase) && hasDigits;

        // Для более строгих требований:
        // return hasUpperCase && hasLowerCase && hasDigits && hasSpecialChar;
    }

    public static bool ValidateRequired(string value, int maxLength = MAX_GENERAL_TEXT_LENGTH)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               value.Trim().Length > 0 &&
               value.Length <= maxLength;
    }

    public static bool ValidatePositiveNumber(int? value, int maxValue = int.MaxValue)
    {
        return value.HasValue &&
               value.Value > 0 &&
               value.Value <= maxValue;
    }

    public static bool ValidateNonNegativeNumber(int? value, int maxValue = int.MaxValue)
    {
        return value.HasValue &&
               value.Value >= 0 &&
               value.Value <= maxValue;
    }

    // Новые методы валидации

    public static bool ValidateLength(string value, int minLength, int maxLength)
    {
        if (value == null)
            return minLength == 0;

        return value.Length >= minLength && value.Length <= maxLength;
    }

    public static bool ValidateOnlyLetters(string value, bool allowSpaces = true)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        string pattern = allowSpaces
            ? @"^[a-zA-Zа-яА-ЯёЁ\s]+$"
            : @"^[a-zA-Zа-яА-ЯёЁ]+$";

        return Regex.IsMatch(value, pattern);
    }

    public static bool ValidateOnlyDigits(string value)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               Regex.IsMatch(value, @"^\d+$");
    }

    public static bool ValidateAlphanumeric(string value, bool allowSpaces = true)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        string pattern = allowSpaces
            ? @"^[a-zA-Zа-яА-ЯёЁ0-9\s]+$"
            : @"^[a-zA-Zа-яА-ЯёЁ0-9]+$";

        return Regex.IsMatch(value, pattern);
    }

    public static bool ValidateDateRange(DateTime? date, DateTime? minDate = null, DateTime? maxDate = null)
    {
        if (!date.HasValue)
            return false;

        bool isValid = true;

        if (minDate.HasValue)
            isValid = isValid && date.Value >= minDate.Value;

        if (maxDate.HasValue)
            isValid = isValid && date.Value <= maxDate.Value;

        return isValid;
    }

    public static bool ValidateDecimal(decimal? value, decimal minValue = 0, decimal maxValue = decimal.MaxValue)
    {
        return value.HasValue &&
               value.Value >= minValue &&
               value.Value <= maxValue;
    }

    public static string GetValidationMessage(string fieldName, string errorType)
    {
        var messages = new Dictionary<string, Dictionary<string, string>>
        {
            ["Email"] = new Dictionary<string, string>
            {
                ["Required"] = "Email обязателен для заполнения",
                ["InvalidFormat"] = "Неверный формат email",
                ["TooLong"] = $"Email не должен превышать {MAX_EMAIL_LENGTH} символов"
            },
            ["Phone"] = new Dictionary<string, string>
            {
                ["Required"] = "Телефон обязателен для заполнения",
                ["InvalidFormat"] = "Неверный формат телефона",
                ["TooLong"] = $"Телефон не должен превышать {MAX_PHONE_LENGTH} символов"
            },
            ["Password"] = new Dictionary<string, string>
            {
                ["Required"] = "Пароль обязателен для заполнения",
                ["TooShort"] = $"Пароль должен содержать не менее {MIN_PASSWORD_LENGTH} символов",
                ["TooLong"] = $"Пароль не должен превышать {MAX_PASSWORD_LENGTH} символов",
                ["Weak"] = "Пароль должен содержать хотя бы одну букву и одну цифру"
            }
        };

        return messages.ContainsKey(fieldName) &&
               messages[fieldName].ContainsKey(errorType)
            ? messages[fieldName][errorType]
            : $"Ошибка валидации поля {fieldName}";
    }

    // Метод для комплексной проверки модели с возвратом ошибок
    public static Dictionary<string, string> ValidateModel(object model)
    {
        var errors = new Dictionary<string, string>();

        // Здесь можно реализовать рефлексию для автоматической проверки
        // атрибутов валидации на свойствах модели

        return errors;
    }
}