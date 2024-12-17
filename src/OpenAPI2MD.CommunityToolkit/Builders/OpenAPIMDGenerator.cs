using OpenApi2Doc.CommunityToolkit.Builders;
using OpenApi2Doc.CommunityToolkit.Generators;
using MdColor = OpenAPI2MD.CommunityToolkit.Models.MdColor;
using RequestBody = OpenApi2Doc.CommunityToolkit.Models.RequestBody;

namespace OpenAPI2MD.CommunityToolkit.Builders;

public class OpenApiMdGenerator : DocumentBuilder<StringBuilder>
{
    public override void Reset()
    {
        Doc.Clear();
    }

    public override void InitDoc()
    {
        Doc = new StringBuilder();
    }

    public override void BuildInfo()
    {
        Doc.Append($"{ApiDocument.Info.Description} \n");
        Doc.Append($"{ApiDocument.Info?.Contact?.Name} \n");
        Doc.Append($"{ApiDocument.Info?.Contact?.Email} \n");
    }

    public override void BeginBuildPathItem()
    {
        Doc.Append(@"
<table>");
    }

    public override void AfterBuildPathItem()
    {
        Doc.Append("</table>");
    }

    public override void BuildTag()
    {
        var firstTag = CurrentOperation.Tags.FirstOrDefault();
        if (firstTag != null && !firstTag.Name.Equals(CurrentPathTag))
        {
            CurrentPathTag = firstTag.Name;
            Doc.Append($" \n## {CurrentPathTag} \n");
        }
    }

    public override void BuildSummary()
    {
        if (string.Equals("获取报警/事件历史数据【此方法以后不在维护请使用alarms/actions/get-history】", CurrentOperation.Summary, StringComparison.Ordinal))
        {
            // Do nothing
        }
        Doc.Append($@"

### {CurrentOperation.Summary}
");
    }

    public override void BuildOperationId()
    {
        Doc.Append($@"
<tr bgcolor=""{MdColor.Bgcolor}"">
<td>OperationId</td>
<td colspan=""5"">{CurrentOperation.OperationId}</td>
</tr>
");
    }

    public override void BuildDescription()
    {
        Doc.Append($@"
<tr>
<td>接口描述</td>
<td colspan=""5"">{CurrentOperation.Description}</td>
</tr>");
    }

    public override void BuildRequestMethod()
    {
        Doc.Append($@"
<tr>
<td>请求方式</td>
<td colspan=""5"">{CurrentPathItem.Operations.Keys.FirstOrDefault()}</td>
</tr>");
    }

    public override void BuildRequestType()
    {
        Doc.Append("");
    }

    public override void BuildRequestParams()
    {
        Doc.Append(@"
<tr>
<td>参数名</td>
<td>数据类型</td>
<td>参数类型</td>
<td>必需</td>
<td>描述</td>
<td>示例</td>
</tr>");
        CurrentOperation?.Parameters.ToList().ForEach(p =>
        {
            var va = string.Empty;
            if (p.Schema.Type == "array" && p.Example != null)
            {
                dynamic d = p.Example;
                var v = d[0].Value;
                va = v.ToString();
            }
            else if (p.Example != null)
            {
                dynamic d = p.Example;
                var v = d.Value;
                va = v.ToString();
            }

            var r = p.Required ? "Y" : "N";
            Doc.Append($@"<tr>
<td>{p.Name}</td>
<td>{p.Schema.Type}</td>
<td>{p.In}</td>
<td>{r}</td>
<td>{p.Description}</td>
<td>{va}</td>
</tr>");
        });
    }

    public override void BuildRequestBodies()
    {
        var requestBodys = new List<RequestBody>();
        var c = new RequestProperiesGenerator().Excute(CurrentOperation.RequestBody?.Content?.FirstOrDefault().Value?.Schema);
        if (c != null)
            requestBodys.AddRange(c);
        if (requestBodys.Count == 0)
            return;
        Doc.Append($@"
<tr>
<td bgcolor=""{MdColor.Bgcolor}"">参数名</td>
<td bgcolor=""{MdColor.Bgcolor}"">数据类型</td>
<td bgcolor=""{MdColor.Bgcolor}"">参数类型</td>
<td bgcolor=""{MdColor.Bgcolor}"">必需</td>
<td colspan=""2"" bgcolor=""{MdColor.Bgcolor}"">描述</td>
</tr>");
        requestBodys.ForEach(r =>
        {
            Doc.Append($@"
<tr>
<td>{r.PropertyName}</td>
<td>{r.PropertyType}</td>
<td>{r.ParamType}</td>
<td>{r.IsRequired}</td>
<td colspan=""2"">{r.Description}</td>
</tr>");
        });
        Doc.Append($@"
<tr>
<td colspan=""6"">示例</td>
</tr>
<tr>
<td colspan=""6"">

{new ExampleValueGenerator().Excute(CurrentOperation.RequestBody?.Content?.FirstOrDefault().Value?.Schema)}

</td>
</tr>");
    }

    public override void BuildRequestBodyExample()
    {
        return;
    }

    public override void BuildResponse()
    {
        var type = new StringBuilder();
        if (CurrentResponse.Content.Count > 0)
        {
            type.Append(CurrentResponse.Content.FirstOrDefault().Value.Schema.Type);
            if (type.ToString() == "array")
                type.Append($":{CurrentResponse.Content.FirstOrDefault().Value.Schema?.Items?.Reference?.Id}");
            if (type.ToString() == "object")
                type.Append($":{CurrentResponse.Content.FirstOrDefault().Value.Schema?.Reference?.Id}");
        }

        Doc.Append($@"
<tr>
<td colspan=""2"" bgcolor=""{MdColor.Bgcolor}"">状态码</td>
<td colspan=""2"" bgcolor=""{MdColor.Bgcolor}"">描述</td>
<td colspan=""2"" bgcolor=""{MdColor.Bgcolor}"">类型</td>
</tr>");

        Doc.Append($@"<tr>
<td colspan=""2"">{CurrentResponseCode}</td>
<td colspan=""2"">{CurrentResponse.Description}</td>
<td colspan=""2"">{CurrentResponse.Content.FirstOrDefault().Key}</td>
</tr>");
    }

    public override void BuildResponseFields()
    {
        Doc.Append($@"
<tr>
<td bgcolor=""{MdColor.Bgcolor}"">返回属性名</td>
<td colspan=""2"" bgcolor=""{MdColor.Bgcolor}"">数据类型</td>
<td colspan=""3"" bgcolor=""{MdColor.Bgcolor}"">说明</td>
</tr>");
        var c = new ResponseProperiesGenerator().Excute(CurrentResponse.Content.FirstOrDefault().Value?.Schema);
        foreach (var s in c)
        {
            Doc.Append($@"
<tr>
<td>{s.PropertyName}</td>
<td colspan=""2"">{s.PropertyType}</td>
<td colspan=""3"">{s.Description}</td>
</tr>");
        }
    }

    public override void BuildResponseExample()
    {
        var example = new ExampleValueGenerator().Excute(CurrentResponse.Content.Count > 0 ? CurrentResponse.Content.FirstOrDefault().Value.Schema : default);
        Doc.Append($@"
<tr>
<td>示例</td>
<td colspan=""6"">

{example}

</td>
</tr>");
    }

    public override void BuildTitle()
    {
        Doc.Append($" # {ApiDocument.Info.Title}({ApiDocument.Info.Version}) \n");
    }

    public override void BuildToc()
    {
        Doc.Append("<!-- @import \"[TOC]\" {cmd=\"toc\" depthFrom=2 depthTo=3 orderedList=false} -->\n");
    }

    public override StringBuilder OutputDoc(string savePath = "")
    {
        var s = Doc.ToString();
        File.WriteAllText(Path.Combine(savePath, "swagger.md"), s);
        if (File.Exists(Path.Combine(savePath, "swagger.md")))
        {
            return Doc;
        }
        return new StringBuilder();
    }
}