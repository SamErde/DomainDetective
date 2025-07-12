using System;
using System.Globalization;

namespace DomainDetective.Helpers
{
    public static class DomainHelper
    {
        private static readonly IdnMapping _idn = new();

        public static string ValidateIdn(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentNullException(nameof(domain));
            }

            try
            {
                return _idn.GetAscii(domain.Trim().Trim('.'));
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException("Invalid domain name.", nameof(domain), e);
            }
        }
    }
}
