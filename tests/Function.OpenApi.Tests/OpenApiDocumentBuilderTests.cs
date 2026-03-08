using Microsoft.OpenApi;
using System.Text.Json.Nodes;
using Function.OpenApi.Builders;

namespace Function.OpenApi.Tests;

public class OpenApiDocumentBuilderTests
{
    [Fact]
    public async Task FullDocument_SerializesExpectedJson()
    {
        var document = new OpenApiDocumentBuilder(typeof(TestFunctions).Assembly).BuildDocument();

        var json = await document.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0, TestContext.Current.CancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd");
        var expectedJson = $$"""
            {
              "openapi": "3.0.4",
              "info": {
                "title": "Test OpenApi Implementation",
                "version": "1.0.0"
              },
              "servers": [
                {
                  "url": "http://localhost:7136/api"
                }
              ],
              "paths": {
                "/Function1": {
                  "get": {
                    "operationId": "TestFunctionsGetFunction1",
                    "responses": {
                      "200": {
                        "description": "",
                        "content": {
                          "application/json": {
                            "schema": {
                              "$ref": "#/components/schemas/Function.OpenApi.Tests.ResponseBody"
                            }
                          }
                        }
                      }
                    }
                  },
                  "post": {
                    "operationId": "TestFunctionsPostFunction1",
                    "responses": {
                      "200": {
                        "description": "",
                        "content": {
                          "application/json": {
                            "schema": {
                              "$ref": "#/components/schemas/Function.OpenApi.Tests.ResponseBody"
                            }
                          }
                        }
                      }
                    }
                  }
                },
                "/function2/{id}": {
                  "post": {
                    "operationId": "TestFunctionsPostFunction2",
                    "parameters": [
                      {
                        "$ref": "#/components/parameters/System.Guid_id"
                      },
                      {
                        "$ref": "#/components/parameters/System.String_x-correlation-id_header"
                      }
                    ],
                    "requestBody": {
                      "$ref": "#/components/requestBodies/Function.OpenApi.Tests.RequestBody"
                    },
                    "responses": {
                      "200": {
                        "description": "",
                        "headers": {
                          "x-response-id": {
                            "required": true,
                            "schema": {
                              "$ref": "#/components/schemas/System.Guid"
                            }
                          }
                        },
                        "content": {
                          "application/json": {
                            "schema": {
                              "$ref": "#/components/schemas/Function.OpenApi.Tests.ResponseBody"
                            }
                          }
                        }
                      },
                      "400": {
                        "description": "",
                        "content": {
                          "application/json": {
                            "schema": {
                              "$ref": "#/components/schemas/Microsoft.AspNetCore.Mvc.ValidationProblemDetails"
                            }
                          }
                        }
                      }
                    }
                  }
                }
              },
              "components": {
                "schemas": {
                  "System.String": {
                    "type": "string"
                  },
                  "System.Int32": {
                    "type": "integer",
                    "format": "int32"
                  },
                  "System.Collections.Generic.IEnumerable_System.Int32": {
                    "type": "array",
                    "items": {
                      "$ref": "#/components/schemas/System.Int32"
                    }
                  },
                  "System.Nullable_System.Guid": {
                    "type": "string",
                    "format": "uuid",
                    "nullable": true
                  },
                  "Nullable_System.String": {
                    "type": "string",
                    "nullable": true
                  },
                  "System.Nullable_System.Int32": {
                    "type": "integer",
                    "format": "int32",
                    "nullable": true
                  },
                  "System.Nullable_System.DateOnly": {
                    "type": "string",
                    "format": "date",
                    "nullable": true
                  },
                  "System.Int64": {
                    "type": "integer",
                    "format": "int64"
                  },
                  "System.Single": {
                    "type": "number",
                    "format": "float"
                  },
                  "Function.OpenApi.Tests.NestedClass": {
                    "type": "object",
                    "properties": {
                      "id": {
                        "$ref": "#/components/schemas/System.Nullable_System.Guid"
                      },
                      "name": {
                        "$ref": "#/components/schemas/Nullable_System.String"
                      },
                      "age": {
                        "$ref": "#/components/schemas/System.Nullable_System.Int32"
                      },
                      "dateOnly": {
                        "$ref": "#/components/schemas/System.Nullable_System.DateOnly"
                      },
                      "longInteger": {
                        "$ref": "#/components/schemas/System.Int64"
                      },
                      "floatNumber": {
                        "$ref": "#/components/schemas/System.Single"
                      }
                    }
                  },
                  "Function.OpenApi.Tests.ResponseBody": {
                    "type": "object",
                    "properties": {
                      "name": {
                        "$ref": "#/components/schemas/System.String"
                      },
                      "listOfInts": {
                        "$ref": "#/components/schemas/System.Collections.Generic.IEnumerable_System.Int32"
                      },
                      "nestedClass": {
                        "$ref": "#/components/schemas/Function.OpenApi.Tests.NestedClass"
                      }
                    }
                  },
                  "System.Guid": {
                    "type": "string",
                    "format": "uuid"
                  },
                  "System.StringArray": {
                    "type": "array",
                    "items": {
                      "$ref": "#/components/schemas/System.String"
                    }
                  },
                  "System.Collections.Generic.IDictionary_System.String_System.StringArray": {
                    "type": "object",
                    "additionalProperties": {
                      "$ref": "#/components/schemas/System.StringArray"
                    }
                  },
                  "System.Object": {
                    "type": "object"
                  },
                  "System.Collections.Generic.IDictionary_System.String_System.Object": {
                    "type": "object",
                    "additionalProperties": {
                      "$ref": "#/components/schemas/System.Object"
                    }
                  },
                  "Microsoft.AspNetCore.Mvc.ValidationProblemDetails": {
                    "type": "object",
                    "properties": {
                      "errors": {
                        "$ref": "#/components/schemas/System.Collections.Generic.IDictionary_System.String_System.StringArray"
                      },
                      "type": {
                        "$ref": "#/components/schemas/Nullable_System.String"
                      },
                      "title": {
                        "$ref": "#/components/schemas/Nullable_System.String"
                      },
                      "status": {
                        "$ref": "#/components/schemas/System.Nullable_System.Int32"
                      },
                      "detail": {
                        "$ref": "#/components/schemas/Nullable_System.String"
                      },
                      "instance": {
                        "$ref": "#/components/schemas/Nullable_System.String"
                      },
                      "extensions": {
                        "$ref": "#/components/schemas/System.Collections.Generic.IDictionary_System.String_System.Object"
                      }
                    }
                  },
                  "System.DateOnly": {
                    "type": "string",
                    "format": "date",
                    "example": "{{today}}"
                  },
                  "System.Collections.Generic.List_System.String": {
                    "type": "array",
                    "items": {
                      "$ref": "#/components/schemas/System.String"
                    }
                  },
                  "Function.OpenApi.Tests.RequestKind": {
                    "enum": [
                      "Standard",
                      "Urgent"
                    ],
                    "type": "string"
                  },
                  "Function.OpenApi.Tests.RequestBody": {
                    "type": "object",
                    "properties": {
                      "id": {
                        "$ref": "#/components/schemas/System.Guid"
                      },
                      "name": {
                        "$ref": "#/components/schemas/System.String"
                      },
                      "age": {
                        "$ref": "#/components/schemas/System.Int32"
                      },
                      "dateOnly": {
                        "$ref": "#/components/schemas/System.DateOnly"
                      },
                      "listOfStrings": {
                        "$ref": "#/components/schemas/System.Collections.Generic.List_System.String"
                      },
                      "kind": {
                        "$ref": "#/components/schemas/Function.OpenApi.Tests.RequestKind"
                      }
                    }
                  }
                },
                "parameters": {
                  "System.Guid_id": {
                    "name": "id",
                    "in": "path",
                    "required": true,
                    "schema": {
                      "$ref": "#/components/schemas/System.Guid"
                    }
                  },
                  "System.String_x-correlation-id_header": {
                    "name": "x-correlation-id",
                    "in": "header",
                    "required": true,
                    "schema": {
                      "$ref": "#/components/schemas/System.String"
                    }
                  }
                },
                "requestBodies": {
                  "Function.OpenApi.Tests.RequestBody": {
                    "content": {
                      "application/json": {
                        "schema": {
                          "$ref": "#/components/schemas/Function.OpenApi.Tests.RequestBody"
                        }
                      }
                    }
                  }
                }
              }
            }
            """;

        var actualNode = JsonNode.Parse(json);
        var expectedNode = JsonNode.Parse(expectedJson);

        Assert.NotNull(actualNode);
        Assert.NotNull(expectedNode);
        Assert.Equal(expectedJson.Replace("\r\n","\n"), json);
        
    }

