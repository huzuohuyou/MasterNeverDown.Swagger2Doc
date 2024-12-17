using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace OpenApi2Md.CommunityToolkit.Builders
{
    
    public class ExampleValueGenerator
    {
        /// <summary>
        /// 递归中断记录器
        /// </summary>
        private List<string?> ReferenceIds = new();
        public string? Excute(OpenApiSchema? schema)
        {
            if (Equals(null, schema))
                return default;
            var r = InitEntity(schema);
            if (!Equals(null,r))
                return ConvertJsonString(JsonConvert.SerializeObject(r));
            return schema.Type;
        }

        private object InitEntity(OpenApiSchema? schema)
        {
            if (Equals(schema?.Type, "array"))
            {
                var temp = new List<object>();

                if (ReferenceIds.Count(id => id == schema.Items?.Reference?.Id) <= 3)
                    temp.Add(InitEntity(schema.Items));
                if (  !string.IsNullOrWhiteSpace(schema.Items?.Reference?.Id))
                    ReferenceIds.Add(schema.Items?.Reference?.Id);
                return temp;
            }
            else if (Equals(schema?.Type, "object"))
            {
                if (!string.IsNullOrWhiteSpace(schema.Reference?.Id))
                    ReferenceIds.Add(schema.Reference?.Id);
                var temp = new Dictionary<string, object>();
                schema.Properties.Keys.ToList().ForEach(r =>
                {
                    if (schema.Properties.Keys.Contains(r))
                    {
                        if (!string.IsNullOrWhiteSpace(schema.Properties[r].Items?.Reference?.Id))
                            ReferenceIds.Add(schema.Properties[r]?.Items?.Reference?.Id);
                        try
                        {
                            if (ReferenceIds.Where(id => id == schema.Properties[r]?.Reference?.Id).ToList().Count() <= 3)
                                temp.Add(r, InitEntity(schema.Properties[r]));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }

                    }


                });
                return temp;
            }
            else
            {//基本类型
                if (Equals(schema?.Type, "integer"))
                {
                    Int64 result;
                    Int64.TryParse((schema.Example == null ? default : (schema.Example as dynamic).Value)?.ToString(), out result);
                    return result;
                }
                else if (Equals(schema?.Type, "number"))
                {
                    double result;
                    Double.TryParse((schema.Example == null ? default : (schema.Example as dynamic).Value)?.ToString(), out result);
                    return result;
                }
                else if (Equals(schema?.Type, "boolean"))
                {
                    bool result;
                    bool.TryParse((schema.Example == null ? default : (schema.Example as dynamic).Value)?.ToString(), out result);
                    return result;
                }
                else if (schema != null && schema.Enum.Any())
                {
                    return string.Join('|', schema.Enum.ToArray().Select(r => (r as dynamic).Value));
                }
                return (schema?.Example == null ? default : (schema.Example as dynamic).Value) ?? "string";
            }
        }

        private string? ConvertJsonString(string? str)
        {
            //格式化json字符串
            JsonSerializer serializer = new JsonSerializer();
            if (string.IsNullOrWhiteSpace(str)) return default;
            TextReader tr = new StringReader(str);
            JsonTextReader jtr = new JsonTextReader(tr);
            object? obj = serializer.Deserialize(jtr);
            if (obj != null)
            {
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = '·'
                };
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
            else
            {
                return str;
            }
        }
    }
}
