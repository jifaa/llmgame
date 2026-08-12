from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
import requests
import json
import re

app = FastAPI()

# ==================== CONFIG 9ROUTER ====================
NINEROUTER_URL = "http://192.168.1.177:20128/v1/chat/completions"
# Nama model atau combo di dashboard 9Router
MODEL_NAME = "juancombo"
# API Key 9Router
NINEROUTER_API_KEY = "sk-b06ae4c59b658f32-gfdyq4-5e889a4c"
# =========================================================

class AIRequest(BaseModel):
    prompt: str

def parse_9router_response(text: str) -> str:
    text = text.strip()
    if not text:
        return ""

    # 1. Parse format Standard JSON
    try:
        data = json.loads(text)
        if isinstance(data, dict):
            if "choices" in data and len(data["choices"]) > 0:
                choice = data["choices"][0]
                msg = choice.get("message", {})
                content = msg.get("content") or msg.get("reasoning_content") or ""
                if not content and "delta" in choice:
                    content = choice["delta"].get("content") or choice["delta"].get("reasoning_content") or ""
                return str(content)
            elif "response" in data:
                return str(data.get("response", ""))
    except Exception:
        pass

    # 2. Parse format SSE Streaming (data: {...}\n\ndata: [DONE])
    collected_text = []
    for line in text.splitlines():
        line = line.strip()
        if not line or line.startswith("data: [DONE]"):
            continue
        if line.startswith("data:"):
            json_str = line[len("data:"):].strip()
            try:
                chunk = json.loads(json_str)
                if "choices" in chunk and len(chunk["choices"]) > 0:
                    choice = chunk["choices"][0]
                    delta = choice.get("delta", {})
                    content_piece = delta.get("content") or delta.get("reasoning_content") or ""
                    if not content_piece:
                        msg = choice.get("message", {})
                        content_piece = msg.get("content") or msg.get("reasoning_content") or ""
                    if content_piece:
                        collected_text.append(content_piece)
            except Exception:
                continue

    if collected_text:
        return "".join(collected_text)

    return text

def clean_reply(text: str) -> str:
    if not text:
        return "Aku belum bisa jawab itu."

    text = text.strip().strip('"“”')

    bad_patterns = [
        "Okay, let's",
        "Let me",
        "I need to",
        "The user",
        "First,"
    ]

    for bad in bad_patterns:
        if text.lower().startswith(bad.lower()):
            return "Hm? Maksudmu apa?"

    # Bersihkan tag reasoning/thinking jika menggunakan model seperti DeepSeek R1 / Qwen reasoning
    text = re.sub(r"<think>.*?</think>", "", text, flags=re.DOTALL)
    return text.strip()

@app.post("/generate")
def generate_text(data: AIRequest):
    headers = {
        "Content-Type": "application/json",
        "Authorization": f"Bearer {NINEROUTER_API_KEY}"
    }

    payload = {
        "model": MODEL_NAME,
        "messages": [
            {
                "role": "user",
                "content": data.prompt
            }
        ],
        "stream": False,
        "temperature": 0.4,
        "max_tokens": 300
    }

    print(f"\n[INFO] Mengirim prompt ke 9Router ({NINEROUTER_URL}) | Model: {MODEL_NAME}")
    try:
        response = requests.post(
            NINEROUTER_URL,
            headers=headers,
            json=payload,
            timeout=120
        )

        if response.status_code != 200:
            print(f"[ERROR 9ROUTER] Status: {response.status_code}")
            print(f"[ERROR 9ROUTER] Response Body: {response.text}")
            raise HTTPException(status_code=500, detail=f"9Router Error ({response.status_code}): {response.text}")

        # Parsing respons dengan aman (mendukung Standard JSON maupun SSE format)
        raw_reply = parse_9router_response(response.text)
        reply = clean_reply(raw_reply)

        print(f"[SUCCESS] NPC Reply: {reply}\n")

        return {
            "reply": reply
        }

    except requests.exceptions.RequestException as e:
        print(f"[ERROR KONEKSI] Gagal terhubung ke 9Router: {e}")
        raise HTTPException(status_code=500, detail=f"Gagal koneksi ke 9Router: {str(e)}")
    except Exception as e:
        print(f"[ERROR LAIN] Terjadi error: {e}")
        raise HTTPException(status_code=500, detail=str(e))