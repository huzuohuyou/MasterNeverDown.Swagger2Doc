namespace OpenApi2Doc.CommunityToolkit.Builders;

public abstract class DocumentBuilder<T>
{
    public T Doc;

    public OpenApiDocument ApiDocument { get; set; }

    public string CurrentPathTag { get; set; }
    public OpenApiPathItem CurrentPathItem { get; set; }
    public OpenApiOperation CurrentOperation { get; set; }

    public OpenApiResponse CurrentResponse { get; set; }
    public string CurrentResponseCode { get; set; }

    public abstract void Reset();

    public abstract void InitDoc();

    public abstract void BuildInfo();

    private void BuildPathItem()
    {
        BuildTag();
        BuildSummary();
        BeginBuildPathItem();
        BuildOperationId();
        BuildDescription();
        BuildRequestMethod();
        BuildRequestType();
        BuildRequestParams();
        BuildRequestBodies();
        BuildRequestBodyExample();
        foreach (var currentOperationResponse in CurrentOperation.Responses)
        {
            CurrentResponse = currentOperationResponse.Value;
            CurrentResponseCode = currentOperationResponse.Key;
            BuildResponse();
            BuildResponseFields();
            BuildResponseExample();
        }

        AfterBuildPathItem();
    }

    public abstract void BeginBuildPathItem();
    public abstract void AfterBuildPathItem();

    public abstract void BuildTag();

    public abstract void BuildSummary();

    public abstract void BuildOperationId();

    public abstract void BuildDescription();

    public abstract void BuildRequestMethod();

    public abstract void BuildRequestType();

    public abstract void BuildRequestParams();

    public abstract void BuildRequestBodies();

    public abstract void BuildRequestBodyExample();

    public abstract void BuildResponse();
    public abstract void BuildResponseFields();

    public abstract void BuildResponseExample();

    public abstract void BuildTitle();

    public abstract void BuildToc();

    public abstract T OutputDoc(string savePath = "");

    public async Task<T> Build(string? requestUri, string savePath = "")
    {
        Found:
        {
            if (string.IsNullOrWhiteSpace(requestUri))
            {
                Console.WriteLine("请输入swagger.json的url:");
                requestUri = Console.ReadLine();
            }
                
        }
        Stream stream;
        var client = new HttpClient();
        try
        {
            stream = await client.GetStreamAsync(requestUri);
        }
        catch (Exception e)
        {
            goto Found;
        }


        ApiDocument = new OpenApiStreamReader().Read(stream, out _);
        InitDoc();
        BuildTitle();
        BuildToc();
        BuildInfo();
        ApiDocument.Paths.ToList().ForEach(r =>
        {
            CurrentPathItem = r.Value;
            CurrentOperation = CurrentPathItem.Operations.Values.FirstOrDefault();
            BuildPathItem();
        });

        return OutputDoc(savePath);
    }

}