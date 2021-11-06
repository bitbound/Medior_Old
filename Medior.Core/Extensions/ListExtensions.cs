using Medior.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Core.Extensions
{
    public static class ListExtensions
    {
        public static bool TryReplace<T>(this List<T> self, T newItem)
            where T : IdModel
        {
            if (newItem is null)
            {
                return false;
            }

            var index = self.FindIndex(x => x.Id == newItem.Id);
            if (index < 0)
            {
                return false;
            }

            self[index] = newItem;
            return true;
        }
    }
}
