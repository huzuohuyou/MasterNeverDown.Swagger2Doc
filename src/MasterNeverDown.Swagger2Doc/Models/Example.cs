namespace OpenApi2Doc.CommunityToolkit.Models;

public class Example
{
    public string? RequestParam { get; set; }
    public string? ResponseResult { get; set; }
    public override string ToString()
    {
        return $@"<tr>
                        <td colspan=""5"" bgcolor=""{MdColor.Bgcolor}"">示例</td>
                      </tr>
                      <tr>
                        <td bgcolor=""{MdColor.Bgcolor}"">请求参数</td>
                        <td colspan=""4"" bgcolor=""{MdColor.Bgcolor}""></td>
                      </tr>
                     <tr>
                        <td bgcolor=""{MdColor.Bgcolor}"">返回值</td>
                        <td colspan=""4"" bgcolor=""{MdColor.Bgcolor}""></td>
                      </tr>";
    }
}