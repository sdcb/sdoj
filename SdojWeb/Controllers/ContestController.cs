using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using Microsoft.AspNet.Identity;
using SdojWeb.Infrastructure.Alerts;
using SdojWeb.Infrastructure.Identity;
using SdojWeb.Manager;
using SdojWeb.Models;
using SdojWeb.Models.ContestModels;
using SdojWeb.Models.DbModels;

namespace SdojWeb.Controllers
{
    [SdojAuthorize]
    [RoutePrefix("contest")]
    public class ContestController : Controller
    {
        public ContestController(
            ContestManager manager, 
            IPrincipal identity, 
            ApplicationDbContext db)
        {
            _manager = manager;
            _identity = identity;
            _db = db;
        }

        public ActionResult Index()
        {
            bool isManager = _identity.IsInRole(SystemRoles.ContestAdmin);
            IQueryable<ContestListModel> data = _manager.List(GetCurrentUserId(), isManager);
            return View(data);
        }

        [SdojAuthorize(Roles = SystemRoles.ContestAdminOrCreator)]
        [HttpGet]
        public ActionResult Create()
        {
            return View(new ContestCreateModel());
        }

        [ValidateAntiForgeryToken, HttpPost, ActionName(nameof(Create))]
        [SdojAuthorize(Roles = SystemRoles.ContestAdminOrCreator)]
        public async Task<ActionResult> CreateConfirmed(ContestCreateModel model)
        {
            await model.Validate(_db, ModelState);
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            int id = await _manager.Create(model, GetCurrentUserId());
            return RedirectToAction(nameof(Details), new { id = id });
        }

        [Route("details/{contestId}")]
        [Route("details/{contestId}/question/{rank}")]
        public async Task<ActionResult> Details(int contestId, int rank = 1)
        {
            if (!await _manager.CheckAccess(contestId, User.IsInRole(SystemRoles.ContestAdmin), GetCurrentUserId()))
            {
                return RedirectToAction("Index").WithWarning("此考试不存在或你无权限访问。");
            }
            ContestDetailsModel details = await _manager.Get(contestId);
            details.CurrentQuestion = await _manager.GetQuestion(contestId, rank);
            details.Rank = rank;
            return View(details);
        }

        [Route("details/{contestId}/question-{questionId}/solutions")]
        public async Task<ActionResult> QuestionSolutions(int contestId, int questionId)
        {
            if (!await _manager.CheckAccess(contestId, questionId, User.IsInRole(SystemRoles.ContestAdmin), GetCurrentUserId()))
            {
                return new HttpUnauthorizedResult();
            }

            List<SolutionSummaryModel> data = await _manager.GetQuestionSolutions(questionId);
            return PartialView(data);
        }

        [Route("details/{contestId}/question-{questionId}/submit")]
        [HttpPost]
        public async Task<ActionResult> Submit(int contestId, int questionId, SolutionCreateModel model)
        {
            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(400, String.Join("\n", ModelState.Select(x => $"{x.Key}:{x.Value}")));
            }

            if (!await _manager.CheckAccess(contestId, questionId, User.IsInRole(SystemRoles.ContestAdmin), GetCurrentUserId()))
            {
                return new HttpUnauthorizedResult();
            }

            int solutionId = await _manager.CreateSolution(contestId, model);
            await _manager.PushSolutionJudge(solutionId);
            return new HttpStatusCodeResult(201, solutionId.ToString());
        }

        private int GetCurrentUserId()
        {
            return _identity.Identity.GetUserId<int>();
        }

        private readonly ContestManager _manager;
        private readonly IPrincipal _identity;
        private readonly ApplicationDbContext _db;
    }
}
