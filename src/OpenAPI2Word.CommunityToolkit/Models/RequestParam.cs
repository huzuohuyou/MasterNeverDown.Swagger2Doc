namespace OpenAPI2Word.CommunityToolkit.Models;

public class RequestParam
{
    
    public string? PropertyName { get; set; }
    public string? PropertyType { get; set; }
    public string? ParamType { get; set; } = "Body";
    public string? IsRequired { get; set; }
    public string IsRequiredDisplay => Equals("True", IsRequired) ? "Y  " : "N";
    public string? Description { get; set; }
    
    public string? Example { get; set; }
    public void Generate(XWPFTable table,int index=0)
    {
        var propertyRow = table.CreateRow();
        if (index % 2 == 1)
        {
            propertyRow.SetColor("#DCDCDC");
        }
        propertyRow.GetCell(0).SetColumCellText(PropertyName ?? "", 12);
        propertyRow.GetCell(1).SetColumCellText(PropertyType ?? "", 8);
        propertyRow.GetCell(2).SetColumCellText(ParamType ?? "", 8);
        propertyRow.GetCell(3).SetColumCellText(IsRequiredDisplay ?? "", 6);
        propertyRow.GetCell(4).SetColumCellText(Description ?? "", 8);
        propertyRow.GetCell(5).SetColumCellText(Example ?? "", 14);
    }
}