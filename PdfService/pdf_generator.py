import io
import os
import qrcode
from reportlab.lib.pagesizes import A4, landscape, portrait
from reportlab.lib.units import mm
from reportlab.lib.colors import HexColor
from reportlab.pdfgen import canvas
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from PIL import Image


# A4 dimensions in points
A4_WIDTH_PT = A4[0]   # 595.27
A4_HEIGHT_PT = A4[1]  # 841.89

FONT_DIR = os.path.join(os.path.dirname(__file__), "fonts")


def _register_fonts():
    """Register custom TrueType fonts if available."""
    if not os.path.isdir(FONT_DIR):
        return

    for filename in os.listdir(FONT_DIR):
        if filename.lower().endswith(".ttf"):
            font_name = os.path.splitext(filename)[0]
            try:
                pdfmetrics.registerFont(TTFont(font_name, os.path.join(FONT_DIR, filename)))
            except Exception:
                pass


_register_fonts()


def _resolve_font(font_family: str, is_bold: bool, is_italic: bool) -> str:
    """Resolve font name with bold/italic variants."""
    # Custom TTF font variants (Turkish character support)
    custom_fonts = {
        "Arial": ("Arial", "Arial-Bold", "Arial-Italic", "Arial-BoldItalic"),
    }

    # Built-in fonts (no Turkish character support)
    builtin_fonts = {
        "Helvetica": ("Helvetica", "Helvetica-Bold", "Helvetica-Oblique", "Helvetica-BoldOblique"),
        "Times": ("Times-Roman", "Times-Bold", "Times-Italic", "Times-BoldItalic"),
        "Courier": ("Courier", "Courier-Bold", "Courier-Oblique", "Courier-BoldOblique"),
    }

    # Map Helvetica to Arial for Turkish support
    if font_family == "Helvetica":
        font_family = "Arial"

    all_fonts = {**custom_fonts, **builtin_fonts}

    if font_family in all_fonts:
        variants = all_fonts[font_family]
        if is_bold and is_italic:
            return variants[3]
        elif is_bold:
            return variants[1]
        elif is_italic:
            return variants[2]
        else:
            return variants[0]

    # For other custom fonts, just return the font_family name
    return font_family


def _get_alignment_x(alignment: str, x: float, width: float, text_width: float) -> float:
    """Calculate x position based on alignment within the field box."""
    if alignment == "center":
        return x + (width - text_width) / 2
    elif alignment == "right":
        return x + width - text_width
    else:  # left
        return x


def generate_certificate_pdf(
    orientation: str,
    background_image_path: str | None,
    layout: list[dict],
    signatures: list[dict],
    dynamic_values: dict[str, str],
) -> bytes:
    """Generate a single certificate PDF and return bytes."""

    if orientation == "landscape":
        page_size = landscape(A4)
    else:
        page_size = portrait(A4)

    page_width, page_height = page_size

    buffer = io.BytesIO()
    c = canvas.Canvas(buffer, pagesize=page_size)

    # Draw background image
    if background_image_path and os.path.isfile(background_image_path):
        c.drawImage(
            background_image_path,
            0, 0,
            width=page_width,
            height=page_height,
            preserveAspectRatio=False,
            mask="auto",
        )

    # Draw layout fields
    for field in layout:
        field_type = field.get("type", "static")
        text = ""

        if field_type == "dynamic":
            key = field.get("key", "")
            text = dynamic_values.get(key, f"[{key}]")
        elif field_type == "static":
            text = field.get("text", "")
        elif field_type == "qrcode":
            qr_data = dynamic_values.get("QrCode", dynamic_values.get("CertificateNo", ""))
            if qr_data:
                _draw_qr_code(c, qr_data, field, page_width, page_height)
            continue
        else:
            continue

        if not text:
            continue

        font_family = field.get("font_family", "Helvetica")
        font_size = field.get("font_size", 14)
        font_color = field.get("font_color", "#000000")
        is_bold = field.get("is_bold", False)
        is_italic = field.get("is_italic", False)
        alignment = field.get("alignment", "center")

        font_name = _resolve_font(font_family, is_bold, is_italic)

        # Field position - convert from top-left origin (frontend) to bottom-left origin (PDF)
        # Frontend sends coordinates as percentage (0-100) of page dimensions
        x = field.get("x", 0) / 100 * page_width
        y_from_top = field.get("y", 0) / 100 * page_height
        width = field.get("width", 100) / 100 * page_width
        height = field.get("height", 10) / 100 * page_height

        # Convert to PDF coordinate system (bottom-left origin)
        y = page_height - y_from_top - height

        c.setFont(font_name, font_size)
        c.setFillColor(HexColor(font_color))

        text_width = c.stringWidth(text, font_name, font_size)
        text_x = _get_alignment_x(alignment, x, width, text_width)
        text_y = y + (height - font_size) / 2  # Vertical center

        c.drawString(text_x, text_y, text)

    # Draw signatures
    if signatures:
        _draw_signatures(c, signatures, page_width, page_height)

    c.save()
    return buffer.getvalue()


