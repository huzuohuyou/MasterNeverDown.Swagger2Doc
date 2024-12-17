using System.Text;
using OpenApi2Doc.CommunityToolkit.Generators;

namespace OpenApi2Doc.CommunityToolkit.Models;

public class Response
{
    public string? Code { get; set; }
    public string? Des { get; set; }
    public string? Remark { get; set; }
    public string? ResponseType { get; set; }
    public string? ResponseDataType { get; set; }
    public List<Schema> Schemas { get; set; } = new();
    public OpenApiSchema? OpenApiSchema { get; set; }
    public override string ToString()
    {
        var fieldsResult = string.Empty;
        var fields = new StringBuilder(
$@"<tr>
    <td bgcolor=""{MdColor.Bgcolor}"">返回属性名</td>
    <td colspan=""2"" bgcolor=""{MdColor.Bgcolor}"">数据类型</td>
    <td colspan=""3"" bgcolor=""{MdColor.Bgcolor}"">说明</td>
    
</tr>");
        Schemas.ForEach(r =>
        {
            fields.Append(r);
        });
        if (Schemas.Any())
        {
            fieldsResult=fields.ToString();
        }

        var responseExampleResult = string.Empty;
        var example = new ExampleValueGenerator().Excute(OpenApiSchema);
        
        var responseExample = new StringBuilder(
$@"<tr>
    <td colspan=""6"" bgcolor=""{MdColor.Bgcolor}"">示例</td>
</tr>
<tr>
<td colspan=""6"">

{example}
</td>
</tr>
");

        if (!string.IsNullOrWhiteSpace(example))
            responseExampleResult=responseExample.ToString().Trim();
        return 
$@"<tr>
    <td >{Code}</td>
    <td >{Des}</td>
    <td colspan=""2"" >{ResponseType}</td>
    <td colspan=""2"" >{ResponseDataType}</td>
</tr>
{fieldsResult}


{responseExampleResult}


";
    }
}