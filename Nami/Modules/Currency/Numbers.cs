using System;
using System.Text;

namespace Nami.Modules.Currency
{
    internal class Numbers
    {
        internal static string? ToWords(decimal balance)
        {
            var bal = balance.ToString("C");
            var main = bal.Split('.');





            var sb = new StringBuilder();


            return sb.ToString();
        }
    }
}