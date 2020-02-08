using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreTutorial.Utilities
{
    public class ValidEmailDomainAttribute : ValidationAttribute
    {
        private readonly string domainName;

        public ValidEmailDomainAttribute(string domainName)
        {
            this.domainName = domainName;
        }
        public override bool IsValid(object value)
        {
            string domain = ((string)value).ToUpperInvariant().Substring(((string)value).IndexOf('@', StringComparison.InvariantCultureIgnoreCase) + 1);

            if (domain == null || domain != domainName.ToUpperInvariant())
                return false;
            else return true;
            
        }
    }
}
