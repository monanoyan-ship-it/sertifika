import os
import io
import json
import uuid
from fastapi import FastAPI, HTTPException
from fastapi.responses import StreamingResponse
from pydantic import BaseModel
from pdf_generator import generate_certificate_pdf

app = FastAPI(title="Sertifika PDF Servisi", version="1.0.0")


class SignatureInfo(BaseModel):
    name: str
    title: str | None = None
    image_url: str | None = None


class FieldLayout(BaseModel):
    type: str  # "static" or "dynamic"
    key: str | None = None
    text: str | None = None
    x: float
    y: float
    width: float
    height: float
    font_family: str = "Helvetica"
    font_size: float = 14
    font_color: str = "#000000"
    is_bold: bool = False
    is_italic: bool = False
    alignment: str = "center"


class CertificateRequest(BaseModel):
    orientation: str = "landscape"  # "landscape" or "portrait"
    background_image_path: str | None = None
    layout: list[FieldLayout]
    signatures: list[SignatureInfo] = []
    dynamic_values: dict[str, str] = {}
    output_filename: str | None = None


class BatchCertificateRequest(BaseModel):
    orientation: str = "landscape"
    background_image_path: str | None = None
    layout: list[FieldLayout]
    signatures: list[SignatureInfo] = []
    participants: list[dict[str, str]]
    output_dir: str


class PreviewRequest(BaseModel):
    orientation: str = "landscape"
    background_image_path: str | None = None
    layout: list[FieldLayout]
    signatures: list[SignatureInfo] = []
    dynamic_values: dict[str, str] = {}


@app.get("/health")
def health_check():
    return {"status": "healthy", "service": "pdf-generator"}


@app.post("/generate")
def generate_single(request: CertificateRequest):
    try:
        pdf_bytes = generate_certificate_pdf(
            orientation=request.orientation,
            background_image_path=request.background_image_path,
            layout=[f.model_dump() for f in request.layout],
            signatures=[s.model_dump() for s in request.signatures],
            dynamic_values=request.dynamic_values,
        )

        filename = request.output_filename or f"certificate_{uuid.uuid4().hex[:8]}.pdf"

        return StreamingResponse(
            io.BytesIO(pdf_bytes),
            media_type="application/pdf",
            headers={"Content-Disposition": f'attachment; filename="{filename}"'},
        )
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/generate-batch")
def generate_batch(request: BatchCertificateRequest):
    os.makedirs(request.output_dir, exist_ok=True)
    results = []

    for participant in request.participants:
        try:
            pdf_bytes = generate_certificate_pdf(
                orientation=request.orientation,
                background_image_path=request.background_image_path,
                layout=[f.model_dump() for f in request.layout],
                signatures=[s.model_dump() for s in request.signatures],
                dynamic_values=participant,
            )

            filename = participant.get("_filename", f"certificate_{uuid.uuid4().hex[:8]}.pdf")
            filepath = os.path.join(request.output_dir, filename)

            with open(filepath, "wb") as f:
                f.write(pdf_bytes)

            results.append({"participant": participant.get("HolderName", ""), "file": filepath, "success": True})
        except Exception as e:
            results.append({"participant": participant.get("HolderName", ""), "error": str(e), "success": False})

    return {"total": len(request.participants), "results": results}


@app.post("/preview")
def preview(request: PreviewRequest):
    try:
        pdf_bytes = generate_certificate_pdf(
            orientation=request.orientation,
            background_image_path=request.background_image_path,
            layout=[f.model_dump() for f in request.layout],
            signatures=[s.model_dump() for s in request.signatures],
            dynamic_values=request.dynamic_values,
        )

        return StreamingResponse(
            io.BytesIO(pdf_bytes),
            media_type="application/pdf",
            headers={"Content-Disposition": 'inline; filename="preview.pdf"'},
        )
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="127.0.0.1", port=5050)
