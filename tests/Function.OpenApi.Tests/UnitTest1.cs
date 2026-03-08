using Microsoft.OpenApi;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Linq;
using System.Threading.Tasks;
using Function.OpenApi.Builders;

namespace Function.OpenApi.Tests;

public class UnitTest1
{
    [Fact]
    public async Task Test1()
    {
        var document = new OpenApiDocumentBuilder(typeof(Function1).Assembly).BuildDocument();

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
                    "operationId": "Function1GetFunction1",
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
                    "operationId": "Function1PostFunction1",
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
                    "operationId": "Function1PostFunction2",
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

        Assert.Equal(expectedJson.Replace("\r\n","\n"), json);
        
    }

    private static void RemoveDateOnlyExample(JsonNode? node)
    {
        if (node is null)
        {
            return;
        }

        var schema = node["components"]?["schemas"]?["System.DateOnly"] as JsonObject;
        schema?.Remove("example");
    }
}
