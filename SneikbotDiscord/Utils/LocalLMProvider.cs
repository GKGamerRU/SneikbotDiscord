using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public struct Response
{
    public bool isError;
    public string message;

    public Response(bool isError, string message)
    {
        this.isError = isError;
        this.message = message;
    }
}

public class Prompt
{
    public string Text { get; set; }
    public string ImagePath { get; private set; }
    public string Base64Image { get; private set; }
    public string ModelName { get; set; }

    public void SetImageFromPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Image path is empty.");
        if (!File.Exists(path)) throw new FileNotFoundException("Image file not found", path);
        ImagePath = path;
        Base64Image = Convert.ToBase64String(File.ReadAllBytes(path));
    }

    public void SetBase64Image(string base64)
    {
        Base64Image = base64;
        ImagePath = null;
    }

    public bool HasImage => !string.IsNullOrEmpty(Base64Image) || !string.IsNullOrEmpty(ImagePath);

    public string GetBase64OrLoad()
    {
        if (!string.IsNullOrEmpty(Base64Image)) return Base64Image;
        if (!string.IsNullOrEmpty(ImagePath)) return Convert.ToBase64String(File.ReadAllBytes(ImagePath));
        return null;
    }

    public string GetDataUriForImage()
    {
        var base64 = GetBase64OrLoad();
        if (base64 == null) return null;
        var mime = GuessMimeType(ImagePath);
        return $"data:{mime};base64,{base64}";
    }

    private static string GuessMimeType(string path)
    {
        if (string.IsNullOrEmpty(path)) return "image/png";
        var ext = Path.GetExtension(path)?.ToLowerInvariant();
        switch (ext)
        {
            case ".jpg":
            case ".jpeg": return "image/jpeg";
            case ".png": return "image/png";
            case ".gif": return "image/gif";
            case ".webp": return "image/webp";
            case ".bmp": return "image/bmp";
            default: return "image/png";
        }
    }
}

public interface ILlmProvider
{
    Task<Response> SendMessageAsync(Prompt prompt, bool includeHistory, CancellationToken ct = default);
    Task<string[]> GetModelsAsync(CancellationToken ct = default);
    void ClearHistory();
    void SetSystemPrompt(string systemPrompt);
}

public abstract class BaseProvider : ILlmProvider
{
    public string _baseUrl;
    protected string _systemPrompt;
    protected readonly List<JObject> _history = new List<JObject>(); // role/content(/images) сообщ.

    protected BaseProvider(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl)) throw new ArgumentNullException(nameof(baseUrl));
        _baseUrl = baseUrl.TrimEnd('/');
    }

    protected static readonly HttpClient Http = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(120)
    };

    public void ClearHistory() => _history.Clear();
    public void SetSystemPrompt(string systemPrompt) => _systemPrompt = string.IsNullOrWhiteSpace(systemPrompt) ? null : systemPrompt;

    public abstract Task<Response> SendMessageAsync(Prompt prompt, bool includeHistory, CancellationToken ct = default);
    public abstract Task<string[]> GetModelsAsync(CancellationToken ct = default);

    protected static async Task<string> SendJsonAsync(string url, JObject payload, string apiKey, CancellationToken ct)
    {
        using (var req = new HttpRequestMessage(HttpMethod.Post, url))
        {
            if (!string.IsNullOrEmpty(apiKey))
                req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            req.Headers.Accept.ParseAdd("application/json");
            req.Content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json");

            using (var resp = await Http.SendAsync(req, HttpCompletionOption.ResponseContentRead, ct).ConfigureAwait(false))
            {
                var respBody = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                {
                    // Попробуем вытащить сообщение об ошибке
                    try
                    {
                        var err = JObject.Parse(respBody);
                        var msg = err["error"]?["message"]?.ToString() ?? err["error"]?.ToString() ?? respBody;
                        throw new Exception($"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}: {msg}");
                    }
                    catch
                    {
                        throw new Exception($"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}: {respBody}");
                    }
                }
                return respBody;
            }
        }
    }
}

/// Ollama (нативный API: /api/chat и /api/tags)
public class OllamaProvider : BaseProvider
{
    private readonly string _apiKey; // обычно не нужен, но оставим опционально

    public OllamaProvider(string baseUrl = "http://localhost:11434", string apiKey = null) : base(baseUrl)
    {
        _apiKey = apiKey;
    }

    public override async Task<Response> SendMessageAsync(Prompt prompt, bool includeHistory, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(prompt?.Text))
            return new Response(true, "Пустой текст запроса.");

        var messages = new JArray();

        if (includeHistory && _history.Count > 0)
        {
            foreach (var m in _history)
                messages.Add(m);
        }

        if (!string.IsNullOrEmpty(_systemPrompt))
        {
            messages.Add(new JObject
            {
                ["role"] = "system",
                ["content"] = _systemPrompt
            });
        }

        var userMsg = new JObject
        {
            ["role"] = "user",
            ["content"] = prompt.Text
        };

        // Для vision у Ollama: messages[].images = [ base64 или пути к файлам ]
        if (prompt.HasImage)
        {
            var base64 = prompt.GetBase64OrLoad();
            userMsg["images"] = new JArray { base64 };
        }

        messages.Add(userMsg);

        var payload = new JObject
        {
            ["model"] = prompt.ModelName,
            ["messages"] = messages,
            ["stream"] = false
        };

