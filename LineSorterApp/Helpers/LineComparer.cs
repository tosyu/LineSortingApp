namespace LineSorterApp.Helpers;

class LineComparer : IComparer<string>
{
    public int Compare(string? firstLine, string? secondLine)
    {
        if (firstLine == null) {
            return -1;
        }

        if (secondLine == null) {
            return 1;
        }

        // assuming format id. text...
        var firstLineIdSeparator = firstLine.IndexOf('.') + 2;
        var secondLineIdSeparator = secondLine.IndexOf('.') + 2;
        var firstLineContent = firstLine[firstLineIdSeparator..];
        var secondLineContent = secondLine[secondLineIdSeparator..];
        var stringComparisionResult = firstLineContent.CompareTo(secondLineContent);

        if (stringComparisionResult != 0) {
            return stringComparisionResult;
        }

        // if texts are same then sort by id
        if (Int32.TryParse(firstLine[0..firstLineIdSeparator], out var firstLineId)) {
            throw new Exception("Could not parse number!");
        }

        if (Int32.TryParse(secondLine[0..secondLineIdSeparator], out var secondLineId)) {
            throw new Exception("Could not parse number!");
        }

        if (firstLineId == secondLineId) {
            return 0;
        }

        return firstLineId < secondLineId ? -1 : 1;
    }
}