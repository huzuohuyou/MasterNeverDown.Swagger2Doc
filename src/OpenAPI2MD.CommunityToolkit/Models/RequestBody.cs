namespace OpenAPI2MD.CommunityToolkit.Models;

public class RequestBody
{
    public string? PropertyName { get; set; }
    public string? PropertyType { get; set; }
    public string? ParamType { get; set; } = "Body";
    public string? IsRequired { get; set; }
    public string? Description { get; set; }

    public override string ToString()
    {
        return
            $@"<tr>
    <td >{PropertyName}</td>
    <td >{PropertyType}</td>
    <td >{ParamType}</td>
    <td >{IsRequired}</td>
    <td colspan=""2"">{Description}</td>
</tr>";
    }

}