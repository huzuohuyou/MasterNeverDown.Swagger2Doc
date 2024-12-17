namespace OpenApi2Doc.CommunityToolkit.Models;

public class Schema
{
    public string? PropertyName { get; set; }
    public string? PropertyType { get; set; }
    public string? Description { get; set; }
    public string? Example { get; set; }
    public override string ToString()
    {
        return 
$@"<tr>
    <td >{PropertyName}</td>
    <td colspan=""2"">{PropertyType}</td>
    <td colspan=""3"" >{Description}</td>
   
</tr>";
    }
}