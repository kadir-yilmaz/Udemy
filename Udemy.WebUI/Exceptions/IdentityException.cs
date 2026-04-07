using System;
using System.Collections.Generic;
using System.Linq;

namespace Udemy.WebUI.Exceptions
{
    /// <summary>
    /// Kimlik doğrulama işlemlerinde oluşan hataları temsil eder.
    /// </summary>
    public class IdentityException : Exception
    {
        public List<string> Errors { get; }

        public IdentityException(string message) : base(message)
        {
            Errors = new List<string> { message };
        }

        public IdentityException(List<string> errors) : base(errors.FirstOrDefault() ?? "Identity error")
        {
            Errors = errors;
        }
    }
}
