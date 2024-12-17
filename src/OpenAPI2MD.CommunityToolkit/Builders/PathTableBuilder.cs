using Microsoft.OpenApi.Models;
using OpenApi2Doc.CommunityToolkit.Builders;
using SharpYaml.Tokens;

namespace OpenApi2Md.CommunityToolkit.Builders;

public class PathTableBuilder : PathBuilder<StringBuilder>
{
    private string DisplayDeprecated
    {
        get
        {
            if (Deprecated)
                return $@"<tr>
    <td >方法过时</td>
    <td colspan=""5"" >此方法以后不再维护,或在后续更新中移除；</td>
</tr>";
            return string.Empty;
        }
    }

    private string DeprecatedStyle
    {
        get
        {
            if (Deprecated)
                return @"style=""text-decoration:line-through;""";
            return string.Empty;
        }
    }
 

    public override string ToString()
    {
        var info = $@"
<tr>
    <td colspan=""6"" bgcolor=""{MdColor.bgcolor}"">{Summary}</td>
</tr>
<tr>
    <td {DeprecatedStyle}>OperationId</td>
    <td colspan=""5"" {DeprecatedStyle}>{OperationId}</td>
</tr>
{DisplayDeprecated}
<tr>
    <td  {DeprecatedStyle}>接口描述</td>
    <td colspan=""5""  {DeprecatedStyle}>{Description}</td>
</tr>
<tr>
    <td {DeprecatedStyle}>URL</td>
    <td colspan=""5"" {DeprecatedStyle}>{Url}</td>
</tr>
<tr>
    <td  {DeprecatedStyle}>请求方式</td>
    <td colspan=""5""  {DeprecatedStyle}>{RequestMethod}</td>
</tr>
<tr>
    <td >请求类型</td>
    <td colspan=""5"">{RequestType}</td>
</tr>";
        //        < tr >
        //    < td > 返回类型 </ td >
        //    < td colspan = ""4"" >{ ResponseType}</ td >
        //</ tr >
        var paramHeader = new StringBuilder(
$@"<tr>
    <td bgcolor=""{MdColor.bgcolor}"">参数名</td>
    <td bgcolor=""{MdColor.bgcolor}"">数据类型</td>
    <td bgcolor=""{MdColor.bgcolor}"">参数类型</td>
    <td bgcolor=""{MdColor.bgcolor}"">是否必填</td>
    <td bgcolor=""{MdColor.bgcolor}"">说明</td>
    <td bgcolor=""{MdColor.bgcolor}"">示例</td>
</tr>");
        RequestParams.ForEach(r =>
        {
            paramHeader.Append(r);
        });

        var paramsResult = string.Empty;
        if (!Equals(RequestParams.Count, 0))
            paramsResult = paramHeader.ToString();

        var requestBodyProperties = new StringBuilder(
            $@"<tr>
    <td bgcolor=""{MdColor.bgcolor}"">参数名</td>
    <td bgcolor=""{MdColor.bgcolor}"">数据类型</td>
    <td bgcolor=""{MdColor.bgcolor}"">参数类型</td>
    <td bgcolor=""{MdColor.bgcolor}"">是否必填</td>
    <td colspan=""2"" bgcolor=""{MdColor.bgcolor}"">说明</td>
</tr>");
        RequestBodys.ForEach(r =>
        {
            requestBodyProperties.Append(r);
        });

        var requestBodyPropertiesResult = string.Empty;
        if (!Equals(RequestBodys.Count, 0))
            requestBodyPropertiesResult = requestBodyProperties.ToString();


        var responses = new StringBuilder();
        Responses.ForEach(r =>
        {
            responses.Append(
$@"<tr>
    <td bgcolor=""{MdColor.bgcolor}"">状态码</td>
    <td colspan=""1"" bgcolor=""{MdColor.bgcolor}"">描述</td>
    <td colspan=""2"" bgcolor=""{MdColor.bgcolor}"">类型</td>
    <td colspan=""2"" bgcolor=""{MdColor.bgcolor}"">数据类型</td>
</tr>
{r.ToString()}");
        });
        var examples = $@"";

        var requestBody = new ExampleValueGenerator().Excute(RequestBody);
        if (!string.IsNullOrWhiteSpace(requestBody))
        {
            requestBody =
$@"<tr>
    <td colspan=""6"" bgcolor=""{MdColor.bgcolor}"">示例</td>
    </tr>
    <tr>
    <td colspan=""6"">

 {requestBody.Trim()}</td>
</tr>";
        }
        return
$@"<table>
    {info}
    {paramsResult}
{requestBodyPropertiesResult}
    {requestBody}
 {responses}
    {""}
    {examples}
</table>";
    }

    
 

    protected override void BuildTag(OpenApiOperation operation, ref string tag)
    {

        if (!tag.Equals(operation?.Tags.FirstOrDefault()?.Name))
        {
            tag = operation?.Tags.FirstOrDefault()?.Name;
            doc.Append($" \n## {tag} \n");
        }

    }

    protected override void BuildSummary(OpenApiOperation operation)
    {
        throw new NotImplementedException();
    }

    protected override void BuildOperationId(OpenApiOperation operation)
    {
        throw new NotImplementedException();
    }

    protected override void BuildDescription(OpenApiOperation operation)
    {
        throw new NotImplementedException();
    }

    protected override void BuildRequestMethod(OpenApiOperation operation)
    {
        throw new NotImplementedException();
    }

    protected override void BuildRequestType(OpenApiOperation operation)
    {
        throw new NotImplementedException();
    }

    protected override void BuildRequestParams(OpenApiOperation operation)
    {
        throw new NotImplementedException();
    }

    protected override void BuildRequestBodies(OpenApiOperation operation)
    {
        throw new NotImplementedException();
    }

    protected override void BuildRequestBodyExample(OpenApiOperation operation)
    {
        throw new NotImplementedException();
    }

    protected override void BuildResponses(OpenApiOperation operation)
    {
        throw new NotImplementedException();
    }

    protected override void BuildResponsesExample(OpenApiOperation operation)
    {
        throw new NotImplementedException();
    }
}