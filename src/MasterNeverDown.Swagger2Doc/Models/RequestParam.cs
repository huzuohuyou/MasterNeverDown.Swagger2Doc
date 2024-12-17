namespace OpenApi2Doc.CommunityToolkit.Models;

public class RequestParam
{
    public string? PropertyName { get; set; }
    public string? PropertyType { get; set; }
    public string? ParamType { get; set; } = "Body";
    public string? IsRequired { get; set; }
    public string IsRequiredDisplay => Equals("True", IsRequired) ? "Y" : "N";
    public string? Description { get; set; }
    
    public string? Example { get; set; }
    public override string ToString()
    {
        return 
$@"<tr>
    <td >{PropertyName}</td>
    <td >{PropertyType}</td>
    <td >{ParamType}</td>
    <td >{IsRequiredDisplay}</td>
    <td >{Description}</td>
    <td >{Example}</td>
</tr>";
    }
}