        try
        {
            var body = await SendJsonAsync($"{_baseUrl}/api/chat", payload, _apiKey, ct).ConfigureAwait(false);
            var json = JObject.Parse(body);

            // Ответ /api/chat: { "message": {"role":"assistant","content":"..."}, "done": true, ...}
            var assistant = json["message"]?["content"]?.ToString();
            if (assistant == null)
                return new Response(true, "Пустой ответ от Ollama или неизвестный формат.");

            // Обновим историю
            _history.Add(userMsg);
            _history.Add(new JObject
            {
                ["role"] = "assistant",
                ["content"] = assistant
            });

            return new Response(false, assistant.Trim());
        }
        catch (Exception ex)
        {
            return new Response(true, $"Ollama error: {ex.Message}");
        }
    }

    public override async Task<string[]> GetModelsAsync(CancellationToken ct = default)
    {
        using (var req = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/api/tags"))
        {
            if (!string.IsNullOrEmpty(_apiKey))
                req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            using (var resp = await Http.SendAsync(req, HttpCompletionOption.ResponseContentRead, ct).ConfigureAwait(false))
            {
                var text = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new Exception($"Ollama /api/tags failed: {(int)resp.StatusCode} {resp.ReasonPhrase} {text}");

                var json = JObject.Parse(text);
                var models = json["models"] as JArray;
                if (models == null) return new string[0];

                var list = new List<string>();
                foreach (var m in models)
                {
                    var name = m["name"]?.ToString();
                    if (!string.IsNullOrEmpty(name)) list.Add(name);
                }
                return list.ToArray();
            }
        }
    }
}

/// LM Studio (OpenAI-совместимый /v1)
public class LmStudioProvider : BaseProvider
{
    private readonly string _apiKey; // если включена авторизация в LM Studio

    public LmStudioProvider(string baseUrl = "http://localhost:1234", string apiKey = null) : base(baseUrl)
    {
        _apiKey = apiKey;
    }

    public override async Task<Response> SendMessageAsync(Prompt prompt, bool includeHistory, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(prompt?.Text) && !prompt.HasImage)
            return new Response(true, "Пустой запрос (нет текста и изображения).");

        var messages = new JArray();

        if (!string.IsNullOrEmpty(_systemPrompt))
        {
            messages.Add(new JObject
            {
                ["role"] = "system",
                ["content"] = _systemPrompt
            });
        }

        if (includeHistory && _history.Count > 0)
        {
            foreach (var m in _history)
                messages.Add(m);
        }

        JObject userMsg;
        if (prompt.HasImage)
        {
            // OpenAI-совместимый vision: content как массив частей
            var content = new JArray();

            if (!string.IsNullOrWhiteSpace(prompt.Text))
            {
                content.Add(new JObject
                {
                    ["type"] = "text",
                    ["text"] = prompt.Text
                });
            }

            var dataUri = prompt.GetDataUriForImage();
            content.Add(new JObject
            {
                ["type"] = "image_url",
                ["image_url"] = new JObject
                {
                    ["url"] = dataUri
                }
            });

            userMsg = new JObject
            {
                ["role"] = "user",
                ["content"] = content
            };
        }
        else
        {
            userMsg = new JObject
            {
                ["role"] = "user",
                ["content"] = prompt.Text
            };
        }

        messages.Add(userMsg);

        var payload = new JObject
        {
            ["model"] = prompt.ModelName,
            ["messages"] = messages,
            ["stream"] = false
        };

        try
        {
            var body = await SendJsonAsync($"{_baseUrl}/v1/chat/completions", payload, _apiKey, ct).ConfigureAwait(false);
            var json = JObject.Parse(body);

            // OpenAI формат: choices[0].message.content
            var content = json["choices"]?[0]?["message"]?["content"]?.ToString();
            if (content == null)
            {
                // Иногда могут вернуть "error"
                var err = json["error"]?["message"]?.ToString();
                return new Response(true, err ?? "Неизвестный формат ответа LM Studio.");
            }

            // Сохраним историю как role+string content (без image частей). Это самый совместимый вариант.
            _history.Add(userMsg);
            _history.Add(new JObject
            {
                ["role"] = "assistant",
                ["content"] = content
            });

            return new Response(false, content.Trim());
        }
        catch (Exception ex)
        {
            return new Response(true, $"LM Studio error: {ex.Message}");
        }
    }

    public override async Task<string[]> GetModelsAsync(CancellationToken ct = default)
    {
        using (var req = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/v1/models"))
        {
            if (!string.IsNullOrEmpty(_apiKey))
                req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            using (var resp = await Http.SendAsync(req, HttpCompletionOption.ResponseContentRead, ct).ConfigureAwait(false))
            {
                var text = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new Exception($"LM Studio /v1/models failed: {(int)resp.StatusCode} {resp.ReasonPhrase} {text}");

                var json = JObject.Parse(text);
                var data = json["data"] as JArray;
                if (data == null) return new string[0];

                var list = new List<string>();
                foreach (var m in data)
                {
                    var id = m["id"]?.ToString();
                    if (!string.IsNullOrEmpty(id)) list.Add(id);
                }
                return list.ToArray();
            }
        }
    }
}

// ===== Пример использования =====
// var ollama = new OllamaProvider("http://localhost:11434");
// ollama.SetSystemPrompt("Ты полезный помощник.");
// var r1 = await ollama.SendMessageAsync(new Prompt { Text = "Привет!", ModelName = "llama3" }, includeHistory: true);
//
// var lm = new LmStudioProvider("http://localhost:1234"); // включите OpenAI API в LM Studio
// var p = new Prompt { Text = "Опиши картинку", ModelName = "your-vision-model" };
// p.SetImageFromPath("image.png");
// var r2 = await lm.SendMessageAsync(p, includeHistory: false);