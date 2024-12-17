namespace OpenAPI2Word.CommunityToolkit.Generators
{
    public class RequestProperiesGenerator
    {
        private string IndentChar = "····";
        private int times = 0;

        private string? GetSchemaType(OpenApiSchema? schema)
        {
            if (schema?.Type == "array" && schema.Items?.Reference != null)
                return $@"{schema.Type}:{schema.Items?.Reference.Id}";
            if (schema?.Type == "array" && schema.Items?.Reference == null)
                return $@"{schema.Type}:{schema.Items?.Type}";
            if (schema?.Type == "object" && schema.Reference != null)
                return $@"{schema.Type}:{schema.Reference.Id}";
            return schema?.Type;

        }
        public List<RequestBody>? Schemata { get; set; } = new();

        public List<RequestBody>? Excute(OpenApiSchema? schema)
        {
            if (Equals(null, schema))
                return default;
            InitEntity(schema, Schemata, times);
            return Schemata != null && Schemata.Count > 0 ? Schemata : new List<RequestBody>() { new RequestBody() { PropertyName = "_", PropertyType = schema.Type, Description = schema.Description } };
        }

        private string IndentStr(int t)
        {
            var temp = string.Empty;
            for (int i = 0; i < t - 1; i++)
            {
                temp += IndentChar;
            }
            return temp;
        }



        public void InitEntity(OpenApiSchema? schema, List<RequestBody>? schematas, int indentTime)
        {
            if (Equals(schema?.Type, "array"))
            {
                indentTime++;
                if (!string.IsNullOrWhiteSpace(schema.Items?.Reference?.Id))
                    schematas?.Add(new RequestBody()
                    {
                        PropertyName = $@"{IndentStr(indentTime)}{schema.Items?.Reference?.Id}",
                        PropertyType = GetSchemaType(schema),
                        Description = schema.Description,
                        IsRequired = schema.Required != null && schema.Required.Contains(schema.Items?.Reference?.Id) ? "Y" : "",
                    });

                indentTime++;
                schema.Items?.Properties.ToList().ForEach(prop =>
                {
                    if (!string.IsNullOrWhiteSpace(prop.Key))
                        schematas?.Add(new RequestBody()
                        {
                            PropertyName = $@"{IndentStr(indentTime)}{prop.Key}",
                            PropertyType = GetSchemaType(prop.Value),
                            Description = prop.Value.Description,
                            IsRequired = schema.Required != null && schema.Required.Contains(prop.Key) ? "Y" : "",
                        });
                    if (prop.Value.Type == "array")
                        InitEntity(prop.Value.Items, schematas, indentTime);
                    else
                        InitEntity(prop.Value, schematas, indentTime);
                });
            }
            else if (Equals(schema?.Type, "object"))
            {
                indentTime++;
                if (!string.IsNullOrWhiteSpace(schema.Reference?.Id))
                    schematas?.Add(new RequestBody()
                    {
                        PropertyName = $@"{IndentStr(indentTime)}{schema.Reference.Id}",
                        PropertyType = GetSchemaType(schema),
                        IsRequired = schema.Required.Contains(schema.Reference.Id) ? "Y" : "",
                    });
                indentTime++;
                schema.Properties.ToList().ForEach(prop =>
                {
                    if (!string.IsNullOrWhiteSpace(prop.Key))
                        schematas?.Add(new RequestBody()
                        {
                            PropertyName = $@"{IndentStr(indentTime)}{prop.Key}",
                            PropertyType = GetSchemaType(prop.Value),
                            Description = prop.Value.Description,
                            IsRequired = schema.Required.Contains(prop.Key) ? "Y" : "",
                        });

                    InitEntity(prop.Value, schematas, indentTime);
                });
            }
            else
            {//基本类型

                //schematas.Add(new RequestBody()
                //{
                //    PropertyName = $@"{IndentStr(IndentTime)}_",
                //    PropertyType = schema.Type,
                //    Description = schema.Description,
                //    //IsRequired = schema.Required ? "Y" : "",
                //});

            }

        }
    }
}
