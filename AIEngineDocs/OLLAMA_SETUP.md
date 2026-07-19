# Ollama Setup & Run Guide (Quick README)

## 1. Install Ollama

### Windows

* Download from: https://ollama.com/download
* Run installer
* Verify:

```bash
ollama --version
```

### Linux (Ubuntu)

```bash
curl -fsSL https://ollama.com/install.sh | sh
```

### macOS

```bash
brew install ollama
```

---

## 2. Start Ollama Service

```bash
ollama serve
```

(Default runs on: http://localhost:11434)

---

## 3. Pull a Model

Example:

```bash
ollama pull phi3:latest
```

or

```bash
ollama pull qwen:0.5b
```

or

```bash
ollama pull smollm:135m
```
---

## 4. Run Model (CLI Test)

```bash
ollama run phi3
```

Type your prompt directly.

---

## 5. API Usage (Your .NET Code)

Endpoint:

```
POST http://localhost:11434/api/chat
```

Sample Payload:

```json
{
  "model": "phi3",
  "messages": [
    { "role": "user", "content": "Hello" }
  ]
}
```

---

## 6. Verify Working

```bash
curl http://localhost:11434
```

Should return status OK.

---

## 7. Useful Commands

```bash
ollama list        # installed models
ollama pull <name> # download model
ollama run <name>  # run model
ollama rm <name>   # delete model
```

---

## Done

You now have a local AI model running and ready for your .NET integration.
