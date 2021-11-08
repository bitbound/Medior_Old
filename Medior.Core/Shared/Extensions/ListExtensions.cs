using Medior.Core.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Core.Shared.Extensions
{
    public static class ListExtensions
    {
        public static bool TryReplace<T>(this IList<T> self, T newItem)
            where T : IdModel
        {
            if (newItem is null)
            {
                return false;
            }

            var foundItem = self.FirstOrDefault(x => x.Id == newItem.Id);

            if (foundItem is null)
            {
                return false;
            }

            var index = self.IndexOf(foundItem);

            if (index == -1)
            {
                return false;
            }

            self[index] = newItem;
            return true;
        }
    }
}
