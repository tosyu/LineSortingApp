using System.Globalization;
using LineSorterApp.DataStructures;

namespace LineSorterApp.Helpers;

class RowComparer : IComparer<Row>
{
    public int Compare(Row x, Row y)
    {
        var stringComparisionResult = string.Compare(x.Content, y.Content, true, CultureInfo.InvariantCulture);

        if (stringComparisionResult != 0)
        {
            return stringComparisionResult;
        }

        // if texts are same then sort by id
        if (int.TryParse(x.Id, out var firstLineId) == false)
        {
            throw new Exception("Could not parse number!");
        }

        if (int.TryParse(y.Id, out var secondLineId) == false)
        {
            throw new Exception("Could not parse number!");
        }

        if (firstLineId == secondLineId)
        {
            return 0;
        }

        return firstLineId < secondLineId ? -1 : 1;
    }
}