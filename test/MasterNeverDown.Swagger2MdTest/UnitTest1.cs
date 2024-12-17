using Microsoft.OpenApi.Models;
using OpenAPI2MD.CommunityToolkit.Builders;

namespace MasterNeverDown.Swagger2MdTest;

public class UnitTest1
{
    [Fact]
    public void Reset_ClearsDocument()
    {
        // Arrange
        var generator = new OpenApiMdGenerator();
        generator.InitDoc();
        generator.Doc.Append("Test content");

        // Act
        generator.Reset();

        // Assert
        Assert.Equal(string.Empty, generator.Doc.ToString());
    }

    [Fact]
    public void InitDoc_InitializesDocument()
    {
        // Arrange
        var generator = new OpenApiMdGenerator();

        // Act
        generator.InitDoc();

        // Assert
        Assert.NotNull(generator.Doc);
        Assert.Equal(string.Empty, generator.Doc.ToString());
    }

    [Fact]
    public void BuildInfo_AppendsInfoToDocument()
    {
        // Arrange
        var generator = new OpenApiMdGenerator();
        generator.ApiDocument = new OpenApiDocument
        {
            Info = new OpenApiInfo
            {
                Description = "Test Description",
                Contact = new OpenApiContact
                {
                    Name = "Test Name",
                    Email = "test@example.com"
                }
            }
        };
        generator.InitDoc();

        // Act
        generator.BuildInfo();

        // Assert
        var expected = "Test Description \nTest Name \ntest@example.com \n";
        Assert.Equal(expected, generator.Doc.ToString());
    }

    [Fact]
    public void BuildTag_AppendsTagToDocument()
    {
        // Arrange
        var generator = new OpenApiMdGenerator();
        generator.CurrentOperation = new OpenApiOperation
        {
            Tags = new List<OpenApiTag> { new OpenApiTag { Name = "TestTag" } }
        };
        generator.InitDoc();

        // Act
        generator.BuildTag();

        // Assert
        var expected = " \n## TestTag \n";
        Assert.Equal(expected, generator.Doc.ToString());
    }

    [Fact]
    public void BuildSummary_AppendsSummaryToDocument()
    {
        // Arrange
        var generator = new OpenApiMdGenerator();
        generator.CurrentOperation = new OpenApiOperation
        {
            Summary = "Test Summary"
        };
        generator.InitDoc();

        // Act
        generator.BuildSummary();

        // Assert
        var expected = "\n\n### Test Summary\n";
        Assert.Equal(expected, generator.Doc.ToString());
    }

    [Fact]
    public void BuildOperationId_AppendsOperationIdToDocument()
    {
        // Arrange
        var generator = new OpenApiMdGenerator();
        generator.CurrentOperation = new OpenApiOperation
        {
            OperationId = "TestOperationId"
        };
        generator.InitDoc();

        // Act
        generator.BuildOperationId();

        // Assert
        var expected = "\n<tr bgcolor=\"\">\n<td>OperationId</td>\n<td colspan=\"5\">TestOperationId</td>\n</tr>\n";
        Assert.Equal(expected, generator.Doc.ToString());
    }

    [Fact]
    public void BuildDescription_AppendsDescriptionToDocument()
    {
        // Arrange
        var generator = new OpenApiMdGenerator();
        generator.CurrentOperation = new OpenApiOperation
        {
            Description = "Test Description"
        };
        generator.InitDoc();

        // Act
        generator.BuildDescription();

        // Assert
        var expected = "\n<tr>\n<td>接口描述</td>\n<td colspan=\"5\">Test Description</td>\n</tr>\n";
        Assert.Equal(expected, generator.Doc.ToString());
    }
}