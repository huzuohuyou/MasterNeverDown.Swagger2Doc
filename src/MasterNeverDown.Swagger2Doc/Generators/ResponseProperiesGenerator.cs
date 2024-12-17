using OpenApi2Doc.CommunityToolkit.Models;

namespace OpenApi2Doc.CommunityToolkit.Generators
{
    public class ResponseProperiesGenerator
    {
        private string IndentChar = "····";
        private  int times=0;
        private List<string?> ReferenceIds=new List<string?>();
        public List<Schema>? Schemata { get; set; } = new();
        private string? GetSchemaType(OpenApiSchema? schema)
        {
            //if (schema?.Type == "array" && schema.Items?.Reference != null)
            //    return $@"{schema.Type}:{schema.Items?.Reference.Id}";
            //if (schema?.Type == "array" && schema.Items?.Reference == null)
            //    return $@"{schema.Type}:{schema.Items?.Type}";
            //if (schema?.Type == "object" && schema.Reference != null)
            //    return $@"{schema.Type}:{schema.Reference.Id}";
            return schema?.Type;

        }
        public IEnumerable<Schema>? Excute(OpenApiSchema? schema)
        {
            if (Equals(null, schema))
                 return new List<Schema>();
            InitEntity(schema, Schemata, times);
            return (Schemata != null && Schemata.Count>0?Schemata: new List<Schema>() { new Schema() { PropertyName = "_", PropertyType = schema.Type, Description = schema.Description } }).Where(r => !string.IsNullOrWhiteSpace(r.PropertyName)).ToArray();
        }

        private string IndentStr(int t)
        {
            var temp = string.Empty;
            for (int i = 0; i < t-1 ; i++)
            {
                temp += IndentChar;
            }
            return temp;
        }

        public void InitEntity(OpenApiSchema? schema, List<Schema>? schematas,int indentTime)
        {
            if (Equals(schema?.Type, "array"))
            {
                indentTime++;
                if (Equals(schema.Reference, null))
                {
                    var t = GetSchemaType(schema);
                    var p = schema.Items?.Reference?.Id;
                    if (!"string|number|integer|array".Contains(p??""))
                        p = "";
                    schematas?.Add(new Schema()
                    {
                        PropertyName =p,// $@"{IndentStr(indentTime)}{schema.Items.Reference?.Id}",
                        PropertyType =t,// GetSchemaType(schema),
                        Description = schema.Description,
                    });
                    if (schematas != null) ReferenceIds.Add(schematas.Last().PropertyName?.Trim('·'));
                }

                indentTime++;
                schema.Items.Properties.ToList().ForEach(prop =>
                {
                    schematas?.Add(new Schema()
                    {
                        PropertyName = $@"{IndentStr(indentTime)}{prop.Key}",
                        PropertyType = GetSchemaType(prop.Value), 
                        Description = prop.Value.Description,
                    });
                    if (schematas != null)
                    {
                        ReferenceIds.Add(schematas.Last().PropertyName?.Trim('·'));
                        if (ReferenceIds.Count(r => r == schematas.Last().PropertyName?.Trim('·')) < 2)
                        {
                            if (prop.Value.Type == "array")
                                InitEntity(prop.Value.Items, schematas, indentTime);
                            else
                                InitEntity(prop.Value, schematas, indentTime);
                        }
                    }
                });
            }
            else if (Equals(schema?.Type, "object"))
            {
                //IndentTime++;
                //schematas.Add(new Schema()
                //{
                //    PropertyName = $@"{IndentStr(IndentTime)}{schema.Reference.Id}",
                //    PropertyType = schema.Type == "object"
                //        ? $@"{schema.Type}:{schema?.Reference?.Id}"
                //        : schema.Type,
                //    Remark = schema.Description,
                //});
                indentTime++;
                schema.Properties.ToList().ForEach(prop =>
                {
                    schematas?.Add(new Schema()
                    {
                        PropertyName = $@"{IndentStr(indentTime)}{prop.Key}",
                        PropertyType = GetSchemaType(prop.Value),
                        Description = prop.Value.Description,
                    });
                    if (schematas != null)
                    {
                        ReferenceIds.Add(schematas.Last().PropertyName?.Trim('·'));
                        if (ReferenceIds.Count(r => r == schematas.Last().PropertyName?.Trim('·')) < 2)
                            InitEntity(prop.Value, schematas, indentTime);
                    }
                });
            }
            
        }
    }
}
