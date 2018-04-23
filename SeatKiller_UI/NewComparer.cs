using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeatKiller_UI
{
    class NewComparer : IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            DictionaryEntry X = (DictionaryEntry)x;
            DictionaryEntry Y = (DictionaryEntry)y;
            if (int.Parse(X.Value.ToString().Substring(0, 3)) < int.Parse(Y.Value.ToString().Substring(0, 3)))
                return -1;
            else if (int.Parse(X.Value.ToString().Substring(0, 3)) > int.Parse(Y.Value.ToString().Substring(0, 3)))
                return 1;
            else
                return 0;
        }
    }
}
