namespace OpenAPI2Word.CommunityToolkit.Models;

public class Example
{
    public string? RequestParam { get; set; }
    public string? ResponseResult { get; set; }
    public override string ToString()
    {
        return $@"<tr>
                        <td colspan=""5"" bgcolor=""{MdColor.bgcolor}"">示例</td>
                      </tr>
                      <tr>
                        <td bgcolor=""{MdColor.bgcolor}"">请求参数</td>
                        <td colspan=""4"" bgcolor=""{MdColor.bgcolor}""></td>
                      </tr>
                     <tr>
                        <td bgcolor=""{MdColor.bgcolor}"">返回值</td>
                        <td colspan=""4"" bgcolor=""{MdColor.bgcolor}""></td>
                      </tr>";
    }
}