    [Fact]
    public async Task DefaultBehavior_GeneratesOpenApi304()
    {
        var document = new OpenApiDocumentBuilder(typeof(TestFunctions).Assembly).BuildDocument();

        var json = await document.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0, TestContext.Current.CancellationToken);
        var node = JsonNode.Parse(json);

        Assert.Equal("3.0.4", node!["openapi"]?.GetValue<string>());
    }

    [Fact]
    public async Task ExplicitOpenApi30_GeneratesOpenApi304()
    {
        var options = new OpenApiDocumentOptions
        {
            Title = "Test OpenApi Implementation",
            Version = "1.0.0",
            ServerUrls = ["http://localhost:7136"],
            Assemblies = [typeof(TestFunctions).Assembly],
            SpecVersion = OpenApiSpecVersion.OpenApi3_0
        };
        var document = new OpenApiDocumentBuilder(options).BuildDocument();

        var json = await document.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0, TestContext.Current.CancellationToken);
        var node = JsonNode.Parse(json);

        Assert.Equal("3.0.4", node!["openapi"]?.GetValue<string>());
    }

    [Fact]
    public async Task ExplicitOpenApi30_MatchesDefaultOutput()
    {
        var defaultDocument = new OpenApiDocumentBuilder(typeof(TestFunctions).Assembly).BuildDocument();
        var defaultJson = await defaultDocument.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0, TestContext.Current.CancellationToken);

        var explicitOptions = new OpenApiDocumentOptions
        {
            Title = "Test OpenApi Implementation",
            Version = "1.0.0",
            ServerUrls = ["http://localhost:7136"],
            Assemblies = [typeof(TestFunctions).Assembly],
            SpecVersion = OpenApiSpecVersion.OpenApi3_0
        };
        var explicitDocument = new OpenApiDocumentBuilder(explicitOptions).BuildDocument();
        var explicitJson = await explicitDocument.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0, TestContext.Current.CancellationToken);

        Assert.Equal(defaultJson, explicitJson);
    }

    [Fact]
    public async Task DefaultBehavior_HasNullableKeyword_In30()
    {
        var document = new OpenApiDocumentBuilder(typeof(TestFunctions).Assembly).BuildDocument();

        var json = await document.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0, TestContext.Current.CancellationToken);
        var node = JsonNode.Parse(json);

        // In OpenAPI 3.0, nullable types use "nullable": true
        var nullableGuidSchema = node!["components"]?["schemas"]?["System.Nullable_System.Guid"];
        Assert.NotNull(nullableGuidSchema);
        Assert.True(nullableGuidSchema!["nullable"]?.GetValue<bool>());
    }
}
