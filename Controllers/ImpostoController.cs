using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using STUR_mvc.Models;
using STUR_mvc.Services;

namespace STUR_mvc.Controllers
{
    public class ImpostoController : Controller
    {
        private readonly STURDBContext context;
        private readonly IPTUCalculoService iPTUCalculoService;

        public ImpostoController(STURDBContext context, IPTUCalculoService iPTUCalculoService)
        {
            this.context = context;
            this.iPTUCalculoService = iPTUCalculoService;
        }

        // GET: ImpostoController
        public ActionResult Index()
        {            
            return View();
        }

        [HttpPost]
        public ActionResult Index(IPTUConsultaViewModel viewModel)
        {
            var impostos = context.Impostos.ToList();
            return View("RetornoConsulta", new IPTURetornoConsultaViewModel { Impostos = impostos });
        }

        public ActionResult Calculo()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Calculo(IPTUCalculoViewModel viewModel)
        {
            //processamento iptu
            var impostosCalculados = await iPTUCalculoService.CalcularIPTU(viewModel.AnoBase, viewModel.InscricaoImovel);
            var retorno = new IPTURetornoCalculoViewModel { Impostos = impostosCalculados };
            return View("RetornoCalculo", retorno);
        }

        // GET: ImpostoController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ImpostoController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ImpostoController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ImpostoController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ImpostoController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ImpostoController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ImpostoController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
