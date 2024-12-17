using System.Collections.Generic;
using System.Text;
using OpenApi2Md.CommunityToolkit.Builders;
using OpenAPI2MD.CommunityToolkit.Models;

namespace OpenApi2Doc.CommunityToolkit.Builders
{
    public abstract class PathBuilder<T>
    {
        protected T doc;

        protected OpenApiDocument apiDocument;

        public string OperationId { get; set; }

        public string Summary { get; set; }
        public string Name { get; set; }

        public string? Description { get; set; }

        public string Url { get; set; }

        public string? RequestMethod { get; set; }

        public string? RequestType { get; set; }

        public bool Deprecated { get; set; }

        public string? ResponseType { get; set; }

        public List<RequestParam> RequestParams { get; set; } = new();

        public List<RequestBody> RequestBodys { get; set; } = new();

        public OpenApiSchema? RequestBody { get; set; }

        public List<Response> Responses { get; set; } = new();

        public List<Example> Examples { get; set; } = new();

        public void InitDoc(T doc)
        {
            this.doc = doc;
        }

        public void IniApiDocument(OpenApiDocument  apiDocument)
        {
            this.apiDocument = apiDocument;
        }

        protected abstract void BuildTag(OpenApiOperation operation, ref string tag);

        protected abstract void BuildSummary(OpenApiOperation operation);

        protected abstract void BuildOperationId(OpenApiOperation operation);

        protected abstract void BuildDescription(OpenApiOperation operation);

        protected abstract void BuildRequestMethod(OpenApiOperation operation);

        protected abstract void BuildRequestType(OpenApiOperation operation);

        protected abstract void BuildRequestParams(OpenApiOperation operation);

        protected abstract void BuildRequestBodies(OpenApiOperation operation);

        protected abstract void BuildRequestBodyExample(OpenApiOperation operation);

        protected abstract void BuildResponses(OpenApiOperation operation);

        protected abstract void BuildResponsesExample(OpenApiOperation operation);
        public void Build(  )
        {
            var tag = string.Empty;
            apiDocument.Paths.ToList().ForEach(r =>
            {
                var operation = r.Value.Operations.Values.FirstOrDefault();
                BuildTag(operation, ref tag);
                BuildSummary(operation);
                BuildOperationId(operation);
                BuildDescription(operation);
                BuildRequestMethod(operation);
                BuildRequestType(operation);

                var t = new PathTableBuilder()
                {
                    OperationId = operation.OperationId,
                    Summary = operation?.Summary,
                    Description = operation?.Description,
                    Name = operation?.Summary,
                    Url = r.Key,
                    Deprecated = operation.Deprecated,
                    RequestMethod = r.Value.Operations.Keys.FirstOrDefault().ToString()
                };
                operation?.Parameters.ToList().ForEach(p =>
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

                    t.RequestParams.Add(new RequestParam()
                    {
                        PropertyName = p.Name,
                        Description = p.Description,
                        ParamType = p.In.ToString(),
                        PropertyType = p.Schema.Type,
                        IsRequired = p.Required.ToString(),
                        Example = va
                    });
                });
                if (!Equals(null, operation?.RequestBody))
                {
                    var c = new RequestProperiesGenerator().Excute(operation.RequestBody?.Content?.FirstOrDefault()
                        .Value?.Schema);
                    if (!Equals(null, c))
                        t.RequestBodys.AddRange(c);
                }

                t.RequestBody = operation?.RequestBody?.Content?.FirstOrDefault().Value?.Schema;
                operation?.Responses.ToList().ForEach(keyValuePair =>
                {
                    new ExampleValueGenerator().Excute(keyValuePair.Value.Content.Count > 0 ? keyValuePair.Value.Content.FirstOrDefault().Value.Schema : default);
                    var type = new StringBuilder();
                    if (keyValuePair.Value.Content.Count > 0)
                    {
                        type = type.Append(keyValuePair.Value.Content.Count > 0 ? keyValuePair.Value.Content.FirstOrDefault().Value.Schema.Type : default);
                        if (Equals("array", type.ToString()))
                            type.Append($@":{keyValuePair.Value.Content.FirstOrDefault().Value.Schema?.Items?.Reference?.Id}");
                        if (Equals("object", type.ToString()))
                            type.Append($@":{keyValuePair.Value.Content.FirstOrDefault().Value.Schema?.Reference?.Id}");
                    }
                    var response = new Response()
                    {
                        Code = keyValuePair.Key,
                        Des = keyValuePair.Value.Description,
                        ResponseType = keyValuePair.Value.Content.Count > 0 ? keyValuePair.Value.Content.FirstOrDefault().Key : default,
                        ResponseDataType = type.ToString(),
                        OpenApiSchema = keyValuePair.Value.Content.Count > 0 ? keyValuePair.Value.Content.FirstOrDefault().Value.Schema : default

                    };
                    var c = new ResponseProperiesGenerator().Excute(keyValuePair.Value.Content.FirstOrDefault().Value?.Schema);
                    if (!Equals(null, c))
                        response.Schemas.AddRange(c);

                    t.Responses.Add(response);
                });
                var s = t.ToString();
                var displayDeprecated = operation.Deprecated ? "【已过时】" : "";
                sb.Append($"\n### {operation?.Summary}{displayDeprecated} \n");
                sb.Append($"{s} \n");
            });

        }
    }
}