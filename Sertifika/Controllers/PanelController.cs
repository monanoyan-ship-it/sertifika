using Microsoft.AspNetCore.Mvc;

namespace Sertifika.Controllers;

public class PanelController : Controller
{
    private readonly IConfiguration _configuration;

    public PanelController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private void SetViewData(string activeMenu, string title)
    {
        ViewData["ActiveMenu"] = activeMenu;
        ViewData["Title"] = title;
    }

    public IActionResult Login() => View();

    public IActionResult Dashboard()
    {
        SetViewData("dashboard", "Dashboard");
        return View();
    }

    public IActionResult Templates()
    {
        SetViewData("templates", "Sablon Yonetimi");
        return View();
    }

    public IActionResult TemplateEditor(int? id)
    {
        SetViewData("templates", id.HasValue ? "Sablon Duzenle" : "Yeni Sablon");
        ViewData["TemplateId"] = id;
        return View();
    }

    public IActionResult Signatures()
    {
        SetViewData("signatures", "Imza Kutuphanesi");
        return View();
    }

    public IActionResult Trainings()
    {
        SetViewData("trainings", "Egitim Yonetimi");
        return View();
    }

    public IActionResult TrainingDetail(int id)
    {
        SetViewData("trainings", "Egitim Detay");
        ViewData["TrainingId"] = id;
        return View();
    }

    public IActionResult Companies()
    {
        SetViewData("companies", "Firma Yonetimi");
        return View();
    }

    public IActionResult EmailTemplates()
    {
        SetViewData("emailtemplates", "E-posta Sablonlari");
        return View();
    }

    public IActionResult Settings()
    {
        SetViewData("settings", "Ayarlar");
        return View();
    }

    public IActionResult OAuthCallback()
    {
        return View();
    }

    [Microsoft.AspNetCore.Mvc.Route("Panel/Error/{code:int}")]
    public IActionResult Error(int code)
    {
        Response.StatusCode = code;
        ViewData["StatusCode"] = code;
        return View("Error");
    }
}
