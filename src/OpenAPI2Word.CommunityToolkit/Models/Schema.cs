namespace OpenAPI2Word.CommunityToolkit.Models;

public class Schema
{
    public string? PropertyName { get; set; }
    public string? PropertyType { get; set; }
    public string? Description { get; set; }
    public string? Example { get; set; }
    public void Generate(XWPFTable table,int index)
    {
        var fieldsHeaderRow = table.CreateRow();
        if (index%2==1)
        {
            fieldsHeaderRow.SetColor("#DCDCDC");
        }
        fieldsHeaderRow.MergeCells(0, 1);
        fieldsHeaderRow.GetCell(0).SetColumCellText(index==0&& (PropertyType??"").Contains(":")? "": PropertyName??"", 25);

        fieldsHeaderRow.MergeCells(1, 2);

        fieldsHeaderRow.GetCell(1).SetColumCellText(PropertyType.Contains(":")? PropertyType.Substring(0,PropertyType.IndexOf(":")): PropertyType ?? "",25);
        fieldsHeaderRow.MergeCells(2, 3);
        fieldsHeaderRow.GetCell(2).SetText3(Description ?? "");
     

    }
}