def _draw_qr_code(c: canvas.Canvas, data: str, field: dict, page_width: float, page_height: float):
    """Draw a QR code on the canvas."""
    qr = qrcode.QRCode(version=1, box_size=10, border=1)
    qr.add_data(data)
    qr.make(fit=True)
    qr_img = qr.make_image(fill_color="black", back_color="white")

    img_buffer = io.BytesIO()
    qr_img.save(img_buffer, format="PNG")
    img_buffer.seek(0)

    from reportlab.lib.utils import ImageReader
    img_reader = ImageReader(img_buffer)

    x = field.get("x", 0) / 100 * page_width
    y_from_top = field.get("y", 0) / 100 * page_height
    width = field.get("width", 10) / 100 * page_width
    height = field.get("height", 10) / 100 * page_height
    size = min(width, height)

    y = page_height - y_from_top - size

    c.drawImage(img_reader, x, y, width=size, height=size, mask="auto")


def _draw_signatures(c: canvas.Canvas, signatures: list[dict], page_width: float, page_height: float):
    """Draw signature images, names, and titles using percentage-based positions."""
    if not signatures:
        return

    from reportlab.lib.utils import ImageReader

    for sig in signatures:
        # All positions are percentages (0-100) of page dimensions
        img_x = sig.get("image_x", 0) / 100 * page_width
        img_y_from_top = sig.get("image_y", 0) / 100 * page_height
        img_w = sig.get("image_width", 12) / 100 * page_width
        img_h = sig.get("image_height", 8) / 100 * page_height
        img_y = page_height - img_y_from_top - img_h

        # Draw signature image (with rotation support)
        image_url = sig.get("image_url")
        rotation = sig.get("image_rotation", 0)
        if image_url and os.path.isfile(image_url):
            try:
                pil_img = Image.open(image_url)
                from PIL import ImageOps
                pil_img = ImageOps.exif_transpose(pil_img)
                if rotation:
                    pil_img = pil_img.rotate(-rotation * 90, expand=True)
                img_buf = io.BytesIO()
                pil_img.save(img_buf, format="PNG")
                img_buf.seek(0)
                c.drawImage(
                    ImageReader(img_buf),
                    img_x, img_y,
                    width=img_w, height=img_h,
                    preserveAspectRatio=True,
                    mask="auto",
                )
            except Exception:
                pass

        c.setFillColor(HexColor("#000000"))

        # Draw name
        # Editor element: top=nameY%, height=3%, text vertically centered
        if sig.get("show_name", True):
            name = sig.get("name", "")
            if name:
                name_fs = sig.get("name_font_size", 8)
                c.setFont("Arial", name_fs)
                nx = sig.get("name_x", 0) / 100 * page_width
                name_y_top = sig.get("name_y", 0) / 100 * page_height
                el_h = 0.03 * page_height  # 3% height matching editor
                ny = page_height - name_y_top - el_h + (el_h - name_fs) / 2
                c.drawString(nx, ny, name)

        # Draw title
        # Editor element: top=titleY%, height=3%, text vertically centered
        if sig.get("show_title", True):
            title = sig.get("title", "")
            if title:
                title_fs = sig.get("title_font_size", 7)
                c.setFont("Arial", title_fs)
                tx = sig.get("title_x", 0) / 100 * page_width
                title_y_top = sig.get("title_y", 0) / 100 * page_height
                el_h = 0.03 * page_height  # 3% height matching editor
                ty = page_height - title_y_top - el_h + (el_h - title_fs) / 2
                c.drawString(tx, ty, title)
