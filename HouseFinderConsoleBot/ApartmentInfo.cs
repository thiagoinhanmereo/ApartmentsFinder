using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HouseFinderConsoleBot
{
    public class ApartmentInfo
    {
        public string Href { get; set; }
        public string Rua { get; set; }
        public string Bairro { get; set; }
        public string Area { get; set; }
        public string Aluguel { get; set; }
        public string Total { get; set; }

        public override int GetHashCode()
        {
            int hashCode = this.GetHashCodeOnProperties();
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            return GetHashCode() == obj.GetHashCode();
        }
    }

    public static class HashCodeByPropertyExtensions
    {
        public static int GetHashCodeOnProperties<T>(this T inspect)
        {
            return inspect.GetType().GetProperties().Select(o => o.GetValue(inspect)).GetListHashCode();
        }

        public static int GetListHashCode<T>(this IEnumerable<T> sequence)
        {
            return sequence
                .Where(item => item != null)
                .Select(item => item.GetHashCode())
                .Aggregate((total, nextCode) => total ^ nextCode);
        }
    }
}
