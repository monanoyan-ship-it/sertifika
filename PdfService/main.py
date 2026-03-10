import os
import io
import json
import uuid
import logging
from fastapi import FastAPI, HTTPException, Request
from fastapi.responses import StreamingResponse, JSONResponse
from pydantic import BaseModel
from pdf_generator import generate_certificate_pdf

logging.basicConfig(level=logging.DEBUG)
logger = logging.getLogger(__name__)

app = FastAPI(title="Sertifika PDF Servisi", version="1.0.0")


@app.exception_handler(422)
async def validation_exception_handler(request: Request, exc):
    body = await request.body()
    logger.error(f"Validation error for {request.url}. Body: {body[:2000]}")
    logger.error(f"Error: {exc}")
    return JSONResponse(status_code=422, content={"detail": str(exc)})


class SignatureInfo(BaseModel):
    name: str
    title: str | None = None
    image_url: str | None = None
    show_name: bool = True
    show_title: bool = True
    image_x: float = 0
    image_y: float = 0
    image_width: float = 12
    image_height: float = 8
    image_rotation: int = 0
    name_x: float = 0
    name_y: float = 0
    title_x: float = 0
    title_y: float = 0
    name_font_size: int = 8
    title_font_size: int = 7


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

        # Use RFC 5987 encoding for non-ASCII filenames
        from urllib.parse import quote
        ascii_filename = filename.encode("ascii", errors="replace").decode("ascii")
        encoded_filename = quote(filename)
        content_disp = f"attachment; filename=\"{ascii_filename}\"; filename*=UTF-8''{encoded_filename}"

        return StreamingResponse(
            io.BytesIO(pdf_bytes),
            media_type="application/pdf",
            headers={"Content-Disposition": content_disp},
        )
    except Exception as e:
        import traceback
        traceback.print_exc()
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
