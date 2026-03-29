using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;

namespace RelojesLamur.API.Services;

public class FirebaseStorageService
{
    private readonly StorageClient _storageClient;
    private readonly string _bucketName = "relojes-lamur-e1fb3.appspot.com";

    public FirebaseStorageService()
    {
        var credentialsJson = @"{
  ""type"": ""service_account"",
  ""project_id"": ""relojes-lamur-e1fb3"",
  ""private_key_id"": ""b55ecdbc205e6385874bab83160a869c962d0638"",
  ""private_key"": ""-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQDXplV/WAVbONak\nsQnm8fXJuHB89c4GssjWRPVfhOlWtbIXNbIJPhljo3FIGqwQjXM723/0kVQSrvTE\nJGbDvsHIoGe9QdEfrn2FOZqmqJR6erq7B9K+b4tBEQKhlm51ayRMAJX6pB1lk5za\nLBW+xg6QvXU+wm0UfTk/sCaP+258xmjZ5wOexhGOav6Shie0LtT7r1kkvqPhYTwh\nmftfy1RhNcv4uCclGyb9cDC69O4JrTxP2XBqc9Kg20dY6Z8ALvRe9FuO94TPJhfy\nA2FuuiYsjBk5zbRGX+0JEthBAlUmzxgI5EH234sAiVbBzJbqzARZkt/BP4dGCY5f\n8F0y2nYDAgMBAAECggEABWhu8XQYIksBGv0zld6QfuAItiGF6qYWIkV1b9FJ9ro0\nvp9CmTe89EI3h4rfsMSTVLdy9IOme5d2nOqaxyLkAi9/mn/piiVakXo51h8ZJh8o\nyqLuFod+kwGlsr6ug3RN4zgKitANyqvSfagoXXHjEJ8nrwy6Jo7jXiUBOpAgG7Hk\nS0xrNuqezwxakxE4UQ5/3K28k9Ny851NBHy8iLIZHzh18gjCPRzKnEHYI8O7Tbou\nDiGbKgPfGmU0AYjriOISp8QLY5et7cHnmdjCbf/5hqoUUIVyOSXZX4X2fABnOcKj\n4ml5x9Uyt9fETCgMDCx1NX4qtgvBn3sV7OzgWwdgUQKBgQDsz3r7FQmnSfj7R+32\nQu1t6GF6Wt9RXHWEJSByO6X806p0Y5GK+AxnbfuuUBa6Nr1p1QJXMCdCh8oVCuIX\nsNCHMpa5/oNbcXziKD8PHT8BL75BHgGT2JK/UIrjrm9r3k78bHixhU47WfHvcCET\nG76RWxIJcG7HHl3gL0gr1bzv8wKBgQDpH+JprullaZHs843GY3+4qyjp/nBf08bI\nJBPRWVGvgrSsAhOkX600YGvNerK7vEaKFBaG3hSf8cRDjsLnNtGvybw7++vJMNOZ\nrB2ILDCvSQjh5/cAe39e4nUbSSXrDI0/Wdo0km/4LqeU0Hys+Wt4w+CF6GFGO1eo\n5HDaOlf1sQKBgCOQHRXr2OFImJ2T9caP0nw487qePv9G9Vb9BFxjXNAVMXn9IfRO\nv/4gZDWPTXp1kgh8trdFQgAZyF1UnxOTuB01mbEg6bUn2+tSw/WPHNEezGGXEgGT\n+qXEgLckkRMP9aiu2Yk/TbpYDZ85pZ3rArlthc3pFpnMk6iBMUNz+8XFAoGBAInw\nXc3euydQchHcgtUSZq5kSE88SE3LR2GBR2CmExlgo3rrt6eZHSlSrDbnFP7UhoCJ\nJlMi8N069sBqppSc8TGskn6Dr55aD7psBwVd8GinNGRHFvXoHRONt3EvtQoBYUpM\n5joZM7uMD4iUXFEVPCw26pERUQ13a4T5tnTJ9OXhAoGAZURSvSq8FPeoOoqoa7pe\nd+cllu8oenOxEEuczYQPwMzW3Y2d1Fb9ZsrKtG8644j6/CJuBDKa6NOFIEgB6jMy\ngRCm8ATICnI9RwUReOvo5QJ5rxYYfozdxKfT453vQKz+ZpWMDR2mdSb1RQmom/HY\nyo5cqu0Ec7Eh2ROvZxerjCI=\n-----END PRIVATE KEY-----\n"",
  ""client_email"": ""firebase-adminsdk-fbsvc@relojes-lamur-e1fb3.iam.gserviceaccount.com"",
  ""client_id"": ""104911800961867000578"",
  ""auth_uri"": ""https://accounts.google.com/o/oauth2/auth"",
  ""token_uri"": ""https://oauth2.googleapis.com/token"",
  ""auth_provider_x509_cert_url"": ""https://www.googleapis.com/oauth2/v1/certs"",
  ""client_x509_cert_url"": ""https://www.googleapis.com/robot/v1/metadata/x509/firebase-adminsdk-fbsvc%40relojes-lamur-e1fb3.iam.gserviceaccount.com"",
  ""universe_domain"": ""googleapis.com""
}";

        var credential = GoogleCredential.FromJson(credentialsJson);
        _storageClient = StorageClient.Create(credential);
    }

    public async Task<string> UploadAsync(IFormFile file)
    {
        // Validar tipo de archivo (solo imágenes)
        if (!file.ContentType.StartsWith("image/"))
            throw new ArgumentException("Solo se permiten archivos de imagen.");

        // Validar tamaño (máximo 5MB)
        if (file.Length > 5 * 1024 * 1024)
            throw new ArgumentException("El archivo no puede superar los 5MB.");

        // Generar nombre único
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var objectName = $"products/{fileName}";

        // Subir a Firebase Storage
        using var stream = file.OpenReadStream();
        var obj = await _storageClient.UploadObjectAsync(
            _bucketName,
            objectName,
            file.ContentType,
            stream,
            new UploadObjectOptions
            {
                PredefinedAcl = PredefinedObjectAcl.PublicRead
            }
        );

        // Devolver URL pública
        return $"https://firebasestorage.googleapis.com/v0/b/{_bucketName}/o/{Uri.EscapeDataString(objectName)}?alt=media";
    }
}