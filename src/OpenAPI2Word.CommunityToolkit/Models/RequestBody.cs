namespace OpenAPI2Word.CommunityToolkit.Models;

public class RequestBody
{
    public string? PropertyName { get; set; }
    public string? PropertyType { get; set; }
    public string? ParamType { get; set; } = "Body";
    public string? IsRequired { get; set; }
    public string? Description { get; set; }

    public void Generate(XWPFTable table,int index=0)
    {
        var propertyRow = table.CreateRow();
        if (index%2==1)
        {
            propertyRow.SetColor("#dcdcdc");
        }
        propertyRow.GetCell(0).SetColumCellText(index == 0 && (PropertyType ?? "").Contains(":") ? "" : PropertyName ?? "" ?? "", 13);
        propertyRow.GetCell(1).SetColumCellText(PropertyType.Contains(":") ? PropertyType.Substring(0, PropertyType.IndexOf(":")) : PropertyType ?? "" ?? "",10);
        propertyRow.GetCell(2).SetColumCellText(ParamType ?? "", 10);
        propertyRow.GetCell(3).SetColumCellText(IsRequired ?? "");
        propertyRow.GetCell(4).SetColumCellText(Description ?? "", 20);
        propertyRow.MergeCells(4,5);
    }
}