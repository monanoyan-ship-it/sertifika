# Sertifika Projesi

## ClaudeManager Entegrasyonu

Tum proje bilgileri, mimari kurallar, teknoloji stack'i ve API endpoint'leri **ClaudeManager** uzerinde tutulur (Proje ID: 62).

**Her session basinda asagidaki API'leri MUTLAKA oku ve uygula:**

```bash
curl -s "http://127.0.0.1:41847/api/guide?cwd=$(pwd)"
curl -s "http://127.0.0.1:41847/api/projects/62/patterns"
```

- Pattern'leri oku, kurallara uy
- Yeni ogrenilenler oldugunda pattern olarak kaydet: `POST /api/patterns` (project_id: 62, type: rule/mistake/preference)
- Dashboard: http://127.0.0.1:41847/
