using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace GameServer.Models;

public static class CommandSerializer
{
    private static readonly JsonSerializerOptions _options;

    static CommandSerializer()
    {
        _options = new JsonSerializerOptions
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers =
                {
                    static typeInfo =>
                    {
                        if (typeInfo.Type != typeof(Command))
                        {
                            return;
                        }

                        typeInfo.PolymorphismOptions = new()
                        {
                            TypeDiscriminatorPropertyName = "__type",
                            DerivedTypes =
                            {
                                new JsonDerivedType(typeof(LoginRequest), nameof(LoginRequest)),
                                new JsonDerivedType(typeof(UpdateResourcesRequest), nameof(UpdateResourcesRequest)),
                                new JsonDerivedType(typeof(SendGiftRequest), nameof(SendGiftRequest)),
                                
                                new JsonDerivedType(typeof(LoginResponse), nameof(LoginResponse)),
                                new JsonDerivedType(typeof(ResourcesResponse), nameof(ResourcesResponse)),
                                new JsonDerivedType(typeof(SendGiftResponse), nameof(SendGiftResponse))
                            }
                        };
                    }
                }
            }
        };
        
    }

    public static string Pack<T>(T request) where T : Command
    {
        var cmd = JsonSerializer.Serialize<Command>(request, _options);
        return cmd;
    }
    
    public static Command Unpack(byte[] command)
    {
        var cmd = JsonSerializer.Deserialize<Command>(command, _options);
        return cmd!;
    }
}